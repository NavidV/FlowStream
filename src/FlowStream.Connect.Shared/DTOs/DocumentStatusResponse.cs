using FlowStream.Connect.Shared.Enums;

namespace FlowStream.Connect.Shared.DTOs;

public sealed record DocumentStatusResponse(
    string DocumentId,
    DocumentStatus Status,
    DateTimeOffset LastUpdatedAtUtc,
    string? ErrorCode);
