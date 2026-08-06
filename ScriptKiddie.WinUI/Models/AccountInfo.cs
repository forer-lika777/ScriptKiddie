using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
