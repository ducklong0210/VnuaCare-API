/**
 * Nghiệp vụ Vô hiệu hóa / Khóa tài khoản người dùng (DeleteUserCommand)
 * Dành cho Quản trị viên để khóa truy cập của tài khoản người dùng
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Users.UserCommands;

public class DeleteUserCommand : IRequest<Unit>
{
    public int UserId { get; }

    public DeleteUserCommand(int userId)
    {
        UserId = userId;
    }

    public class Handler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(
            VnuaCareDataContext dataContext,
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            Log.Information($"Bắt đầu vô hiệu hóa tài khoản người dùng ID: {request.UserId}");

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng khóa tài khoản (Yêu cầu quyền Quản trị viên).");
            }

            // Không cho phép tự khóa tài khoản của chính mình
            if (_contextAccessor.UserId == request.UserId)
            {
                throw new ArgumentException("Bạn không thể tự khóa tài khoản của chính mình.");
            }
            #endregion

            #region 2. Kiểm tra tồn tại
            var entity = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            if (entity == null)
            {
                throw new ArgumentException($"Không tìm thấy tài khoản người dùng với ID: {request.UserId}");
            }
            #endregion

            #region 3. Khóa tài khoản (Soft Delete)
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.Now;

            // Xóa RefreshToken để vô hiệu hóa phiên làm việc hiện tại
            entity.RefreshToken = null;
            entity.RefreshTokenExpiryTime = null;

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 4. Xóa Cache
            _cacheService.Remove(UserConstant.BuildCacheKey(request.UserId.ToString()));
            _cacheService.Remove(UserConstant.BuildCacheKey());
            _cacheService.Remove(VnuaCareCacheConstant.LIST_SELECT);
            #endregion

            Log.Information($"Khóa tài khoản {entity.Username} thành công.");
            return Unit.Value;
        }
    }
}
