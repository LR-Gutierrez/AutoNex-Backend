using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.Suppliers;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<SupplierResponse>> GetAllAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Suppliers
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .AsQueryable();

        return await query.ToPagedResponseAsync(page, pageSize, s => s.ToResponse(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<SupplierResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        return supplier?.ToResponse();
    }

    public async Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return supplier.ToResponse();
    }

    public async Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (supplier is null) return null;

        supplier.Name = request.Name;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return supplier.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (supplier is null) return false;

        supplier.IsDeleted = true;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
