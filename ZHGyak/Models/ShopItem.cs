using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZHGyak.Models
{
    public class ShopItem : INotifyPropertyChanged
    {
        private bool isOwn;
        private string imageSource;
        private int quantity;
        private string description;
        private string name;

        public bool IsOwn
        {
            get { return isOwn; }
            set { isOwn = value; OnPropertyChanged(); }
        }

        public string ImageSource
        {
            get { return imageSource; }
            set { imageSource = value; OnPropertyChanged(); }
        }

        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(); }
        }

        public ShopItem(bool isOwn, string imageSource, int quantity, string description, string name)
        {
            IsOwn = isOwn;
            ImageSource = imageSource;
            Quantity = quantity;
            Description = description;
            Name = name;
        }

        public ShopItem()
        {
            
        }

        public static ShopItem Parse(string text)
        {
            var split = text.Split(';');
            return new ShopItem
            {
                IsOwn = bool.Parse(split[0]),
                ImageSource = split[1],
                Quantity = int.Parse(split[2]),
                Description = split[3],
                Name = split[4]
            };
        }

        public override string ToString()
        {
            return $"{this.isOwn};{this.ImageSource};{this.Quantity};{this.Description};{this.Name}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
