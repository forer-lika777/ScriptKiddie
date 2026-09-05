using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.Core.Services;
using ScriptKiddie.Core.ViewModels;
using System;

namespace ScriptKiddie.WinUI.Pages.Controls;

public sealed partial class SelectScheduleEditPanel : UserControl
{
    private readonly Panel root;
    public SelectScheduleEditPanelModel ViewModel { get; set; }

    public SelectScheduleEditPanel(Panel root, SelectScheduleEditPanelModel? viewModel = null)
    {
        InitializeComponent();
        this.root = root;
        ViewModel = viewModel ?? new SelectScheduleEditPanelModel(App.Current.Services.GetRequiredService<SelectScheduleProvider>());
        ViewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    public void Show()
    {
        root.Children.Add(this);
    }

    public void Close()
    {
        root.Children.Remove(this);
    }
}
