namespace Entry_Data_Processing.Features.RequestKodeBarang.Models
{
    public class ApprovalActionDto
    {
        public int Id { get; set; }
        public string ApproverNip { get; set; } = string.Empty;
        public string? Alasan { get; set; }
    }
}
