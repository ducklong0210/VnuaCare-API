using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Authorizations.AuthorizationCommands;

public class RefreshTokenCommand : IRequest<LoginResponseModel>
{
    public RefreshTokenModel Model { get; set; }

    public RefreshTokenCommand(RefreshTokenModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<RefreshTokenCommand, LoginResponseModel>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IJwtService _jwtService;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, IJwtService jwtService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseModel> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.Model.AccessToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            var user = await _dataContext.VcUsers.FirstOrDefaultAsync(u => u.UserId == userId,cancellationToken);

            if (user == null || user.RefreshToken != request.Model.RefreshToken)
            {
                throw new UnauthorizedAccessException(" Invalid refresh token");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            _dataContext.VcUsers.Update(user);
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            _cacheService.Remove(AuthorizationConstant.BuildCacheKey());
            
            return new LoginResponseModel
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role,
                AccessToken = newAccessToken, 
                RefreshToken = newRefreshToken
            };
        }
    }
}
