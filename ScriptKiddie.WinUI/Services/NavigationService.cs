using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace ScriptKiddie.WinUI.Services;

public class NavigationService
{
    private Frame? frame;

    public void Initialize(Frame frame)
    {
        this.frame = frame;
    }

    public void NavigateTo<T>(object? parameter = null) where T : Page
    {
        frame?.Navigate(typeof(T), parameter, new DrillInNavigationTransitionInfo());
    }

    public bool CanGoBack => frame?.CanGoBack ?? false;
    public void GoBack() => frame?.GoBack();
}
