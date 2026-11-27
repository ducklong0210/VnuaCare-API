/**
 * Định nghĩa các lớp truyền dữ liệu (DTO) của Phân hệ Xét nghiệm Cận lâm sàng (LabTests: Huyết học, Sinh hóa, Nước tiểu)
 */

using System.ComponentModel.DataAnnotations;

namespace VnuaCare.Business.Business.LabTests;

/// <summary>
/// DTO danh mục chỉ số xét nghiệm chuẩn kèm khoảng tham chiếu (trả về cho Frontend hiển thị bảng chọn)
/// </summary>
public record LabIndicatorModel
{
    public int IndicatorId { get; init; }                        // ID danh mục chỉ số
    public string IndicatorCode { get; init; } = string.Empty;   // Mã ngắn chỉ số (WBC, RBC, GLU, AST...)
    public string IndicatorName { get; init; } = string.Empty;   // Tên đầy đủ chỉ số xét nghiệm
    public string CategoryGroup { get; init; } = string.Empty;   // Nhóm xét nghiệm (HEMATOLOGY, BIOCHEMISTRY, URINALYSIS...)
    public string? Unit { get; init; }                           // Đơn vị đo lường
    public decimal? RefMin { get; init; }                        // Ngưỡng tham chiếu cận dưới
    public decimal? RefMax { get; init; }                        // Ngưỡng tham chiếu cận trên
    public string? RefTextRange { get; init; }                   // Chuỗi khoảng tham chiếu hiển thị
    public string? GenderSpecific { get; init; }                 // Giới tính áp dụng riêng (nếu có)
    public string? MeasuringDevice { get; init; }                // Tên thiết bị/máy đo xét nghiệm
}

/// <summary>
/// DTO kết quả từng chỉ số xét nghiệm (kèm thông tin danh mục tham chiếu và cờ bất thường)
/// </summary>
public record LabTestResultModel
{
    public int ResultId { get; init; }                           // ID dòng kết quả xét nghiệm
    public int OrderId { get; init; }                            // ID phiếu chỉ định xét nghiệm cha
    public int IndicatorId { get; init; }                        // ID chỉ số trong danh mục chuẩn
    public string IndicatorCode { get; init; } = string.Empty;   // Mã ngắn chỉ số (lấy từ bảng danh mục)
    public string IndicatorName { get; init; } = string.Empty;   // Tên đầy đủ chỉ số
    public string CategoryGroup { get; init; } = string.Empty;   // Nhóm xét nghiệm
    public string? Unit { get; init; }                           // Đơn vị đo
    public decimal? RefMin { get; init; }                        // Ngưỡng cận dưới
    public decimal? RefMax { get; init; }                        // Ngưỡng cận trên
    public string? RefTextRange { get; init; }                   // Chuỗi khoảng tham chiếu chuẩn
    public string? MeasuringDevice { get; init; }                // Thiết bị phân tích
    public string ResultValue { get; init; } = string.Empty;     // Giá trị kết quả đo được (chuỗi)
    public decimal? NumericValue { get; init; }                  // Giá trị định lượng dạng số
    public string? Flag { get; init; }                           // Cờ đánh giá: NORMAL, HIGH, LOW, ABNORMAL
    public string? Note { get; init; }                           // Ghi chú chuyên môn
    public DateTime? CreatedAt { get; init; }                    // Thời điểm lưu kết quả
}

/// <summary>
/// DTO phiếu chỉ định xét nghiệm kèm danh sách toàn bộ chỉ số kết quả bên trong
/// </summary>
public record LabTestOrderModel
{
    public int OrderId { get; init; }                            // ID phiếu chỉ định xét nghiệm
    public int RecordId { get; init; }                           // ID hồ sơ khám sức khỏe liên kết
    public int? ServiceId { get; init; }                         // ID dịch vụ xét nghiệm (tùy chọn)
    public string? SampleBarcode { get; init; }                  // Mã vạch dán trên mẫu bệnh phẩm
    public string SampleType { get; init; } = string.Empty;      // Loại mẫu bệnh phẩm (MÁU, NƯỚC TIỂU...)
    public DateTime? CollectedAt { get; init; }                  // Thời gian lấy mẫu
    public DateTime? ReceivedAt { get; init; }                   // Thời gian phòng lab nhận mẫu
    public string? Status { get; init; }                         // Trạng thái phiếu (PENDING, PROCESSING, COMPLETED)
    public List<LabTestResultModel> Results { get; init; } = new(); // Danh sách các chỉ số kết quả thuộc phiếu
}

/// <summary>
/// DTO đầu vào tạo mới phiếu chỉ định xét nghiệm
/// </summary>
public record CreateLabTestOrderModel
{
    [Required(ErrorMessage = "ID hồ sơ khám sức khỏe không được để trống")]
    public int RecordId { get; init; }                           // ID hồ sơ khám cần tạo phiếu xét nghiệm

    public int? ServiceId { get; init; }                         // ID dịch vụ y tế (tùy chọn)
    public string? SampleBarcode { get; init; }                  // Mã vạch mẫu (để trống hệ thống tự lấy theo mã SID hồ sơ)

    [Required(ErrorMessage = "Loại mẫu bệnh phẩm không được để trống (ví dụ: MÁU, NƯỚC TIỂU)")]
    public string SampleType { get; init; } = "MÁU";             // Loại mẫu bệnh phẩm

    public DateTime? CollectedAt { get; init; }                  // Thời gian lấy mẫu (mặc định là thời điểm hiện tại)
    public DateTime? ReceivedAt { get; init; }                   // Thời gian nhận mẫu (mặc định là thời điểm hiện tại)
    public string? Status { get; init; }                         // Trạng thái khởi tạo (mặc định PENDING)
}

/// <summary>
/// DTO đầu vào cho 1 dòng kết quả chỉ số khi gửi lưu hàng loạt
/// </summary>
public record LabTestResultItemInput
{
    [Required]
    public int IndicatorId { get; init; }                        // ID chỉ số xét nghiệm trong danh mục chuẩn

    [Required(ErrorMessage = "Giá trị kết quả không được để trống")]
    public string ResultValue { get; init; } = string.Empty;     // Giá trị đo được (số hoặc định tính như Âm tính/Dương tính)

    public decimal? NumericValue { get; init; }                  // Giá trị số (tùy chọn, bỏ trống hệ thống tự tách từ ResultValue)
    public string? Flag { get; init; }                           // Cờ cảnh báo (bỏ trống hệ thống tự so sánh với ref_min / ref_max)
    public string? Note { get; init; }                           // Ghi chú thêm của bác sĩ
}

/// <summary>
/// DTO đầu vào lưu / cập nhật hàng loạt kết quả xét nghiệm cho một phiếu chỉ định (Upsert)
/// </summary>
public record SaveLabTestResultsModel
{
    [Required(ErrorMessage = "ID phiếu chỉ định xét nghiệm không được để trống")]
    public int OrderId { get; init; }                            // ID phiếu chỉ định cần nhập/cập nhật kết quả

    public string? OrderStatus { get; init; } = "COMPLETED";     // Trạng thái cập nhật cho phiếu sau khi lưu kết quả

    public bool ReplaceAll { get; init; } = false;               // Nếu true: đồng bộ xóa các chỉ số cũ không còn trong danh sách gửi lên

    [Required]
    public List<LabTestResultItemInput> Results { get; init; } = new(); // Danh sách các chỉ số kết quả cần lưu
}
