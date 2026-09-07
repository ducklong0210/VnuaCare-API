using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Data.Systems.DataContext;

public class VnuaCareReadDataContext : DbContext
{
    
    
    public DbSet<VcUsers> VcUsers { get; set; }
    public DbSet<VcStaffs> VcStaffs { get; set; }
    public DbSet<VcDoctors> VcDoctors { get; set; }
}