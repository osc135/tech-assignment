using System.Threading.Tasks;
using HijackPoker.Models;

namespace HijackPoker.Api
{
    public interface IPokerApi
    {
        Task<HealthResponse> GetHealthAsync();
        Task<ProcessResponse> ProcessStepAsync(int tableId);
        Task<TableResponse> GetTableStateAsync(int tableId);
    }
}
