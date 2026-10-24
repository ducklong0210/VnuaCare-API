/**
 * Nghiệp vụ người dùng tự cập nhật thông tin hồ sơ cá nhân (SĐT, Địa chỉ, Avatar, Email...)
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

namespace VnuaCare.Business.Business.Users.UserCommands;

/// <summary>
/// Lệnh người dùng tự cập nhật thông tin cá nhân của tài khoản đang đăng nhập
/// </summary>
public class UpdateMyProfileCommand : IRequest<Unit>
{
    public UpdateMyProfileModel Model { get; set; }

    public UpdateMyProfileCommand(UpdateMyProfileModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateMyProfileCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // DbContext Ghi
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất UserId từ Token

        public Handler(
            VnuaCareDataContext dataContext, 
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            var currentUserId = _contextAccessor.UserId;

            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc Token không hợp lệ.");
            }

            Log.Information($"Người dùng ID {currentUserId} cập nhật thông tin cá nhân: {JsonSerializer.Serialize(model)}");

            #region 1. Kiểm tra tính hợp lệ dữ liệu (Validation)
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                throw new ArgumentException("Họ và tên không được để trống.");
            }

            if (!string.IsNullOrWhiteSpace(model.Email) && !ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }

            // Kiểm tra trùng Email trong bảng VcUsers
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var isEmailExisted = await _dataContext.VcUsers
                    .AnyAsync(x => x.Email == model.Email.Trim() && x.UserId != currentUserId.Value, cancellationToken);
                if (isEmailExisted)
                {
                    throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi tài khoản khác.");
                }
            }
            #endregion

            #region 2. Cập nhật thông tin tài khoản người dùng
            var userEntity = await _dataContext.VcUsers
                .FirstOrDefaultAsync(x => x.UserId == currentUserId.Value, cancellationToken);

            if (userEntity == null || !userEntity.IsActive)
            {
                throw new ArgumentException("Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                userEntity.Email = model.Email.Trim();
            }
            userEntity.UpdatedAt = DateTime.Now;
            _dataContext.VcUsers.Update(userEntity);
            #endregion

            #region 3. Cập nhật hồ sơ Cán bộ hoặc Bác sĩ tương ứng
            if (userEntity.Role == Role.STAFF.ToString())
            {
                var staffEntity = await _dataContext.VcStaffs
                    .FirstOrDefaultAsync(x => x.UserId == currentUserId.Value, cancellationToken);

                if (staffEntity != null)
                {
                    staffEntity.FullName = model.FullName.Trim();
                    if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
                    {
                        staffEntity.PhoneNumber = model.PhoneNumber.Trim();
                    }
                    if (!string.IsNullOrWhiteSpace(model.Address))
                    {
                        staffEntity.Address = model.Address.Trim();
                    }
                    if (!string.IsNullOrWhiteSpace(model.AvatarUrl))
                    {
                        staffEntity.AvatarUrl = model.AvatarUrl.Trim();
                    }
                    if (!string.IsNullOrWhiteSpace(model.Gender))
                    {
                        staffEntity.Gender = model.Gender.Trim().ToUpper();
                    }
                    if (model.DateOfBirth.HasValue)
                    {
                        staffEntity.DateOfBirth = model.DateOfBirth.Value;
                    }
                    _dataContext.VcStaffs.Update(staffEntity);
                }
            }
            else if (userEntity.Role == Role.DOCTOR.ToString())
            {
                var doctorEntity = await _dataContext.VcDoctors
                    .FirstOrDefaultAsync(x => x.UserId == currentUserId.Value, cancellationToken);

                if (doctorEntity != null)
                {
                    doctorEntity.FullName = model.FullName.Trim();
                    _dataContext.VcDoctors.Update(doctorEntity);
                }
            }
            #endregion

            await _dataContext.SaveChangesAsync(cancellationToken);

            #region 4. Xóa Cache hồ sơ người dùng
            _cacheService.Remove(UserConstant.BuildCacheKey(currentUserId.Value.ToString()));
            #endregion

            Log.Information($"Cập nhật hồ sơ cá nhân thành công cho UserId: {currentUserId.Value}");

            return Unit.Value;
        }
    }
}