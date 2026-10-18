using RenewGuard.Domain.Models;

namespace RenewGuard.Application.Abstractions;

public interface IContractExtractor
{
    Task<ContractExtraction> ExtractAsync(
        string contractText, CancellationToken cancellationToken = default);
}
