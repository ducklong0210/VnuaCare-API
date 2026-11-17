/**
 * Nghiệp vụ Cập nhật kết quả khám chuyên khoa lẻ (UpdateSpecialistExamCommand)
 * Chuẩn CRUD: Cập nhật thông tin bản ghi kết quả khám chuyên khoa
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.SpecialistExams.SpecialistExamCommands;

public class UpdateSpecialistExamCommand : IRequest<Unit>
{
    public UpdateSpecialistExamModel Model { get; }

    public UpdateSpecialistExamCommand(UpdateSpecialistExamModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateSpecialistExamCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để cập nhật kết quả khám chuyên khoa
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi cập nhật
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và cập nhật kết quả khám chuyên khoa lẻ
        public async Task<Unit> Handle(UpdateSpecialistExamCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật kết quả khám chuyên khoa lâm sàng: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ chuyên khoa hoặc Quản trị viên y tế mới có quyền cập nhật kết quả khám chuyên khoa.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var exam = await _dataContext.VcSpecialistExams
                .FirstOrDefaultAsync(x => x.SpecialistExamId == model.SpecialistExamId, cancellationToken);
            if (exam == null)
            {
                throw new ArgumentException($"Không tìm thấy kết quả khám chuyên khoa với ID: {model.SpecialistExamId}");
            }
            #endregion

            #region Cập nhật CSDL
            model.UpdateEntity(exam);
            _dataContext.VcSpecialistExams.Update(exam);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(SpecialistExamConstant.BuildCacheKey(exam.RecordId.ToString()));
            #endregion

            Log.Information($"Cập nhật kết quả khám chuyên khoa ID {model.SpecialistExamId} thành công.");
            return Unit.Value;
        }
    }
}
