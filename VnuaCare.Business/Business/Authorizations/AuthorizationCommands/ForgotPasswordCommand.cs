/**
 * Nghiệp vụ Quên mật khẩu và Đặt lại mật khẩu qua Email (ForgotPasswordCommand & ResetPasswordWithTokenCommand)
 */

using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Authorizations.AuthorizationCommands;

/// <summary>
/// Command yêu cầu cấp mã xác thực đặt lại mật khẩu gửi qua email
/// </summary>
public class ForgotPasswordCommand : IRequest<string>
{
    public ForgotPasswordModel Model { get; }

    public ForgotPasswordCommand(ForgotPasswordModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<ForgotPasswordCommand, string>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.Model.Email.Trim().ToLower();
            Log.Information($"Yêu cầu quên mật khẩu cho email: {email}");

            if (!ValidationUtils.IsEmail(email))
            {
                throw new ArgumentException("Email không đúng định dạng.");
            }

            var user = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == email && x.IsActive, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy tài khoản nào liên kết với địa chỉ email này hoặc tài khoản đã bị khóa.");
            }

            // Tạo mã OTP 6 số bảo mật
            string otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // Lưu mã OTP vào Cache thời hạn 15 phút
            string cacheKey = $"pwd-reset-{email}";
            await _cacheService.Set(cacheKey, otp, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            Log.Information($"Mã OTP đặt lại mật khẩu cho {email}: {otp}");

            return $"Mã xác thực đặt lại mật khẩu đã được tạo thành công cho email {email}. Mã có hiệu lực trong vòng 15 phút.";
        }
    }
}

/// <summary>
/// Command xác nhận mã OTP và cập nhật mật khẩu mới
/// </summary>
public class ResetPasswordWithTokenCommand : IRequest<Unit>
{
    public ResetPasswordRequestModel Model { get; }

    public ResetPasswordWithTokenCommand(ResetPasswordRequestModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<ResetPasswordWithTokenCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IBcryptPasswordHasher _passwordHasher;

        public Handler(
            VnuaCareDataContext dataContext,
            ICacheService cacheService,
            IBcryptPasswordHasher passwordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Unit> Handle(ResetPasswordWithTokenCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            var email = model.Email.Trim().ToLower();

            Log.Information($"Xác nhận đặt lại mật khẩu cho email: {email}");

            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new ArgumentException("Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            if (model.NewPassword.Length < 6)
            {
                throw new ArgumentException("Mật khẩu mới phải có tối thiểu 6 ký tự.");
            }

            // Kiểm tra mã OTP trong cache
            string cacheKey = $"pwd-reset-{email}";
            var cachedOtp = await _cacheService.GetOrCreate<string?>(cacheKey, () => Task.FromResult<string?>(null));

            if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != model.TokenOrOtp.Trim())
            {
                throw new ArgumentException("Mã xác thực không hợp lệ hoặc đã hết hạn (Mã có hiệu lực trong 15 phút).");
            }

            var user = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == email && x.IsActive, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException("Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            // Đổi mật khẩu
            user.Password = _passwordHasher.HashPassword(model.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.Now;

            await _dataContext.SaveChangesAsync(cancellationToken);

            // Xóa mã OTP khỏi cache
            _cacheService.Remove(cacheKey);

            Log.Information($"Đặt lại mật khẩu thành công cho tài khoản {user.Username}.");
            return Unit.Value;
        }
    }
}
