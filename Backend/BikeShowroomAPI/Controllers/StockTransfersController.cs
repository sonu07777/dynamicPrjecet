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
public class StockTransfersController : ControllerBase
{
    private readonly BikeShowroomContext _context;

    public StockTransfersController(BikeShowroomContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransferDTO>>> GetStockTransfers(
        [FromQuery] int? companyId = null,
        [FromQuery] int? fromBranchId = null,
        [FromQuery] int? toBranchId = null,
        [FromQuery] string? status = null)
    {
        var query = _context.StockTransfers
            .Include(st => st.FromBranch)
            .Include(st => st.ToBranch)
            .Include(st => st.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(st => st.CompanyId == companyId.Value);
        if (fromBranchId.HasValue)
            query = query.Where(st => st.FromBranchId == fromBranchId.Value);
        if (toBranchId.HasValue)
            query = query.Where(st => st.ToBranchId == toBranchId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(st => st.Status == status);

        var transfers = await query
            .OrderByDescending(st => st.TransferDate)
            .Select(st => new StockTransferDTO
            {
                Id = st.Id,
                CompanyId = st.CompanyId,
                FromBranchId = st.FromBranchId,
                ToBranchId = st.ToBranchId,
                TransferNumber = st.TransferNumber,
                TransferDate = st.TransferDate,
                Status = st.Status,
                Notes = st.Notes,
                InitiatedBy = st.InitiatedBy,
                FromBranchName = st.FromBranch.Name,
                ToBranchName = st.ToBranch.Name,
                Items = st.Items.Select(i => new StockTransferItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToListAsync();

        return Ok(transfers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockTransferDTO>> GetStockTransfer(int id)
    {
        var st = await _context.StockTransfers
            .Include(x => x.FromBranch)
            .Include(x => x.ToBranch)
            .Include(x => x.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (st == null)
            return NotFound();

        return Ok(new StockTransferDTO
        {
            Id = st.Id,
            CompanyId = st.CompanyId,
            FromBranchId = st.FromBranchId,
            ToBranchId = st.ToBranchId,
            TransferNumber = st.TransferNumber,
            TransferDate = st.TransferDate,
            Status = st.Status,
            Notes = st.Notes,
            InitiatedBy = st.InitiatedBy,
            FromBranchName = st.FromBranch.Name,
            ToBranchName = st.ToBranch.Name,
            Items = st.Items.Select(i => new StockTransferItemDTO
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity
            }).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<ActionResult<StockTransferDTO>> CreateStockTransfer(CreateStockTransferDTO createDto)
    {
        var transferNumber = $"ST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var transfer = new StockTransfer
        {
            CompanyId = createDto.CompanyId,
            FromBranchId = createDto.FromBranchId,
            ToBranchId = createDto.ToBranchId,
            TransferNumber = transferNumber,
            TransferDate = DateTime.UtcNow,
            Status = "Pending",
            Notes = createDto.Notes,
            InitiatedBy = User.Identity?.Name ?? "Unknown"
        };

        foreach (var itemDto in createDto.Items)
        {
            transfer.Items.Add(new StockTransferItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            });
        }

        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStockTransfer), new { id = transfer.Id }, null);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTransferStatusDTO statusDto)
    {
        var transfer = await _context.StockTransfers
            .Include(st => st.Items)
            .FirstOrDefaultAsync(st => st.Id == id);

        if (transfer == null)
            return NotFound();

        transfer.Status = statusDto.Status;

        if (statusDto.Status == "InTransit")
        {
            // Deduct from source branch
            foreach (var item in transfer.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.BranchId == transfer.FromBranchId);

                if (inventory != null)
                {
                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        if (statusDto.Status == "Received")
        {
            // Add to destination branch
            foreach (var item in transfer.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.BranchId == transfer.ToBranchId);

                if (inventory != null)
                {
                    inventory.Quantity += item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.Inventories.Add(new Inventory
                    {
                        ProductId = item.ProductId,
                        BranchId = transfer.ToBranchId,
                        Quantity = item.Quantity,
                        ReservedQuantity = 0
                    });
                }
            }
        }

        if (statusDto.Status == "Cancelled" && transfer.Status == "InTransit")
        {
            // Restore source branch inventory
            foreach (var item in transfer.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.BranchId == transfer.FromBranchId);

                if (inventory != null)
                {
                    inventory.Quantity += item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteStockTransfer(int id)
    {
        var transfer = await _context.StockTransfers.FindAsync(id);
        if (transfer == null)
            return NotFound();

        _context.StockTransfers.Remove(transfer);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class UpdateTransferStatusDTO
{
    public string Status { get; set; } = string.Empty;
}
