using SRS_Stock.Core.ViewModels;

namespace SRS_Stock.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage(AboutViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        BindingContext = viewModel;
        InitializeComponent();
        PageSafeArea.Apply(this);
    }
}
