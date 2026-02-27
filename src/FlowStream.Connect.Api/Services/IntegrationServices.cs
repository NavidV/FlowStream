using FlowStream.Connect.Shared.DTOs;
using FlowStream.Connect.Shared.Enums;

namespace FlowStream.Connect.Api.Services;

public interface IS3DocumentStore
{
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken);
}

public interface IDocumentRepository
{
    Task<SubmitDocumentResponse> SaveAsync(SubmitDocumentRequest request, CancellationToken cancellationToken);
    Task<DocumentStatusResponse?> GetStatusAsync(string documentId, CancellationToken cancellationToken);
}

public interface ISqsDispatcher
{
    Task EnqueueAsync(string documentId, CancellationToken cancellationToken);
}

internal sealed class InMemoryS3DocumentStore : IS3DocumentStore
{
    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrWhiteSpace(storageKey));
}

internal sealed class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly Dictionary<string, DocumentStatusResponse> _documents = new();

    public Task<SubmitDocumentResponse> SaveAsync(SubmitDocumentRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString("N");
        var created = DateTimeOffset.UtcNow;
        _documents[id] = new DocumentStatusResponse(id, DocumentStatus.Pending, created, null);

        return Task.FromResult(new SubmitDocumentResponse(id, DocumentStatus.Pending, created));
    }

    public Task<DocumentStatusResponse?> GetStatusAsync(string documentId, CancellationToken cancellationToken)
    {
        _documents.TryGetValue(documentId, out var response);
        return Task.FromResult(response);
    }
}

internal sealed class InMemorySqsDispatcher : ISqsDispatcher
{
    public Task EnqueueAsync(string documentId, CancellationToken cancellationToken) => Task.CompletedTask;
}
