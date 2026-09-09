using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Serilog;
using VnuaCare.Business.Business.Authorizations;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Authorizations.AuthorizationCommands;

public class UserLoginCommand : IRequest<LoginResponseModel>
{
    public LoginModel LoginModel { get; }

    public UserLoginCommand(LoginModel loginModel)
    {
        LoginModel = loginModel;
    }

    public class Handler : IRequestHandler<UserLoginCommand, LoginResponseModel>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly IBcryptPasswordHasher _passwordHasher;
        private readonly IStringLocalizer<UserLoginCommand> _localizer;
        private readonly IJwtService _jwtService;
        private ICacheService _cacheService;
        public Handler(
            VnuaCareDataContext dataContext,
            IBcryptPasswordHasher passwordHasher,
            IStringLocalizer<UserLoginCommand> localizer,
            IJwtService jwtService,
            ICacheService cacheService)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _localizer = localizer;
            _jwtService = jwtService;
            _cacheService = cacheService;
        }

        public async Task<LoginResponseModel> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            var model = request.LoginModel;
            var inputIdentifier = model.LoginIdentifier?.Trim() ?? string.Empty;

            Log.Information($"[Auth] User Login Attempt: {inputIdentifier}");

            var user = await _dataContext.VcUsers
                .FirstOrDefaultAsync(x => 
                    x.IsActive && 
                    (x.Username == inputIdentifier || x.Email == inputIdentifier), 
                    cancellationToken);

            if (user == null || !_passwordHasher.VerifyPassword(model.Password, user.Password))
            {
                Log.Warning($"[Auth] Login failed for user: {inputIdentifier}");
                throw new ArgumentException("Tài khoản hoặc mật khẩu không chính xác.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? "STAFF"),
            };

            var accessToken = _jwtService.GenerateAccessToken(claims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _dataContext.VcUsers.Update(user);
            await _dataContext.SaveChangesAsync(cancellationToken);

            Log.Information($"[Auth] User {user.Username} logged in successfully.");
            _cacheService.Remove(AuthorizationConstant.BuildCacheKey());
            return new LoginResponseModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Fullname = user.Username,
                Email = user.Email,
                Role = user.Role,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime.Value
            };
        }
    }
}
