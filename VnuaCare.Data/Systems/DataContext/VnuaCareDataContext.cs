/**
 * DbContext Ghi: Quản lý lưu trữ, thêm, sửa, xóa dữ liệu vào CSDL SQL Server
 */

using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Shared.ContextAccessor;

namespace VnuaCare.Data.Systems.DataContext;

/// <summary>
/// DbContext chính dùng cho các tác vụ ghi dữ liệu (Commands) của hệ thống V-Care Health
/// </summary>
public class VnuaCareDataContext : DbContext
{
    private readonly IContextAccessor? _contextAccessor;

    public VnuaCareDataContext(
        DbContextOptions<VnuaCareDataContext> options, 
        Func<IContextAccessor> contextAccessorFactory) : base(options)
    {
        _contextAccessor = contextAccessorFactory?.Invoke();
    }

    public DbSet<VcUsers> VcUsers { get; set; }                // Quản lý bảng tài khoản người dùng đăng nhập
    public DbSet<VcStaffs> VcStaffs { get; set; }              // Quản lý bảng hồ sơ cán bộ Học viện Nông nghiệp
    public DbSet<VcDoctors> VcDoctors { get; set; }            // Quản lý bảng hồ sơ bác sĩ BVĐK MEDLATEC
    public DbSet<VcDepartments> VcDepartments { get; set; }    // Quản lý bảng danh mục Khoa / Phòng ban
    public DbSet<VcSpecialties> VcSpecialties { get; set; }    // Quản lý bảng danh mục Chuyên khoa y tế
    public DbSet<VcHealthCheckupRecords> VcHealthCheckupRecords { get; set; }
    public DbSet<VcInternalMedicineExams> VcInternalMedicineExams { get; set; }
    public DbSet<VcCheckupCampaigns>  VcCheckupCampaigns { get; set; }
}
