using FlowStream.Connect.Shared.Enums;

namespace FlowStream.Connect.Worker.Services;

public sealed class DocumentDispatchWorker(
    ISqsMessageReceiver sqsMessageReceiver,
    IKivraClient kivraClient,
    ILogger<DocumentDispatchWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await sqsMessageReceiver.ReceiveAsync(stoppingToken);
            if (message is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                continue;
            }

            try
            {
                var sendResult = await kivraClient.SendAsync(message.DocumentId, stoppingToken);
                logger.LogInformation("Document {DocumentId} dispatched with status {Status}", message.DocumentId, sendResult);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Dispatch failed for {DocumentId}; will retry", message.DocumentId);
                await sqsMessageReceiver.RetryAsync(message, stoppingToken);
            }

            var status = await kivraClient.GetStatusAsync(message.DocumentId, stoppingToken);
            logger.LogInformation("Status check for {DocumentId}: {Status}", message.DocumentId, status);
        }
    }
}

public sealed record DispatchMessage(string DocumentId, int RetryCount);

public interface ISqsMessageReceiver
{
    Task<DispatchMessage?> ReceiveAsync(CancellationToken cancellationToken);
    Task RetryAsync(DispatchMessage message, CancellationToken cancellationToken);
}

public interface IKivraClient
{
    Task<DocumentStatus> SendAsync(string documentId, CancellationToken cancellationToken);
    Task<DocumentStatus> GetStatusAsync(string documentId, CancellationToken cancellationToken);
}

internal sealed class InMemorySqsMessageReceiver : ISqsMessageReceiver
{
    public Task<DispatchMessage?> ReceiveAsync(CancellationToken cancellationToken) =>
        Task.FromResult<DispatchMessage?>(null);

    public Task RetryAsync(DispatchMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FakeKivraClient : IKivraClient
{
    public Task<DocumentStatus> SendAsync(string documentId, CancellationToken cancellationToken) =>
        Task.FromResult(DocumentStatus.Sent);

    public Task<DocumentStatus> GetStatusAsync(string documentId, CancellationToken cancellationToken) =>
        Task.FromResult(DocumentStatus.Delivered);
}
