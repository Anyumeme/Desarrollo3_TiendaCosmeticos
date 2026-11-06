using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;
using Administrador = TiendaCosmeticos.Models.Administrador;

namespace TiendaCosmeticos.Commands.Administrador.PersonalCommand
{
    public class ListarPersonalCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly EmpleadoService _empleadoService;
        private readonly AdministradorService _administradorService;

        public ListarPersonalCommand(
            IConsoleWriter writer,
            EmpleadoService empleadoService,
            AdministradorService administradorService)
        {
            _writer = writer;
            _empleadoService = empleadoService;
            _administradorService = administradorService;
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
                "Listar solo Administradores",
                "Buscar personal por ID o Cedula",
                "Volver al menu anterior"
            );

            string opcion = new Validator("Ingrese su opcion (1-8): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 8)
                .ReadAndValidate();

            // Opcion 8: Salir al menu anterior
            if (opcion == "8")
            {
                return;
            }

            // Busqueda individual puntual por ID o Cedula
            if (opcion == "7")
            {
                string criterio = new Validator("\nIngrese el ID o Cedula a buscar: ")
                    .NotEmpty()
                    .ReadAndValidate();

                try
                {
                    Empleado emp;
                    try
                    {
                        emp = _empleadoService.BuscarEmpleado(criterio);
                    }
                    catch (TiendaException)
                    {
                        emp = _administradorService.BuscarAdministrador(criterio);
                    }

                    _writer.ShowMessage("\n--- Ficha del Personal ---");
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

            // Consulta de lista completa con proteccion de excepciones
            try
            {
                List<Empleado> listaFinal = new();

                if (opcion == "6")
                {
                    // Solo administradores
                    var admins = _administradorService.ListarAdministradores("Todos");
                    listaFinal.AddRange(admins);
                }
                else
                {
                    Rol? rolFiltro = opcion switch
                    {
                        "4" => Rol.Vendedor,
                        "5" => Rol.Bodeguero,
                        _ => null
                    };

                    string estadoFiltro = opcion switch
                    {
                        "2" => "Activos",
                        "3" => "Inactivos",
                        _ => "Todos"
                    };

                    List<Empleado> empleados = _empleadoService.ListarEmpleados(rolFiltro, estadoFiltro);
                    listaFinal.AddRange(empleados);

                    // Si se lista todo o por estado general, incluir tambien a los administradores
                    if (rolFiltro == null)
                    {
                        var admins = _administradorService.ListarAdministradores(estadoFiltro);
                        listaFinal.AddRange(admins);
                    }
                }

                if (listaFinal.Count == 0)
                {
                    _writer.ShowMessage("\nNo se encontraron registros de personal con el filtro seleccionado.");
                    _writer.WaitForKey();
                    return;
                }

                // Renderizado de tabla limpia en consola
                _writer.ShowMessage("\n" + new string('-', 105));
                _writer.ShowMessage(string.Format("{0,-10} | {1,-14} | {2,-23} | {3,-18} | {4,-10} | {5,-10} | {6,-8}",
                    "ID", "CARGO", "NOMBRE", "CEDULA", "CELULAR", "USUARIO", "ESTADO"));
                _writer.ShowMessage(new string('-', 105));

                foreach (var emp in listaFinal)
                {
                    string estadoTexto = emp.Estado ? "Activo" : "Inactivo";
                    string cargoTexto = emp.Rol.ToString();

                    _writer.ShowMessage(string.Format("{0,-10} | {1,-14} | {2,-23} | {3,-18} | {4,-10} | {5,-10} | {6,-8}",
                        emp.Id,
                        cargoTexto,
                        emp.Nombre.Length > 23 ? emp.Nombre.Substring(0, 20) + "..." : emp.Nombre,
                        emp.Cedula,
                        emp.NumeroCelular,
                        emp.User,
                        estadoTexto));
                }

                _writer.ShowMessage(new string('-', 105));
                _writer.ShowMessage($"Total de registros encontrados: {listaFinal.Count}");
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
