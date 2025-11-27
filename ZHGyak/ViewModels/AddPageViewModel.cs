using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZHGyak.Models;

namespace ZHGyak.ViewModels
{
    public class AddPageViewModel
    {
        public ShopItem Item { get; set; }

        public Command C_Save { get; private set; }
        public Command C_PickImage { get; private set; }

        public AddPageViewModel()
        {
            this.Item = new ShopItem();
            this.C_Save = new Command(Save);
            this.C_PickImage = new Command(PickImage);
        }

        private async void PickImage()
        {
            FileResult? image = await MediaPicker.Default.PickPhotoAsync();
            if (image != null)
            {
                this.Item.ImageSource = image.FullPath;
            }
        }

        private async void Save()
        {
            var param = new ShellNavigationQueryParameters()
            {
                {"newitem", Item }
            };

            await Shell.Current.GoToAsync("..", param);
        }
    }
}
