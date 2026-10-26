namespace SRS_Stock.Core.Navigation;

public interface IAppNavigator
{
    Task GoToProductEditAsync(int productId);

    Task GoBackAsync();
}
