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

public class DeleteStaffCommand : IRequest<Unit>
{
    public int StaffId { get; init; }
    // public DeleteStaffModel Model { get; init; }
    
    public DeleteStaffCommand(int staffId)
    {
        StaffId = staffId;
    }

    public class Handler : IRequestHandler<DeleteStaffCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
        {
            if (_contextAccessor.Role != Role.SUPER_ADMIN.ToString() &&
                _contextAccessor.Role != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa cán bộ.");
            }

            var staffId = request.StaffId;
            Log.Information($"Bắt đầu xóa cán bộ ID: {staffId}");
            
            // Tìm theo StaffId 
            var data = await _dataContext.VcStaffs
                .FirstOrDefaultAsync(x => x.StaffId == staffId, cancellationToken);
            if (data == null)
            {
                throw new ArgumentException("Không tìm thấy hồ sơ cán bộ cần xóa.");
            }

            // Tìm tài khoản 
            var dataUser = await _dataContext.VcUsers
                .FirstOrDefaultAsync(x => x.UserId == data.UserId, cancellationToken);
            // _dataContext.VcStaffs.Remove(data); // xóa toàn bộ staff khổi hệ thống
            if (dataUser != null)
            {
                dataUser.IsActive = false;
                dataUser.UpdatedAt = DateTime.UtcNow;
                // _dataContext.VcUsers.Remove(dataUser);
                _dataContext.VcUsers.Update(dataUser);
            }
            
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            _cacheService.Remove(StaffConstant.BuildCacheKey(staffId.ToString()));
            _cacheService.Remove(StaffConstant.BuildCacheKey());
            
            Log.Information("Deleting success {UserConstant.CachePrefix}: {id}");
            return  Unit.Value;
        }

    }
}