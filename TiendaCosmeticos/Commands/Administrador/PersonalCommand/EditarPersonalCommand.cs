using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.PersonalCommand
{
    public class EditarPersonalCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly EmpleadoService _empleadoService;
        private readonly AdministradorService _administradorService;

        public EditarPersonalCommand(
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
            _writer.ShowHeader("EDICION DE DATOS DE PERSONAL");

            // 1. Solicitar el ID o Cedula del empleado a modificar
            string criterio = new Validator("Ingrese el ID o Cedula del personal a editar: ")
                .NotEmpty()
                .ReadAndValidate();

            Empleado empleado;
            bool esAdmin = false;

            try
            {
                empleado = _empleadoService.BuscarEmpleado(criterio);

                if (!empleado.Estado)
                {
                    _writer.ShowError($"El empleado '{empleado.Id}' se encuentra inactivo. Debe reactivarlo antes de editar sus datos.");
                    _writer.WaitForKey();
                    return;
                }
            }
            catch (TiendaException)
            {
                try
                {
                    empleado = _administradorService.BuscarAdministrador(criterio);
                    esAdmin = true;

                    if (!empleado.Estado)
                    {
                        _writer.ShowError($"El administrador '{empleado.Id}' se encuentra inactivo. Debe reactivarlo antes de editar sus datos.");
                        _writer.WaitForKey();
                        return;
                    }
                }
                catch (TiendaException exAdmin)
                {
                    _writer.ShowError(exAdmin.Message);
                    _writer.WaitForKey();
                    return;
                }
            }

            // 2. Mostrar la ficha actual del trabajador
            _writer.ShowMessage("\n--- Datos Actuales del Trabajador ---");
            _writer.ShowMessage($"ID (Inmutable):    {empleado.Id}");
            _writer.ShowMessage($"Cargo:             {empleado.Rol}");
            _writer.ShowMessage($"Cedula:            {empleado.Cedula}");
            _writer.ShowMessage($"Nombre:            {empleado.Nombre}");
            _writer.ShowMessage($"Celular:           {empleado.NumeroCelular}");
            _writer.ShowMessage($"Genero:            {empleado.Genero}");
            _writer.ShowMessage($"Fecha Nacimiento:  {empleado.FechaNacimiento:dd/MM/yyyy}");
            _writer.ShowMessage($"Correo:            {empleado.CorreoElectronico}");
            _writer.ShowMessage($"Direccion:         {empleado.Direccion}");
            _writer.ShowMessage($"Estado Civil:      {empleado.EstadoCivil}");

            _writer.ShowMessage("\n--- Ingrese los Nuevos Datos ---");

            // 3. Captura y validacion de nuevos datos
            string nuevoNombre = new Validator("Nuevo nombre completo: ")
                .NotEmpty()
                .SoloLetras()
                .MinLength(3)
                .MaxLength(50)
                .ReadAndValidate();

            string celularTexto = new Validator("Nuevo numero de celular: ")
                .NotEmpty()
                .NumeroCelular()
                .ReadAndValidate();
            int nuevoCelular = int.Parse(celularTexto.Trim());

            _writer.DisplayMenu(
                "Seleccione el nuevo genero:",
                "Femenino",
                "Masculino"
            );
            string generoOpcion = new Validator("Seleccione el genero: ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 2)
                .ReadAndValidate();
            Genero nuevoGenero = generoOpcion == "1" ? Genero.Femenino : Genero.Masculino;

            string nuevoCorreo = new Validator("Nuevo correo electronico: ")
                .NotEmpty()
                .FormatoCorreo()
                .ReadAndValidate();

            string nuevaDireccion = new Validator("Nueva direccion domiciliar: ")
                .NotEmpty()
                .MinLength(5)
                .MaxLength(100)
                .ReadAndValidate();

            string nuevoEstadoCivil = new Validator("Nuevo estado civil (Soltero/a, Casado/a, etc.): ")
                .NotEmpty()
                .SoloLetras()
                .ReadAndValidate();

            // 4. Preguntar si desea cambiar la cedula
            string cambiarCedula = new Validator("\n¿Desea modificar el numero de cedula? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            string? nuevaCedula = null;
            DateTime nuevaFechaNacimiento = empleado.FechaNacimiento;

            if (cambiarCedula.Trim().ToUpper() == "S")
            {
                nuevaCedula = new Validator("Nueva cedula (ej. 001-120590-0001A): ")
                    .NotEmpty()
                    .FormatoCedula()
                    .ReadCedula(out DateTime fechaExtraida);

                nuevaFechaNacimiento = fechaExtraida;
                _writer.ShowMessage($"Nueva fecha de nacimiento asignada: {nuevaFechaNacimiento:dd/MM/yyyy}");
            }

            // 5. Confirmacion antes de aplicar los cambios
            string confirmacion = new Validator("\n¿Esta seguro de guardar los cambios en este personal? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nEdicion cancelada por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 6. Aplicar actualizacion con captura de excepciones
            try
            {
                if (esAdmin)
                {
                    _administradorService.ActualizarAdministrador(
                        empleado.Id,
                        nuevoNombre,
                        nuevoCelular,
                        nuevoGenero,
                        nuevaFechaNacimiento,
                        nuevoCorreo,
                        nuevaDireccion,
                        nuevoEstadoCivil,
                        nuevaCedula
                    );
                }
                else
                {
                    _empleadoService.ActualizarEmpleado(
                        empleado.Id,
                        nuevoNombre,
                        nuevoCelular,
                        nuevoGenero,
                        nuevaFechaNacimiento,
                        nuevoCorreo,
                        nuevaDireccion,
                        nuevoEstadoCivil,
                        nuevaCedula
                    );
                }

                _writer.ShowMessage($"\nDatos de '{empleado.Nombre}' ({empleado.Id}) actualizados correctamente.");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al actualizar: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
