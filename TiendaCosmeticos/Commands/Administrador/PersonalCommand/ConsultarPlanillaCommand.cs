using System;
using System.Collections.Generic;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.PersonalCommand
{
    public class ConsultarPlanillaCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ContratoService _contratoService;

        public ConsultarPlanillaCommand(IConsoleWriter writer, ContratoService contratoService)
        {
            _writer = writer;
            _contratoService = contratoService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("PLANILLA DE PAGOS MENSUAL");

            try
            {
                // 1. Obtener los contratos vigentes de los empleados activos
                List<Contrato> planilla = _contratoService.ObtenerPlanillaActiva();

                if (planilla.Count == 0)
                {
                    _writer.ShowMessage("\nNo se encontraron empleados activos con contratos laborales vigentes.");
                    _writer.WaitForKey();
                    return;
                }

                // 2. Calcular el total general de la planilla
                decimal totalPlanilla = _contratoService.CalcularTotalPlanilla();

                // 3. Renderizar tabla con desglose individual
                _writer.ShowMessage("\n" + new string('-', 102));
                _writer.ShowMessage(string.Format("{0,-10} | {1,-25} | {2,-18} | {3,-18} | {4,-18}",
                    "ID", "NOMBRE DEL EMPLEADO", "CEDULA", "CARGO", "SALARIO MENSUAL"));
                _writer.ShowMessage(new string('-', 102));

                foreach (var contrato in planilla)
                {
                    string idEmp = contrato.Empleado?.Id ?? "N/A";
                    string nombreEmp = contrato.Empleado?.Nombre ?? "N/A";
                    string cedulaEmp = contrato.Empleado?.Cedula ?? "N/A";
                    string cargo = contrato.Cargo;
                    string salarioTexto = $"C$ {contrato.SalarioMensual:N2}";

                    if (nombreEmp.Length > 25)
                    {
                        nombreEmp = nombreEmp.Substring(0, 22) + "...";
                    }

                    _writer.ShowMessage(string.Format("{0,-10} | {1,-25} | {2,-18} | {3,-18} | {4,18}",
                        idEmp,
                        nombreEmp,
                        cedulaEmp,
                        cargo,
                        salarioTexto));
                }

                _writer.ShowMessage(new string('-', 102));
                _writer.ShowMessage($"Total de empleados en planilla: {planilla.Count}");
                _writer.ShowMessage($"MONTO TOTAL DE PLANILLA MENSUAL: C$ {totalPlanilla:N2}");
                _writer.ShowMessage(new string('-', 102));
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al consultar la planilla: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
