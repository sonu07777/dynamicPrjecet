using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly BikeShowroomContext _context;

    public PurchaseOrdersController(BikeShowroomContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDTO>>> GetPurchaseOrders(
        [FromQuery] int? companyId = null,
        [FromQuery] int? branchId = null,
        [FromQuery] string? status = null)
    {
        var query = _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .Include(po => po.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(po => po.CompanyId == companyId.Value);
        if (branchId.HasValue)
            query = query.Where(po => po.BranchId == branchId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(po => po.Status == status);

        var orders = await query
            .OrderByDescending(po => po.OrderDate)
            .Select(po => new PurchaseOrderDTO
            {
                Id = po.Id,
                CompanyId = po.CompanyId,
                BranchId = po.BranchId,
                SupplierId = po.SupplierId,
                OrderNumber = po.OrderNumber,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                ReceivedDate = po.ReceivedDate,
                Status = po.Status,
                TotalAmount = po.TotalAmount,
                Notes = po.Notes,
                CreatedBy = po.CreatedBy,
                SupplierName = po.Supplier.Name,
                BranchName = po.Branch.Name,
                Items = po.Items.Select(i => new PurchaseOrderItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    QuantityOrdered = i.QuantityOrdered,
                    QuantityReceived = i.QuantityReceived,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrderDTO>> GetPurchaseOrder(int id)
    {
        var po = await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Branch)
            .Include(po => po.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (po == null)
            return NotFound();

        return Ok(new PurchaseOrderDTO
        {
            Id = po.Id,
            CompanyId = po.CompanyId,
            BranchId = po.BranchId,
            SupplierId = po.SupplierId,
            OrderNumber = po.OrderNumber,
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            ReceivedDate = po.ReceivedDate,
            Status = po.Status,
            TotalAmount = po.TotalAmount,
            Notes = po.Notes,
            CreatedBy = po.CreatedBy,
            SupplierName = po.Supplier.Name,
            BranchName = po.Branch.Name,
            Items = po.Items.Select(i => new PurchaseOrderItemDTO
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                QuantityOrdered = i.QuantityOrdered,
                QuantityReceived = i.QuantityReceived,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<ActionResult<PurchaseOrderDTO>> CreatePurchaseOrder(CreatePurchaseOrderDTO createDto)
    {
        var orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var purchaseOrder = new PurchaseOrder
        {
            CompanyId = createDto.CompanyId,
            BranchId = createDto.BranchId,
            SupplierId = createDto.SupplierId,
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = createDto.ExpectedDeliveryDate,
            Status = "Pending",
            Notes = createDto.Notes,
            CreatedBy = User.Identity?.Name ?? "Unknown",
            TotalAmount = createDto.Items.Sum(i => i.QuantityOrdered * i.UnitPrice)
        };

        foreach (var itemDto in createDto.Items)
        {
            purchaseOrder.Items.Add(new PurchaseOrderItem
            {
                ProductId = itemDto.ProductId,
                QuantityOrdered = itemDto.QuantityOrdered,
                UnitPrice = itemDto.UnitPrice,
                TotalPrice = itemDto.QuantityOrdered * itemDto.UnitPrice
            });
        }

        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = purchaseOrder.Id }, null);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDTO statusDto)
    {
        var po = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (po == null)
            return NotFound();

        po.Status = statusDto.Status;

        if (statusDto.Status == "Received" || statusDto.Status == "PartiallyReceived")
        {
            po.ReceivedDate = DateTime.UtcNow;
            foreach (var item in po.Items)
            {
                var receivedQty = statusDto.Status == "Received"
                    ? item.QuantityOrdered
                    : item.QuantityReceived + (statusDto.ReceivedQuantities?.ContainsKey(item.ProductId) == true
                        ? statusDto.ReceivedQuantities[item.ProductId]
                        : 0);

                item.QuantityReceived = Math.Min(receivedQty, item.QuantityOrdered);

                // Update inventory
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.BranchId == po.BranchId);

                if (inventory != null)
                {
                    var qtyToAdd = statusDto.Status == "Received"
                        ? item.QuantityOrdered - (item.QuantityReceived - item.QuantityOrdered)
                        : statusDto.ReceivedQuantities?.GetValueOrDefault(item.ProductId, 0) ?? 0;

                    inventory.Quantity += qtyToAdd;
                    inventory.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (po.Items.All(i => i.QuantityReceived >= i.QuantityOrdered))
                po.Status = "Received";
            else if (po.Items.Any(i => i.QuantityReceived > 0))
                po.Status = "PartiallyReceived";
        }

        if (statusDto.Status == "Cancelled")
        {
            po.Status = "Cancelled";
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeletePurchaseOrder(int id)
    {
        var po = await _context.PurchaseOrders.FindAsync(id);
        if (po == null)
            return NotFound();

        _context.PurchaseOrders.Remove(po);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class UpdateOrderStatusDTO
{
    public string Status { get; set; } = string.Empty;
    public Dictionary<int, int>? ReceivedQuantities { get; set; }
}
