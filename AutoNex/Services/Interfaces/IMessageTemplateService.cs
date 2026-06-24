using AutoNex.DTOs;
using AutoNex.DTOs.MessageTemplates;

namespace AutoNex.Services.Interfaces;

public interface IMessageTemplateService
{
    Task<PagedResponse<MessageTemplateResponse>> GetAllAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);
    Task<MessageTemplateResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MessageTemplateResponse?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<MessageTemplateResponse> CreateAsync(CreateMessageTemplateRequest request, CancellationToken cancellationToken = default);
    Task<MessageTemplateResponse?> UpdateAsync(int id, UpdateMessageTemplateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
