using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Authorizations.AuthorizationCommands;

public class UserLogoutCommand : IRequest<Unit>
{
    public LogoutModel Model { get; set; }

    public UserLogoutCommand(LogoutModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UserLogoutCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<Unit> Handle(UserLogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.VcUsers.FirstOrDefaultAsync(u => u.RefreshToken == request.Model.RefreshToken,
                cancellationToken);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                _dataContext.VcUsers.Update(user);
                await _dataContext.SaveChangesAsync(cancellationToken);
            }
            
            _cacheService.Remove(AuthorizationConstant.BuildCacheKey());
            
            return Unit.Value;
        }
    }
}
