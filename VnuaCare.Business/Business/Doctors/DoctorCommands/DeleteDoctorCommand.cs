using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Doctors.DoctorCommands;

public class DeleteDoctorCommand : IRequest<Unit>
{
    
    public int DoctorId { get; set; }
    
    public DeleteDoctorCommand(int doctorId)
    {
        DoctorId = doctorId;
    }

    public class Handler : IRequestHandler<DeleteDoctorCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService,Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
        {
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa cán bộ.");
            }

            var doctorId = request.DoctorId;
            Log.Information($"Deleting doctor {doctorId}");

            var data = _dataContext.VcDoctors
                    .FirstOrDefaultAsync(x => x.DoctorId == doctorId ,cancellationToken);

            if (data == null)
            {
                throw new ArgumentException("doctor not found");
            }
            var dataUser = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.UserId == data.User)
        }
        
    }
}