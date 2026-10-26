using SRS_Stock.Core.ViewModels;

namespace SRS_Stock.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        BindingContext = viewModel;
        InitializeComponent();
    }
}
