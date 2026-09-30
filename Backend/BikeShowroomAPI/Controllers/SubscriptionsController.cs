using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;
using BikeShowroomAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly RazorpayClient _razorpay;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        MongoDbContext context,
        RazorpayClient razorpay,
        ILogger<SubscriptionsController> logger)
    {
        _context = context;
        _razorpay = razorpay;
        _logger = logger;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanResponse>>> GetPlans(CancellationToken cancellationToken)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var filter = isSuperAdmin
            ? Builders<SubscriptionPlan>.Filter.Empty
            : Builders<SubscriptionPlan>.Filter.Eq(plan => plan.IsActive, true);
        var plans = await _context.SubscriptionPlans
            .Find(filter)
            .SortBy(plan => plan.Amount)
            .ToListAsync(cancellationToken);
        return Ok(plans.Select(ToResponse));
    }

    [HttpPost("plans")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<SubscriptionPlanResponse>> CreatePlan(
        CreateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Features is null || request.Features.Count > 20 ||
            request.Features.Any(feature => string.IsNullOrWhiteSpace(feature) || feature.Length > 100))
            return BadRequest(new { message = "A plan can have at most 20 features, each up to 100 characters." });

        try
        {
            var plan = new SubscriptionPlan
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                Amount = request.Amount,
                Period = request.Period.ToLowerInvariant(),
                Interval = request.Interval,
                Features = request.Features.Select(feature => feature.Trim()).Where(feature => feature.Length > 0).ToList(),
                RazorpayPlanId = await _razorpay.CreatePlanAsync(
                    request.Name.Trim(),
                    request.Description.Trim(),
                    request.Amount,
                    request.Period.ToLowerInvariant(),
                    request.Interval,
                    cancellationToken)
            };
            await _context.SubscriptionPlans.InsertOneAsync(plan, cancellationToken: cancellationToken);
            return Created("/api/subscriptions/plans", ToResponse(plan));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Subscription payments are not configured." });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Razorpay plan creation failed.");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Razorpay could not create the subscription plan." });
        }
    }

    [HttpPatch("plans/{id}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SetPlanStatus(
        string id,
        SetSubscriptionPlanStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return NotFound();

        var result = await _context.SubscriptionPlans.UpdateOneAsync(
            plan => plan.Id == id,
            Builders<SubscriptionPlan>.Update.Set(plan => plan.IsActive, request.IsActive),
            cancellationToken: cancellationToken);
        return result.MatchedCount == 0 ? NotFound() : NoContent();
    }

    [HttpPost("checkout")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<ActionResult<SubscriptionCheckoutResponse>> CreateCheckout(
        CreateSubscriptionCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var companyId = User.FindFirstValue("CompanyId");
        if (string.IsNullOrWhiteSpace(companyId))
            return Forbid();
        if (!MongoDB.Bson.ObjectId.TryParse(request.PlanId, out _))
            return BadRequest(new { message = "Select a valid subscription package." });

        var plan = await _context.SubscriptionPlans
            .Find(candidate => candidate.Id == request.PlanId && candidate.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
        if (plan is null)
            return NotFound(new { message = "The selected subscription package is not available." });

        var current = await _context.CompanySubscriptions
            .Find(subscription => subscription.CompanyId == companyId &&
                (subscription.Status == "created" || subscription.Status == "active" ||
                 subscription.Status == "authenticated" || subscription.Status == "pending"))
            .AnyAsync(cancellationToken);
        if (current)
            return Conflict(new { message = "This company already has a subscription in progress or active." });

        var company = await _context.Companies
            .Find(candidate => candidate.Id == companyId)
            .FirstOrDefaultAsync(cancellationToken);
        if (company is null)
            return Forbid();

        try
        {
            var (razorpaySubscriptionId, status) = await _razorpay.CreateSubscriptionAsync(
                plan.RazorpayPlanId,
                companyId,
                plan.Id,
                cancellationToken);
            var subscription = new CompanySubscription
            {
                CompanyId = companyId,
                PlanId = plan.Id,
                RazorpaySubscriptionId = razorpaySubscriptionId,
                Status = status
            };
            await _context.CompanySubscriptions.InsertOneAsync(subscription, cancellationToken: cancellationToken);

            return Ok(new SubscriptionCheckoutResponse(
                _razorpay.KeyId,
                razorpaySubscriptionId,
                company.Name,
                plan.Name,
                User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                User.FindFirstValue(ClaimTypes.Email) ?? string.Empty));
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Subscription payments are not configured." });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Razorpay subscription creation failed for company {CompanyId}.", companyId);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Razorpay could not start checkout." });
        }
    }

    [HttpGet("current")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<ActionResult<CompanySubscriptionResponse>> GetCurrent(CancellationToken cancellationToken)
    {
        var companyId = User.FindFirstValue("CompanyId");
        if (string.IsNullOrWhiteSpace(companyId))
            return Forbid();

        var subscription = await _context.CompanySubscriptions
            .Find(candidate => candidate.CompanyId == companyId)
            .SortByDescending(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (subscription is null)
            return NoContent();

        var plan = await _context.SubscriptionPlans
            .Find(candidate => candidate.Id == subscription.PlanId)
            .FirstOrDefaultAsync(cancellationToken);
        if (plan is null)
            return NotFound();

        return Ok(new CompanySubscriptionResponse(
            plan.Name,
            plan.Amount,
            plan.Period,
            subscription.Status,
            subscription.CancelAtCycleEnd,
            new DateTimeOffset(DateTime.SpecifyKind(subscription.CreatedAt, DateTimeKind.Utc))));
    }

    [HttpPost("verify")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> Verify(
        VerifySubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var companyId = User.FindFirstValue("CompanyId");
        if (string.IsNullOrWhiteSpace(companyId))
            return Forbid();

        var subscription = await _context.CompanySubscriptions
            .Find(candidate => candidate.CompanyId == companyId &&
                candidate.RazorpaySubscriptionId == request.RazorpaySubscriptionId)
            .FirstOrDefaultAsync(cancellationToken);
        if (subscription is null)
            return NotFound();
        if (!_razorpay.VerifyPaymentSignature(
                request.RazorpaySubscriptionId,
                request.RazorpayPaymentId,
                request.RazorpaySignature))
            return Unauthorized(new { message = "Razorpay signature verification failed." });

        try
        {
            var (providerPlanId, status) = await _razorpay.GetSubscriptionAsync(
                subscription.RazorpaySubscriptionId,
                cancellationToken);
            var plan = await _context.SubscriptionPlans
                .Find(candidate => candidate.Id == subscription.PlanId)
                .FirstOrDefaultAsync(cancellationToken);
            if (plan is null || providerPlanId != plan.RazorpayPlanId)
                return BadRequest(new { message = "Razorpay subscription does not match this package." });

            await _context.CompanySubscriptions.UpdateOneAsync(
                candidate => candidate.Id == subscription.Id,
                Builders<CompanySubscription>.Update
                    .Set(candidate => candidate.LastPaymentId, request.RazorpayPaymentId)
                    .Set(candidate => candidate.Status, status)
                    .Set(candidate => candidate.UpdatedAt, DateTime.UtcNow),
                cancellationToken: cancellationToken);
            return Ok(new { status });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Razorpay subscription verification failed.");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Could not confirm the subscription with Razorpay." });
        }
    }

    [HttpPost("current/cancel")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> CancelCurrent(CancellationToken cancellationToken)
    {
        var companyId = User.FindFirstValue("CompanyId");
        if (string.IsNullOrWhiteSpace(companyId))
            return Forbid();

        var subscription = await _context.CompanySubscriptions
            .Find(candidate => candidate.CompanyId == companyId && candidate.Status == "active")
            .SortByDescending(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (subscription is null)
            return NotFound();

        try
        {
            await _razorpay.CancelSubscriptionAsync(subscription.RazorpaySubscriptionId, cancellationToken);
            await _context.CompanySubscriptions.UpdateOneAsync(
                candidate => candidate.Id == subscription.Id,
                Builders<CompanySubscription>.Update
                    .Set(candidate => candidate.CancelAtCycleEnd, true)
                    .Set(candidate => candidate.UpdatedAt, DateTime.UtcNow),
                cancellationToken: cancellationToken);
            return NoContent();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Razorpay subscription cancellation failed.");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Could not schedule cancellation with Razorpay." });
        }
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    [RequestSizeLimit(65536)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["X-Razorpay-Signature"].ToString();
        if (!_razorpay.VerifyWebhookSignature(payload, signature))
            return Unauthorized();

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var eventName = root.GetProperty("event").GetString();
            var subscriptionId = root.GetProperty("payload")
                .GetProperty("subscription")
                .GetProperty("entity")
                .GetProperty("id")
                .GetString();
            if (string.IsNullOrWhiteSpace(subscriptionId))
                return BadRequest();

            var status = eventName switch
            {
                "subscription.activated" or "subscription.charged" => "active",
                "subscription.authenticated" => "authenticated",
                "subscription.pending" => "pending",
                "subscription.halted" => "halted",
                "subscription.cancelled" => "cancelled",
                "subscription.completed" => "completed",
                _ => null
            };
            if (status is null)
                return Ok();

            await _context.CompanySubscriptions.UpdateOneAsync(
                subscription => subscription.RazorpaySubscriptionId == subscriptionId,
                Builders<CompanySubscription>.Update
                    .Set(subscription => subscription.Status, status)
                    .Set(subscription => subscription.UpdatedAt, DateTime.UtcNow),
                cancellationToken: cancellationToken);
            return Ok();
        }
        catch (JsonException)
        {
            return BadRequest();
        }
        catch (KeyNotFoundException)
        {
            return BadRequest();
        }
        catch (InvalidOperationException)
        {
            return BadRequest();
        }
    }

    private static SubscriptionPlanResponse ToResponse(SubscriptionPlan plan) => new(
        plan.Id,
        plan.Name,
        plan.Description,
        plan.Amount,
        plan.Period,
        plan.Interval,
        plan.Features,
        plan.IsActive);
}