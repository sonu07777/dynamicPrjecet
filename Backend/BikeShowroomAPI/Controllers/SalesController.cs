using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.Models;
using System.Security.Claims;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly BikeShowroomContext _context;

    public SalesController(BikeShowroomContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sale>>> GetSales([FromQuery] int? branchId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var query = _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Customer)
            .Include(s => s.Payments)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value);

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Take(100)
            .ToListAsync();

        return Ok(sales);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Sale>> GetSale(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Customer)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
            return NotFound();

        return Ok(sale);
    }

    [HttpPost]
    public async Task<ActionResult<Sale>> CreateSale(CreateSaleDTO createSaleDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        // Generate invoice number
        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var sale = new Sale
        {
            CompanyId = createSaleDto.CompanyId,
            BranchId = createSaleDto.BranchId,
            CustomerId = createSaleDto.CustomerId,
            InvoiceNumber = invoiceNumber,
            SubTotal = createSaleDto.SubTotal,
            TaxAmount = createSaleDto.TaxAmount,
            DiscountAmount = createSaleDto.DiscountAmount,
            TotalAmount = createSaleDto.TotalAmount,
            PaymentMethod = createSaleDto.PaymentMethod,
            PaymentStatus = createSaleDto.PaymentStatus,
            AmountPaid = createSaleDto.AmountPaid,
            AmountDue = createSaleDto.AmountDue,
            Notes = createSaleDto.Notes,
            CashierId = userId
        };

        // Add sale items
        foreach (var itemDto in createSaleDto.Items)
        {
            var saleItem = new SaleItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Discount = itemDto.Discount,
                TotalPrice = itemDto.TotalPrice
            };

            sale.Items.Add(saleItem);

            // Update inventory
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == itemDto.ProductId && i.BranchId == createSaleDto.BranchId);

            if (inventory != null)
            {
                inventory.Quantity -= itemDto.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Add payment if fully paid
        if (createSaleDto.AmountPaid > 0)
        {
            var payment = new Payment
            {
                PaymentMethod = createSaleDto.PaymentMethod,
                Amount = createSaleDto.AmountPaid,
                ReferenceNumber = createSaleDto.PaymentReference
            };
            sale.Payments.Add(payment);
        }

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<SalesStatsDTO>> GetSalesStats([FromQuery] int? branchId = null)
    {
        var query = _context.Sales.AsQueryable();

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        var today = DateTime.Today;
        var todaySales = await query.Where(s => s.SaleDate >= today).ToListAsync();
        var thisMonth = await query.Where(s => s.SaleDate.Month == DateTime.Now.Month && s.SaleDate.Year == DateTime.Now.Year).ToListAsync();

        var stats = new SalesStatsDTO
        {
            TodaySales = todaySales.Count,
            TodayRevenue = todaySales.Sum(s => s.TotalAmount),
            MonthSales = thisMonth.Count,
            MonthRevenue = thisMonth.Sum(s => s.TotalAmount),
            AverageSale = todaySales.Any() ? todaySales.Average(s => s.TotalAmount) : 0
        };

        return Ok(stats);
    }
}

public class CreateSaleDTO
{
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string? Notes { get; set; }
    public string? PaymentReference { get; set; }
    public List<CreateSaleItemDTO> Items { get; set; } = new();
}

public class CreateSaleItemDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class SalesStatsDTO
{
    public int TodaySales { get; set; }
    public decimal TodayRevenue { get; set; }
    public int MonthSales { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal AverageSale { get; set; }
}
