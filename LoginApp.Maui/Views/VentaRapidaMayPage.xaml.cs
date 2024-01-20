using LoginApp.Maui.ViewModels;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
//using static UIKit.UIGestureRecognizer;
//using Windows.UI.Xaml;
//using ZXing;
//using ZXing.Mobile;

namespace LoginApp.Maui.Views;

public partial class VentaRapidaMayPage : ContentPage
{
    public ObservableCollection<ProductoMayViewModel> listaProductos = new ObservableCollection<ProductoMayViewModel>();

    public class Producto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Cantidad { get; set; }
    }

    public class Totalgen
    {
        public string total { get; set; }
    }
    public VentaRapidaMayPage()
    {
        InitializeComponent();
         BindingContext = this;
        montoPagadoEntry.TextChanged += MontoPagadoEntry_TextChanged;

        // Suscribirse al evento ProductoSeleccionado de BuscarProductosPage
        tipoDocumentoPicker.SelectedIndexChanged += TipoDocumentoPicker_SelectedIndexChanged;
        // Establece el índice inicial (0 para DNI)
        tipoDocumentoPicker.SelectedIndex = 0;

        // Asegúrate de que el evento se ejecute al inicio para mostrar la caja de texto correcta
        TipoDocumentoPicker_SelectedIndexChanged(null, EventArgs.Empty);
        MessagingCenter.Subscribe<ScanBarCodeMayPage, ProductoMayViewModel>(this, "scan", (sender, producto) =>
        {
            Debug.WriteLine($"Recibido Producto Seleccionado en VentaRapidaPage: {producto.Codigo}");

            CargarPreciosDesdeAPIScan(producto.Codigo);

            //Navigation.PushAsync(new ScanBarCodePage());
            //var currentPage = Navigation.NavigationStack.LastOrDefault();
            //// Eliminar la página anterior de la pila de navegación
            //if (currentPage != null)
            //{
            //    Navigation.RemovePage(currentPage);
            //}
            //base.OnAppearing();

            //DisplayAlert("Codigo", producto.Codigo, "Aceptar");
        });
        MessagingCenter.Subscribe<BuscarProductosMayPage, ProductoMayViewModel>(this, "ProductoSeleccionado", (sender, producto) =>
        {
            Debug.WriteLine($"Recibido Producto Seleccionado en VentaRapidaPage: {producto.Codigo}");

             CargarPreciosDesdeAPI(producto.Codigo,producto.Nombre);
            //bool isOddRow = true;
            //foreach (var item in listaProductos)
            //{
            //    item.OddRow = isOddRow;
            //    item.EvenRow = !isOddRow;
            //    isOddRow = !isOddRow;
            //}
        });


        listaProductosListView.ItemsSource = listaProductos;
        // Suscribirse al evento CambiosRealizados en cada ProductoViewModel de la lista
        listaProductos.CollectionChanged += (sender, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (ProductoMayViewModel nuevoProducto in e.NewItems)
                {
                    nuevoProducto.PropertyChanged += ProductoViewModel_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ProductoMayViewModel viejoProducto in e.OldItems)
                {
                    viejoProducto.PropertyChanged -= ProductoViewModel_PropertyChanged;
                }
            }

            ActualizarTotalPreciosGenerales();
        };
    }


    private async void CargarPreciosDesdeAPIScan(string codigoProducto)
    {
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // URL de tu API con el código del precio
                string apiUrl = $"https://ventarapida-dms.000webhostapp.com/venta?codprecio={codigoProducto}";

                // Realizar la solicitud GET
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Obtener el contenido de la respuesta como cadena JSON
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    // Deserializar la cadena JSON a una lista de precios
                    var precios = JsonConvert.DeserializeObject<List<Precio>>(jsonContent);
                    decimal PrecioMin = 0;
                    decimal PrecioMay = 0;
                    string cod_listPr1 = "";
                    string cod_listPr2 = "";
                    // Agregar los precios a la lista de productos
                    foreach (var precio in precios)
                    {
                        if (precio.DESC_CORTO == "P.MIN")
                        {
                            PrecioMin = Math.Round(decimal.Parse(precio.PRE_ACT), 2);
                            cod_listPr1 = precio.COD_LISPRE;
                        }
                        else if (precio.DESC_CORTO == "P.MAY")
                        {
                            PrecioMay = Math.Round(decimal.Parse(precio.PRE_ACT), 2);
                            cod_listPr2 = precio.COD_LISPRE;
                        }
                    }

                    listaProductos.Add(new ProductoMayViewModel
                    {
                        Codigo = codigoProducto,
                        Nombre = precios[0].ADESCRI,
                        Precio1 = PrecioMin,
                        Precio2 = PrecioMay,
                        Precio1Seleccionado = false,
                        Precio2Seleccionado = true,
                        Cantidad = 1,
                        TotalPrecios = PrecioMin,
                        COD_LISPRE1 = cod_listPr1,
                        COD_LISPRE2 = cod_listPr2,

                    }); bool isOddRow = true;
                    foreach (var producto in listaProductos)
                    {
                        producto.RowColor = isOddRow ? Colors.Gray : Colors.White;
                        isOddRow = !isOddRow;
                    }
                    ActualizarTotalCantidades(); // Actualizar el total después de agregar un producto
                }
                else
                {
                    Console.WriteLine($"Error en la solicitud: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }
    }

    // Método para manejar el cambio en el monto pagado
    private void MontoPagadoEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Verificar si el texto es un número válido
        if (decimal.TryParse(e.NewTextValue, out decimal montoPagado))
        {
            // Calcular el vuelto
            decimal vuelto = TotalPreciosSeleccionadosGeneral - montoPagado;
            if(vuelto>0)
            {

                vueltoLabel.Text = $"Pago Restante: {vuelto:N2}";
            }
            else
            {

                vueltoLabel.Text = $"Vuelto: {vuelto:N2}";
            }
            // Mostrar el vuelto en el label correspondiente
        }
        else
        {
            // Si el texto no es un número válido, mostrar un mensaje de error o realizar la acción apropiada
            vueltoLabel.Text = "Monto no válido";
        }
    }
    private void ProductoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductoMayViewModel.TotalPrecios))
        {
            ActualizarTotalPreciosGenerales();
        }else if(e.PropertyName == nameof(ProductoMayViewModel.Precio1Seleccionado))
        {
            ActualizarTotalPreciosGenerales();
        }else if(e.PropertyName == nameof(ProductoMayViewModel.Precio2Seleccionado))
        {
            ActualizarTotalPreciosGenerales();
        }else if (e.PropertyName == nameof(ProductoMayViewModel.Cantidad))
        {
            ActualizarTotalPreciosGenerales();
        }else if (e.PropertyName == nameof(ProductoMayViewModel.PrecioTotal))
        {
            ActualizarTotalPreciosGenerales();
        }
    }

    private void ActualizarTotalPreciosGenerales()
    {
        OnPropertyChanged(nameof(TotalPreciosSeleccionadosGeneral));
        // Aquí puedes realizar cualquier otra actualización necesaria
        // También podrías enviar un mensaje a través de MessagingCenter si es necesario
        labelTotal.Text = $"Total: S/. {TotalPreciosSeleccionadosGeneral}";
        labelTotal2.Text = $"S/. {TotalPreciosSeleccionadosGeneral}";
    }
    //private decimal _totalPreciosSeleccionadosGeneral;

    //public decimal TotalPreciosSeleccionadosGeneral
    //{
    //    get { return _totalPreciosSeleccionadosGeneral; }
    //    set
    //    {
    //        if (_totalPreciosSeleccionadosGeneral != value)
    //        {
    //            _totalPreciosSeleccionadosGeneral = value;
    //            OnPropertyChanged(nameof(TotalPreciosSeleccionadosGeneral));
    //        }
    //    }
    //}

    //private void ActualizarTotalPrecios()
    //{
    //    // Calcular el total general y notificar a la interfaz de usuario
    //    TotalPreciosSeleccionadosGeneral = listaProductos.Sum(producto => producto.TotalPrecios);

    //    // Notificar que la propiedad TotalPreciosSeleccionadosGeneral ha cambiado
    //    OnPropertyChanged(nameof(TotalPreciosSeleccionadosGeneral));
    //}

    private void TipoDocumentoPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Obtén el índice seleccionado
        int selectedIndex = tipoDocumentoPicker.SelectedIndex;

        // Oculta ambas cajas de texto al principio
        dniEntry.IsVisible = false;
        RucEntry.IsVisible = false;

        // Muestra la caja de texto correspondiente según la selección
        if (selectedIndex == 0) // DNI seleccionado
        {
            RucEntry.IsVisible = false;
            dniEntry.IsVisible = true;
        }
        else if (selectedIndex == 1) // RUC seleccionado
        {
            dniEntry.IsVisible = false;
            RucEntry.IsVisible = true;
        }
    }

    private void AgregarProducto_Clicked(object sender, EventArgs e)

    {
        // Agregar más productos según tus necesidades
        //listaProductos.Add(new ProductoViewModel { Nombre = "Nuevo Producto", Precio1 = "$30", Precio2 = "$35", Precio1Seleccionado = false, Precio2Seleccionado = false, Cantidad =1 });
        //ActualizarTotalCantidades(); // Actualizar el total después de agregar un producto
        // Abrir la página de búsqueda de productos
        Navigation.PushAsync(new BuscarProductosMayPage());
    }
    // Propiedad para la suma de cantidades
    private decimal totalCantidades;

    public decimal TotalCantidades
    {
        get { return totalCantidades; }
        set
        {
            if (totalCantidades != value)
            {
                totalCantidades = value;
                OnPropertyChanged(nameof(TotalCantidades));
            }
        }
    }

    public decimal TotalPreciosSeleccionadosGeneral
    {
        get
        {
            return listaProductos.Sum(producto => producto.PrecioTotal);
        }
    }

    public void ActualizarTotalPrecios()
    {
        OnPropertyChanged(nameof(TotalPreciosSeleccionadosGeneral));
    }
    private void EscanearCodigoBarras_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ScanBarCodeMayPage());
        //var scanner = new ZBar.ZBarScanner();
        //var scanPage = new ZBarScannerPage();

        //scanPage.OnScanResult += (result) =>
        //{
        //    // Manejar el resultado del escaneo, por ejemplo, mostrar en una alerta
        //    DisplayAlert("Código escaneado", result.Text, "OK");
        //};

        //await Navigation.PushAsync(scanPage);
    }
    //public string MensajeTotal => $"Total: {TotalCantidades}";
    //// Método para actualizar el total de cantidades
    public void ActualizarTotalCantidades()
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            TotalCantidades = listaProductos.Sum(producto => producto.TotalPrecios);
            OnPropertyChanged(nameof(TotalCantidades));  // Forzar actualización de la interfaz de usuario
            labelTotal.Text = $"Total: S/. {TotalPreciosSeleccionadosGeneral}";
            labelTotal2.Text = $"S/. {TotalPreciosSeleccionadosGeneral}";

        });
    }
    private void CancelarPopupFinalizaVenta(object sender, EventArgs e)
    {
        CancelarPoputFinaliza();
    }
    private void CancelarPoputFinaliza()
    {
        popupViewFinalizaVenta.IsVisible = false;
        finalizarVentaButton.IsVisible = true;
        btnAgregar.IsVisible = true;
        labelTotal.IsVisible = true;
        btnScan.IsVisible = true;

        montoPagadoEntry.Text = "";
    }

    private void FinalizarVenta_Clicked(object sender, EventArgs e)
    {
        string exiteproducto = "0";
        foreach (var detalle in listaProductos)
        {
            exiteproducto="1";
            string estadoseleccion = "0";
            if (detalle.Precio1Seleccionado == true)
            { estadoseleccion = "1"; }
            if (detalle.Precio2Seleccionado == true) { estadoseleccion = "1"; }
            if(estadoseleccion == "0")
            {
                string mensaje = "Por favor Seleccionar Precio del producto antes de continuar.";
                MostrarMensaje(mensaje);
                return;
            }
        }
        if (exiteproducto == "0") {
            string mensaje2 = "Por favor ingresar productos antes de continuar.";
            MostrarMensaje(mensaje2);
            return;
        }
            // Realizar acciones finales de la venta
            // Puedes acceder a la lista completa de productos en 'listaProductos'

            // Mostrar la "ventana emergente"
            popupViewFinalizaVenta.IsVisible = true;
        //listaProductosListView.IsVisible = false;
        // Para ocultar el botón
        finalizarVentaButton.IsVisible = false;
        btnAgregar.IsVisible = false;
        btnScan.IsVisible = false;

        labelTotal.IsVisible = false;
    }

    private void EliminarCommand(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            if (button.BindingContext is ProductoMayViewModel producto)
            {
                listaProductos.Remove(producto);
                ActualizarTotalCantidades(); // Actualizar el total después de agregar un producto

            }
        }
    }
    // Implementación de INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Desuscribirse del evento al desaparecer la página
        //MessagingCenter.Unsubscribe<VentaRapidaPage, Producto>(this, VentaRapidaPage.AlertSignalName);
    }

    private void EliminarMenuItem_Clicked(object sender, EventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.CommandParameter is ProductoMayViewModel producto)
        {
            // Aquí debes eliminar el producto de tu listaProductos
            listaProductos.Remove(producto);
            ActualizarTotalCantidades(); // Actualiza el total después de eliminar un producto
        }
    }

    private void LimpiarVenta() 
    {
         nombreEntry.Text="";
        direccionEntry.Text = "";
       dniEntry.Text = "";
       RucEntry.Text = "";
        vueltoLabel.Text = "";

        montoPagadoEntry.Text = "";
        // Limpiar la lista después de guardar
        listaProductos.Clear();
    }
    private async void CerrarPopupFinalizaVenta(object sender, EventArgs e)
    {
        // Ocultar la "ventana emergente"
        popupViewFinalizaVenta.IsVisible = false;

        // Para mostrar el botón

        labelTotal.IsVisible = true;
        btnScan.IsVisible = true;
        btnAgregar.IsVisible = true;
        finalizarVentaButton.IsVisible = true;
        string tipoDocumentoSeleccionado = tipoDocumentoPicker.SelectedItem.ToString();

        String pNombre = nombreEntry.Text;
        String pDireccion = direccionEntry.Text;
        String pdni = dniEntry.Text;
        String pRuc = RucEntry.Text;

        String MontoIngresado = montoPagadoEntry.Text;
        if (MontoIngresado == "" || MontoIngresado == null)
        {
            MostrarMensaje("Monto no ingresado");
            return;
        }

        if (tipoDocumentoSeleccionado == "DNI")
        {
            if (pdni == "" || pdni==null) {
                string mensajedn = "Campo DNI vacio, por favor completar.";
                Console.WriteLine(mensajedn);
                MostrarMensaje(mensajedn);
                return; }
            pRuc = "";
        }
        else if (tipoDocumentoSeleccionado == "RUC") { 
            
            pdni = "";
            if (pRuc == "" || pRuc == null) {
                string mensajeruc = "Campo RUC vacio, por favor completar.";
                Console.WriteLine(mensajeruc);
                MostrarMensaje(mensajeruc);
                return; }
        }

        VentaData ventaData = new VentaData
        {
            Faccab = new FACCAB
            {
                CFTD = App.user.TPD.ToString(),               // Tipo de documento (DNI, RUC, etc.)
                CFNUMSER = App.user.serie_doc.ToString(),          // Número de serie
                CFVENDE = App.user.Id.ToString(),            // Código de vendedor
                CFPUNVEN = "03",           // Punto de venta
                CFCODCLI = pdni,  // Código de cliente (DNI)
                CFNOMBRE = pNombre,// Nombre del cliente
                CFDIRECC = pDireccion, // Dirección del cliente
                CFRUC = pRuc,      // RUC del cliente
                CFALMA = "03",             // Código de almacén
                CFIMPORTE = TotalPreciosSeleccionadosGeneral,         // Importe total de la venta
                CFFORVEN = "01",           // Forma de venta (al contado)
                CFSALDO = 0.0m,            // Saldo (puede ser 0 si es al contado)
                CFTIPCAM = 3.5m,           // Tipo de cambio
                CFCODMON = "MN",           // Código de moneda
                CFESTADO = "V",            // Estado de la venta
                CFUSER = App.user.Id.ToString(),     // Código de usuario
                CFIMPTARMN = TotalPreciosSeleccionadosGeneral,        // Importe total en moneda nacional
                CFIGV = ( TotalPreciosSeleccionadosGeneral - (TotalPreciosSeleccionadosGeneral / 1.18m)),             // IGV (calcular como TOTAL / 1.18)
                TarjetaVisaSoles = 0.0m,   // Monto pagado con tarjeta Visa
                TarjetaMastercardSoles = 0.0m, // Monto pagado con tarjeta Mastercard,
                Token = App.user.token,
                MontoPagado= decimal.Parse(MontoIngresado),
            },
            FacdetList = new List<FACDET>()
        };

        int Secuencia = 0;
        foreach (var detalle in listaProductos)
        {
            decimal PrecioUnida = 0m;
            string cod_lispre = "";
            if (detalle.Precio1Seleccionado == true) 
            {
                PrecioUnida = detalle.Precio1;
                cod_lispre = detalle.COD_LISPRE1;
            }
            else if (detalle.Precio2Seleccionado == true) 
            {
                PrecioUnida = detalle.Precio2;

                cod_lispre = detalle.COD_LISPRE2;
            }


                Secuencia++;
            // Aquí debes llenar los datos de detalle según tu lógica
            FACDET facdet = new FACDET
            {
                DFTD = "NV",                // Tipo de detalle (nota de venta)
                DFNUMSER = "006",           // Número de serie
                DFSECUEN = Secuencia, // Secuencia del detalle (debe obtenerse de algún lugar)
                DFCODIGO = detalle.Codigo, // Código del producto
                DFCANTID = detalle.Cantidad, // Cantidad
                DFPREC_VEN = PrecioUnida, // Precio de venta por unidad

                
                DFDESCTO = 0.0m,            // Descuento
                DFIGV = 1.8m,               // IGV del detalle
                DFIGVPOR = ( detalle.PrecioTotal - (detalle.PrecioTotal / 1.8m)),           // Porcentaje de IGV
                DFIMPMN = detalle.PrecioTotal,            // Importe en moneda nacional
                DFUNIDAD = "UND",           // Unidad de medida
                DFESTADO = "V",             // Estado del detalle
                DFALMA = "03",              // Código de almacén
                DFDESCRI = detalle.Nombre, // Descripción del producto
                DFDESCLI = 0.0m,            // Descuento del cliente
                DFCODLIS = cod_lispre        // Código de lista de preci

                // ... (llenar el resto de las propiedades según tu modelo)
            };

            ventaData.FacdetList.Add(facdet);
        }

        // Convertir el objeto a JSON
        string json = JsonConvert.SerializeObject(ventaData);

        // Enviar los datos a través de la API
        //await EnviarDatosAPI(json);
        // Enviar los datos a través de la API
        string respuestaJson = await EnviarDatosAPI(json);

        // Deserializar la respuesta JSON
        var respuesta = JsonConvert.DeserializeObject<RespuestaVenta>(respuestaJson);
        string mensaje = $"Tipo de documento: {respuesta.result.tpd}\nSerie: {respuesta.result.serie_doc}\nNúmero de documento: {respuesta.result.numero_doc}";
        Console.WriteLine(mensaje);
        MostrarMensaje(mensaje);

        if (respuesta.status == "ok")
        {
            string pdfUrl = $"https://ventarapida-dms.000webhostapp.com/crearpdf?tpd={respuesta.result.tpd}&serieDoc={respuesta.result.serie_doc}&numeroDoc={respuesta.result.numero_doc}";

            // Download the PDF file to a local path
            string localFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "downloaded.pdf");

            using (HttpClient client = new HttpClient())
            {
                byte[] pdfData = await client.GetByteArrayAsync(pdfUrl);
                File.WriteAllBytes(localFilePath, pdfData);
            }

            // Open the local PDF file
            Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(new Microsoft.Maui.ApplicationModel.OpenFileRequest
            {
                File = new Microsoft.Maui.Storage.ReadOnlyFile(localFilePath, "application/pdf")
            });


        }
        // Mostrar el mensaje con los datos obtenidos
        if (respuesta.status == "ok") 
        {
            LimpiarVenta();
        }
        CancelarPoputFinaliza();
    }
    private void MostrarMensaje(string mensaje)
    {
        // Utilizar DisplayAlert para mostrar el mensaje en el móvil
        App.Current.MainPage.DisplayAlert("Mensaje", mensaje, "Aceptar");
    }
    private async Task<string> EnviarDatosAPI(string jsonData)
    {
        string respuesta = null;

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // URL de tu API
                string apiUrl = "https://ventarapida-dms.000webhostapp.com/venta";

                // Configurar la solicitud HTTP
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Realizar la solicitud POST
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                // Manejar la respuesta (puedes verificar el código de estado, etc.)
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Datos enviados correctamente");
                    respuesta = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"Error en la solicitud: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }

        return respuesta;
    }

    private async void CargarPreciosDesdeAPI(string codigoProducto,string nombreProducto)
    {
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // URL de tu API con el código del precio
                string apiUrl = $"https://ventarapida-dms.000webhostapp.com/venta?codprecio={codigoProducto}";

                // Realizar la solicitud GET
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Obtener el contenido de la respuesta como cadena JSON
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    // Deserializar la cadena JSON a una lista de precios
                    var precios = JsonConvert.DeserializeObject<List<Precio>>(jsonContent);
                    decimal PrecioMin =0;
                    decimal PrecioMay = 0;
                    string cod_listPr1 = "";
                    string cod_listPr2 = "";
                    // Agregar los precios a la lista de productos
                    foreach (var precio in precios)
                    {
                        if(precio.DESC_CORTO== "P.MIN") { PrecioMin = Math.Round(decimal.Parse(precio.PRE_ACT), 2);
                            cod_listPr1 = precio.COD_LISPRE;
                        }
                        else if(precio.DESC_CORTO == "P.MAY") { PrecioMay = Math.Round(decimal.Parse(precio.PRE_ACT), 2);
                            cod_listPr2 = precio.COD_LISPRE;
                        }
                    }

                    listaProductos.Add(new ProductoMayViewModel
                    {   Codigo= codigoProducto,
                        Nombre = nombreProducto,
                        Precio1 = PrecioMin,
                        Precio2 = PrecioMay,
                        Precio1Seleccionado = false,
                        Precio2Seleccionado = true,
                        Cantidad = 1,
                        TotalPrecios = PrecioMin,
                        COD_LISPRE1= cod_listPr1,
                        COD_LISPRE2 = cod_listPr2,

                    });
                    bool isOddRow = true;
                    foreach (var producto in listaProductos)
                    {
                        producto.RowColor = isOddRow ? Colors.Gray : Colors.White;
                        isOddRow = !isOddRow;
                    }
                    ActualizarTotalCantidades(); // Actualizar el total después de agregar un producto
                }
                else
                {
                    Console.WriteLine($"Error en la solicitud: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }
    }
    //private decimal _precioTotal;

    //public decimal PrecioTotal
    //{
    //    get { return _precioTotal; }
    //    set
    //    {
    //        if (_precioTotal != value)
    //        {
    //            _precioTotal = value;
    //            OnPropertyChanged(nameof(PrecioTotal));
    //        }
    //    }
    //}
    // Declarar un evento que se disparará cuando haya cambios relevantes
    public event EventHandler CambiosRealizados;

    // Método para notificar que se han realizado cambios
    protected virtual void OnCambiosRealizados()
    {
        CambiosRealizados?.Invoke(this, EventArgs.Empty);
    }
    public class VentaData
    {
        public FACCAB Faccab { get; set; }
        public List<FACDET> FacdetList { get; set; }
    }
    public class FACCAB
    {
        public string CFTD { get; set; }
        public string CFNUMSER { get; set; }
        public string CFNUMDOC { get; set; }
        public DateTime CFFECDOC { get; set; }
        public DateTime CFFECVEN { get; set; }
        public string CFVENDE { get; set; }
        public string CFPUNVEN { get; set; }
        public string CFCODCLI { get; set; }
        public string CFNOMBRE { get; set; }
        public string CFDIRECC { get; set; }
        public string CFRUC { get; set; }
        public string CFALMA { get; set; }
        public decimal CFIMPORTE { get; set; }
        public string CFFORVEN { get; set; }
        public decimal CFSALDO { get; set; }
        public decimal CFTIPCAM { get; set; }
        public string CFCODMON { get; set; }
        public string CFESTADO { get; set; }
        public string CFUSER { get; set; }
        public decimal CFIMPTARMN { get; set; }
        public decimal CFIGV { get; set; }
        public decimal TarjetaVisaSoles { get; set; }
        public decimal TarjetaMastercardSoles { get; set; }

        public string Token { get; set; }

        public decimal MontoPagado { get; set; }
    }
    public class FACDET
    {
        public string DFTD { get; set; }
        public string DFNUMSER { get; set; }
        public string DFNUMDOC { get; set; }
        public int DFSECUEN { get; set; }
        public string DFCODIGO { get; set; }
        public int DFCANTID { get; set; }
        public decimal DFPREC_VEN { get; set; }
        public decimal DFDESCTO { get; set; }
        public decimal DFIGV { get; set; }
        public decimal DFIGVPOR { get; set; }
        public decimal DFIMPMN { get; set; }
        public string DFUNIDAD { get; set; }
        public string DFESTADO { get; set; }
        public string DFALMA { get; set; }
        public string DFDESCRI { get; set; }
        public decimal DFDESCLI { get; set; }
        public string DFCODLIS { get; set; }
    }
    public class RespuestaVenta
    {
        public string status { get; set; }
        public ResultVenta result { get; set; }
    }
    public class ResultVenta
    {
        public string mensaje { get; set; }
        public string tpd { get; set; }
        public string serie_doc { get; set; }
        public string numero_doc { get; set; }
    }
    public class Precio
    {
        public string COD_LISPRE { get; set; }
        public string PRE_ACT { get; set; }
        public string DESC_CORTO { get; set; }
        public string ADESCRI { get; set; }
    }
    

}
// ViewModel para representar un producto en la lista
//public class ProductoViewModel
//{
//    public string Nombre { get; set; }
//    public string Precio1 { get; set; }
//    public string Precio2 { get; set; }
//    public bool Precio1Seleccionado { get; set; }
//    public bool Precio2Seleccionado { get; set; }
//}