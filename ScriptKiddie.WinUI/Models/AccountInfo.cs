using System.Text.Json.Serialization;

namespace ScriptKiddie.WinUI.Models;

public class AccountInfo
{
    [JsonPropertyName(nameof(AccountName))]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName(nameof(AccountId))]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName(nameof(Grade))]
    public string Grade { get; set; } = string.Empty;
}
