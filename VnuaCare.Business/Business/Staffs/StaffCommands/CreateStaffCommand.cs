/**
 * Nghiệp vụ thêm mới Cán bộ: Lưu hồ sơ và tự động tạo tài khoản đăng nhập
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Staffs.StaffCommands;

public class CreateStaffCommand : IRequest<Unit>
{
    public CreateStaffModel Model { get; set; }
    
    public CreateStaffCommand(CreateStaffModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateStaffCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để ghi dữ liệu User và Staff
        private readonly ICacheService _cacheService;            // Dịch vụ xóa cache sau khi thêm mới
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(
            VnuaCareDataContext dataContext, 
            ICacheService cacheService, 
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, tạo tài khoản đăng nhập và lưu hồ sơ Cán bộ
        public async Task<Unit> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm cán bộ mới: " + JsonSerializer.Serialize(model));
           
            #region Phân quyền người thực hiện
            var currentUserRole = _contextAccessor.Role;

            // Chấp nhận SUPER_ADMIN, hoặc HEALTH_ADMIN
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng này (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region Validation

            if (model.DepartmentId <= 0)
            {
                throw new ArgumentException("Vui lòng chọn Khoa / Phòng ban hợp lệ.");
            }

            // Kiểm tra xem Khoa/Phòng ban có tồn tại và đang hoạt động trong CSDL không
            var departmentExists = await _dataContext.VcDepartments
                .AnyAsync(x => x.DepartmentId == model.DepartmentId && x.IsActive, cancellationToken);
            if (!departmentExists)
            {
                throw new ArgumentException("Khoa / Phòng ban được chọn không tồn tại hoặc đã bị vô hiệu hóa.");
            }
            #endregion

            #region Kiểm tra trùng lặp thông tin định danh (Check Duplicate)
            //  Kiểm tra trùng Mã cán bộ trong bảng vc_staffs
            var checkCode = await _dataContext.VcStaffs
                .AnyAsync(x => x.EmployeeCode == model.StaffCode.Trim(), cancellationToken);
            if (checkCode)
            {
                throw new ArgumentException($"Mã cán bộ '{model.StaffCode}' đã tồn tại trên hệ thống.");
            }

            // Kiểm tra trùng Số điện thoại trong bảng 
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var checkPhone = await _dataContext.VcStaffs
                    .AnyAsync(x => x.PhoneNumber == model.PhoneNumber.Trim(), cancellationToken);
                if (checkPhone)
                {
                    throw new ArgumentException($"Số điện thoại '{model.PhoneNumber}' đã được đăng ký bởi cán bộ khác.");
                }
            }
            
            // kiểm tra còn hoạt động không
            var userEntity = await _dataContext.VcUsers
                .FirstOrDefaultAsync(x => x.UserId == model.UserId && x.IsActive, cancellationToken);
            if (userEntity == null)
            {
                throw new ArgumentException("Tài khoản người dùng không tồn tại hoặc đã bị khóa.");
            }
            #endregion

            #region Lưu tk vào CSDL
            if (!string.IsNullOrWhiteSpace(model.FullName))
            {
                userEntity.FullName = model.FullName.Trim();
                userEntity.UpdatedAt = DateTime.Now;
                _dataContext.VcUsers.Update(userEntity);
            }

            //Tạo hồ sơ cán bộ VcStaffs gắn liền với UserId vừa sinh
            var staffEntity = new VcStaffs
            {
                UserId = model.UserId,
                DepartmentId = model.DepartmentId,
                EmployeeCode = model.StaffCode.Trim(),
                Gender = string.IsNullOrWhiteSpace(model.Gender) ? "NAM" : model.Gender.Trim().ToUpper(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                AcademicTitle = model.AcademicTitle?.Trim(),
                JobTitle = model.JobTitle?.Trim(),
                Address = string.IsNullOrWhiteSpace(model.Adrress) ? "" : model.Adrress.Trim(),
                AvatarUrl = string.IsNullOrWhiteSpace(model.AvatarUrl) ? "" : model.AvatarUrl.Trim(),
            };

            await _dataContext.VcStaffs.AddAsync(staffEntity, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache danh sách cán bộ và cache người dùng liên kết để cập nhật dữ liệu mới
            _cacheService.Remove(StaffConstant.BuildCacheKey());
            _cacheService.Remove(StaffConstant.BuildCacheKey($"dept-{staffEntity.DepartmentId}"));
            _cacheService.Remove(VnuaCare.Business.Business.Users.UserConstant.BuildCacheKey(model.UserId.ToString()));
            #endregion

            Log.Information($"Thêm cán bộ thành công: {userEntity.FullName} ({staffEntity.EmployeeCode}) - UserId: {model.UserId}");

            return Unit.Value;
        }
    }
}
