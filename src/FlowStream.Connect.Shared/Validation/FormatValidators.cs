using System.Text.RegularExpressions;

namespace FlowStream.Connect.Shared.Validation;

public static partial class FormatValidators
{
    [GeneratedRegex("^[A-Za-z0-9_-]{3,64}$")]
    private static partial Regex ExternalIdRegex();

    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")]
    private static partial Regex EmailRegex();

    public static bool IsValidExternalId(string value) =>
        !string.IsNullOrWhiteSpace(value) && ExternalIdRegex().IsMatch(value);

    public static bool IsValidRecipient(string value) =>
        !string.IsNullOrWhiteSpace(value) && EmailRegex().IsMatch(value);
}
