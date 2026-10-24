/**
 * Nghiệp vụ Import danh sách Cán bộ hàng loạt (ImportrStaffFormExcelCommand)
 * Hỗ trợ tạo đồng thời tài khoản đăng nhập và hồ sơ cán bộ Học viện Nông nghiệp
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Business.Users;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Staffs.StaffCommands;

public class ImportrStaffFormExcelCommand : IRequest<ImportStaffResultModel>
{
    public ImportStaffBatchModel Model { get; }

    public ImportrStaffFormExcelCommand(ImportStaffBatchModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<ImportrStaffFormExcelCommand, ImportStaffResultModel>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBcryptPasswordHasher _passwordHasher;

        public Handler(
            VnuaCareDataContext dataContext,
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor,
            IBcryptPasswordHasher passwordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
            _passwordHasher = passwordHasher;
        }

        public async Task<ImportStaffResultModel> Handle(ImportrStaffFormExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new ImportStaffResultModel();

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện import danh sách cán bộ (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            if (request.Model.Staffs == null || request.Model.Staffs.Count == 0)
            {
                result.Errors.Add("Danh sách cán bộ cần nhập trống.");
                return result;
            }

            int index = 0;
            foreach (var staff in request.Model.Staffs)
            {
                index++;
                try
                {
                    #region Kiểm tra dữ liệu
                    if (string.IsNullOrWhiteSpace(staff.FullName))
                    {
                        result.TotalFailed++;
                        result.Errors.Add($"Dòng {index}: Họ và tên không được để trống.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(staff.StaffCode))
                    {
                        result.TotalFailed++;
                        result.Errors.Add($"Dòng {index}: Mã cán bộ không được để trống.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(staff.Username))
                    {
                        result.TotalFailed++;
                        result.Errors.Add($"Dòng {index} ({staff.FullName}): Tên đăng nhập không được để trống.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(staff.Email) || !ValidationUtils.IsEmail(staff.Email.Trim()))
                    {
                        result.TotalFailed++;
                        result.Errors.Add($"Dòng {index} ({staff.FullName}): Email không hợp lệ.");
                        continue;
                    }

                    string password = string.IsNullOrWhiteSpace(staff.Password) ? "Vnua@123456" : staff.Password;

                    // Kiểm tra phòng ban
                    var deptExists = await _dataContext.VcDepartments
                        .AnyAsync(d => d.DepartmentId == staff.DepartmentId, cancellationToken);
                    if (!deptExists)
                    {
                        result.TotalFailed++;
                        result.Errors.Add($"Dòng {index} ({staff.FullName}): ID Khoa/Phòng ban '{staff.DepartmentId}' không tồn tại.");
                        continue;
                    }

                    // Kiểm tra trùng lặp
                    var userExists = await _dataContext.VcUsers
                        .AnyAsync(u => u.Username == staff.Username.Trim() || u.Email == staff.Email.Trim(), cancellationToken);
                    if (userExists)
                    {
                        result.TotalFailed++;
                        result.Errors.Add($"Dòng {index} ({staff.FullName}): Tên đăng nhập hoặc Email đã tồn tại.");
                        continue;
                    }

                    var codeExists = await _dataContext.VcStaffs
                        .AnyAsync(s => s.EmployeeCode == staff.StaffCode.Trim(), cancellationToken);
                    if (codeExists)
                    {
                        result.TotalFailed++;
                        result.Errors.Add($"Dòng {index} ({staff.FullName}): Mã cán bộ '{staff.StaffCode}' đã tồn tại.");
                        continue;
                    }
                    #endregion

                    #region Tạo User và Staff
                    var user = new VcUsers
                    {
                        Username = staff.Username.Trim(),
                        Email = staff.Email.Trim(),
                        Password = _passwordHasher.HashPassword(password),
                        Role = Role.STAFF.ToString(),
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await _dataContext.VcUsers.AddAsync(user, cancellationToken);
                    await _dataContext.SaveChangesAsync(cancellationToken);

                    var staffEntity = new VcStaffs
                    {
                        UserId = user.UserId,
                        EmployeeCode = staff.StaffCode.Trim().ToUpper(),
                        FullName = staff.FullName.Trim(),
                        Gender = staff.Gender,
                        DateOfBirth = staff.DateOfBirth ?? DateTime.Now,
                        DepartmentId = staff.DepartmentId,
                        JobTitle = staff.JobTitle,
                        AcademicTitle = staff.AcademicTitle,
                        PhoneNumber = staff.PhoneNumber,
                        Address = staff.Adrress,
                        AvatarUrl = staff.AvatarUrl
                    };
                    await _dataContext.VcStaffs.AddAsync(staffEntity, cancellationToken);
                    await _dataContext.SaveChangesAsync(cancellationToken);

                    result.TotalSuccess++;
                    #endregion
                }
                catch (Exception ex)
                {
                    result.TotalFailed++;
                    result.Errors.Add($"Dòng {index} ({staff.FullName}): Lỗi xử lý - {ex.Message}");
                    Log.Error(ex, $"Lỗi import cán bộ dòng {index}");
                }
            }

            #region Xóa cache
            _cacheService.Remove(StaffConstant.BuildCacheKey());
            _cacheService.Remove(UserConstant.BuildCacheKey());
            _cacheService.Remove(VnuaCareCacheConstant.LIST_SELECT);
            #endregion

            Log.Information($"Hoàn tất import cán bộ: Thành công {result.TotalSuccess}, Thất bại {result.TotalFailed}");
            return result;
        }
    }
}