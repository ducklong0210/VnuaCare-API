/**
 * Nghiệp vụ Lấy chỉ số sinh tồn và thể lực theo ID hồ sơ (GetVitalSignsByRecordIdQuery)
 * Sử dụng DbContext chỉ đọc VnuaCareReadDataContext kết hợp AsNoTracking() tối ưu hiệu năng
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.VitalSigns.VitalSignsQueries;

public class GetVitalSignsByRecordIdQuery : IRequest<VitalSignsModel?>
{
    public int RecordId { get; set; }

    public GetVitalSignsByRecordIdQuery(int recordId)
    {
        RecordId = recordId;
    }

    public class Handler : IRequestHandler<GetVitalSignsByRecordIdQuery, VitalSignsModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;

        public Handler(VnuaCareReadDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<VitalSignsModel?> Handle(GetVitalSignsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            var vital = await _dataContext.VcVitalSigns.AsNoTracking()
                .Include(x => x.MeasuredByDoctor)
                .Where(x => x.RecordId == request.RecordId)
                .FirstOrDefaultAsync(cancellationToken);

            if (vital == null) return null;

            // Đánh giá thể trạng BMI theo chuẩn WHO cho người châu Á (IDI & WPRO)
            string? bmiClass = null;
            if (vital.Bmi.HasValue)
            {
                var val = vital.Bmi.Value;
                if (val < 18.5m) bmiClass = "Gầy / Thiếu cân";
                else if (val < 23.0m) bmiClass = "Bình thường (Lý tưởng)";
                else if (val < 25.0m) bmiClass = "Thừa cân / Tiền béo phì";
                else if (val < 30.0m) bmiClass = "Béo phì độ I";
                else bmiClass = "Béo phì độ II (Nặng)";
            }

            // Đánh giá phân loại huyết áp theo Hội Tim mạch học Việt Nam / ISH
            string? bpClass = null;
            if (vital.SystolicBp.HasValue && vital.DiastolicBp.HasValue)
            {
                int sys = vital.SystolicBp.Value;
                int dia = vital.DiastolicBp.Value;
                if (sys < 120 && dia < 80) bpClass = "Huyết áp tối ưu / Bình thường";
                else if (sys <= 129 && dia < 80) bpClass = "Huyết áp bình thường cao";
                else if ((sys >= 130 && sys <= 139) || (dia >= 80 && dia <= 89)) bpClass = "Tăng huyết áp độ 1 (Nhẹ)";
                else if ((sys >= 140 && sys <= 179) || (dia >= 90 && dia <= 109)) bpClass = "Tăng huyết áp độ 2 (Trung bình)";
                else if (sys >= 180 || dia >= 110) bpClass = "Tăng huyết áp độ 3 (Khẩn cấp y tế)";
            }

            return new VitalSignsModel
            {
                VitalSignId = vital.VitalSignId,
                RecordId = vital.RecordId,
                HeightCm = vital.HeightCm,
                WeightKg = vital.WeightKg,
                Bmi = vital.Bmi,
                BmiClassification = bmiClass,
                WaistCm = vital.WaistCm,
                SystolicBp = vital.SystolicBp,
                DiastolicBp = vital.DiastolicBp,
                BloodPressureClassification = bpClass,
                HeartRateBpm = vital.HeartRateBpm,
                RespiratoryRate = vital.RespiratoryRate,
                Spo2Percent = vital.Spo2Percent,
                MeasuredBy = vital.MeasuredBy,
                MeasuredByName = vital.MeasuredByDoctor != null ? vital.MeasuredByDoctor.FullName : null,
                CreatedAt = vital.CreatedAt
            };
        }
    }
}
