using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Pages.Controls;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class CourseListPage : Page, IRecipient<RequestChooseSelectScheduleMessage>, IRecipient<TaskAddFailedMessage>
{
    private readonly NavigationService navigationService;
    private readonly ILogger<CourseListPage> logger;

    public CourseListPageModel ViewModel { get; set; }

    // 用于在数据模板中的元素使用，因为他们只能通过静态资源访问 ViewModel 中的可观测属性。
    public static CourseListPageModel ViewModelCache { get; set; } = null!;

    private readonly Compositor compositor;
    private readonly Visual rootVisual;

    private static string? lastTabElementTag = null;

    public CourseListPage()
    {
        InitializeComponent();
        ViewModelCache = ViewModel = App.Current.Services.GetRequiredService<CourseListPageModel>();
        navigationService = App.Current.Services.GetRequiredService<NavigationService>();
        logger = App.Current.Services.GetRequiredService<ILogger<CourseListPage>>();
        //WeakReferenceMessenger.Default.Register<SelectScheduleRemoveConfirmMessage>(this);
        WeakReferenceMessenger.Default.Register<RequestChooseSelectScheduleMessage>(this);
        WeakReferenceMessenger.Default.Register<TaskAddFailedMessage>(this);

        compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        rootVisual = ElementCompositionPreview.GetElementVisual(this);

        TabSelectorBar.SelectedItem = TabSelectorBar.Items[TabElementIndex(lastTabElementTag) ?? 0];
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        // 离开时将存储的静态值设置为 null，以匹配页面的生命周期
        ViewModelCache = null!;
        // 订阅多次会重复发消息，要取消订阅
        WeakReferenceMessenger.Default.Unregister<RequestChooseSelectScheduleMessage>(this);
        WeakReferenceMessenger.Default.Unregister<TaskAddFailedMessage>(this);
    }

    private async void ViewMoreButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonText = "关闭",
            DefaultButton = ContentDialogButton.Primary,
            Content = new CourseInfoPanel((CourseItem)((Button)sender).DataContext)
        };

        await dialog.ShowAsync();
    }

    private void ConfigureSelectScheduleButton_Click(object sender, RoutedEventArgs e)
    {
        navigationService.NavigateTo<SelectSchedulePage>();
    }

    public void Receive(RequestChooseSelectScheduleMessage message)
    {
        _ = OpenSelectScheduleSelectPanel(message.TaskCompletionSource);
    }

    private async Task OpenSelectScheduleSelectPanel(TaskCompletionSource<SelectSchedule> tcs)
    {
        var panel = new SelectScheduleSelectPanel();

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonText = "确认",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Title = "选择要关联的时间表",
            Content = panel,
        };

        dialog.PrimaryButtonClick += async (sender, e) =>
        {
            // 按钮点击后一条分逻辑是在下面执行 OpenErrorMessageDialog() 方法，
            // 先手动执行 dialog.Hide() 确保对话框完全关闭，防止对话框重叠。
            dialog.Hide();

            tcs.SetResult(panel.ViewModel.SelectedSchedule);
        };

        dialog.SecondaryButtonClick += (sender, e) => tcs.SetCanceled();

        await dialog.ShowAsync();
    }

    public void Receive(TaskAddFailedMessage message)
    {
        _ = OpenErrorMessageDialog(message.Info);
    }

    private async Task OpenErrorMessageDialog(string message)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Primary,
            Title = "添加课程失败",
            Content = message,
        };

        await dialog.ShowAsync();
    }

    private async void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem as SelectorBarItem;
        if (selectedItem == null)
            return;

        var tag = selectedItem.Tag as string;

        UIElement? element = TabElement(tag);

        if (element is null)
            return;

        UIElement? lastTabElement = TabElement(lastTabElementTag);

        lastTabElement?.Visibility = Visibility.Collapsed;

        _ = AnimationService.AnimateMotionEnterAsync(element, 0, 40);

        lastTabElementTag = tag;
    }

#pragma warning disable CA1859 // 尽可能使用具体类型以提高性能
    private UIElement? TabElement(string? tag)
#pragma warning restore CA1859 // 尽可能使用具体类型以提高性能
    {
        switch (tag)
        {
            case "selectable":
                return SelectableCoursesList;
            case "selected":
                return SelectedCoursesList;
            case "tasks":
                return SelectTasksList;
        }

        return null;
    }

    private static int? TabElementIndex(string? tag)
    {
        switch (tag)
        {
            case "selectable":
                return 0;
            case "selected":
                return 1;
            case "tasks":
                return 2;
        }

        return null;
    }
}