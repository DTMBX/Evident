namespace BarberX.Mobile.Views;

using BarberX.Mobile.ViewModels;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
