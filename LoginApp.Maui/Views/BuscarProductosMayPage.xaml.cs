using LoginApp.Maui.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;

namespace LoginApp.Maui.Views;


public partial class BuscarProductosMayPage : ContentPage
{
    private ObservableCollection<ProductoMayViewModel> resultados = new ObservableCollection<ProductoMayViewModel>();

    public BuscarProductosMayPage()
    {
        InitializeComponent();
    }
    //private void Buscar_Clicked(object sender, EventArgs e)
    //{
    //    // Simula una búsqueda de productos (puedes realizar una búsqueda real en tu aplicación)
    //    resultados.Clear();
    //    resultados.Add(new ProductoViewModel { Nombre = "Producto 1", Precio1 = 10, Precio2 = 15 });
    //    resultados.Add(new ProductoViewModel { Nombre = "Producto 2", Precio1 = 20, Precio2 = 25 });
    //    resultados.Add(new ProductoViewModel { Nombre = "Producto 3", Precio1 = 30, Precio2 = 35 });

    //    resultadosLista.ItemsSource = resultados;
    //}
    private async void Buscar_Clicked(object sender, EventArgs e)
    {
        string busqueda = busquedaEntry.Text;

        // Realizar la solicitud a la API
        using (HttpClient httpClient = new HttpClient())
        {
            //resultados.Clear();
            //resultados.Add(new ProductoViewModel { Nombre = "Producto 1", Precio1 = 11, Precio2 = 12 });
            //resultados.Add(new ProductoViewModel { Nombre = "Producto 2", Precio1 = 13, Precio2 = 14 });
            //resultados.Add(new ProductoViewModel { Nombre = "Producto 3", Precio1 = 15, Precio2 = 20 });
            // Ocultar el teclado
            //resultadosLista.ItemsSource = resultados;

            string apiUrl = $"https://ventarapida-dms.000webhostapp.com/venta?desprod={busqueda}";
            string jsonResult = await httpClient.GetStringAsync(apiUrl);

            // Deserializar los resultados y actualizar la ListView
            resultados = JsonConvert.DeserializeObject<ObservableCollection<ProductoMayViewModel>>(jsonResult);
            resultadosLista.ItemsSource = resultados;

        }
    }


    private void ResultadoSeleccionado(object sender, SelectedItemChangedEventArgs e)
    {
        // Puedes realizar acciones al seleccionar un resultado si es necesario
    }
    private void SeleccionarProducto_Clicked(object sender, EventArgs e)
    {
        ProductoMayViewModel productoSeleccionado = resultadosLista.SelectedItem as ProductoMayViewModel;

        if (productoSeleccionado != null)
        {
            Debug.WriteLine($"Enviando Producto Seleccionado: {productoSeleccionado.Nombre}");

            MessagingCenter.Send(this, "ProductoSeleccionado", productoSeleccionado);

            Navigation.PopAsync();
        }
    }
    public class Producto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Cantidad { get; set; }
    }

}