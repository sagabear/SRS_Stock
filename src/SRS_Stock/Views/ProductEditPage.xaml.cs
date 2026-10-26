using SRS_Stock.Core.Navigation;
using SRS_Stock.Core.ViewModels;

namespace SRS_Stock.Views;

public partial class ProductEditPage : ContentPage, IQueryAttributable
{
    public ProductEditPage(ProductEditViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        BindingContext = viewModel;
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.ContainsKey(Routes.ProductIdParameter))
            return;

        if (BindingContext is ProductEditViewModel viewModel)
            _ = viewModel.ApplyQueryAsync(query);
    }
}
