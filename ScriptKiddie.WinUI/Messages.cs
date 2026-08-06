using ScriptKiddie.WinUI.Models;

namespace ScriptKiddie.WinUI;

// Account management

public record AccountInfoChangedMessage(AccountInfo value);

public record LoginSuccessMessage();

public record AutoLoginFailedNeedCaptchaMessage();

// Select schedules management

public record SelectScheduleChangedMessage();