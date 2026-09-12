/**
 * Nghiệp vụ làm mới Token: Kiểm tra Refresh Token và cấp cặp Token mới
 */

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
        private readonly VnuaCareDataContext _dataContext; // Kết nối CSDL để kiểm tra và lưu Refresh Token
        private readonly ICacheService _cacheService;     // Dịch vụ quản lý bộ nhớ đệm Cache
        private readonly IJwtService _jwtService;         // Dịch vụ giải mã và sinh JWT Access Token

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, IJwtService jwtService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _jwtService = jwtService;
        }

        // Xử lý kiểm tra Refresh Token và cấp cặp Token mới
        public async Task<LoginResponseModel> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Trích xuất thông tin người dùng từ Access Token đã hết hạn
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.Model.AccessToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            // Tìm tài khoản trong CSDL
            var user = await _dataContext.VcUsers.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            // Kiểm tra tính hợp lệ của Refresh Token
            if (user == null || user.RefreshToken != request.Model.RefreshToken)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn hoặc mã làm mới không hợp lệ.");
            }

            // Sinh Access Token và Refresh Token mới
            var newAccessToken = _jwtService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            
            // Cập nhật Refresh Token mới vào CSDL với thời hạn 7 ngày
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            _dataContext.VcUsers.Update(user);
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            // Xóa cache cũ
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
