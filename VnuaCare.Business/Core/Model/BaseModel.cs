using VnuaCare.Shared.Constant;

namespace VnuaCare.Business;

public record BaseQueryFilterModel
{
    public string? TextSearch { get; set; }
    public int? UserId { get; set; } 
    public int? StaffId { get; set; }
    public int? DoctorId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool? IsActive { get; set; }
    public string PropertyName { get; set; } = "CreatedAt";
    
    public string Ascending { get; set; } = "desc"; // giảm dần haowjc tăng dần
    public BaseQueryFilterModel()
    {
        PageNumber = Constant.QueryFilter.PageNumber;
        PageSize = Constant.QueryFilter.PageSize;
    }
}