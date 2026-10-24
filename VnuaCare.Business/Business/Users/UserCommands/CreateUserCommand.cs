/**
 * Nghiệp vụ Thêm mới Tài khoản người dùng (CreateUserCommand)
 * Dành cho Quản trị viên (SUPER_ADMIN, HEALTH_ADMIN) cấp tài khoản trên hệ thống V-Care Health
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Users.UserCommands;

public class CreateUserCommand : IRequest<Unit>
{
    public CreateUserModel Model { get; }

    public CreateUserCommand(CreateUserModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateUserCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBcryptPasswordHasher _passwordHasher;

        public Handler(
            VnuaCareDataContext dataContext,
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor,
            IBcryptPasswordHasher passwordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
            _passwordHasher = passwordHasher;
        }

        public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu tạo tài khoản người dùng: " + JsonSerializer.Serialize(model));

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng tạo tài khoản (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region 2. Kiểm tra dữ liệu hợp lệ
            if (string.IsNullOrWhiteSpace(model.Username) || model.Username.Trim().Length < 3)
            {
                throw new ArgumentException("Tên đăng nhập phải có ít nhất 3 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(model.Email) || !ValidationUtils.IsEmail(model.Email.Trim()))
            {
                throw new ArgumentException("Email không đúng định dạng.");
            }

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
            {
                throw new ArgumentException("Mật khẩu phải có độ dài tối thiểu 6 ký tự.");
            }

            var upperRole = model.Role.Trim().ToUpper();
            if (!Enum.TryParse<Role>(upperRole, out _))
            {
                throw new ArgumentException($"Vai trò '{model.Role}' không hợp lệ trong hệ thống.");
            }
            #endregion

            #region 3. Kiểm tra trùng lặp
            var isUsernameExists = await _dataContext.VcUsers.AnyAsync(x => x.Username == model.Username.Trim(), cancellationToken);
            if (isUsernameExists)
            {
                throw new ArgumentException($"Tên đăng nhập '{model.Username}' đã tồn tại trong hệ thống.");
            }

            var isEmailExists = await _dataContext.VcUsers.AnyAsync(x => x.Email == model.Email.Trim(), cancellationToken);
            if (isEmailExists)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác.");
            }
            #endregion

            #region 4. Tạo entity và lưu CSDL
            var user = new VcUsers
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Role = upperRole,
                Password = _passwordHasher.HashPassword(model.Password),
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _dataContext.VcUsers.AddAsync(user, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 5. Xóa Cache
            _cacheService.Remove(UserConstant.BuildCacheKey());
            _cacheService.Remove(VnuaCareCacheConstant.LIST_SELECT);
            #endregion

            Log.Information($"Tạo tài khoản {user.Username} thành công với ID: {user.UserId}");
            return Unit.Value;
        }
    }
}
