using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZHGyak.Models;

namespace ZHGyak.ViewModels
{
	[QueryProperty(nameof(NewItem), "newitem")] // itt kapja meg az uj elem oldalrol a hozzadando elemet
    public class MainPageViewModel
    {
		private readonly string path = Path.Combine(FileSystem.AppDataDirectory, "adat.txt");

        public  ObservableCollection<ShopItem> Items { get; set; }
		private ShopItem selectedItem;
		public ShopItem NewItem { set {Items.Add(value); Save(); } } // ide mentjuk az elem oldalrol erkezo elemet

		public ShopItem SelectedItem
		{
			get { return selectedItem; }
			set { selectedItem = value; }
		}

        public MainPageViewModel()
        {
			this.Items = new ObservableCollection<ShopItem>();

			this.C_AddItem = new Command(AddItem);
			this.C_RemoveItem = new Command(RemoveItem);
			this.C_ShareItem = new Command(ShareItem);
        }

        public Command C_AddItem { get; private set; }
		public Command C_RemoveItem { get; private set; }
		public Command C_ShareItem { get; private set; }

		private async void AddItem()
		{
			await Shell.Current.GoToAsync("newitem");
        }
		private void RemoveItem()
		{
			if (this.SelectedItem != null)
			{
				this.Items.Remove(this.SelectedItem);
				this.SelectedItem = null;
            }
        }

		private async void ShareItem()
		{
            if (this.SelectedItem != null)
            {
				await Share.Default.RequestAsync(new ShareTextRequest()
				{
					Title = "Micsoda megosztás",
					Text = this.SelectedItem.ToString()
				});
            }
        }

		public void Save()
		{
			StringBuilder sb = new StringBuilder();
            foreach (var item in Items)
            {
				sb.AppendLine(item.ToString());
            }

			File.WriteAllText(path, sb.ToString());
        }

		public void Load()
		{
			if (File.Exists(path))
			{
				var data = File.ReadAllLines(path);
				this.Items.Clear();
                foreach (var item in data)
                {
                    this.Items.Add(ShopItem.Parse(item));
                }
            }
        }

    }
}
