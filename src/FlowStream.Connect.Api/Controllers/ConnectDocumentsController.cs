using FlowStream.Connect.Api.Services;
using FlowStream.Connect.Shared.Constants;
using FlowStream.Connect.Shared.DTOs;
using FlowStream.Connect.Shared.ProblemDetails;
using FlowStream.Connect.Shared.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowStream.Connect.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/connect/documents")]
public sealed class ConnectDocumentsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SubmitDocumentResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ConnectProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> SubmitDocument(
        [FromBody] SubmitDocumentRequest request,
        [FromServices] IS3DocumentStore s3DocumentStore,
        [FromServices] IDocumentRepository documentRepository,
        [FromServices] ISqsDispatcher sqsDispatcher,
        CancellationToken cancellationToken)
    {
        if (!FormatValidators.IsValidExternalId(request.ExternalId) || !FormatValidators.IsValidRecipient(request.Recipient))
        {
            return Results.BadRequest(new ConnectProblemDetails
            {
                Title = "Invalid request",
                Status = StatusCodes.Status400BadRequest,
                Detail = "ExternalId or Recipient format is invalid.",
                ErrorCode = ErrorCodes.ValidationError
            });
        }

        if (!await s3DocumentStore.ExistsAsync(request.StorageKey, cancellationToken))
        {
            return Results.BadRequest(new ConnectProblemDetails
            {
                Title = "Storage object not found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The requested S3 object does not exist.",
                ErrorCode = ErrorCodes.StorageFailure
            });
        }

        var response = await documentRepository.SaveAsync(request, cancellationToken);
        await sqsDispatcher.EnqueueAsync(response.DocumentId, cancellationToken);
        return Results.Accepted($"/api/connect/documents/{response.DocumentId}", response);
    }

    [HttpGet("{documentId}")]
    [ProducesResponseType(typeof(DocumentStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ConnectProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetStatus(
        string documentId,
        [FromServices] IDocumentRepository documentRepository,
        CancellationToken cancellationToken)
    {
        var response = await documentRepository.GetStatusAsync(documentId, cancellationToken);
        if (response is null)
        {
            return Results.NotFound(new ConnectProblemDetails
            {
                Title = "Document not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No document found for id '{documentId}'.",
                ErrorCode = ErrorCodes.NotFound
            });
        }

        return Results.Ok(response);
    }
}
