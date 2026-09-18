using Entry_Data_Processing.Features.RequestKodeBarang.ViewModels;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Services;

public sealed class RejectReasonViewModelFactory : IRejectReasonViewModelFactory
{
    public RejectReasonViewModel Create() => new();
}
