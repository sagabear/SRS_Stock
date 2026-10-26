using SRS_Stock.Core.ViewModels;

namespace SRS_Stock.Views;

public partial class ProductListPage : ContentPage
{
    public ProductListPage(ProductListViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        BindingContext = viewModel;
        InitializeComponent();
        PageSafeArea.Apply(this);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProductListViewModel viewModel)
            _ = viewModel.LoadCommand.ExecuteAsync(null);
    }
}
