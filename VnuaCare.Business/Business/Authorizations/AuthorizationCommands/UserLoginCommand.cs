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
        private readonly ICacheService _cacheService;

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

            Log.Information($"User Login Attempt: {model.LoginIdentifier}");

            // Tìm user theo username hoặc email
            var user = await _dataContext.VcUsers
                .FirstOrDefaultAsync(u => u.IsActive && (u.Username == model.LoginIdentifier || u.Email == model.LoginIdentifier), cancellationToken);

            if (user == null || !_passwordHasher.VerifyPassword(model.Password, user.Password))
            {
                Log.Warning($"Login failed for user: {model.LoginIdentifier}");
                throw new ArgumentException(_localizer["user.login.failed"]);
            }

            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Sinh token
            var accessToken = _jwtService.GenerateAccessToken(claims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Lưu refresh token vào DB
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // Cấu hình sau
            _dataContext.VcUsers.Update(user);
            await _dataContext.SaveChangesAsync(cancellationToken);

            Log.Information($"User {user.Username} logged in successfully.");

            _cacheService.Remove(AuthorizationConstant.BuildCacheKey());

            string? fullname = null;
            string? staffcode = null;
            string? doctorcode = null;

            if (user.Role == "STAFF")
            {
                var staff = await _dataContext.VcStaffs.FirstOrDefaultAsync(s => s.UserId == user.UserId, cancellationToken);
                if (staff != null)
                {
                    fullname = staff.FullName;
                    staffcode = staff.EmployeeCode;
                }
            }else if (user.Role == "DOCTOR")
            {
                var doctor = await _dataContext.VcDoctors.FirstOrDefaultAsync(d => d.UserId == user.UserId, cancellationToken);
                if (doctor != null)
                {
                    fullname = doctor.FullName;
                    doctorcode = doctor.DoctorCode;
                }
            }
            return new LoginResponseModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Fullname = fullname,
                Email = user.Email,
                Role = user.Role,
                StaffCode = staffcode,
                DoctorCode = doctorcode,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime ?? DateTime.Now.AddDays(7)
            };
        }
    }
}
