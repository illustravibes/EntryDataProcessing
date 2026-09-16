namespace Entry_Data_Processing.Features.RequestKodeBarang.Models
{
    public class ReqEdpFilter
    {
        public int? Status { get; set; } // null = All, 0 = Pending, 1 = Approved, 2 = Rejected
        public string? Keyword { get; set; }
    }
}
