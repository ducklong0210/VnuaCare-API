/**
 * Nghiệp vụ cập nhật thông tin Bác sĩ: Cập nhật hồ sơ chuyên môn và tài khoản người dùng
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Doctors.DoctorCommands;

/// <summary>
/// Lệnh cập nhật thông tin Bác sĩ
/// </summary>
public class UpdateDoctorCommand : IRequest<Unit>
{
    public UpdateDoctorModel Model { get; set; }
    
    public UpdateDoctorCommand(UpdateDoctorModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateDoctorCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để tìm kiếm và cập nhật dữ liệu
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi cập nhật
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(
            VnuaCareDataContext dataContext, 
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor, 
            IBcryptPasswordHasher cryptPasswordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và lưu thông tin cập nhật của Bác sĩ vào CSDL
        public async Task<Unit> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật thông tin bác sĩ ID: " + model.DoctorId);
            
            // Tìm hồ sơ bác sĩ trong CSDL
            var entity = await _dataContext.VcDoctors.FirstOrDefaultAsync(x => x.DoctorId == model.DoctorId);
            if (entity == null)
            {
                throw new ArgumentException("Không tìm thấy hồ sơ bác sĩ cần cập nhật.");
            }

            #region Kiểm tra tính hợp lệ (Validation)
            // Kiểm tra phân quyền người thực hiện
            var currentRole = _contextAccessor.Role;
            if (currentRole != Role.SUPER_ADMIN.ToString() &&
                currentRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new ArgumentException("Bạn không có quyền thay đổi thông tin bác sĩ.");
            }

            // Kiểm tra định dạng Email
            if (!ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }
            
            // Kiểm tra trùng Email trong hệ thống
            var isEmailExisted = await _dataContext.VcUsers
                .AnyAsync(x => x.Email == model.Email.Trim() && x.UserId != entity.UserId, cancellationToken);

            if (isEmailExisted)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác.");
            }
            #endregion
            
            // Cập nhật thông tin Email của tài khoản User liên kết
            var userEntity = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.UserId == entity.UserId);
            if (userEntity != null && !string.IsNullOrEmpty(userEntity.Email))
            {
                userEntity.Email = model.Email.Trim();
                userEntity.UpdatedAt = DateTime.Now;
                _dataContext.VcUsers.Update(userEntity);
            }

            // Cập nhật thông tin hồ sơ bác sĩ
            entity.DoctorId = model.DoctorId;
            model.UpdateEntity(entity);
            
            _dataContext.VcDoctors.Update(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            // Xóa Cache thông tin bác sĩ để cập nhật dữ liệu mới nhất
            _cacheService.Remove(DoctorConstant.BuildCacheKey(entity.DoctorId.ToString()));
            _cacheService.Remove(DoctorConstant.BuildCacheKey());
            
            Log.Information("Cập nhật bác sĩ thành công ID: " + entity.DoctorId);
            return Unit.Value;
        }
    }
}
