/**
 * Nghiệp vụ Cập nhật Tài khoản người dùng (UpdateUserCommand)
 * Cho phép Quản trị viên cập nhật vai trò, email và trạng thái hoạt động của tài khoản
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Users.UserCommands;

public class UpdateUserCommand : IRequest<Unit>
{
    public UpdateUserModel Model { get; }

    public UpdateUserCommand(UpdateUserModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateUserCommand, Unit>
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

        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật tài khoản người dùng: " + JsonSerializer.Serialize(model));

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng cập nhật tài khoản (Yêu cầu quyền Quản trị viên).");
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
            if (string.IsNullOrWhiteSpace(model.Email) || !ValidationUtils.IsEmail(model.Email.Trim()))
            {
                throw new ArgumentException("Email không đúng định dạng.");
            }

            var upperRole = model.Role.Trim().ToUpper();
            if (!Enum.TryParse<Role>(upperRole, out _))
            {
                throw new ArgumentException($"Vai trò '{model.Role}' không hợp lệ.");
            }

            // Kiểm tra trùng lặp email với các tài khoản khác
            var isEmailExisted = await _dataContext.VcUsers
                .AnyAsync(x => x.Email == model.Email.Trim() && x.UserId != model.UserId, cancellationToken);
            if (isEmailExisted)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác.");
            }
            #endregion

            #region 4. Cập nhật và lưu dữ liệu
            model.UpdateEntity(entity);
            _dataContext.VcUsers.Update(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 5. Xóa Cache
            _cacheService.Remove(UserConstant.BuildCacheKey(entity.UserId.ToString()));
            _cacheService.Remove(UserConstant.BuildCacheKey());
            _cacheService.Remove(VnuaCareCacheConstant.LIST_SELECT);
            #endregion

            Log.Information($"Cập nhật tài khoản {entity.Username} thành công.");
            return Unit.Value;
        }
    }
}
