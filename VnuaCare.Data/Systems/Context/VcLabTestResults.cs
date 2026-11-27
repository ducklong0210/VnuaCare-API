using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng kết quả chi tiết từng chỉ số xét nghiệm gắn với phiếu chỉ định (vc_lab_test_results)
/// </summary>
[Table("vc_lab_test_results")]
public class VcLabTestResults
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("result_id", TypeName = "int")]
    public int ResultId { get; set; } // Khóa chính: ID dòng kết quả xét nghiệm

    [Required]
    [Column("order_id", TypeName = "int")]
    public int OrderId { get; set; } // Khóa ngoại: ID phiếu chỉ định xét nghiệm cha (vc_lab_test_orders)

    [Required]
    [Column("indicator_id", TypeName = "int")]
    public int IndicatorId { get; set; } // Khóa ngoại: ID chỉ số trong danh mục chuẩn (vc_lab_indicator_catalog)

    [Required]
    [Column("result_value", TypeName = "nvarchar(100)")]
    public string ResultValue { get; set; } = string.Empty; // Giá trị kết quả dạng chuỗi hiển thị (VD: "5.4", "47.5", "Âm tính")

    [Column("numeric_value", TypeName = "numeric(10,4)")]
    public decimal? NumericValue { get; set; } // Giá trị định lượng dạng số (tự động chuyển đổi từ ResultValue để so sánh ngưỡng)

    [Column("flag", TypeName = "varchar(20)")]
    public string? Flag { get; set; } // Cờ cảnh báo bất thường (NORMAL: Bình thường, HIGH: Cao, LOW: Thấp, ABNORMAL: Bất thường)

    [Column("note", TypeName = "nvarchar(max)")]
    public string? Note { get; set; } // Ghi chú chuyên môn của bác sĩ/kỹ thuật viên (VD: Tăng, Giảm...)

    [Column("created_at", TypeName = "datetime2")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now; // Thời điểm ghi nhận kết quả chỉ số

    [ForeignKey("OrderId")]
    public virtual VcLabTestOrders? LabTestOrder { get; set; } // Điều hướng: Phiếu chỉ định xét nghiệm cha

    [ForeignKey("IndicatorId")]
    public virtual VcLabIndicatorCatalog? Indicator { get; set; } // Điều hướng: Thông tin danh mục chỉ số chuẩn
}
