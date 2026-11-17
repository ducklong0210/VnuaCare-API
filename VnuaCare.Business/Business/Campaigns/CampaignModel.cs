/**
 * Định nghĩa các Model / DTO của Phân hệ Đợt khám sức khỏe (Campaigns)
 */

using System.ComponentModel.DataAnnotations;

namespace VnuaCare.Business.Business.Campaigns;

/// <summary>
/// DTO thêm mới Đợt khám sức khỏe định kỳ
/// </summary>
public record CreateCampaignModel
{
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
/// DTO cập nhật thông tin Đợt khám sức khỏe
/// </summary>
public record UpdateCampaignModel : CreateCampaignModel
{
    [Required(ErrorMessage = "ID đợt khám không được để trống")]
    public int CampaignId { get; init; }
}

/// <summary>
/// DTO danh mục Đợt khám sức khỏe phục vụ hiển thị combobox / dropdown chọn trên giao diện
/// </summary>
public record CampaignSelectModel
{
    public int CampaignId { get; init; }
    public string CampaignName { get; init; } = string.Empty;
    public string CampaignCode { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// DTO chi tiết Đợt khám sức khỏe kèm thống kê số lượng hồ sơ
/// </summary>
public record CampaignDetailModel : CreateCampaignModel
{
    public int CampaignId { get; init; }
    public int TotalRecords { get; init; }
    public int CompletedRecords { get; init; }
}
