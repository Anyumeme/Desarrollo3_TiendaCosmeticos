using System;
using System.Collections.Generic;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.PersonalCommand
{
    public class ListarPersonalCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly EmpleadoService _empleadoService;

        public ListarPersonalCommand(IConsoleWriter writer, EmpleadoService empleadoService)
        {
            _writer = writer;
            _empleadoService = empleadoService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("CONSULTA Y LISTADO DE PERSONAL");

            // 1. Menu de seleccion de filtro
            _writer.DisplayMenu(
                "Seleccione el filtro a aplicar:",
                "Listar todo el personal (Activos e Inactivos)",
                "Listar solo personal activo",
                "Listar solo personal inactivo",
                "Listar solo Vendedores",
                "Listar solo Bodegueros",
                "Buscar empleado por ID o Cedula",
                "Volver al menu anterior"
            );

            string opcion = new Validator("Ingrese su opcion (1-7): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 7)
                .ReadAndValidate();

            // Opcion 7: Salir al menu anterior
            if (opcion == "7")
            {
                return;
            }

            // Busqueda individual puntual por ID o Cedula
            if (opcion == "6")
            {
                string criterio = new Validator("\nIngrese el ID o Cedula a buscar: ")
                    .NotEmpty()
                    .ReadAndValidate();

                try
                {
                    Empleado emp = _empleadoService.BuscarEmpleado(criterio);

                    _writer.ShowMessage("\n--- Ficha del Empleado ---");
                    _writer.ShowMessage($"ID:                 {emp.Id}");
                    _writer.ShowMessage($"Cargo:              {emp.Rol}");
                    _writer.ShowMessage($"Nombre:             {emp.Nombre}");
                    _writer.ShowMessage($"Cedula:             {emp.Cedula}");
                    _writer.ShowMessage($"Celular:            {emp.NumeroCelular}");
                    _writer.ShowMessage($"Genero:             {emp.Genero}");
                    _writer.ShowMessage($"Fecha Nacimiento:   {emp.FechaNacimiento:dd/MM/yyyy}");
                    _writer.ShowMessage($"Correo Electronico: {emp.CorreoElectronico}");
                    _writer.ShowMessage($"Direccion:          {emp.Direccion}");
                    _writer.ShowMessage($"Estado Civil:       {emp.EstadoCivil}");
                    _writer.ShowMessage($"Usuario de Login:   {emp.User}");
                    _writer.ShowMessage($"Estado:             {(emp.Estado ? "ACTIVO" : "INACTIVO")}");
                }
                catch (TiendaException ex)
                {
                    _writer.ShowError(ex.Message);
                }
                catch (Exception ex)
                {
                    _writer.ShowError($"Ocurrio un error inesperado al buscar: {ex.Message}");
                }

                _writer.WaitForKey();
                return;
            }

            Rol? rolFiltro = null;
            string estadoFiltro = "Todos";

            switch (opcion)
            {
                case "1":
                    estadoFiltro = "Todos";
                    break;
                case "2":
                    estadoFiltro = "Activos";
                    break;
                case "3":
                    estadoFiltro = "Inactivos";
                    break;
                case "4":
                    rolFiltro = Rol.Vendedor;
                    break;
                case "5":
                    rolFiltro = Rol.Bodeguero;
                    break;
            }

            // 2. Consulta al servicio con proteccion de excepciones
            try
            {
                List<Empleado> lista = _empleadoService.ListarEmpleados(rolFiltro, estadoFiltro);

                if (lista.Count == 0)
                {
                    _writer.ShowMessage("\nNo se encontraron registros de personal con el filtro seleccionado.");
                    _writer.WaitForKey();
                    return;
                }

                // 3. Renderizado de tabla limpia en consola
                _writer.ShowMessage("\n" + new string('-', 105));
                _writer.ShowMessage(string.Format("{0,-10} | {1,-10} | {2,-25} | {3,-18} | {4,-10} | {5,-12} | {6,-8}",
                    "ID", "CARGO", "NOMBRE", "CEDULA", "CELULAR", "USUARIO", "ESTADO"));
                _writer.ShowMessage(new string('-', 105));

                foreach (var emp in lista)
                {
                    string estadoTexto = emp.Estado ? "Activo" : "Inactivo";
                    string cargoTexto = emp.Rol == Rol.Vendedor ? "Vendedor" : "Bodeguero";

                    _writer.ShowMessage(string.Format("{0,-10} | {1,-10} | {2,-25} | {3,-18} | {4,-10} | {5,-12} | {6,-8}",
                        emp.Id,
                        cargoTexto,
                        emp.Nombre.Length > 25 ? emp.Nombre.Substring(0, 22) + "..." : emp.Nombre,
                        emp.Cedula,
                        emp.NumeroCelular,
                        emp.User,
                        estadoTexto));
                }

                _writer.ShowMessage(new string('-', 105));
                _writer.ShowMessage($"Total de registros encontrados: {lista.Count}");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al listar: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
