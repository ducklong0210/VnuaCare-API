/**
 * DbContext Đọc: Chuyên dùng để truy vấn, lọc danh sách (AsNoTracking) tối ưu hiệu năng
 */

using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Data.Systems.DataContext;

/// <summary>
/// DbContext chỉ đọc dùng cho các truy vấn dữ liệu (Queries) nhằm tối ưu tốc độ phản hồi
/// </summary>
public class VnuaCareReadDataContext : DbContext
{
    protected VnuaCareReadDataContext()
    {
    }

    public VnuaCareReadDataContext(DbContextOptions<VnuaCareReadDataContext> options) : base(options)
    {
    }

    public DbSet<VcUsers> VcUsers { get; set; }                // Đọc dữ liệu bảng tài khoản người dùng
    public DbSet<VcStaffs> VcStaffs { get; set; }              // Đọc dữ liệu bảng hồ sơ cán bộ
    public DbSet<VcDoctors> VcDoctors { get; set; }            // Đọc dữ liệu bảng hồ sơ bác sĩ
    public DbSet<VcDepartments> VcDepartments { get; set; }    // Đọc dữ liệu bảng danh mục Khoa / Phòng ban
    public DbSet<VcSpecialties> VcSpecialties { get; set; }    // Đọc dữ liệu bảng danh mục Chuyên khoa y tế
    public DbSet<VcHealthCheckupRecords> VcHealthCheckupRecords { get; set; }
    public DbSet<VcInternalMedicineExams> VcInternalMedicineExams { get; set; }
    public DbSet<VcCheckupCampaigns> VcCheckupCampaigns { get; set; }
    
}
