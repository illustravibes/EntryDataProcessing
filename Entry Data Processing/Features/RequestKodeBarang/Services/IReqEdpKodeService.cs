using System.Collections.Generic;
using System.Threading.Tasks;
using Entry_Data_Processing.Core.Common;
using Entry_Data_Processing.Features.RequestKodeBarang.Models;

namespace Entry_Data_Processing.Features.RequestKodeBarang.Services
{
    public interface IReqEdpKodeService
    {
        Task<IEnumerable<ReqEdpKodeRecord>> GetRequestsAsync(ReqEdpFilter filter);
        Task<StatusCountsDto> GetStatusCountsAsync(ReqEdpFilter filter);
        Task<ReqEdpKodeRecord?> GetRequestByIdAsync(int id);
        Task<Result<bool>> ApproveRequestAsync(ApprovalActionDto action);
        Task<Result<bool>> RejectRequestAsync(ApprovalActionDto action);
        Task<IEnumerable<string>> GetDistinctAreasAsync();
        Task<IEnumerable<string>> GetDistinctTokosAsync();
    }
}
