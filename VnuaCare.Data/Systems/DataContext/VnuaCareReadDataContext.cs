/**
 * DbContext Đọc: Chuyên dùng để truy vấn, lọc danh sách (AsNoTracking) tối ưu hiệu năng
 */

using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Data.Systems.DataContext;

public class VnuaCareReadDataContext : DbContext
{
    protected VnuaCareReadDataContext()
    {
    }

    public VnuaCareReadDataContext(DbContextOptions<VnuaCareReadDataContext> options) : base(options)
    {
    }
    public DbSet<VcUsers> VcUsers { get; set; }
    public DbSet<VcStaffs> VcStaffs { get; set; }
    public DbSet<VcDoctors> VcDoctors { get; set; }
    public DbSet<VcDepartments> VcDepartments { get; set; } 
    public DbSet<VcSpecialties> VcSpecialties { get; set; }

}