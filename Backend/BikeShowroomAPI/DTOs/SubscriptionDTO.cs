using System.ComponentModel.DataAnnotations;

namespace BikeShowroomAPI.DTOs;

/// <summary>A subscription package offered to authenticated companies.</summary>
public sealed record SubscriptionPlanResponse(
    string Id,
    string Name,
    string Description,
    decimal Amount,
    string Period,
    int Interval,
    List<string> Features,
    bool IsActive);

/// <summary>Data required to create a recurring INR subscription plan.</summary>
public sealed record CreateSubscriptionPlanRequest
{
    [Required, StringLength(80, MinimumLength = 2)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "1", "10000000")]
    public required decimal Amount { get; init; }

    [Required, RegularExpression("^(monthly|yearly)$", ErrorMessage = "Period must be monthly or yearly.")]
    public required string Period { get; init; }

    [Range(1, 12)]
    public int Interval { get; init; } = 1;

    public List<string> Features { get; init; } = [];
}

/// <summary>Identifies the package selected for checkout.</summary>
public sealed record CreateSubscriptionCheckoutRequest
{
    [Required]
    public required string PlanId { get; init; }
}

/// <summary>Razorpay checkout values required by the browser widget.</summary>
public sealed record SubscriptionCheckoutResponse(
    string KeyId,
    string SubscriptionId,
    string CompanyName,
    string PlanName,
    string CustomerName,
    string CustomerEmail);

/// <summary>Proof returned by Razorpay after subscription authorization.</summary>
public sealed record VerifySubscriptionRequest
{
    [Required]
    public required string RazorpaySubscriptionId { get; init; }

    [Required]
    public required string RazorpayPaymentId { get; init; }

    [Required]
    public required string RazorpaySignature { get; init; }
}

/// <summary>Subscription state associated with the authenticated company.</summary>
public sealed record CompanySubscriptionResponse(
    string PlanName,
    decimal Amount,
    string Period,
    string Status,
    bool CancelAtCycleEnd,
    DateTimeOffset CreatedAt);

/// <summary>Request to activate or deactivate a plan for new subscriptions.</summary>
public sealed record SetSubscriptionPlanStatusRequest(bool IsActive);