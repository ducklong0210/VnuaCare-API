/**
 * DbContext Ghi: Quản lý lưu trữ, thêm, sửa, xóa dữ liệu vào CSDL SQL Server
 */

using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Shared.ContextAccessor;

namespace VnuaCare.Data.Systems.DataContext;

/// <summary>
/// DbContext chính của hệ thống V-Care Health
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

    public DbSet<VcUsers> VcUsers { get; set; }
    public DbSet<VcStaffs> VcStaffs { get; set; }
    public DbSet<VcDoctors> VcDoctors { get; set; }
    public DbSet<VcDepartments> VcDepartments { get; set; } 
}
