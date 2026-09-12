using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Doctors.DoctorCommands;

public class UpdateDoctorCommand : IRequest<Unit>
{
    public UpdateDoctorModel Model { get; set; }
    
    public UpdateDoctorCommand(UpdateDoctorModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateDoctorCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService,
            Func<IContextAccessor> contextAccessor, IBcryptPasswordHasher cryptPasswordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("cupdating doctor");
            
            // search doctor in db
            var entity = await _dataContext.VcDoctors.FirstOrDefaultAsync(x => x.DoctorId == model.DoctorId);
            if (entity == null)
            {
                throw new ArgumentException(" Doctor not found");
            }

            #region validation

            var currentRole = _contextAccessor.Role;
            if (currentRole != Role.SUPER_ADMIN.ToString() &&
                currentRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new ArgumentException(" Doctor not role");
            }
            if (!ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }
            
            // kiểm tra trùng email
            var isEmailExisted = await _dataContext.VcUsers
                .AnyAsync(x => x.Email == model.Email.Trim() && x.UserId != entity.UserId, cancellationToken);

            if (isEmailExisted)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác.");
            }
            #endregion
            
            var userEntity = await _dataContext.VcUsers.FirstOrDefaultAsync(x => x.UserId == entity.UserId);
            if (userEntity != null && !string.IsNullOrEmpty(userEntity.Email))
            {
                userEntity.Email = model.Email.Trim();
                userEntity.UpdatedAt =  DateTime.Now;
                _dataContext.VcUsers.Update(userEntity);
            }
            entity.DoctorId = model.DoctorId;
            model.UpdateEntity(entity);
            
            _dataContext.VcDoctors.Update(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            _cacheService.Remove(DoctorConstant.BuildCacheKey(entity.DoctorId.ToString()));
            _cacheService.Remove(DoctorConstant.BuildCacheKey());
            
            return Unit.Value;
        }
    }
    
}