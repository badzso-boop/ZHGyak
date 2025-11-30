# Mobilprogramozás ZH Összefoglaló

Ez az összefoglaló a laborok (`Labor4`-`Labor9`), az előadásanyagok, és kiemelten a **ZHGyak** mappa egyszerűsített megoldásai alapján készült. A cél, hogy a ZH-n a lehető leggyorsabban és legegyszerűbben tudd implementálni a feladatokat.

## 1. Architektúra (MVVM)

### 1.1 ViewModel (Hagyományos, de egyszerű - ZHGyak módszer)
Nem feltétlenül kell Toolkit, ha csak egy működő megoldás kell. A `ZHGyak` példája a legegyszerűbb:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class AddPageViewModel : INotifyPropertyChanged
{
    // Az adat objektum, amit kötünk a felülethez
    public ShopItem Item { get; set; } 
    
    public Command C_Save { get; private set; } // Parancs gombokhoz

    public AddPageViewModel()
    {
        this.Item = new ShopItem();
        this.C_Save = new Command(Save); // Metódus bekötése
    }

    private async void Save()
    {
        // Mentés logika (lásd Navigáció fejezet)
    }

    // INotifyPropertyChanged implementáció
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 1.2 Model (Adat osztály)
Érdemes felkészíteni az osztályt a fájlba írásra (`ToString`) és olvasásra (`Parse`). Így megúszod a JSON szerializációt vagy az adatbázist.

```csharp
public class ShopItem : INotifyPropertyChanged
{
    private string name;
    public string Name 
    { 
        get => name; 
        set { name = value; OnPropertyChanged(); } 
    }
    
    public int Quantity { get; set; }
    public string ImageSource { get; set; }

    // Szöveges konverzió mentéshez (pl. pontosvesszővel elválasztva)
    public override string ToString()
    {
        return $"{Name};{Quantity};{ImageSource}";
    }

    // Visszaolvasás fájlból
    public static ShopItem Parse(string text)
    {
        var split = text.Split(';');
        return new ShopItem
        {
            Name = split[0],
            Quantity = int.Parse(split[1]),
            ImageSource = split[2]
        };
    }
    
    // INotifyPropertyChanged implementáció itt is kell, ha a lista eleme változik
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 1.3 Dependency Injection (MauiProgram.cs)
Regisztráld a Page-eket és ViewModel-eket!

```csharp
// MauiProgram.cs
builder.Services.AddSingleton<MainPage>();
builder.Services.AddSingleton<MainPageViewModel>();

// A részletező/hozzáadó oldalaknál jobb a Transient (mindig új példány)
builder.Services.AddTransient<AddPage>(); 
builder.Services.AddTransient<AddPageViewModel>();
```

### 1.4 View és ViewModel összekötése (Code-behind)
A ViewModel-t a View (az oldal) konstruktorában kérjük el (Dependency Injection), és beállítjuk a `BindingContext`-et. Így tud a XAML hozzáférni a ViewModel adataihoz.

```csharp
// MainPage.xaml.cs
public partial class MainPage : ContentPage
{
    MainPageViewModel viewModel;
    // A rendszer automatikusan beadja a regisztrált ViewModel példányt
    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();
        
        // Összekötés: A felület (View) kötési forrása a ViewModel lesz
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
```

## 2. UI és XAML Alapok (Részletesen)

A felületet XAML nyelven definiáljuk. Minden oldalnak (`ContentPage`) van egy `BindingContext`-e (a ViewModel), amit általában a code-behindban (`.xaml.cs`) állítunk be Dependency Injection segítségével.

### 2.1 Elrendezések (Layouts)

Ezek határozzák meg, hogyan helyezkedjenek el az elemek.

*   **VerticalStackLayout**: Egymás alá pakolja az elemeket fentről lefelé.
    *   `Spacing="10"`: Elemek közötti térköz.
    *   `Padding="20"`: A keret és a belső tartalom közötti távolság.
    ```xml
    <VerticalStackLayout Spacing="10" Padding="20">
        <Label Text="Első sor" />
        <Label Text="Második sor" />
    </VerticalStackLayout>
    ```

*   **HorizontalStackLayout**: Egymás mellé pakolja az elemeket balról jobbra.
    ```xml
    <HorizontalStackLayout>
        <Button Text="Bal" />
        <Button Text="Jobb" />
    </HorizontalStackLayout>
    ```

*   **Grid**: A legrugalmasabb rácsos elrendezés. Sorokat és oszlopokat kell definiálni.
    *   `RowDefinitions`: Sorok magassága. `Auto` (akkora, amekkora kell), `*` (maradék hely kitöltése), `100` (fix pixel).
    *   `ColumnDefinitions`: Oszlopok szélessége.
    ```xml
    <Grid RowDefinitions="Auto, *, 50" ColumnDefinitions="*, 2*">
        <!-- 0. sor, 0. oszlop (Bal felső) -->
        <Label Text="Név:" Grid.Row="0" Grid.Column="0" VerticalOptions="Center"/>
        
        <!-- 0. sor, 1. oszlop -->
        <Entry Placeholder="Írd be a neved" Grid.Row="0" Grid.Column="1"/>
        
        <!-- 1. sor, mindkét oszlopot átéri (ColumnSpan) -->
        <Image Source="dotnet_bot.png" Grid.Row="1" Grid.ColumnSpan="2" Aspect="AspectFit"/>
        
        <!-- 2. sor (Alja), 1. oszlop -->
        <Button Text="Mégse" Grid.Row="2" Grid.Column="0"/>
        <!-- 2. sor, 2. oszlop -->
        <Button Text="Mentés" Grid.Row="2" Grid.Column="1"/>
    </Grid>
    ```

*   **ScrollView**: Ha a tartalom nem fér ki a képernyőre, ebbe kell csomagolni a gyökér elrendezést.
    ```xml
    <ScrollView>
        <VerticalStackLayout>
            <!-- Sok tartalom... -->
        </VerticalStackLayout>
    </ScrollView>
    ```

### 2.2 Vezérlők (Controls)

*   **Label**: Szöveg kiírása.
    *   `Text`: A szöveg tartalma (`{Binding PropName}`).
    *   `FontSize`: Betűméret (pl. `18`, `Large`).
    *   `FontAttributes`: `Bold`, `Italic`.
    *   `HorizontalTextAlignment`: `Center`, `Start`, `End`.
*   **Entry**: Szövegmező (input).
    *   `Text`: Kétirányú kötés (`{Binding PropName}`).
    *   `Placeholder`: Súgószöveg.
    *   `Keyboard`: `Numeric`, `Email`, `Text`.
*   **Button**: Gomb.
    *   `Text`: Felirat.
    *   `Command`: Eseménykezelés ViewModel-en keresztül (`{Binding SaveCommand}`).
    *   `BackgroundColor`, `TextColor`.
*   **Image**: Kép.
    *   `Source`: Kép forrása (fájlnév vagy URL).
    *   `Aspect`: `AspectFit` (arányos), `AspectFill` (kitöltés).
    *   `HeightRequest`, `WidthRequest`: Méretek.
*   **CheckBox**: Jelölőnégyzet.
    *   `IsChecked`: `{Binding IsDone}` (bool).

### 2.3 Listázás (CollectionView)

Ez a modern lista vezérlő.

```xml
<!-- ItemsSource: A lista forrása (ObservableCollection) -->
<!-- SelectedItem: A kiválasztott elem kötése -->
<!-- SelectionMode: Single (egy elem választható) -->
<CollectionView ItemsSource="{Binding Items}" 
                SelectedItem="{Binding SelectedItem}" 
                SelectionMode="Single">
    
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <!-- Egy lista elem kinézete -->
            <Border Stroke="Gray" StrokeShape="RoundRectangle 10" Margin="5" Padding="10">
                <Grid ColumnDefinitions="Auto, *">
                    <!-- Bal oldalon kép -->
                    <Image Source="{Binding ImageSource}" WidthRequest="50" HeightRequest="50" Grid.Column="0"/>
                    
                    <!-- Jobb oldalon szövegek -->
                    <VerticalStackLayout Grid.Column="1" Margin="10,0,0,0">
                        <Label Text="{Binding Name}" FontAttributes="Bold" FontSize="16"/>
                        <Label Text="{Binding Quantity, StringFormat='Darab: {0}'}" TextColor="Gray"/>
                    </VerticalStackLayout>
                </Grid>
            </Border>
        </DataTemplate>
    </CollectionView.ItemTemplate>
    
</CollectionView>
```

## 3. Navigáció és Adatátadás (A legegyszerűbb módszer)

Ez a `ZHGyak`-ban látott módszer a leggyorsabb: az objektumot egyben küldjük át paraméterként a Shell navigációval.

### 3.1 Navigáció oda (Main -> Add)
```csharp
// AppShell.xaml.cs-ben regisztráció (CSAK EGYSZER KELL):
Routing.RegisterRoute("newitem", typeof(AddPage));

// ViewModel-ben hívás (pl. Hozzáadás gomb):
await Shell.Current.GoToAsync("newitem");
```

### 3.2 Navigáció vissza és Adatküldés (Add -> Main)
Az új (vagy szerkesztett) elemet visszaküldjük a hívó félnek.

```csharp
// AddPageViewModel.cs - Mentés gomb parancsa
private async void Save()
{
    // Paraméterek összeállítása
    var param = new ShellNavigationQueryParameters()
    {
        {"newitem", Item } // Kulcs-érték pár (az Item maga az objektum)
    };
    
    // ".." jelenti, hogy visszalépünk az előző oldalra, és visszük a paramétert
    await Shell.Current.GoToAsync("..", param);
}
```

### 3.3 Adat fogadása (MainPageViewModel)
A fogadó oldalon a `QueryProperty` attribútummal kötjük be a paramétert egy property-be.

```csharp
// MainPageViewModel.cs
// A "NewItem" property kapja meg a "newitem" kulcsú adatot
[QueryProperty(nameof(NewItem), "newitem")] 
public class MainPageViewModel
{
    public ObservableCollection<ShopItem> Items { get; set; }

    // Trükkös setter: amint megérkezik az adat a navigációból, 
    // a rendszer meghívja ezt a settert.
    public ShopItem NewItem 
    { 
        set 
        {
            if (value != null)
            {
                Items.Add(value); 
                Save(); // Azonnali mentés fájlba
            }
        } 
    }
}
```

## 4. Adatkezelés (Fájlba írás - ZHGyak módszer)

Ha nem kérnek explicit adatbázist, a szöveges fájlba mentés a legegyszerűbb (`System.IO`).

```csharp
private readonly string path = Path.Combine(FileSystem.AppDataDirectory, "adat.txt");

// Mentés
public void Save()
{
    StringBuilder sb = new StringBuilder();
    foreach (var item in Items)
    {
        // A Model ToString()-je csv formátumot (pl. "Alma;5;kep.png") ad vissza
        sb.AppendLine(item.ToString()); 
    }
    File.WriteAllText(path, sb.ToString());
}

// Betöltés
public void Load()
{
    if (File.Exists(path))
    {
        var data = File.ReadAllLines(path);
        Items.Clear();
        foreach (var line in data)
        {
            // A Model Parse metódusa feldolgozza a sort és visszaad egy objektumot
            Items.Add(ShopItem.Parse(line)); 
        }
    }
}
```
*Tipp: A `Load()` metódust a `MainPage.xaml.cs` `OnAppearing` metódusában érdemes meghívni: `(BindingContext as MainPageViewModel).Load();`.*

## 5. Képkiválasztás és Kamera (MediaPicker)

Két fő funkció: kép kiválasztása a galériából, vagy új kép készítése a kamerával.

```csharp
// AddPageViewModel.cs

// 1. Kép kiválasztása a galériából (ZHGyak módszer)
private async void PickImage()
{
    FileResult? image = await MediaPicker.Default.PickPhotoAsync();
    if (image != null)
    {
        // Elmentjük az elérési utat a modellbe
        // A felületen az Image Source-ja ehhez a propertyhez van kötve, így egyből látszik
        this.Item.ImageSource = image.FullPath;
    }
}

// 2. Új kép készítése a kamerával
private async void TakePhoto()
{
    if (MediaPicker.Default.IsCaptureSupported) // Ellenőrizni kell
    {
        FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo != null)
        {
            this.Item.ImageSource = photo.FullPath;
        }
    }
}
```

## 6. Megosztás (Share)

Elem megosztása a rendszer alapértelmezett megosztójával (pl. email, messenger).

```csharp
private async void ShareItem()
{
    if (this.SelectedItem != null)
    {
        await Share.Default.RequestAsync(new ShareTextRequest()
        {
            Title = "Elem megosztása",
            Text = this.SelectedItem.ToString() // Vagy bármilyen szöveg
        });
    }
}
```

## 7. Szenzorok és Engedélyek

A szenzorokhoz Androidon engedélyeket kell kérni a `Platforms/Android/AndroidManifest.xml` fájlban.

### 7.1 AndroidManifest.xml beállítások
Keresd meg a `Platforms` -> `Android` mappában az `AndroidManifest.xml` fájlt, és add hozzá a `manifest` tag-en belülre (de az `application`-ön kívülre):

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application ...></application>
    
    <!-- Hálózat (Connectivity) -->
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.INTERNET" />
    
    <!-- GPS (Geolocation) - EZT GYAKRAN KÉRIK! -->
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    
    <!-- Kamera (opcionális, MediaPicker-hez néha kellhet régebbi androidon) -->
    <uses-permission android:name="android.permission.CAMERA" />
</manifest>
```

### 7.2 Szenzorok használata (C# Kód)

Mindig ellenőrizd, hogy támogatott-e (`IsSupported`) és megy-e már (`IsMonitoring`).

**Gyorsulásmérő (Accelerometer)**
```csharp
if (Accelerometer.Default.IsSupported)
{
    if (!Accelerometer.Default.IsMonitoring)
    {
        Accelerometer.Default.ReadingChanged += (s, e) => 
        {
            // e.Reading.Acceleration.X, Y, Z értékek
            Console.WriteLine($"X: {e.Reading.Acceleration.X}");
        };
        Accelerometer.Default.Start(SensorSpeed.UI);
    }
    else 
    {
        Accelerometer.Default.Stop(); // Leállítás
    }
}
```

**Iránytű (Compass)**
```csharp
if (Compass.Default.IsSupported)
{
    Compass.Default.ReadingChanged += (s, e) => 
    {
        // e.Reading.HeadingMagneticNorth (fok, merre van Észak)
        // Pl. nyíl forgatása: ArrowImage.Rotation = -e.Reading.HeadingMagneticNorth;
    };
    Compass.Default.Start(SensorSpeed.UI);
}
```

**GPS Pozíció (Geolocation)** - *Aszinkron hívás!*
```csharp
try 
{
    // Utolsó ismert pozíció (gyorsabb)
    var location = await Geolocation.Default.GetLastKnownLocationAsync();
    
    // Vagy aktuális pozíció lekérése (pontosabb, de lassabb)
    // var location = await Geolocation.Default.GetLocationAsync();

    if (location != null)
    {
        double lat = location.Latitude;
        double lon = location.Longitude;
    }
}
catch (Exception ex)
{
    // Kezeld le, ha nincs engedély vagy nem sikerült a lekérés
    await DisplayAlert("Hiba", "Nem sikerült a helymeghatározás", "OK");
}
```

**Hálózat (Connectivity)**
```csharp
if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
{
    // Van internet
}
else
{
    // Nincs internet
}
```

**Akkumulátor (Battery)**
```csharp
var level = Battery.Default.ChargeLevel; // 0.0 és 1.0 közötti szám
var state = Battery.Default.State; // Charging, Discharging, Full, NotCharging
```
