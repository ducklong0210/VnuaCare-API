using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng phiếu chỉ định xét nghiệm máu, nước tiểu gắn với hồ sơ khám sức khỏe (vc_lab_test_orders)
/// </summary>
[Table("vc_lab_test_orders")]
public class VcLabTestOrders
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("order_id", TypeName = "int")]
    public int OrderId { get; set; } // Khóa chính: ID phiếu chỉ định xét nghiệm

    [Required]
    [Column("record_id", TypeName = "int")]
    public int RecordId { get; set; } // Khóa ngoại: ID hồ sơ khám sức khỏe liên kết

    [Column("service_id", TypeName = "int")]
    public int? ServiceId { get; set; } // Khóa ngoại tùy chọn: ID dịch vụ y tế tương ứng (nếu có)

    [Column("sample_barcode", TypeName = "varchar(50)")]
    public string? SampleBarcode { get; set; } // Mã vạch dán trên ống mẫu bệnh phẩm (mặc định kế thừa mã SID của hồ sơ)

    [Required]
    [Column("sample_type", TypeName = "varchar(30)")]
    public string SampleType { get; set; } = string.Empty; // Loại mẫu bệnh phẩm xét nghiệm (VD: MÁU, NƯỚC TIỂU, KHÁC)

    [Column("collected_at", TypeName = "datetime2")]
    public DateTime? CollectedAt { get; set; } // Thời điểm điều dưỡng/kỹ thuật viên lấy mẫu bệnh phẩm

    [Column("received_at", TypeName = "datetime2")]
    public DateTime? ReceivedAt { get; set; } // Thời điểm phòng xét nghiệm tiếp nhận mẫu

    [Column("status", TypeName = "varchar(20)")]
    public string? Status { get; set; } = "PENDING"; // Trạng thái xử lý phiếu (PENDING: Chờ lấy mẫu, PROCESSING: Đang xét nghiệm, COMPLETED: Đã có kết quả)

    [ForeignKey("RecordId")]
    public virtual VcHealthCheckupRecords? HealthCheckupRecord { get; set; } // Điều hướng: Hồ sơ khám sức khỏe cha

    public virtual ICollection<VcLabTestResults> Results { get; set; } = new List<VcLabTestResults>(); // Điều hướng: Danh sách kết quả các chỉ số con thuộc phiếu này
}
