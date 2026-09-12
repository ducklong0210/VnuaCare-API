/**
 * Nghiệp vụ cập nhật thông tin Cán bộ (Học vị, chức vụ, phòng ban, SĐT, địa chỉ)
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Staffs.StaffCommands;

public class UpdateStaffCommand : IRequest<Unit>
{
    public UpdateStaffModel Model { get; set; }
    public UpdateStaffCommand(UpdateStaffModel model)
    {
        Model = model;
    }
    
    public class Handler : IRequestHandler<UpdateStaffCommand,Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật thông tin cán bộ: " + JsonSerializer.Serialize(model));

            // tìm hồ sơ cán bộ
            var entity = await _dataContext.VcStaffs.FirstOrDefaultAsync(x => x.StaffId == model.StaffId,cancellationToken);
            if (entity == null)
            {
                throw new ArgumentException("Không tìm thấy hồ sơ cán bộ cần cập nhật.");
            }

            #region Tính hợp lệ ( Validation )
            
            var currentRole = _contextAccessor.Role;
            if (currentRole != Role.SUPER_ADMIN.ToString() && 
                currentRole != Role.HEALTH_ADMIN.ToString() &&
                currentRole != Role.STAFF.ToString())
            {
                throw new ArgumentException("Bạn không có quyền thay đổi thông tin cán bộ.");
            }
            
            if (!ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }
            
            // kiểm tra trùng email
            var isEmailExisted = await _dataContext.VcUsers.AnyAsync(x => x.Email == model.Email.Trim());
            if (isEmailExisted)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác.");
            }
            // kiểm tra số điện thoại
            var isPhoneExisted = await _dataContext.VcStaffs.AnyAsync(x => x.PhoneNumber == model.PhoneNumber);
            if (isPhoneExisted)
            {
                throw new ArgumentException($"Số điện thoại '{model.PhoneNumber}' đã được sử dụng bởi cán bộ khác.");
            }
            #endregion

            var userEntity =
                await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.UserId == entity.UserId, cancellationToken);
            if (userEntity != null && !string.IsNullOrEmpty(model.Email))
            {
                userEntity.Email = model.Email.Trim();
                userEntity.UpdatedAt = DateTime.Now;
                _dataContext.VcUsers.Update(userEntity); // update bảng user
            }
            
            entity.StaffId = model.StaffId;
            model.UpdateEntity(entity);
            
            _dataContext.VcStaffs.Update(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            // xóa cache
            _cacheService.Remove(StaffConstant.BuildCacheKey(entity.StaffId.ToString()));
            _cacheService.Remove((StaffConstant.BuildCacheKey()));
            
            return Unit.Value;
        }
        
    }
}