using MediatR;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.CheckupRecordCommands;

public class CreateHealthCheckupRecordCommand : IRequest<Unit>
{
    public CreateHealthCheckupRecordModel Model {get;  set;}
    
    public CreateHealthCheckupRecordCommand(CreateHealthCheckupRecordModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateHealthCheckupRecordCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(CreateHealthCheckupRecordCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information(" them baos cao ksk");
            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException(" Khong du quyen");
            }
            

        }
        
        
    }
    
}