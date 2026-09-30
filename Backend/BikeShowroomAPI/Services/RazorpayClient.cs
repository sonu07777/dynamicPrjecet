using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BikeShowroomAPI.Services;

public sealed class RazorpayClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RazorpayClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public string KeyId => _configuration["Razorpay:KeyId"] ?? string.Empty;

    public async Task<string> CreatePlanAsync(
        string name,
        string description,
        decimal amount,
        string period,
        int interval,
        CancellationToken cancellationToken)
    {
        var amountInPaise = amount * 100m;
        if (amountInPaise != decimal.Truncate(amountInPaise))
            throw new ArgumentException("Plan amount must have at most two decimal places.");

        using var response = await SendAsync(HttpMethod.Post, "plans", new
        {
            period,
            interval,
            item = new
            {
                name,
                amount = decimal.ToInt64(amountInPaise),
                currency = "INR",
                description
            }
        }, cancellationToken);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        return document.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Razorpay did not return a plan identifier.");
    }

    public async Task<(string Id, string Status)> CreateSubscriptionAsync(
        string razorpayPlanId,
        string companyId,
        string planId,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Post, "subscriptions", new
        {
            plan_id = razorpayPlanId,
            total_count = 120,
            customer_notify = 1,
            notes = new { company_id = companyId, package_id = planId }
        }, cancellationToken);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        var root = document.RootElement;
        return (root.GetProperty("id").GetString()!, root.GetProperty("status").GetString() ?? "created");
    }

    public async Task<(string PlanId, string Status)> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"subscriptions/{Uri.EscapeDataString(subscriptionId)}",
            null,
            cancellationToken);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        var root = document.RootElement;
        return (root.GetProperty("plan_id").GetString()!, root.GetProperty("status").GetString() ?? "created");
    }

    public async Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            $"subscriptions/{Uri.EscapeDataString(subscriptionId)}/cancel",
            new { cancel_at_cycle_end = 1 },
            cancellationToken);
    }

    public bool VerifyPaymentSignature(string subscriptionId, string paymentId, string signature)
    {
        var secret = _configuration["Razorpay:KeySecret"];
        return !string.IsNullOrWhiteSpace(secret) &&
            VerifySignature(secret, $"{subscriptionId}|{paymentId}", signature);
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        var secret = _configuration["Razorpay:WebhookSecret"];
        return !string.IsNullOrWhiteSpace(secret) && VerifySignature(secret, payload, signature);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        var keyId = _configuration["Razorpay:KeyId"];
        var keySecret = _configuration["Razorpay:KeySecret"];
        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            throw new InvalidOperationException("Razorpay credentials are not configured.");

        using var request = new HttpRequestMessage(method, path);
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return response;

        response.Dispose();
        throw new HttpRequestException("The Razorpay request failed.");
    }

    private static bool VerifySignature(string secret, string payload, string signature)
    {
        try
        {
            var expected = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(secret),
                Encoding.UTF8.GetBytes(payload));
            var supplied = Convert.FromHexString(signature);
            return supplied.Length == expected.Length &&
                CryptographicOperations.FixedTimeEquals(supplied, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}