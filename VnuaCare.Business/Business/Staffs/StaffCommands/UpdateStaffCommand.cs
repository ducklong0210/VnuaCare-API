/**
 * Nghiệp vụ cập nhật thông tin Cán bộ: Cập nhật chức vụ, học hàm, phòng ban, SĐT, Email và hồ sơ liên quan
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

/// <summary>
/// Lệnh cập nhật thông tin cán bộ
/// </summary>
public class UpdateStaffCommand : IRequest<Unit>
{
    public UpdateStaffModel Model { get; set; }
    
    public UpdateStaffCommand(UpdateStaffModel model)
    {
        Model = model;
    }
    
    public class Handler : IRequestHandler<UpdateStaffCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để kiểm tra và cập nhật dữ liệu
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi cập nhật
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

        // Xử lý kiểm tra dữ liệu và lưu thông tin cập nhật cán bộ vào CSDL
        public async Task<Unit> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật thông tin cán bộ: " + JsonSerializer.Serialize(model));

            // Tìm hồ sơ cán bộ theo StaffId trong CSDL
            var entity = await _dataContext.VcStaffs.FirstOrDefaultAsync(x => x.StaffId == model.StaffId, cancellationToken);
            if (entity == null)
            {
                throw new ArgumentException("Không tìm thấy hồ sơ cán bộ cần cập nhật.");
            }

            #region Kiểm tra tính hợp lệ (Validation)
            // Kiểm tra phân quyền người thực hiện
            var currentRole = _contextAccessor.Role;
            if (currentRole != Role.SUPER_ADMIN.ToString() && 
                currentRole != Role.HEALTH_ADMIN.ToString() &&
                currentRole != Role.STAFF.ToString())
            {
                throw new ArgumentException("Bạn không có quyền thay đổi thông tin cán bộ.");
            }
            
            // Kiểm tra định dạng Email
            if (!ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }
            
            // Kiểm tra trùng Email trong bảng User
            var isEmailExisted = await _dataContext.VcUsers.AnyAsync(x => x.Email == model.Email.Trim());
            if (isEmailExisted)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác.");
            }
            
            // Kiểm tra trùng Số điện thoại trong bảng Staff
            var isPhoneExisted = await _dataContext.VcStaffs.AnyAsync(x => x.PhoneNumber == model.PhoneNumber);
            if (isPhoneExisted)
            {
                throw new ArgumentException($"Số điện thoại '{model.PhoneNumber}' đã được sử dụng bởi cán bộ khác.");
            }
            #endregion

            // Tìm và cập nhật thông tin Email của tài khoản User liên kết
            var userEntity = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.UserId == entity.UserId, cancellationToken);
            if (userEntity != null && !string.IsNullOrEmpty(model.Email))
            {
                userEntity.Email = model.Email.Trim();
                userEntity.UpdatedAt = DateTime.Now;
                _dataContext.VcUsers.Update(userEntity);
            }
            
            // Cập nhật thông tin hồ sơ cán bộ
            entity.StaffId = model.StaffId;
            model.UpdateEntity(entity);
            _dataContext.VcStaffs.Update(entity);
            
            // Lưu các thay đổi vào CSDL
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            // Xóa Cache thông tin cán bộ để cập nhật dữ liệu mới nhất
            _cacheService.Remove(StaffConstant.BuildCacheKey(entity.StaffId.ToString()));
            _cacheService.Remove(StaffConstant.BuildCacheKey());
            
            Log.Information($"Cập nhật cán bộ thành công ID: {entity.StaffId}");
            return Unit.Value;
        }
    }
}
