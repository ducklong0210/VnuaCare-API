/**
 * Nghiệp vụ Cập nhật kết quả khám lâm sàng Nội khoa tổng quát (UpdateInternalMedicineExamCommand)
 * Chuẩn CRUD: Cập nhật thông tin bản ghi kết quả khám nội khoa
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.InternalMedicine.InternalMedicineCommands;

public class UpdateInternalMedicineExamCommand : IRequest<Unit>
{
    public UpdateInternalMedicineExamModel Model { get; }

    public UpdateInternalMedicineExamCommand(UpdateInternalMedicineExamModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateInternalMedicineExamCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để cập nhật kết quả khám nội khoa
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi cập nhật
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và cập nhật kết quả khám lâm sàng nội khoa
        public async Task<Unit> Handle(UpdateInternalMedicineExamCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật kết quả khám lâm sàng nội khoa: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền cập nhật kết quả khám nội khoa.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var exam = await _dataContext.VcInternalMedicineExams
                .FirstOrDefaultAsync(x => x.InternalExamId == model.InternalExamId, cancellationToken);
            if (exam == null)
            {
                throw new ArgumentException($"Không tìm thấy kết quả khám nội khoa với ID: {model.InternalExamId}");
            }
            #endregion

            #region Cập nhật CSDL
            model.UpdateEntity(exam);
            _dataContext.VcInternalMedicineExams.Update(exam);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(InternalMedicineConstant.BuildCacheKey(exam.RecordId.ToString()));
            #endregion

            Log.Information($"Cập nhật kết quả khám nội khoa ID {model.InternalExamId} thành công.");
            return Unit.Value;
        }
    }
}
