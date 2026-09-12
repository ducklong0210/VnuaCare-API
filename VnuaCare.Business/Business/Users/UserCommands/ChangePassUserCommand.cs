using VnuaCare.Shared.Utils;
/**
 * Nghiệp vụ đổi mật khẩu: Xác thực mật khẩu cũ và băm mật khẩu mới bằng BCrypt
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Business.Users;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;

namespace VnuaCare.Business.Business.Users.UserCommands;

/// <summary>
/// Lệnh đổi mật khẩu của người dùng đang đăng nhập
/// </summary>
public class ChangePassUserCommand : IRequest<Unit>
{
    public UpdatePasswordUserModel Model { get; }

    public ChangePassUserCommand(UpdatePasswordUserModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<ChangePassUserCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để kiểm tra và cập nhật mật khẩu
        private readonly IBcryptPasswordHasher _passwordHasher;  // Dịch vụ xác thực và băm mật khẩu bằng BCrypt
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất UserId từ Token người đang đăng nhập
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache hồ sơ sau khi đổi mật khẩu

        public Handler(
            VnuaCareDataContext dataContext,
            IBcryptPasswordHasher passwordHasher,
            IContextAccessor contextAccessor,
            ICacheService cacheService)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _contextAccessor = contextAccessor;
            _cacheService = cacheService;
        }

        // Xử lý kiểm tra mật khẩu cũ, băm mật khẩu mới và lưu vào CSDL
        public async Task<Unit> Handle(ChangePassUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;

            // Lấy UserId của người dùng hiện tại từ JWT Bearer Token
            var userId = _contextAccessor.UserId;
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc Token không hợp lệ.");
            }

            // Tìm tài khoản người dùng trong CSDL
            var user = await _dataContext.VcUsers
                .FirstOrDefaultAsync(x => x.UserId == userId.Value && x.IsActive, cancellationToken);

            if (user == null)
            {
                throw new ArgumentException("Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword) || !ValidationUtils.IsPassword(model.NewPassword))
            {
                throw new ArgumentException("Mật khẩu mới phải có tối thiểu 6 ký tự, gồm ít nhất 1 chữ cái và 1 chữ số.");
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new ArgumentException("Mật khẩu xác nhận không khớp với mật khẩu mới.");
            }

            // Xác thực mật khẩu cũ bằng BCrypt
            if (!_passwordHasher.VerifyPassword(model.OldPassword, user.Password))
            {
                Log.Warning($"[User] Đổi mật khẩu thất bại: Mật khẩu cũ không chính xác cho UserId={user.UserId}");
                throw new ArgumentException("Mật khẩu cũ không chính xác.");
            }

            // Kiểm tra mật khẩu mới không được trùng mật khẩu cũ
            if (_passwordHasher.VerifyPassword(model.NewPassword, user.Password))
            {
                throw new ArgumentException("Mật khẩu mới không được trùng với mật khẩu hiện tại.");
            }

            // Băm mật khẩu mới bằng thuật toán BCrypt trước khi lưu vào CSDL
            user.Password = _passwordHasher.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Cập nhật CSDL
            _dataContext.VcUsers.Update(user);
            await _dataContext.SaveChangesAsync(cancellationToken);

            // Xóa cache hồ sơ người dùng để lần sau tải lại thông tin mới nhất
            string cacheKey = UserConstant.BuildCacheKey(user.UserId.ToString());
            _cacheService.Remove(cacheKey);

            Log.Information($"[User] Đổi mật khẩu thành công cho UserId={user.UserId} ({user.Username})");

            return Unit.Value;
        }
    }
}
