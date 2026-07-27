using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Models;

public class PropertyDisplayItem
{
    public PropertyDisplayItem(string displayName, string? value)
    {
        DisplayName = displayName;
        Value = value;
    }

    public string DisplayName { get; }
    public string? Value { get; }
}
