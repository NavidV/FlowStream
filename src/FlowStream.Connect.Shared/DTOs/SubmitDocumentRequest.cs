using FlowStream.Connect.Shared.Enums;

namespace FlowStream.Connect.Shared.DTOs;

public sealed record SubmitDocumentRequest(
    string ExternalId,
    string Recipient,
    DocumentType DocumentType,
    string StorageKey);
