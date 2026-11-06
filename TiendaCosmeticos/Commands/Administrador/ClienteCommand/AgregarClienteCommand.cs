using System;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Administrador.ClienteCommand
{
    public class AgregarClienteCommand : ICommand
    {
        private readonly IConsoleWriter _writer;
        private readonly ClienteService _clienteService;

        public AgregarClienteCommand(IConsoleWriter writer, ClienteService clienteService)
        {
            _writer = writer;
            _clienteService = clienteService;
        }

        public void Execute()
        {
            _writer.Clear();
            _writer.ShowHeader("REGISTRO DE NUEVO CLIENTE");

            // 1. Cedula y extraccion automatica de fecha de nacimiento
            string cedula = new Validator("Cedula (ej. 001-120590-0001A): ")
                .NotEmpty()
                .FormatoCedula()
                .ReadCedula(out DateTime fechaNacimiento);

            _writer.ShowMessage($"Fecha de nacimiento extraida: {fechaNacimiento:dd/MM/yyyy}");

            // 2. Nombre completo
            string nombre = new Validator("Nombre completo: ")
                .NotEmpty()
                .SoloLetras()
                .MinLength(3)
                .MaxLength(50)
                .ReadAndValidate();

            // 3. Celular
            string celularTexto = new Validator("Numero de celular (8 digitos, inicia con 8, 7 o 5): ")
                .NotEmpty()
                .NumeroCelular()
                .ReadAndValidate();
            int celular = int.Parse(celularTexto.Trim());

            // 4. Genero
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

            // 5. Confirmacion antes de registrar
            string confirmacion = new Validator("\n¿Confirma registrar al nuevo cliente? (S/N): ")
                .NotEmpty()
                .ConfirmacionSN()
                .ReadAndValidate();

            if (confirmacion.Trim().ToUpper() != "S")
            {
                _writer.ShowMessage("\nRegistro cancelado por el usuario.");
                _writer.WaitForKey();
                return;
            }

            // 6. Invocacion segura al servicio
            try
            {
                Cliente nuevo = _clienteService.RegistrarCliente(
                    cedula,
                    nombre,
                    celular,
                    fechaNacimiento,
                    genero
                );

                _writer.ShowMessage("\nCliente registrado exitosamente.");
                _writer.ShowMessage($"Codigo asignado: {nuevo.Id} | Nombre: {nuevo.Nombre} | Cedula: {nuevo.Cedula}");
            }
            catch (TiendaException ex)
            {
                _writer.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                _writer.ShowError($"Ocurrio un error inesperado al registrar el cliente: {ex.Message}");
            }

            _writer.WaitForKey();
        }

        public void Undo()
        {
        }
    }
}
