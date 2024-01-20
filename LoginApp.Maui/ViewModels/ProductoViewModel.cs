using LoginApp.Maui.Views;
using System.ComponentModel;
using System.Windows.Input;

namespace LoginApp.Maui.ViewModels;

public class ProductoViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public Color RowColor { get; set; } // Propiedad para el color de fondo;

    private decimal _totalPreciosGeneral;
    public decimal TotalPreciosGeneral
    {
        get { return _totalPreciosGeneral; }
        set
        {
            if (_totalPreciosGeneral != value)
            {
                _totalPreciosGeneral = value;
                OnPropertyChanged(nameof(TotalPreciosGeneral));
            }
        }
    }

    private decimal _totalPrecios;
    public decimal TotalPrecios
    {
        get { return _totalPrecios; }
        set
        {
            if (_totalPrecios != value)
            {
                _totalPrecios = value;
                OnPropertyChanged(nameof(TotalPrecios));
            }
        }
    }
    private decimal _totalPreciosSeleccionados;
    public decimal TotalPreciosSeleccionados
    {
        get { return _totalPreciosSeleccionados; }
        set
        {
            if (_totalPreciosSeleccionados != value)
            {
                _totalPreciosSeleccionados = value;
                OnPropertyChanged(nameof(TotalPreciosSeleccionados));
            }
        }
    }
    public string Nombre { get; set; }
    public string Codigo { get; set; }
    //public int Cantidad { get; set; }
    public decimal Precio1 { get; set; }
    public decimal Precio2 { get; set; }
    public string COD_LISPRE1 { get; set; }
    public string COD_LISPRE2 { get; set; }

    private int _cantidad;

    public int Cantidad
    {
        get { return _cantidad; }
        set
        {
            if (_cantidad != value)
            {
                _cantidad = value;
                OnPropertyChanged(nameof(Cantidad));
                CalcularPrecioTotal();
                OnPropertyChanged(nameof(PrecioTotal));
                //ActualizarTotalPreciosSeleccionados();
            }
        }
    }

    public ICommand RestarCantidadCommand { get; }
    public ICommand SumarCantidadCommand { get; }

    public ProductoViewModel()
    {
        // Inicializar comandos
        RestarCantidadCommand = new Command(RestarCantidad);
        SumarCantidadCommand = new Command(SumarCantidad);
        // Inicializar otras propiedades
        //TotalPreciosSeleccionados = ++PrecioTotal;
        Cantidad = 1;
    }

    private void ProductoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Manejar los cambios en propiedades específicas si es necesario
        if (e.PropertyName == nameof(Precio1) || e.PropertyName == nameof(Precio2) | e.PropertyName == nameof(Cantidad))
        {
            OnPropertyChanged(nameof(TotalPreciosSeleccionados));
        }
    }
    private void RestarCantidad()
    {
        Cantidad = Math.Max(1, Cantidad - 1);
        CalcularPrecioTotal();
    }

    private void SumarCantidad()
    {
        Cantidad++;
        CalcularPrecioTotal();
    }
    /*CHANGE checkbox*/
    private bool _precio1Seleccionado;
    public bool Precio1Seleccionado
    {
        get { return _precio1Seleccionado; }
        set
        {
            if (_precio1Seleccionado != value)
            {
                _precio1Seleccionado = value;
                if (value)
                {
                    // Si se marca Precio1, desmarcar Precio2
                    Precio2Seleccionado = false;
                }
                OnPropertyChanged(nameof(Precio1Seleccionado));

                CalcularPrecioTotal();
                // Actualizar la suma cuando se cambia la selección
                //ActualizarTotalPreciosSeleccionados();
            }
        }
    }
    private decimal _precioTotal;
    public decimal PrecioTotal
    {
        get { return _precioTotal; }
        set
        {
            if (_precioTotal != value)
            {
                _precioTotal = value;
                OnPropertyChanged(nameof(PrecioTotal));
            }
        }
    }

    private void CalcularPrecioTotal()
    {
        decimal precioUnitario = Precio1Seleccionado ? Precio1 : Precio2;
        PrecioTotal = precioUnitario * Cantidad;
        OnPropertyChanged(nameof(PrecioTotal));
    }
    //private void ActualizarTotalPrecios()
    //{
    //    // Calcular el total general y notificar a la interfaz de usuario
    //    TotalPreciosSeleccionadosGeneral = listaProductos.Sum(producto => producto.TotalPrecios);

    //    // Notificar que la propiedad TotalPreciosSeleccionadosGeneral ha cambiado
    //    OnPropertyChanged(nameof(TotalPreciosSeleccionadosGeneral));
    //}

    private bool _precio2Seleccionado;
    public bool Precio2Seleccionado
    {
        get { return _precio2Seleccionado; }
        set
        {
            if (_precio2Seleccionado != value)
            {
                _precio2Seleccionado = value;
                if (value)
                {
                    // Si se marca Precio2, desmarcar Precio1
                    Precio1Seleccionado = false;
                }
                OnPropertyChanged(nameof(Precio2Seleccionado));

                // Actualizar la suma cuando se cambia la selección

                CalcularPrecioTotal();
                //ActualizarTotalPreciosSeleccionados();
            }
        }
    }
    //private void ActualizarTotalPreciosSeleccionados()
    //{
        // Sumar los precios seleccionados y asignar al TotalPreciosSeleccionados
        //TotalPreciosSeleccionados = (Precio1Seleccionado ? Precio1 : 0) + (Precio2Seleccionado ? Precio2 : 0);

        // Notificar que la propiedad TotalPreciosSeleccionados ha cambiado
        //OnPropertyChanged(nameof(TotalPreciosSeleccionados));
        //MessagingCenter.Send(this, "TotalGen", TotalPreciosSeleccionados);

    //}


    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}