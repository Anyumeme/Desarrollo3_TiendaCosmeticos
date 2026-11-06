using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Ventas
{
    public class AnularVentaCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly VentaServices _ventaServices;

        public AnularVentaCommand(IConsoleWriter writer, VentaServices ventaServices)
        {
            _writer = writer;
            _ventaServices = ventaServices;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("ANULACION DE FACTURA DE VENTA");

            // 1. Validar que existan facturas en el sistema
            if (_ventaServices.ObtenerTodas().Count == 0)
            {
                _writer.ShowError("No hay facturas registradas en el sistema para anular.");
                _writer.WaitForKey();
                return;
            }

            // 2. Solicitar el ID (Guid) de la factura
            _writer.ShowMessage("Ingrese el ID (GUID) de la factura que desea anular (o '0' para cancelar):");
            string idTexto = new Validator("ID de la factura: ")
                .NotEmpty()
                .ReadAndValidate();

            if (idTexto == "0")
            {
                return;
            }

            if (!Guid.TryParse(idTexto.Trim(), out Guid idVenta))
            {
                _writer.ShowError("El formato del identificador no es valido (debe ser un GUID completo).");
                _writer.WaitForKey();
                return;
            }

            // 3. Buscar la factura en el servicio
            Venta venta;
            try
            {
                venta = _ventaServices.ObtenerPorId(idVenta);
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
                _writer.WaitForKey();
                return;
            }

            // 4. Validar que la factura este activa
            if (!venta.Estado)
            {
                _writer.ShowError("Esta factura ya se encuentra ANULADA anteriormente.");
                _writer.WaitForKey();
                return;
            }

            // 5. Mostrar los detalles de la factura que se va a anular
            new MostrarComprobanteCommand(_writer, venta).Execute();

            _writer.ShowMessage("\n--- RESUMEN DE RESTITUCION DE INVENTARIO ---");
            _writer.ShowMessage("Al anular la venta, los siguientes productos se reintegraran al stock:");
            foreach (var detalle in venta.Detalles)
            {
                if (detalle.Producto != null)
                {
                    _writer.ShowMessage($" * {detalle.Producto.Nombre} (ID: {detalle.Producto.Id}) -> Devolver +{detalle.Cantidad} unid. (Stock actual: {detalle.Producto.Stock})");
                }
            }
            _writer.ShowMessage(new string('-', 78));

            // 6. Confirmacion del usuario
            _writer.ShowMessage("ADVERTENCIA: Esta accion cancelara la venta y devolvera los productos a bodega.");
            string confirmacion = new Validator("¿Esta seguro de anular esta venta? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nOperacion cancelada por el usuario. La factura no fue modificada.");
                _writer.WaitForKey();
                return;
            }

            // 7. Procesar anulacion y devolucion de stock
            try
            {
                _ventaServices.AnularVenta(idVenta);
                _writer.ShowMessage("\nFactura anulada exitosamente.");
                _writer.ShowMessage("El estado de la venta cambio a ANULADA y el inventario ha sido actualizado.");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError($"Error al anular la venta: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
