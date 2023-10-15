using System;
using System.Windows;
using ADO.NET_DisConnected_Mode_HW.ViewModels;

namespace ADO.NET_DisConnected_Mode_HW.Views;

public partial class MainView : Window {
    public MainView() {
        InitializeComponent();
        DataContext = new MainViewModel(AuthorsDataGridView);
    }
}
