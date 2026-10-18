using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RenewGuard.McpServer;

[McpServerToolType]
public sealed class RenewalTools(HttpClient http)
{
    [McpServerTool(Name = "get_upcoming_renewals")]
    [Description("Lists contracts whose next cancellation deadline falls within the given number of days, soonest first. Each item has the vendor, the cancel-by date, days left, the notice period, and whether it auto-renews.")]
    public async Task<string> GetUpcomingRenewals(
        [Description("Look-ahead window in days (1 to 3650). Default 90.")] int days = 90,
        CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 1, 3650);
        return await GetAsync($"contracts/upcoming?days={days}", cancellationToken);
    }

    [McpServerTool(Name = "get_contract_details")]
    [Description("Returns the full extraction for one contract: every extracted field with its supporting quote from the contract text and a confidence score. Use an id from get_upcoming_renewals.")]
    public async Task<string> GetContractDetails(
        [Description("The contract id (a GUID).")] string id,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var guid))
            return "That is not a valid contract id. Use an id returned by get_upcoming_renewals.";

        return await GetAsync($"contracts/{guid}", cancellationToken);
    }

    private async Task<string> GetAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync(path, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return "No contract found with that id.";

            if (!response.IsSuccessStatusCode)
                return $"The RenewGuard API returned an error ({(int)response.StatusCode}).";

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return "Could not reach the RenewGuard API. Make sure it is running and RENEWGUARD_API_URL is correct.";
        }
    }
}
