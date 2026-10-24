/**
 * Nghiệp vụ Đặt lại mật khẩu tài khoản người dùng bởi Quản trị viên (ResetPasswordUserCommand)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Users.UserCommands;

public class ResetPasswordUserCommand : IRequest<Unit>
{
    public ResetPasswordAdminModel Model { get; }

    public ResetPasswordUserCommand(ResetPasswordAdminModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<ResetPasswordUserCommand, Unit>
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

        public async Task<Unit> Handle(ResetPasswordUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information($"Quản trị viên đặt lại mật khẩu cho tài khoản ID: {model.UserId}");

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng đặt lại mật khẩu (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region 2. Kiểm tra tồn tại
            var entity = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.UserId == model.UserId, cancellationToken);
            if (entity == null)
            {
                throw new ArgumentException($"Không tìm thấy tài khoản người dùng với ID: {model.UserId}");
            }
            #endregion

            #region 3. Kiểm tra dữ liệu hợp lệ
            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
            {
                throw new ArgumentException("Mật khẩu mới phải có độ dài tối thiểu 6 ký tự.");
            }
            #endregion

            #region 4. Cập nhật mật khẩu
            entity.Password = _passwordHasher.HashPassword(model.NewPassword);
            entity.RefreshToken = null; // Bắt buộc đăng nhập lại
            entity.RefreshTokenExpiryTime = null;
            entity.UpdatedAt = DateTime.Now;

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 5. Xóa Cache
            _cacheService.Remove(UserConstant.BuildCacheKey(entity.UserId.ToString()));
            _cacheService.Remove(UserConstant.BuildCacheKey());
            #endregion

            Log.Information($"Đặt lại mật khẩu cho tài khoản {entity.Username} thành công.");
            return Unit.Value;
        }
    }
}
