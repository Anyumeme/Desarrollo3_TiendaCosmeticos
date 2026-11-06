using System;
using System.Globalization;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;
using Administrador = TiendaCosmeticos.Models.Administrador;

namespace TiendaCosmeticos.Commands.Administrador.PersonalCommand
{
    public class AgregarPersonalCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly EmpleadoService _empleadoService;
        private readonly AdministradorService _administradorService;

        public AgregarPersonalCommand(
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
            _writer.ShowHeader("REGISTRO DE NUEVO PERSONAL");

            // 1. Seleccion del cargo a desempenar
            _writer.DisplayMenu(
                "Seleccione el cargo a asignar:",
                "Vendedor (Modulo de Ventas)",
                "Encargado de Bodega (Modulo de Inventario)",
                "Administrador (Gestion y Control General)"
            );

            string rolOpcion = new Validator("Ingrese su opcion: ")
                .NotEmpty()
                .OnlyDigits()
                .InputBetweenXandY(1, 3)
                .ReadAndValidate();

            Rol rol = rolOpcion switch
            {
                "1" => Rol.Vendedor,
                "2" => Rol.Bodeguero,
                _ => Rol.Administrador
            };

            string rolTexto = rol switch
            {
                Rol.Vendedor => "Vendedor",
                Rol.Bodeguero => "Bodeguero",
                _ => "Administrador"
            };

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
            string generoOpcion = new Validator("Seleccione el genero: ")
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
                .MinLength(4)
                .MaxLength(20)
                .ReadAndValidate();

            string password = new Validator("Contrasenia (min 8 caracteres, mayuscula, minuscula, numero y simbolo): ")
                .NotEmpty()
                .PasswordSegura()
                .ReadPassword();

            // 10. Salario mensual acordado para contrato
            string salarioTexto = new Validator("Salario mensual acordado en contrato (C$): ")
                .NotEmpty()
                .DecimalPositivo()
                .ReadAndValidate();
            decimal salario = decimal.Parse(salarioTexto.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);

            // 11. Confirmacion antes de guardar
            string confirmacion = new Validator($"\n¿Confirma el registro del nuevo {rolTexto}? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nRegistro cancelado por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 12. Guardado blindado
            try
            {
                if (rol == Rol.Administrador)
                {
                    int correlativo = Startup.GetAdministradorRepository().GetAll().Count + Startup.GetEmpleadoRepository().GetAll().Count + 1;
                    string nuevoId = $"EMP-{correlativo:D4}";
                    while (Startup.GetAdministradorRepository().GetById(nuevoId) != null || Startup.GetEmpleadoRepository().GetById(nuevoId) != null)
                    {
                        correlativo++;
                        nuevoId = $"EMP-{correlativo:D4}";
                    }

                    TiendaCosmeticos.Models.Administrador nuevoAdmin = new TiendaCosmeticos.Models.Administrador(
                        nuevoId,
                        cedula.Trim(),
                        nombre.Trim(),
                        celular,
                        fechaNacimiento,
                        genero,
                        correo.Trim(),
                        direccion.Trim(),
                        estadoCivil.Trim(),
                        user.Trim(),
                        password
                    );

                    _administradorService.CrearAdministrador(nuevoAdmin);

                    // Registrar tambien su contrato administrativo
                    Startup.GetContratoRepository().Add(new Contrato(nuevoAdmin, salario, "Administrador General", DateTime.Today, true));

                    _writer.ShowMessage("\nAdministrador registrado exitosamente.");
                    _writer.ShowMessage($"Codigo asignado: {nuevoAdmin.Id} | Cargo: Administrador | Usuario: {nuevoAdmin.User}");
                }
                else
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
