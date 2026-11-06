using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Ventas
{
    public class ConsultarVentasCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly VentaServices _ventaServices;

        public ConsultarVentasCommand(IConsoleWriter writer, VentaServices ventaServices)
        {
            _writer = writer;
            _ventaServices = ventaServices;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("CONSULTA E HISTORIAL DE VENTAS");

            _writer.DisplayMenu(
                "Seleccione una opcion de consulta:",
                "Listar todas las facturas",
                "Listar solo facturas activas",
                "Listar solo facturas anuladas",
                "Buscar factura por ID (GUID)",
                "Buscar facturas por cedula de cliente",
                "Buscar facturas por cedula de vendedor",
                "Volver al menu anterior"
            );

            string opcion = new Validator("Ingrese su opcion: ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 7)
                .ReadAndValidate();

            switch (opcion)
            {
                case "1":
                    ListarTodas();
                    break;
                case "2":
                    ListarActivas();
                    break;
                case "3":
                    ListarAnuladas();
                    break;
                case "4":
                    BuscarPorId();
                    break;
                case "5":
                    BuscarPorCliente();
                    break;
                case "6":
                    BuscarPorVendedor();
                    break;
                case "7":
                    return;
            }
        }

        private void ListarTodas()
        {
            List<Venta> todas = _ventaServices.ObtenerTodas();
            MostrarTablaVentas(todas, "TODAS LAS FACTURAS REGISTRADAS");
        }

        private void ListarActivas()
        {
            List<Venta> activas = _ventaServices.ObtenerActivas();
            MostrarTablaVentas(activas, "FACTURAS ACTIVAS");
        }

        private void ListarAnuladas()
        {
            List<Venta> anuladas = _ventaServices.ObtenerAnuladas();
            MostrarTablaVentas(anuladas, "FACTURAS ANULADAS");
        }

        private void BuscarPorId()
        {
            _writer.Clear();
            _writer.ShowHeader("BUSQUEDA DE FACTURA POR ID");

            string idTexto = new Validator("Ingrese el ID (GUID) de la factura (o '0' para cancelar): ")
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

            try
            {
                Venta venta = _ventaServices.ObtenerPorId(idVenta);
                new MostrarComprobanteCommand(_writer, venta).Execute();
                _writer.WaitForKey();
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
                _writer.WaitForKey();
            }
        }

        private void BuscarPorCliente()
        {
            _writer.Clear();
            _writer.ShowHeader("HISTORIAL DE FACTURAS POR CLIENTE");

            string cedula = new Validator("Ingrese la cedula del cliente (o '0' para cancelar): ")
                .NotEmpty()
                .ReadAndValidate();

            if (cedula == "0")
            {
                return;
            }

            List<Venta> ventasCliente = _ventaServices.ObtenerVentasPorCliente(cedula.Trim());
            MostrarTablaVentas(ventasCliente, $"FACTURAS DEL CLIENTE (Cedula: {cedula.Trim()})");
        }

        private void BuscarPorVendedor()
        {
            _writer.Clear();
            _writer.ShowHeader("HISTORIAL DE FACTURAS POR VENDEDOR");

            string cedula = new Validator("Ingrese la cedula del vendedor (o '0' para cancelar): ")
                .NotEmpty()
                .ReadAndValidate();

            if (cedula == "0")
            {
                return;
            }

            List<Venta> ventasVendedor = _ventaServices.ObtenerVentasPorVendedor(cedula.Trim());
            MostrarTablaVentas(ventasVendedor, $"FACTURAS REALIZADAS POR VENDEDOR (Cedula: {cedula.Trim()})");
        }

        private void MostrarTablaVentas(List<Venta> ventas, string titulo)
        {
            _writer.Clear();
            _writer.ShowHeader(titulo);

            if (ventas == null || ventas.Count == 0)
            {
                _writer.ShowMessage("\nNo se encontraron facturas registradas para este criterio.");
                _writer.WaitForKey();
                return;
            }

            _writer.ShowMessage(new string('-', 103));
            _writer.ShowMessage(string.Format("{0,-36} | {1,-16} | {2,-16} | {3,-12} | {4,10} | {5,-8}",
                "ID FACTURA (GUID)", "FECHA", "CLIENTE", "VENDEDOR", "TOTAL", "ESTADO"));
            _writer.ShowMessage(new string('-', 103));

            int cantActivas = 0;
            decimal totalActivas = 0;
            int cantAnuladas = 0;
            decimal totalAnuladas = 0;

            foreach (var v in ventas)
            {
                string clienteNom = v.Cliente != null ? v.Cliente.Nombre : "Consumidor";
                if (clienteNom.Length > 16)
                {
                    clienteNom = clienteNom.Substring(0, 13) + "...";
                }

                string vendedorNom = v.Vendedor != null ? v.Vendedor.Nombre : "N/A";
                if (vendedorNom.Length > 12)
                {
                    vendedorNom = vendedorNom.Substring(0, 9) + "...";
                }

                string estadoTexto = v.Estado ? "ACTIVA" : "ANULADA";

                _writer.ShowMessage(string.Format("{0,-36} | {1,-16:dd/MM/yyyy HH:mm} | {2,-16} | {3,-12} | {4,10:N2} | {5,-8}",
                    v.Id,
                    v.Fecha,
                    clienteNom,
                    vendedorNom,
                    v.Total,
                    estadoTexto));

                if (v.Estado)
                {
                    cantActivas++;
                    totalActivas += v.Total;
                }
                else
                {
                    cantAnuladas++;
                    totalAnuladas += v.Total;
                }
            }

            _writer.ShowMessage(new string('-', 103));
            if (cantAnuladas == 0)
            {
                _writer.ShowMessage($"Total facturas: {ventas.Count} | Total recaudado (Activas): C$ {totalActivas:N2}");
            }
            else if (cantActivas == 0)
            {
                _writer.ShowMessage($"Total facturas anuladas: {cantAnuladas} | Monto anulado: C$ {totalAnuladas:N2}");
            }
            else
            {
                _writer.ShowMessage($"Total facturas listadas: {ventas.Count} (Activas: {cantActivas} | Anuladas: {cantAnuladas})");
                _writer.ShowMessage($"Monto real recaudado (Activas):   C$ {totalActivas:N2}");
                _writer.ShowMessage($"Monto no recaudado  (Anuladas):  C$ {totalAnuladas:N2}");
            }
            _writer.ShowMessage(new string('=', 103));

            // Inspeccion de comprobante opcional
            string verDetalle = new Validator("\n¿Desea abrir el comprobante detallado de alguna factura de la lista? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (verDetalle.Trim().ToUpper() == "S")
            {
                string idIngresado = new Validator("Ingrese el ID (GUID) de la factura: ")
                    .NotEmpty()
                    .ReadAndValidate();

                if (Guid.TryParse(idIngresado.Trim(), out Guid idSelec))
                {
                    var ventaEncontrada = ventas.FirstOrDefault(x => x.Id == idSelec);
                    if (ventaEncontrada != null)
                    {
                        new MostrarComprobanteCommand(_writer, ventaEncontrada).Execute();
                    }
                    else
                    {
                        _writer.ShowError("La factura indicada no pertenece a la lista consultada.");
                    }
                }
                else
                {
                    _writer.ShowError("El identificador ingresado no es valido.");
                }
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
