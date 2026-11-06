
using System;
using System.Globalization;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Bodega
{
    public class EditarProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly BodegaServices _service;

        public EditarProductoCommand(
            IConsoleWriter writer,
            BodegaServices service)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Execute()
        {
            Producto? producto = BuscarProductoEditable();

            if (producto == null)
                return;

            string nombre = producto.Nombre;
            string marca = producto.Marca;
            CategoriaProductos categoria = producto.Categoria;
            decimal precio = producto.Precio;
            int stock = producto.Stock;
            DateTime fecha = producto.FechaVencimiento;

            bool continuar = true;

            while (continuar)
            {
                MostrarResumen(producto, nombre, marca, categoria,
                    precio, stock, fecha);

                _writer.DisplayMenu(
                    "¿Que desea hacer?",
                    "Guardar cambios",
                    "Editar otro campo",
                    "Cancelar");

                string opcion = new Validator("Seleccione (1-3): ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 3)
                    .ReadAndValidate();

                switch (opcion)
                {
                    case "1":
                        try
                        {
                            _service.EditarProducto(
                                producto.Id,
                                nombre,
                                marca,
                                categoria,
                                precio,
                                stock,
                                fecha);

                            _writer.ShowMessage(
                                "Producto actualizado correctamente.");
                            continuar = false;
                        }
                        catch (TiendaException ex)
                        {
                            _writer.ShowError(ex.Message);
                        }
                        break;

                    case "2":
                        EditarCampo(
                            ref nombre, ref marca, ref categoria,
                            ref precio, ref stock, ref fecha);
                        break;

                    case "3":
                        _writer.ShowMessage(
                            "Edicion cancelada. No se guardaron cambios.");
                        continuar = false;
                        break;
                }
            }

            _writer.WaitForKey();
        }

        private Producto? BuscarProductoEditable()
        {
            while (true)
            {
                string id = new Validator(
                    "Ingrese el ID del producto (0 para cancelar): ")
                    .NotEmpty()
                    .ReadAndValidate()
                    .Trim();

                if (id == "0")
                    return null;

                try
                {
                    Producto producto = _service.ObtenerPorId(id);

                    if (!producto.Estado)
                    {
                        throw new RegistroInactivoException(
                            "producto", producto.Id);
                    }

                    return producto;
                }
                catch (TiendaException ex)
                {
                    _writer.ShowError(ex.Message);
                    _writer.ShowMessage(
                        "Ingrese otro ID o escriba 0 para cancelar.");
                }
            }
        }

        private void EditarCampo(
            ref string nombre,
            ref string marca,
            ref CategoriaProductos categoria,
            ref decimal precio,
            ref int stock,
            ref DateTime fecha)
        {
            _writer.DisplayMenu(
                "Seleccione el campo que desea editar:",
                "Nombre",
                "Marca",
                "Categoria",
                "Precio",
                "Stock",
                "Fecha de vencimiento");

            string opcion = new Validator("Seleccione (1-6): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 6)
                .ReadAndValidate();

            switch (opcion)
            {
                case "1":
                    var nombreInput = new BodegaInput(
                        $"Nombre actual: {nombre}. Nuevo nombre (Enter conserva): ");
                    nombreInput.NotEmpty();
                    nombreInput.MaxLength(50);
                    nombre = nombreInput.ReadOptional(nombre).Trim();
                    break;

                case "2":
                    var marcaInput = new BodegaInput(
                        $"Marca actual: {marca}. Nueva marca (Enter conserva): ");
                    marcaInput.NotEmpty();
                    marcaInput.MaxLength(40);
                    marca = marcaInput.ReadOptional(marca).Trim();
                    break;

                case "3":
                    _writer.DisplayMenu(
                        "Seleccione la nueva categoria:",
                        "Maquillaje",
                        "Cuidado de piel",
                        "Cuidado del cabello",
                        "Fragancias",
                        "Accesorios",
                        "Otros");

                    var categoriaInput = new BodegaInput(
                        $"Categoria actual: {(int)categoria + 1}. Nueva categoria (Enter conserva): ");
                    categoriaInput.OnlyDigits();
                    categoriaInput.InputBetweenXandY(1, 6);

                    string nuevaCategoria = categoriaInput.ReadOptional(
                        ((int)categoria + 1).ToString());

                    categoria = (CategoriaProductos)(
                        int.Parse(nuevaCategoria) - 1);
                    break;

                case "4":
                    var precioInput = new BodegaInput(
                        $"Precio actual: {precio}. Nuevo precio (Enter conserva): ");
                    precioInput.DecimalPositivo();

                    string nuevoPrecio = precioInput.ReadOptional(
                        precio.ToString(CultureInfo.InvariantCulture));

                    precio = decimal.Parse(
                        nuevoPrecio.Trim().Replace(',', '.'),
                        CultureInfo.InvariantCulture);
                    break;

                case "5":
                    var stockInput = new BodegaInput(
                        $"Stock actual: {stock}. Nuevo stock (Enter conserva): ");
                    stockInput.EnteroNoNegativo();

                    string nuevoStock = stockInput.ReadOptional(
                        stock.ToString());

                    stock = int.Parse(nuevoStock.Trim());
                    break;

                case "6":
                    var fechaInput = new BodegaInput(
                        $"Fecha actual: {fecha:dd/MM/yyyy}. Nueva fecha (Enter conserva): ");
                    fechaInput.FormatoFecha();
                    fechaInput.FechaNoVencida();

                    string nuevaFecha = fechaInput.ReadOptional(
                        fecha.ToString("dd/MM/yyyy"));

                    fecha = DateTime.ParseExact(
                        nuevaFecha.Trim(),
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture);
                    break;
            }
        }

        private void MostrarResumen(
            Producto producto,
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fecha)
        {
            _writer.ShowHeader("REVISION DE CAMBIOS");

            _writer.ShowMessage($"ID: {producto.Id} (No editable)");
            _writer.ShowMessage($"Nombre: {nombre}");
            _writer.ShowMessage($"Marca: {marca}");
            _writer.ShowMessage($"Categoria: {categoria}");
            _writer.ShowMessage($"Precio: {precio:C}");
            _writer.ShowMessage($"Stock: {stock}");
            _writer.ShowMessage($"Vencimiento: {fecha:dd/MM/yyyy}");
        }

        public void Undo()
        {
            // No se realiza una accion inversa automatica.
        }
    }
}
