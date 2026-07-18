using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI;

public record AccountInfoChangedMessage(AccountInfo value);
public record LoginSuccessMessage();
