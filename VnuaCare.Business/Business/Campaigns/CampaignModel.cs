/**
 * Định nghĩa cấu trúc dữ liệu và DTO cho Đợt khám sức khỏe (CampaignModel, CreateCampaignModel, CampaignSelectModel)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.Campaigns;

public abstract record CampaignBaseModel
{
    public int CampaignId { get; init; }

    [Required(ErrorMessage = "Tên đợt khám không được để trống")]
    public string CampaignName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mã đợt khám không được để trống")]
    public string CampaignCode { get; init; } = string.Empty;

    public int Year { get; init; } = DateTime.Now.Year;

    public DateTime StartDate { get; init; } = DateTime.Today;

    public DateTime EndDate { get; init; } = DateTime.Today.AddDays(7);

    public string HospitalPartner { get; init; } = "Bệnh viện Đa khoa MEDLATEC";

    public string Status { get; init; } = "PLANNING";
}

/// <summary>
/// Dùng để hiển thị chi tiết thông tin Đợt khám sức khỏe
/// </summary>
public record CampaignModel : CampaignBaseModel
{
}

public record CreateCampaignModel : CampaignBaseModel
{
}

public record UpdateCampaignModel : CampaignBaseModel
{
    public void UpdateEntity(VcCheckupCampaigns entity)
    {
        entity.CampaignName = CampaignName.Trim();
        entity.CampaignCode = CampaignCode.Trim();
        entity.Year = Year > 0 ? Year : DateTime.Now.Year;
        entity.StartDate = StartDate;
        entity.EndDate = EndDate;
        entity.HospitalPartner = HospitalPartner.Trim();
        entity.Status = Status.Trim();
    }
}

public record DeleteCampaignModel : CampaignBaseModel
{
    public DeleteCampaignModel(int id)
    {
        CampaignId = id;
    }
}

/// <summary>
/// DTO danh mục Đợt khám sức khỏe phục vụ hiển thị combobox / dropdown
/// </summary>
public record CampaignSelectModel
{
    public int CampaignId { get; init; }
    public string CampaignName { get; init; } = string.Empty;
    public string CampaignCode { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Status { get; init; } = string.Empty;
}
