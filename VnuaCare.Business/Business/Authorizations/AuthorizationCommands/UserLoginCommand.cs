using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Business.Business.Authorizations;
namespace VnuaCare.Business.Business.Authorizations.AuthorizationCommands;

public class UserLoginCommand : IRequest<LoginResponseModel>
{
    public LoginModel LoginModel { get; }


    // Đăng nhập

    public UserLoginCommand(LoginModel loginModel)
    {
        this.LoginModel = loginModel;
    }

    public class Handler : IRequestHandler<UserLoginCommand, LoginResponseModel>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly IBcryptPasswordHasher _passwordHasher;
        private readonly IStringLocalizer<UserLoginCommand> _localizer;
        private readonly IJwtService _jwtService;
        private readonly ICacheService _cacheService;
        // private readonly ICacheS

        public Handler(VnuaCareDataContext dataContext, IBcryptPasswordHasher passwordHasher,
            IStringLocalizer<UserLoginCommand> localizer, IJwtService jwtService, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _localizer = localizer;
            _jwtService = jwtService;
            _cacheService = cacheService;
        }

        public async Task<LoginResponseModel> Handle(UserLoginCommand request,
            CancellationToken cancellationToken)
        {
            var model = request.LoginModel;
            Log.Information($"User Login Attempt: {model.LoginIdentifier}");

            // tìm user
            var user = await _dataContext.VcUsers.FirstOrDefaultAsync(
                x => x.IsActive && (x.Username == model.LoginIdentifier || x.Email == model.LoginIdentifier),
                cancellationToken);
            if (user == null || !_passwordHasher.VerifyPassword(model.Password, user.Password))
            {
                Log.Warning($"Login failed for user: {model.LoginIdentifier}");
                throw new ArgumentException($"{_localizer["user.login.failed"]}");
            }
// Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };
            
            // Sinh token
            var newAccessToken = _jwtService.GenerateAccessToken(claims);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newExpiresAt = DateTime.UtcNow.AddDays(7);
            
            //lưu refresh vào db
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = newExpiresAt;
            _dataContext.VcUsers.Update(user);
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            // _cacheService.Remove(Auth)
            
            
            return new LoginResponseModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Fullname = user.Staff?.FullName ?? user.Doctor?.FullName ?? user.Username,
                Email = user.Email,
                Role = user.Role,
                StaffCode = user.Staff?.StaffCode,
                DoctorCode = user.Doctor?.DoctorCode,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = newExpiresAt
            };
        }
    }
}