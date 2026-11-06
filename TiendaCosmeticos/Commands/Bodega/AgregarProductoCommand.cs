
using System;
using System.Globalization;
using TiendaCosmeticos.Commands.Bodega;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Bodega
{
    public class AgregarProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly BodegaServices _bodegaServices;

        public AgregarProductoCommand(
            IConsoleWriter writer,
            BodegaServices bodegaServices)
        {
            _writer = writer;
            _bodegaServices = bodegaServices;
        }

        public void Execute()
        {
            string nombre = CapturarNombre();
            string marca = CapturarMarca();
            CategoriaProductos categoria = CapturarCategoria();
            decimal precio = CapturarPrecio();
            int stock = CapturarStock();
            DateTime fecha = CapturarFecha();

            bool continuar = true;

            while (continuar)
            {
                MostrarResumen(nombre, marca, categoria, precio, stock, fecha);

                _writer.DisplayMenu(
                    "¿Que desea hacer?",
                    "Confirmar y guardar",
                    "Editar un campo",
                    "Cancelar registro");

                string opcion = new Validator("Seleccione (1-3): ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 3)
                    .ReadAndValidate();

                switch (opcion)
                {
                    case "1":
                        GuardarProducto(
                            nombre, marca, categoria, precio, stock, fecha);
                        continuar = false;
                        break;

                    case "2":
                        EditarCampo(
                            ref nombre,
                            ref marca,
                            ref categoria,
                            ref precio,
                            ref stock,
                            ref fecha);
                        break;

                    case "3":
                        _writer.ShowMessage("Registro cancelado. No se guardaron cambios.");
                        continuar = false;
                        break;
                }
            }

            _writer.WaitForKey();
        }

        private string CapturarNombre()
        {
            return new Validator("Nombre del producto: ")
                .NotEmpty()
                .MaxLength(50)
                .ReadAndValidate().Trim();
        }

        private string CapturarMarca()
        {
            return new Validator("Marca del producto: ")
                .NotEmpty()
                .MaxLength(40)
                .ReadAndValidate().Trim();
        }

        private CategoriaProductos CapturarCategoria()
        {
            _writer.DisplayMenu(
                "Seleccione la categoria:",
                "Maquillaje",
                "Cuidado de piel",
                "Cuidado del cabello",
                "Fragancias",
                "Accesorios",
                "Otros");

            string opcion = new Validator("Categoria (1-6): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 6)
                .ReadAndValidate();

            return (CategoriaProductos)(int.Parse(opcion) - 1);
        }

        private decimal CapturarPrecio()
        {
            string valor = new Validator("Precio: ")
                .NotEmpty()
                .DecimalPositivo()
                .ReadAndValidate();

            return decimal.Parse(
                valor.Trim().Replace(',', '.'),
                CultureInfo.InvariantCulture);
        }

        private int CapturarStock()
        {
            string valor = new Validator("Stock inicial: ")
                .NotEmpty()
                .EnteroNoNegativo()
                .ReadAndValidate();

            return int.Parse(valor.Trim());
        }

        private DateTime CapturarFecha()
        {
            string valor = new Validator("Fecha de vencimiento (dd/MM/yyyy): ")
                .NotEmpty()
                .FormatoFecha()
                .FechaNoVencida()
                .ReadAndValidate();

            return DateTime.ParseExact(
                valor.Trim(),
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture);
        }

        private void MostrarResumen(
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fecha)
        {
            _writer.ShowHeader("RESUMEN DEL PRODUCTO");

            _writer.ShowMessage($"Nombre: {nombre}");
            _writer.ShowMessage($"Marca: {marca}");
            _writer.ShowMessage($"Categoria: {categoria}");
            _writer.ShowMessage($"Precio: {precio:C}");
            _writer.ShowMessage($"Stock: {stock}");
            _writer.ShowMessage($"Vencimiento: {fecha:dd/MM/yyyy}");
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
                "Seleccione el campo que desea corregir:",
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
                case "1": nombre = CapturarNombre(); break;
                case "2": marca = CapturarMarca(); break;
                case "3": categoria = CapturarCategoria(); break;
                case "4": precio = CapturarPrecio(); break;
                case "5": stock = CapturarStock(); break;
                case "6": fecha = CapturarFecha(); break;
            }
        }

        private void GuardarProducto(
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fecha)
        {
            try
            {
                Producto producto = _bodegaServices.RegistrarProducto(
                    nombre, marca, categoria, precio, stock, fecha);

                _writer.ShowMessage(
                    $"\nProducto registrado correctamente. ID asignado: {producto.Id}");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
        }

        public void Undo()
        {
            // No se realiza una accion inversa automatica.
        }
    }
}
