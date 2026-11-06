using System;
using System.Globalization;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.PersonalCommand
{
    public class AgregarPersonalCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly EmpleadoService _empleadoService;

        public AgregarPersonalCommand(IConsoleWriter writer, EmpleadoService empleadoService)
        {
            _writer = writer;
            _empleadoService = empleadoService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("REGISTRO DE NUEVO PERSONAL");

            // 1. Seleccion del cargo a desempenar
            _writer.DisplayMenu(
                "Seleccione el cargo a asignar:",
                "Vendedor (Modulo de Ventas)",
                "Encargado de Bodega (Modulo de Inventario)"
            );

            string rolOpcion = new Validator("Ingrese su opcion (1-2): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 2)
                .ReadAndValidate();

            Rol rol = rolOpcion == "1" ? Rol.Vendedor : Rol.Bodeguero;
            string rolTexto = rol == Rol.Vendedor ? "Vendedor" : "Bodeguero";

            _writer.ShowMessage($"\n--- Datos del {rolTexto} ---");

            // 2. Cedula y extraccion automatica de la fecha de nacimiento
            string cedula = new Validator("Cedula (ej. 001-120590-0001A): ")
                .NotEmpty()
                .FormatoCedula()
                .ReadCedula(out DateTime fechaNacimiento);

            _writer.ShowMessage($"Fecha de nacimiento extraida: {fechaNacimiento:dd/MM/yyyy}");

            // 3. Nombre completo
            string nombre = new Validator("Nombre completo: ")
                .NotEmpty()
                .SoloLetras()
                .MinLength(3)
                .MaxLength(50)
                .ReadAndValidate();

            // 4. Numero celular nicaraguense (8 digitos iniciando con 8, 7 o 5)
            string celularTexto = new Validator("Numero de celular: ")
                .NotEmpty()
                .NumeroCelular()
                .ReadAndValidate();
            int celular = int.Parse(celularTexto.Trim());

            // 5. Genero
            _writer.DisplayMenu(
                "Seleccione el genero:",
                "Femenino",
                "Masculino"
            );
            string generoOpcion = new Validator("Ingrese su opcion (1-2): ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 2)
                .ReadAndValidate();
            Genero genero = generoOpcion == "1" ? Genero.Femenino : Genero.Masculino;

            // 6. Correo electronico
            string correo = new Validator("Correo electronico: ")
                .NotEmpty()
                .FormatoCorreo()
                .ReadAndValidate();

            // 7. Direccion domiciliar
            string direccion = new Validator("Direccion domiciliar: ")
                .NotEmpty()
                .MinLength(5)
                .MaxLength(100)
                .ReadAndValidate();

            // 8. Estado civil
            string estadoCivil = new Validator("Estado civil (Soltero/a, Casado/a, etc.): ")
                .NotEmpty()
                .SoloLetras()
                .ReadAndValidate();

            // 9. Credenciales de acceso
            string user = new Validator("Nombre de usuario para inicio de sesion: ")
                .NotEmpty()
                .SinEspacios()
                .MinLength(3)
                .MaxLength(20)
                .ReadAndValidate();

            string password = new Validator("Contrasenia (min. 8 caracteres con mayuscula, minuscula, numero y simbolo): ")
                .NotEmpty()
                .PasswordSegura()
                .ReadPassword();

            // 10. Salario mensual para su contrato
            string salarioTexto = new Validator("Salario mensual en C$: ")
                .NotEmpty()
                .DecimalPositivo()
                .ReadAndValidate();
            decimal salario = decimal.Parse(salarioTexto.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);

            // 11. Confirmacion antes de guardar
            string confirmacion = new Validator("\n¿Confirma el registro del nuevo trabajador? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nRegistro cancelado por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 12. Llamado al servicio blindado con try/catch para evitar caidas
            try
            {
                Empleado nuevo = _empleadoService.RegistrarEmpleado(
                    rol,
                    cedula,
                    nombre,
                    celular,
                    fechaNacimiento,
                    genero,
                    correo,
                    direccion,
                    estadoCivil,
                    user,
                    password,
                    salario
                );

                _writer.ShowMessage("\nPersonal registrado exitosamente.");
                _writer.ShowMessage($"Codigo asignado: {nuevo.Id} | Cargo: {rolTexto} | Usuario: {nuevo.User}");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al registrar: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
