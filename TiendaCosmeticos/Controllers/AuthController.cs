using System;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Services;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Controllers
{
    public class AuthController
    {
        private readonly AuthService _authService;
        private readonly IConsoleWriter _writer;
        private readonly int _maxIntentosPermitidos;

        public AuthController(AuthService authService, IConsoleWriter writer, int maxIntentos = 3)
        {
            _authService = authService;
            _writer = writer;
            _maxIntentosPermitidos = maxIntentos;
        }

        public IAutentication? IniciarSesion()
        {
            bool cancelar = false;
            while (!cancelar)
            {
                _writer.Clear();
                _writer.DisplayInitialMessage("TIENDA DE COSMETICOS - INICIO DE SESION");
                _writer.DisplayMenu(
                    "Seleccione una opcion:",
                    "Ingresar al sistema",
                    "Salir"
                );

                string opcion = new Validator("Ingrese su opcion: ")
                    .NotEmpty()
                    .OnlyDigits()
                    .InputBetweenXandY(1, 2)
                    .ReadAndValidate();

                if (opcion == "2")
                {
                    cancelar = true;
                    return null;
                }

                int intentosRealizados = 0;
                bool bloqueado = false;

                while (intentosRealizados < _maxIntentosPermitidos && !bloqueado)
                {
                    _writer.Clear();
                    _writer.ShowHeader("AUTENTICACION DE USUARIO");

                    string user = new Validator("Usuario: ")
                        .NotEmpty("El usuario no puede estar vacio.")
                        .ReadAndValidate();

                    string password = new Validator("Contrasenia: ")
                        .NotEmpty("La contrasenia no puede estar vacia.")
                        .ReadPassword();

                    IAutentication? usuario = _authService.Autenticar(user, password);
                    if (usuario != null)
                    {
                        return usuario;
                    }

                    intentosRealizados++;

                    if (intentosRealizados >= _maxIntentosPermitidos)
                    {
                        _writer.Clear();
                        _writer.ShowError("Por motivos de seguridad, el sistema ha sido bloqueado.");
                        _writer.WaitForKey();
                        bloqueado = true;
                    }
                    else
                    {
                        _writer.ShowError("Usuario o contrasenia incorrectos.");
                        _writer.WaitForKey();
                    }
                }
            }

            return null;
        }
    }
}
