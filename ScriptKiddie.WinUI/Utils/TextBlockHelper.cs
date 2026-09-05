using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace ScriptKiddie.WinUI.Utils;

public static class TextBlockHelper
{
    public static readonly DependencyProperty PrefixTextProperty =
        DependencyProperty.RegisterAttached(
            "PrefixText",
            typeof(string),
            typeof(TextBlockHelper),
            new PropertyMetadata(null, OnPrefixTextChanged));

    public static void SetPrefixText(UIElement element, string value) => element.SetValue(PrefixTextProperty, value);
    public static string GetPrefixText(UIElement element) => (string)element.GetValue(PrefixTextProperty);

    private static void OnPrefixTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock textBlock && e.NewValue is string prefix)
        {
            // 确保 TextBlock 加载完成后插入前缀
            textBlock.Loaded -= TextBlock_Loaded;
            textBlock.Loaded += TextBlock_Loaded;

            ApplyPrefix(textBlock, prefix);
        }
    }

    private static void TextBlock_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBlock textBlock)
        {
            string prefix = GetPrefixText(textBlock);
            ApplyPrefix(textBlock, prefix);
        }
    }

    private static void ApplyPrefix(TextBlock textBlock, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return;

        // 检查是否已经添加过前缀，防止重复添加
        if (textBlock.Inlines.Count > 0 && textBlock.Inlines[0] is Run firstRun && firstRun.Text == prefix)
        {
            return;
        }

        // 如果使用 Text 属性赋值，将其转为 Inlines
        if (!string.IsNullOrEmpty(textBlock.Text))
        {
            string originalText = textBlock.Text;
            textBlock.Text = string.Empty; // 清空 Text，改用 Inlines 渲染

            textBlock.Inlines.Add(new Run { Text = prefix });
            textBlock.Inlines.Add(new Run { Text = originalText });
        }
        else if (textBlock.Inlines.Count > 0)
        {
            textBlock.Inlines.Insert(0, new Run { Text = prefix });
        }
    }
}
