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

    public async Task<PagedResponse<SupplierResponse>> GetAllAsync(int? page, int? pageSize)
    {
        var query = _context.Suppliers
            .OrderByDescending(s => s.CreatedAt)
            .AsQueryable();

        var paged = await query.ToPagedAsync(page, pageSize);

        return new PagedResponse<SupplierResponse>
        {
            Items = paged.Items.Select(s => s.ToResponse()).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public async Task<SupplierResponse?> GetByIdAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        return supplier?.ToResponse();
    }

    public async Task<SupplierResponse> CreateAsync(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return supplier.ToResponse();
    }

    public async Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return null;

        supplier.Name = request.Name;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return supplier.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return false;

        supplier.IsDeleted = true;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
