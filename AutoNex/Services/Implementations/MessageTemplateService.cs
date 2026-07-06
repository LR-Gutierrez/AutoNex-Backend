using AutoNex.Data;
using AutoNex.DTOs;
using AutoNex.DTOs.MessageTemplates;
using AutoNex.Helpers;
using AutoNex.Models;
using AutoNex.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Services.Implementations;

public class MessageTemplateService : IMessageTemplateService
{
    private readonly AppDbContext _context;

    public MessageTemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<MessageTemplateResponse>> GetAllAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.MessageTemplates.AsNoTracking().OrderBy(t => t.Key);
        return await query.ToPagedResponseAsync(page, pageSize, t => t.ToResponse(), cancellationToken);
    }

    public async Task<MessageTemplateResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return template?.ToResponse();
    }

    public async Task<MessageTemplateResponse?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Key == key, cancellationToken);
        return template?.ToResponse();
    }

    public async Task<MessageTemplateResponse?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
        return template?.ToResponse();
    }

    public async Task<MessageTemplateResponse> CreateAsync(CreateMessageTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.MessageTemplates.AnyAsync(t => t.Key == request.Key, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Ya existe un template con la clave '{request.Key}'");

        var template = new MessageTemplate
        {
            Key = request.Key,
            Template = request.Template,
            Description = request.Description
        };

        _context.MessageTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return template.ToResponse();
    }

    public async Task<MessageTemplateResponse?> UpdateAsync(int id, UpdateMessageTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template is null) return null;

        template.Template = request.Template;
        template.Description = request.Description;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return template.ToResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template is null) return false;

        _context.MessageTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template is null) return false;

        await _context.MessageTemplates
            .Where(t => t.IsActive)
            .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.IsActive, false), cancellationToken);

        template.IsActive = true;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
