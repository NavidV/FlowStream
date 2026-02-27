namespace FlowStream.Connect.Shared.ProblemDetails;

public sealed class ConnectProblemDetails
{
    public string Type { get; init; } = "about:blank";

    public string Title { get; init; } = string.Empty;

    public int Status { get; init; }

    public string Detail { get; init; } = string.Empty;

    public string Instance { get; init; } = string.Empty;

    public string ErrorCode { get; init; } = string.Empty;
}
