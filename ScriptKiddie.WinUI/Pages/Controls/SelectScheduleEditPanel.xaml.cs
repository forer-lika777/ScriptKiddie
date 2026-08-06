using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.ViewModels;
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
        ViewModel = viewModel ?? new SelectScheduleEditPanelModel();
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
