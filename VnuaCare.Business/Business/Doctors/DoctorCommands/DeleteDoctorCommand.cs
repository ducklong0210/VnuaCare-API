/**
 * Nghiệp vụ xóa Bác sĩ (Xóa mềm bằng cách cập nhật IsActive = false cho tài khoản User)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Doctors.DoctorCommands;

/// <summary>
/// Lệnh xóa Bác sĩ và vô hiệu hóa tài khoản liên kết
/// </summary>
public class DeleteDoctorCommand : IRequest<Unit>
{
    public int DoctorId { get; set; }
    
    public DeleteDoctorCommand(int doctorId)
    {
        DoctorId = doctorId;
    }

    public class Handler : IRequestHandler<DeleteDoctorCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để tìm kiếm và cập nhật dữ liệu
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi xóa
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

        // Xử lý kiểm tra phân quyền, tìm hồ sơ bác sĩ và vô hiệu hóa tài khoản đăng nhập
        public async Task<Unit> Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra phân quyền người thực hiện
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa cán bộ.");
            }

            var doctorId = request.DoctorId;
            Log.Information($"Bắt đầu xóa bác sĩ ID: {doctorId}");

            // Tìm hồ sơ bác sĩ theo DoctorId trong CSDL
            var data = await _dataContext.VcDoctors
                .FirstOrDefaultAsync(x => x.DoctorId == doctorId, cancellationToken);

            if (data == null)
            {
                throw new ArgumentException("doctor not found");
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
            await _dataContext.SaveChangesAsync(cancellationToken);
             
            // Xóa Cache thông tin bác sĩ để cập nhật dữ liệu mới nhất
            _cacheService.Remove(DoctorConstant.BuildCacheKey(doctorId.ToString()));
            _cacheService.Remove(DoctorConstant.BuildCacheKey());
             
            Log.Information("Xóa bác sĩ thành công ID: " + doctorId);
            return Unit.Value;
        }
    }
}
