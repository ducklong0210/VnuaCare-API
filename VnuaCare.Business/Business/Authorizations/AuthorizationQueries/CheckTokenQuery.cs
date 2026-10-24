/**
 * Nghiệp vụ Kiểm tra tính hợp lệ và thời hạn của Token (CheckTokenQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;

namespace VnuaCare.Business.Business.Authorizations.AuthorizationQueries;

/// <summary>
/// Query kiểm tra trạng thái và nạp thông tin người dùng từ JWT Token hiện tại
/// </summary>
public class CheckTokenQuery : IRequest<CheckTokenResponseModel?>
{
    public class Handler : IRequestHandler<CheckTokenQuery, CheckTokenResponseModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor();
        }

        public async Task<CheckTokenResponseModel?> Handle(CheckTokenQuery request, CancellationToken cancellationToken)
        {
            var userId = _contextAccessor.UserId;
            if (!userId.HasValue)
            {
                return null;
            }

            var user = await _dataContext.VcUsers.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId.Value && u.IsActive, cancellationToken);

            if (user == null)
            {
                return null;
            }

            string? fullName = null;
            string? staffCode = null;
            string? doctorCode = null;
            string? avatarUrl = null;

            // Nạp thông tin cán bộ nếu có
            var staff = await _dataContext.VcStaffs.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == user.UserId, cancellationToken);
            if (staff != null)
            {
                fullName = staff.FullName;
                staffCode = staff.EmployeeCode;
                avatarUrl = staff.AvatarUrl;
            }
            else
            {
                // Nạp thông tin bác sĩ nếu có
                var doctor = await _dataContext.VcDoctors.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.UserId == user.UserId, cancellationToken);
                if (doctor != null)
                {
                    fullName = doctor.FullName;
                    doctorCode = doctor.DoctorCode;
                }
            }

            return new CheckTokenResponseModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Fullname = fullName,
                StaffCode = staffCode,
                DoctorCode = doctorCode,
                AvatarUrl = avatarUrl
            };
        }
    }
}
