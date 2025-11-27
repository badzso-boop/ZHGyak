using ZHGyak.ViewModels;

namespace ZHGyak
{
    public partial class MainPage : ContentPage
    {
        MainPageViewModel viewModel;
        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            this.viewModel = vm;
            this.BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.viewModel.Load();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.viewModel.Save();
        }
    }
}
