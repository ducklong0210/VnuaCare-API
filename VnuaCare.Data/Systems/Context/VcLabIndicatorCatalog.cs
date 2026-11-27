using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng danh mục các chỉ số xét nghiệm huyết học, sinh hóa, nước tiểu kèm dải tham chiếu chuẩn (vc_lab_indicator_catalog)
/// </summary>
[Table("vc_lab_indicator_catalog")]
public class VcLabIndicatorCatalog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("indicator_id", TypeName = "int")]
    public int IndicatorId { get; set; } // Khóa chính: ID danh mục chỉ số xét nghiệm

    [Required]
    [Column("indicator_code", TypeName = "varchar(50)")]
    public string IndicatorCode { get; set; } = string.Empty; // Mã định danh ngắn của chỉ số (VD: WBC, RBC, HGB, GLU, AST, ALT...)

    [Required]
    [Column("indicator_name", TypeName = "nvarchar(255)")]
    public string IndicatorName { get; set; } = string.Empty; // Tên đầy đủ tiếng Việt/quốc tế của chỉ số xét nghiệm

    [Required]
    [Column("category_group", TypeName = "varchar(50)")]
    public string CategoryGroup { get; set; } = string.Empty; // Nhóm phân loại xét nghiệm (HEMATOLOGY: Huyết học, BIOCHEMISTRY: Sinh hóa, URINALYSIS: Nước tiểu, IMMUNOLOGY: Miễn dịch)

    [Column("unit", TypeName = "nvarchar(50)")]
    public string? Unit { get; set; } // Đơn vị đo lường của chỉ số (VD: g/L, mmol/L, U/L, %...)

    [Column("ref_min", TypeName = "numeric(10,4)")]
    public decimal? RefMin { get; set; } // Ngưỡng tham chiếu cận dưới dùng để tự động kiểm tra giá trị thấp (LOW)

    [Column("ref_max", TypeName = "numeric(10,4)")]
    public decimal? RefMax { get; set; } // Ngưỡng tham chiếu cận trên dùng để tự động kiểm tra giá trị cao (HIGH)

    [Column("ref_text_range", TypeName = "nvarchar(100)")]
    public string? RefTextRange { get; set; } // Chuỗi khoảng tham chiếu chuẩn hiển thị cho bác sĩ và cán bộ (VD: "3.9 - 6.4", "Âm tính")

    [Column("gender_specific", TypeName = "varchar(10)")]
    public string? GenderSpecific { get; set; } // Áp dụng riêng theo giới tính nếu có (ALL, MALE, FEMALE)

    [Column("measuring_device", TypeName = "nvarchar(100)")]
    public string? MeasuringDevice { get; set; } // Tên máy xét nghiệm sinh hóa / huyết học thực hiện phân tích
}
