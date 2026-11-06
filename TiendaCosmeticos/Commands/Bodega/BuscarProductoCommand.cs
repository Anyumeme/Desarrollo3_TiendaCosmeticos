
using System;
using System.Collections.Generic;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Bodega
{
    public class BuscarProductoCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly BodegaServices _service;

        public BuscarProductoCommand(
            IConsoleWriter writer,
            BodegaServices service)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Execute()
        {
            bool regresar = false;

            while (!regresar)
            {
                _writer.Clear();
                _writer.ShowHeader("CONSULTA DE PRODUCTOS");

                _writer.DisplayMenu(
                    "Seleccione el tipo de consulta:",
                    "Buscar por ID",
                    "Mostrar todos los productos",
                    "Buscar por marca",
                    "Mostrar solo productos activos",
                    "Mostrar solo productos inactivos",
                    "Volver al menu anterior");

                string opcion = new Validator("Seleccione una opcion: ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 6)
                    .ReadAndValidate();

                try
                {
                    switch (opcion)
                    {
                        case "1":
                            BuscarPorId();
                            break;

                        case "2":
                            MostrarLista(_service.ObtenerTodos());
                            break;

                        case "3":
                            BuscarPorMarca();
                            break;

                        case "4":
                            MostrarLista(_service.ObtenerSoloActivos());
                            break;

                        case "5":
                            MostrarLista(_service.ObtenerSoloInactivos());
                            break;

                        case "6":
                            regresar = true;
                            break;
                    }
                }
                catch (TiendaException ex)
                {
                    _writer.ShowError(ex.Message);
                }

                if (!regresar)
                    _writer.WaitForKey();
            }
        }

        private void BuscarPorId()
        {
            string id = new Validator("Ingrese el ID del producto: ")
                .NotEmpty()
                .ReadAndValidate();

            Producto producto = _service.ObtenerPorId(id);

            MostrarProducto(producto);
        }

        private void BuscarPorMarca()
        {
            string marca = new Validator("Ingrese la marca: ")
                .NotEmpty()
                .MaxLength(40)
                .ReadAndValidate();

            List<Producto> productos = _service.ObtenerPorMarca(marca);

            MostrarLista(productos);
        }

        private void MostrarLista(List<Producto> productos)
        {
            if (productos.Count == 0)
            {
                _writer.ShowMessage("No se encontraron productos.");
                return;
            }

            _writer.ShowHeader("RESULTADOS DE LA CONSULTA");

            foreach (Producto producto in productos)
            {
                MostrarProducto(producto);
                _writer.ShowMessage("----------------------------------------");
            }

            _writer.ShowMessage(
                $"Total de productos encontrados: {productos.Count}");
        }

        private void MostrarProducto(Producto producto)
        {
            _writer.ShowMessage($"ID: {producto.Id}");
            _writer.ShowMessage($"Nombre: {producto.Nombre}");
            _writer.ShowMessage($"Marca: {producto.Marca}");
            _writer.ShowMessage($"Categoria: {producto.Categoria}");
            _writer.ShowMessage($"Precio: {producto.Precio:C}");
            _writer.ShowMessage($"Stock: {producto.Stock}");
            _writer.ShowMessage(
                $"Vencimiento: {producto.FechaVencimiento:dd/MM/yyyy}");
            _writer.ShowMessage(
                $"Estado: {(producto.Estado ? "Activo" : "Inactivo")}");
        }

        public void Undo()
        {
            
        }
    }
}
