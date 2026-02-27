using FlowStream.Connect.Shared.Enums;

namespace FlowStream.Connect.Shared.DTOs;

public sealed record SubmitDocumentResponse(
    string DocumentId,
    DocumentStatus Status,
    DateTimeOffset CreatedAtUtc);
