using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Ventas
{
    // Clase auxiliar para buscar y seleccionar productos durante el proceso de venta
    public class SelectorProducto
    {
        private readonly IConsoleWriter _writer;
        private readonly BodegaServices _bodegaServices;

        public SelectorProducto(IConsoleWriter writer, BodegaServices bodegaServices)
        {
            _writer = writer;
            _bodegaServices = bodegaServices;
        }

        // Selecciona el producto y pide la cantidad deseada validando el stock disponible
        public ItemCarrito? CapturarItem(List<ItemCarrito> carrito)
        {
            Producto? producto = SeleccionarProducto();
            if (producto == null)
            {
                return null;
            }

            // Calcular cuanto stock queda restando lo que ya esta en el carrito
            int yaEnCarrito = 0;
            foreach (var item in carrito)
            {
                if (item.Producto.Id == producto.Id)
                {
                    yaEnCarrito += item.Cantidad;
                }
            }

            int disponible = producto.Stock - yaEnCarrito;

            if (disponible <= 0)
            {
                _writer.ShowError($"Ya se agregaron todas las existencias disponibles de '{producto.Nombre}' al carrito.");
                _writer.WaitForKey();
                return null;
            }

            _writer.ShowMessage($"\nProducto: {producto.Nombre} | Precio: C$ {producto.Precio:N2} | Stock disponible: {disponible}");

            // Regla minorista: maximo 200 piezas por compra
            int limiteCompra = disponible;
            if (limiteCompra > 200)
            {
                limiteCompra = 200;
            }

            // Validacion de rango en consola para no permitir cantidades fuera de stock
            string cantTexto = new Validator("Cantidad a comprar: ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, limiteCompra)
                .ReadAndValidate();

            int cantidad = int.Parse(cantTexto.Trim());

            return new ItemCarrito(producto, cantidad);
        }

        // Menu para elegir si buscar por codigo directo o navegar por categoria
        public Producto? SeleccionarProducto()
        {
            while (true)
            {
                _writer.DisplayMenu(
                    "¿Como desea seleccionar el producto?",
                    "Buscar por codigo / ID (ej. PROD-0001)",
                    "Explorar productos por categoria",
                    "Finalizar seleccion de productos (Terminar compra)"
                );

                string opcion = new Validator("Ingrese su opcion: ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 3)
                    .ReadAndValidate();

                if (opcion == "3")
                {
                    return null;
                }

                if (opcion == "1")
                {
                    Producto? prod = BuscarPorCodigo();
                    if (prod != null) return prod;
                }
                else if (opcion == "2")
                {
                    Producto? prod = BuscarPorCategoria();
                    if (prod != null) return prod;
                }
            }
        }

        // Busqueda puntual mediante el codigo de producto
        private Producto? BuscarPorCodigo()
        {
            _writer.ShowMessage("\n--- Busqueda Directa por Codigo ---");
            string id = new Validator("Ingrese el ID del producto (o '0' para volver): ")
                .NotEmpty()
                .ReadAndValidate();

            if (id == "0") return null;

            try
            {
                Producto p = _bodegaServices.ObtenerPorId(id);

                if (!p.Estado)
                {
                    _writer.ShowError($"El producto '{p.Nombre}' esta inactivo en el catalogo.");
                    _writer.WaitForKey();
                    return null;
                }

                if (p.EstaVencido())
                {
                    _writer.ShowError($"El producto '{p.Nombre}' esta vencido. No se puede vender.");
                    _writer.WaitForKey();
                    return null;
                }

                if (p.Stock <= 0)
                {
                    _writer.ShowError($"El producto '{p.Nombre}' no tiene existencias en bodega.");
                    _writer.WaitForKey();
                    return null;
                }

                return p;
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
                _writer.WaitForKey();
                return null;
            }
        }

        // Muestra las categorias y lista los productos disponibles para seleccionarlos con numeros simples
        private Producto? BuscarPorCategoria()
        {
            _writer.DisplayMenu(
                "Seleccione la categoria a explorar:",
                "Maquillaje",
                "Cuidado de la Piel",
                "Cuidado del Cabello",
                "Fragancias",
                "Accesorios",
                "Otros",
                "Volver al menu anterior"
            );

            string catOpcion = new Validator("Ingrese la categoria: ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 7)
                .ReadAndValidate();

            if (catOpcion == "7") return null;

            CategoriaProductos categoria = (CategoriaProductos)(int.Parse(catOpcion) - 1);

            var disponibles = _bodegaServices.ObtenerSoloActivos()
                .Where(p => p.Categoria == categoria && !p.EstaVencido() && p.Stock > 0)
                .ToList();

            if (disponibles.Count == 0)
            {
                _writer.ShowMessage($"\nNo hay productos activos y con existencias en '{categoria}'.");
                _writer.WaitForKey();
                return null;
            }

            _writer.ShowMessage($"\n--- Productos Disponibles en {categoria} ---");
            for (int i = 0; i < disponibles.Count; i++)
            {
                _writer.ShowMessage($"{i + 1}. {disponibles[i].Nombre} (Stock: {disponibles[i].Stock}) - C$ {disponibles[i].Precio:N2}");
            }
            _writer.ShowMessage($"{disponibles.Count + 1}. Volver al menu anterior");
            _writer.ShowMessage(new string('-', 45));

            string prodOpcion = new Validator("Seleccione un producto: ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, disponibles.Count + 1)
                .ReadAndValidate();

            int indice = int.Parse(prodOpcion);
            if (indice == disponibles.Count + 1)
            {
                return null;
            }

            return disponibles[indice - 1];
        }
    }
}

