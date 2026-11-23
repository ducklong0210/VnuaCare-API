/**
 * Nghiệp vụ thêm mới Bác sĩ: Lưu thông tin chuyên khoa và tạo tài khoản DOCTOR
 */

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

namespace VnuaCare.Business.Business.Doctors.DoctorCommands;

/// <summary>
/// Lệnh thêm mới Bác sĩ và tự động tạo tài khoản đăng nhập
/// </summary>
public class CreateDoctorCommand : IRequest<Unit>
{
    public CreateDoctorModel Model { get; set; }

    public CreateDoctorCommand(CreateDoctorModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateDoctorCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;                // Kết nối CSDL để lưu trữ User và Doctor
        private readonly ICacheService _cacheService;                    // Dịch vụ xóa Cache sau khi tạo mới
        private readonly IContextAccessor _contextAccessor;              // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, tạo tài khoản User và lưu hồ sơ Bác sĩ vào CSDL
        public async Task<Unit> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm bác sĩ mới: " + model.FullName);

            // Kiểm tra phân quyền người thực hiện
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("role?");
            }

            #region Kiểm tra tính hợp lệ (Validation)

            // Kiểm tra định dạng Email
            if (string.IsNullOrWhiteSpace(model.Email) || !ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }

            // Kiểm tra Chuyên khoa
            if (model.SpecialtyId <= 0)
            {
                throw new ArgumentException("Vui long dien chuyen khoa");
            }

            // Kiểm tra trùng Mã bác sĩ trong CSDL
            var checkCode = await _dataContext.VcDoctors
                .AnyAsync(x => x.DoctorCode == model.DoctorCode.Trim(), cancellationToken);
            if (checkCode)
            {
                throw new ArgumentException($"Mã bac si '{model.DoctorCode}' đã tồn tại trên hệ thống.");
            }
            
            #endregion

            #region Lưu dữ liệu vào CSDL
            var userEntity = await _dataContext.VcUsers
                .FirstOrDefaultAsync(x => x.UserId == model.UserId, cancellationToken);
            if (userEntity != null && !string.IsNullOrWhiteSpace(model.FullName))
            {
                userEntity.FullName = model.FullName.Trim();
                userEntity.UpdatedAt = DateTime.Now;
                _dataContext.VcUsers.Update(userEntity);
            }

            // Tạo hồ sơ Bác sĩ gắn với UserId vừa sinh
            var doctorEntity = new VcDoctors
            {
                UserId = model.UserId,
                DoctorCode = model.DoctorCode,
                LicenseNumber = model.LicenseNumber,
                SpecialtyId = model.SpecialtyId,
                HospitalName = model.HospitalName,
            };
            await _dataContext.VcDoctors.AddAsync(doctorEntity, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            // Xóa Cache danh sách bác sĩ và cache người dùng liên kết để cập nhật dữ liệu mới nhất
            _cacheService.Remove(DoctorConstant.BuildCacheKey());
            _cacheService.Remove(DoctorConstant.BuildCacheKey($"specialty-{doctorEntity.SpecialtyId}"));
            _cacheService.Remove(VnuaCare.Business.Business.Users.UserConstant.BuildCacheKey(model.UserId.ToString()));
            
            Log.Information("Thêm bác sĩ thành công ID: " + doctorEntity.DoctorId);
            return Unit.Value;
        }
    }
}
