using CommunityToolkit.Mvvm.ComponentModel;

namespace ScriptKiddie.WinUI.Models;

/// <summary>表示单个属性的校验结果状态。</summary>
public partial class ValidationErrorStatus : ObservableObject
{
    public ValidationErrorStatus(string? message = null)
    {
        this.Message = message ?? string.Empty;
        this.Success = string.IsNullOrWhiteSpace(message);
    }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Success { get; private set; }
}
