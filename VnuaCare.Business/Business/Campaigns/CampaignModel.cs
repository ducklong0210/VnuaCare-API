/**
 * Các mô hình dữ liệu (DTO) cho phân hệ Quản lý Đợt khám sức khỏe (Campaigns)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Business;

namespace VnuaCare.Business.Business.Campaigns;

/// <summary>
/// Lớp cơ sở chứa thông tin chung của Đợt khám sức khỏe định kỳ
/// </summary>
public abstract record CampaignBaseModel
{
    public int CampaignId { get; init; }
    public string CampaignName { get; init; } = string.Empty;
    public string CampaignCode { get; init; } = string.Empty;
    public int Year { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string HospitalPartner { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// DTO trả về chi tiết thông tin đợt khám kèm thống kê hồ sơ
/// </summary>
public record CampaignModel : CampaignBaseModel
{
    public int TotalRecords { get; init; }
}

/// <summary>
/// DTO nhận vào khi thêm mới Đợt khám sức khỏe định kỳ
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
/// DTO nhận vào khi cập nhật thông tin Đợt khám sức khỏe
/// </summary>
public record UpdateCampaignModel : CreateCampaignModel
{
    [Required(ErrorMessage = "ID đợt khám không được để trống")]
    public int CampaignId { get; init; }
}

/// <summary>
/// DTO phục vụ lọc, tìm kiếm và phân trang Đợt khám sức khỏe
/// </summary>
public record CampaignFilterModel : BaseQueryFilterModel
{
    public int? Year { get; set; }
    public string? Status { get; set; }
    public string? HospitalPartner { get; set; }
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