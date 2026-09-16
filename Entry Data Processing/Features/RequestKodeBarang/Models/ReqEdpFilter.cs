namespace Entry_Data_Processing.Features.RequestKodeBarang.Models
{
    public class ReqEdpFilter
    {
        public string? TabStatus { get; set; } // "All", "Draft", "Pending", "Approved", "Rejected"
        public int? Status { get; set; }
        public string? Keyword { get; set; }
        public string? Area { get; set; }
        public string? Toko { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
    }
}
