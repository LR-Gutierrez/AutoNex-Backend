using AutoNex.Data;
using AutoNex.DTOs.WorkshopInfo;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class WorkshopInfoService : IWorkshopInfoService
{
    private readonly AppDbContext _context;

    public WorkshopInfoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkshopInfoResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        var info = await _context.WorkshopInfos.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        return info?.ToResponse();
    }

    public async Task<WorkshopInfoResponse> UpsertAsync(UpdateWorkshopInfoRequest request, CancellationToken cancellationToken = default)
    {
        var info = await _context.WorkshopInfos.FirstOrDefaultAsync(cancellationToken);

        if (info is null)
        {
            info = new WorkshopInfo
            {
                BusinessName = request.BusinessName,
                Rif = request.Rif,
                Address = request.Address,
                City = request.City,
                MapsUrl = request.MapsUrl,
                Phone = request.Phone,
                SecondaryPhone = request.SecondaryPhone,
                Email = request.Email,
                Website = request.Website,
                OpeningHours = request.OpeningHours,
            };
            _context.WorkshopInfos.Add(info);
        }
        else
        {
            info.BusinessName = request.BusinessName;
            info.Rif = request.Rif;
            info.Address = request.Address;
            info.City = request.City;
            info.MapsUrl = request.MapsUrl;
            info.Phone = request.Phone;
            info.SecondaryPhone = request.SecondaryPhone;
            info.Email = request.Email;
            info.Website = request.Website;
            info.OpeningHours = request.OpeningHours;
            info.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return info.ToResponse();
    }
}
