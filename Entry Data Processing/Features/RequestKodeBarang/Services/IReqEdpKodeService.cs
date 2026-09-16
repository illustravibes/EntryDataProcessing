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
        Task<Result<int>> ApproveBulkAsync(IEnumerable<int> ids, string approverNip);
        Task<Result<int>> RejectBulkAsync(IEnumerable<int> ids, string approverNip, string? alasan);
        Task<IEnumerable<string>> GetDistinctAreasAsync();
        Task<IEnumerable<string>> GetDistinctTokosAsync();

        // Wizard Approval Methods
        Task<IEnumerable<ProductDataDto>> SearchProductsAsync(string query);
        Task<bool> CheckProductExistsAsync(string brPrdKd);
        Task<bool> CheckItemExistsAsync(string brKd);
        Task<IEnumerable<FactoryDto>> SearchFactoriesAsync(string query);
        Task<IEnumerable<ProductTypeDto>> SearchProductTypesAsync(string query);
        Task<IEnumerable<string>> GetPriceGroupsAsync();
        Task<IEnumerable<string>> SearchPriceGroupsAsync(string query);
        Task<IEnumerable<UnitDto>> SearchUnitsAsync(string query);
        Task<bool> CheckPriceCombinationExistsAsync(string brPrdKd, string brHrgGol, string satKd);
        Task<Result<bool>> ProcessWizardApprovalAsync(ApprovalWizardSubmitDto data);
    }
}
