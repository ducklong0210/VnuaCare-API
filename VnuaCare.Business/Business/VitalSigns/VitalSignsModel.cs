/**
 * Định nghĩa DTO của Phân hệ Khám Thể lực & Chỉ số Sinh tồn (Vital Signs)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.VitalSigns;

/// <summary>
/// Lớp cơ sở chứa các chỉ số sinh tồn và thể lực
/// </summary>
public abstract record VitalSignsBaseModel
{
    public decimal? HeightCm { get; init; }       // Chiều cao cơ thể (cm)
    public decimal? WeightKg { get; init; }       // Cân nặng cơ thể (kg)
    public decimal? Bmi { get; init; }            // Chỉ số khối cơ thể BMI
    public decimal? WaistCm { get; init; }        // Vòng bụng / vòng eo (cm)
    public int? SystolicBp { get; init; }         // Huyết áp tâm thu (mmHg)
    public int? DiastolicBp { get; init; }        // Huyết áp tâm trương (mmHg)
    public int? HeartRateBpm { get; init; }       // Nhịp tim / mạch (lần/phút)
    public int? RespiratoryRate { get; init; }    // Nhịp thở (lần/phút)
    public decimal? Spo2Percent { get; init; }    // Nồng độ oxy trong máu ngoại vi SpO2 (%)
    public int? MeasuredBy { get; init; }         // Bác sĩ / Điều dưỡng thực hiện đo
}

/// <summary>
/// DTO thêm mới chỉ số sinh tồn và thể lực
/// </summary>
public record CreateVitalSignsModel : VitalSignsBaseModel
{
    [Required(ErrorMessage = "ID hồ sơ khám sức khỏe không được để trống")]
    public int RecordId { get; init; }
}

/// <summary>
/// DTO cập nhật chỉ số sinh tồn và thể lực
/// </summary>
public record UpdateVitalSignsModel : VitalSignsBaseModel
{
    [Required(ErrorMessage = "ID bản ghi chỉ số sinh tồn không được để trống")]
    public int VitalSignId { get; init; }

    public void UpdateEntity(VcVitalSigns entity, decimal? calculatedBmi)
    {
        entity.HeightCm = HeightCm;
        entity.WeightKg = WeightKg;
        entity.Bmi = calculatedBmi;
        entity.WaistCm = WaistCm;
        entity.SystolicBp = SystolicBp;
        entity.DiastolicBp = DiastolicBp;
        entity.HeartRateBpm = HeartRateBpm;
        entity.RespiratoryRate = RespiratoryRate;
        entity.Spo2Percent = Spo2Percent;
        if (MeasuredBy.HasValue && MeasuredBy.Value > 0)
        {
            entity.MeasuredBy = MeasuredBy.Value;
        }
    }
}

/// <summary>
/// DTO trả về chi tiết chỉ số sinh tồn kèm thông tin người đo
/// </summary>
public record VitalSignsModel : VitalSignsBaseModel
{
    public int VitalSignId { get; init; }
    public int RecordId { get; init; }
    public string? MeasuredByName { get; init; }
    public DateTime CreatedAt { get; init; }
}
