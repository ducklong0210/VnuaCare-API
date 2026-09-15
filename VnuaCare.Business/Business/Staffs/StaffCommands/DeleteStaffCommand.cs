/**
 * Nghiệp vụ xóa Cán bộ (Xóa mềm bằng cách cập nhật IsActive = false)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Business.Users;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Staffs.StaffCommands;

/// <summary>
/// Lệnh xóa hồ sơ cán bộ và vô hiệu hóa tài khoản đăng nhập
/// </summary>
public class DeleteStaffCommand : IRequest<Unit>
{
    public int StaffId { get; init; }
    
    public DeleteStaffCommand(int staffId)
    {
        StaffId = staffId;
    }

    public class Handler : IRequestHandler<DeleteStaffCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để tìm kiếm và cập nhật trạng thái
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi thực hiện xóa
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(
            VnuaCareDataContext dataContext, 
            ICacheService cacheService, 
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra phân quyền, tìm hồ sơ cán bộ và vô hiệu hóa tài khoản
        public async Task<Unit> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra phân quyền người thực hiện
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa cán bộ.");
            }

            var staffId = request.StaffId;
            Log.Information($"Bắt đầu xóa cán bộ ID: {staffId}");
            
            // Tìm hồ sơ cán bộ theo StaffId trong CSDL
            var data = await _dataContext.VcStaffs
                .FirstOrDefaultAsync(x => x.StaffId == staffId, cancellationToken);
            if (data == null)
            {
                throw new ArgumentException("Không tìm thấy hồ sơ cán bộ cần xóa.");
            }

            // Tìm tài khoản User liên kết tương ứng
            var dataUser = await _dataContext.VcUsers
                .FirstOrDefaultAsync(x => x.UserId == data.UserId, cancellationToken);
            
            // Cập nhật trạng thái tài khoản ngừng hoạt động (Xóa mềm)
            if (dataUser != null)
            {
                dataUser.IsActive = false;
                dataUser.UpdatedAt = DateTime.Now;
                _dataContext.VcUsers.Update(dataUser);
            }
            
            // Lưu thay đổi vào CSDL
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            // Xóa Cache danh sách cán bộ để cập nhật dữ liệu mới nhất
            _cacheService.Remove(StaffConstant.BuildCacheKey(staffId.ToString()));
            _cacheService.Remove(StaffConstant.BuildCacheKey());
            
            Log.Information($"Xóa thành công cán bộ ID: {staffId}");
            return Unit.Value;
        }
    }
}
