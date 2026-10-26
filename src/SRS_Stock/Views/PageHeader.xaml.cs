namespace SRS_Stock.Views;

public partial class PageHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(PageHeader),
        string.Empty);

    public static readonly BindableProperty TrailingProperty = BindableProperty.Create(
        nameof(Trailing),
        typeof(View),
        typeof(PageHeader));

    public static readonly BindableProperty ShowsBackButtonProperty = BindableProperty.Create(
        nameof(ShowsBackButton),
        typeof(bool),
        typeof(PageHeader),
        false);

    public PageHeader()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public View? Trailing
    {
        get => (View?)GetValue(TrailingProperty);
        set => SetValue(TrailingProperty, value);
    }

    public bool ShowsBackButton
    {
        get => (bool)GetValue(ShowsBackButtonProperty);
        set => SetValue(ShowsBackButtonProperty, value);
    }

    private void OpenMenu(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }

    private async void GoBack(object? sender, EventArgs e)
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
