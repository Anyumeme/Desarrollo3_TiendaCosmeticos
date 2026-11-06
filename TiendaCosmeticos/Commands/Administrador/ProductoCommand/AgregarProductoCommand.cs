using System;
using System.Globalization;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ProductoCommand
{
    public class AgregarProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ProductoRepository _productoRepository;

        public AgregarProductoCommand(IConsoleWriter writer, ProductoRepository productoRepository)
        {
            _writer = writer;
            _productoRepository = productoRepository;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("REGISTRO DE NUEVO PRODUCTO");

            // 1. Nombre del producto
            string nombre = new Validator("Nombre del producto cosmetico: ")
                .NotEmpty()
                .MinLength(3)
                .MaxLength(50)
                .ReadAndValidate();

            // 2. Marca o fabricante
            string marca = new Validator("Marca o fabricante: ")
                .NotEmpty()
                .MinLength(2)
                .MaxLength(30)
                .ReadAndValidate();

            // 3. Categoria del cosmetico
            _writer.DisplayMenu(
                "Seleccione la categoria del producto:",
                "Maquillaje",
                "Cuidado de la Piel",
                "Cuidado del Cabello",
                "Fragancias",
                "Accesorios",
                "Otros",
                "Cancelar y volver al menu anterior"
            );

            string catOpcion = new Validator("Ingrese su opcion (1-7): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 7)
                .ReadAndValidate();

            if (catOpcion == "7")
            {
                _writer.ShowMessage("\nRegistro cancelado por el usuario.");
                _writer.WaitForKey();
                return;
            }

            CategoriaProductos categoria = (CategoriaProductos)(int.Parse(catOpcion) - 1);

            // 4. Precio unitario
            string precioTexto = new Validator("Precio unitario en C$: ")
                .NotEmpty()
                .DecimalPositivo()
                .ReadAndValidate();
            decimal precio = decimal.Parse(precioTexto.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);

            // 5. Stock inicial en bodega
            string stockTexto = new Validator("Stock inicial de unidades (0 o mas): ")
                .NotEmpty()
                .EnteroNoNegativo()
                .ReadAndValidate();
            int stock = int.Parse(stockTexto.Trim());

            // 6. Fecha de vencimiento
            string fechaTexto = new Validator("Fecha de vencimiento (dd/MM/yyyy): ")
                .NotEmpty()
                .FormatoFecha("dd/MM/yyyy")
                .FechaNoVencida("dd/MM/yyyy")
                .ReadAndValidate();
            DateTime fechaVencimiento = DateTime.ParseExact(fechaTexto.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // 7. Confirmacion antes de registrar
            string confirmacion = new Validator("\n¿Confirma el registro de este producto? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nRegistro cancelado por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 8. Generacion de ID correlativo y guardado en repositorio
            try
            {
                int correlativo = _productoRepository.GetAll().Count + 1;
                string nuevoId = $"PROD-{correlativo:D4}";
                while (_productoRepository.GetById(nuevoId) != null)
                {
                    correlativo++;
                    nuevoId = $"PROD-{correlativo:D4}";
                }

                Producto nuevo = new Producto(
                    nuevoId,
                    nombre.Trim(),
                    marca.Trim(),
                    categoria,
                    precio,
                    stock,
                    fechaVencimiento,
                    true
                );

                _productoRepository.Add(nuevo);

                _writer.ShowMessage("\nProducto registrado exitosamente en el catalogo.");
                _writer.ShowMessage($"Codigo asignado: {nuevo.Id} | Producto: {nuevo.Nombre} | Categoria: {nuevo.Categoria}");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al registrar el producto: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
