using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Commands.Administrador.ClienteCommand;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Ventas
{
    public class RealizarVentaCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly VentaServices _ventaServices;
        private readonly BodegaServices _bodegaServices;
        private readonly ClienteService _clienteService;
        private readonly Empleado _usuarioActual;
        private readonly SelectorProducto _selectorProducto;

        public RealizarVentaCommand(
            IConsoleWriter writer,
            VentaServices ventaServices,
            BodegaServices bodegaServices,
            ClienteService clienteService,
            Empleado usuarioActual)
        {
            _writer = writer;
            _ventaServices = ventaServices;
            _bodegaServices = bodegaServices;
            _clienteService = clienteService;
            _usuarioActual = usuarioActual;
            _selectorProducto = new SelectorProducto(writer, bodegaServices);
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("REALIZAR VENTA DE PRODUCTOS");

            // 1. Carrito interactivo de compra
            List<ItemCarrito> carrito = CapturarProductosCarrito();
            if (carrito.Count == 0)
            {
                _writer.ShowMessage("\nVenta cancelada: no se agregaron productos al carrito.");
                _writer.WaitForKey();
                return;
            }

            // 2. Identificacion o alta rapida del cliente
            Cliente? cliente = SeleccionarORegistrarCliente();
            if (cliente == null)
            {
                _writer.ShowMessage("\nVenta cancelada: no se asigno ningun cliente a la compra.");
                _writer.WaitForKey();
                return;
            }

            // 3. Calculo de subtotal y descuentos por volumen
            decimal subtotal = 0;
            foreach (var item in carrito)
            {
                subtotal += item.Producto.Precio * item.Cantidad;
            }

            decimal porcentaje = 0;
            if (subtotal > 2000)
            {
                porcentaje = 0.15m;
            }
            else if (subtotal > 1000)
            {
                porcentaje = 0.10m;
            }

            decimal descuento = subtotal * porcentaje;

            // 4. Preparar la venta y mostrar el comprobante
            Venta venta = new Venta(cliente, _usuarioActual, DateTime.Now);
            foreach (var item in carrito)
            {
                venta.AgregarDetalle(item.Producto, item.Cantidad, item.Producto.Precio);
            }
            venta.AplicarDescuento(descuento);

            new MostrarComprobanteCommand(_writer, venta).Execute();

            string confirmacion = new Validator("\n¿Confirma el cobro y registro de esta venta? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nVenta cancelada por el usuario. No se afecto el inventario.");
                _writer.WaitForKey();
                return;
            }

            // 5. Guardado de la transaccion
            try
            {
                _ventaServices.RegistrarVenta(venta);

                _writer.ShowMessage("\n========================================================");
                _writer.ShowMessage("           VENTA REGISTRADA EXITOSAMENTE");
                _writer.ShowMessage($"Factura ID:    {venta.Id}");
                _writer.ShowMessage($"Total cobrado: C$ {venta.Total:N2}");
                _writer.ShowMessage("========================================================");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al procesar la venta: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        private List<ItemCarrito> CapturarProductosCarrito()
        {
            List<ItemCarrito> carrito = new();
            bool agregarMas = true;

            while (agregarMas)
            {
                ItemCarrito? nuevoItem = _selectorProducto.CapturarItem(carrito);
                if (nuevoItem == null)
                {
                    break;
                }

                var itemExistente = carrito.FirstOrDefault(i => i.Producto.Id == nuevoItem.Producto.Id);
                if (itemExistente != null)
                {
                    itemExistente.Cantidad += nuevoItem.Cantidad;
                }
                else
                {
                    carrito.Add(nuevoItem);
                }

                _writer.ShowMessage($"\nAgregado al carrito: {nuevoItem.Cantidad}x {nuevoItem.Producto.Nombre} (Subtotal: C$ {nuevoItem.Producto.Precio * nuevoItem.Cantidad:N2})");

                string mas = new Validator("\n¿Desea agregar otro producto a esta misma compra? (S/N): ")
                    .NotEmpty()
                    .ConfirmacionSN()
                    .ReadAndValidate();

                if (mas.Trim().ToUpper() != "S")
                {
                    agregarMas = false;
                }
            }

            return carrito;
        }

        private Cliente? SeleccionarORegistrarCliente()
        {
            _writer.ShowHeader("IDENTIFICACION DEL CLIENTE");
            _writer.DisplayMenu(
                "Seleccione una opcion:",
                "Buscar cliente existente por ID o Cedula",
                "Registrar nuevo cliente en este momento",
                "Cancelar venta"
            );

            string opcion = new Validator("Ingrese su opcion (1-3): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 3)
                .ReadAndValidate();

            if (opcion == "3") return null;

            if (opcion == "1")
            {
                string criterio = new Validator("\nIngrese ID o Cedula del cliente: ").NotEmpty().ReadAndValidate();
                try
                {
                    Cliente c = _clienteService.BuscarCliente(criterio);
                    if (!c.Estado)
                    {
                        _writer.ShowError($"El cliente '{c.Nombre}' se encuentra inactivo.");
                        return null;
                    }
                    if (c.CalcularEdad() < 16)
                    {
                        _writer.ShowError($"El cliente tiene {c.CalcularEdad()} anios. Por ley debe tener al menos 16 anios.");
                        return null;
                    }

                    _writer.ShowMessage($"\nCliente asignado: {c.Nombre} | Cedula: {c.Cedula}");
                    return c;
                }
                catch (RegistroNoEncontradoException)
                {
                    _writer.ShowMessage($"\nNo se encontro cliente con '{criterio}'.");
                    string reg = new Validator("¿Desea registrarlo ahora mismo? (S/N): ").NotEmpty().ConfirmacionSN().ReadAndValidate();
                    return reg.Trim().ToUpper() == "S" ? RegistrarClienteNuevo() : null;
                }
                catch (TiendaException ex)
                {
                    _writer.ShowError(ex.Message);
                    return null;
                }
            }

            return RegistrarClienteNuevo();
        }

        private Cliente? RegistrarClienteNuevo()
        {
            var cmd = new AgregarClienteCommand(_writer, _clienteService);
            cmd.Execute();
            return cmd.ClienteRegistrado;
        }

        public void Undo()
        {
        }
    }
}
