using ZHGyak.Views;

namespace ZHGyak
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("newitem", typeof(AddPage));
        }
    }
}
