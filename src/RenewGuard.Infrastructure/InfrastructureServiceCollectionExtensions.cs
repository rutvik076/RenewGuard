using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RenewGuard.Application.Abstractions;
using RenewGuard.Infrastructure.Extraction;
using RenewGuard.Infrastructure.Pdf;
using RenewGuard.Infrastructure.Persistence;

namespace RenewGuard.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddRenewGuardInfrastructure(
        this IServiceCollection services,
        string sqliteConnection,
        string anthropicApiKey,
        string model)
    {
        services.AddDbContext<RenewGuardDbContext>(o => o.UseSqlite(sqliteConnection));
        services.AddSingleton(ChatClientFactory.Create(model, anthropicApiKey));
        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
        services.AddSingleton<IContractExtractor, LlmContractExtractor>();
        return services;
    }
}
