using ZHGyak.ViewModels;

namespace ZHGyak.Views;

public partial class AddPage : ContentPage
{
	public AddPage(AddPageViewModel vm)
	{
		InitializeComponent();
		this.BindingContext = vm;
    }
}