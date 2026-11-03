using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiendaCosmeticos.Util
{
    public class Validator
    {
        private readonly string _promptMessage;
        protected readonly List<Func<string, (bool IsValid, string ErrorMessage)>> _rules = new();

        private static readonly Regex CedulaRegex = new(@"^\d{3}-?\d{6}-?\d{4}[A-Za-z]$", RegexOptions.Compiled);

        public Validator(string promptMessage)
        {
            _promptMessage = promptMessage;
        }

        public Validator NotEmpty(string errorMessage = "Error: No se ingreso ningun valor")
        {
            _rules.Add(input => (!string.IsNullOrWhiteSpace(input), errorMessage));
            return this;
        }

        public Validator OnlyDigits(string errorMessage = "Error: Solo se permiten digitos numericos")
        {
            _rules.Add(input => (input.Trim().All(char.IsDigit), errorMessage));
            return this;
        }

        public Validator InputBetweenXandY(int x, int y, string errorMessage = "Error: Solo se permiten valores dentro del rango indicado")
        {
            _rules.Add(input => (CheckRange(input, x, y), errorMessage));
            return this;
        }

        private bool CheckRange(string input, int x, int y)
        {
            if (!int.TryParse(input, out int selectedOption))
            {
                return false;
            }
            return selectedOption >= x && selectedOption <= y;
        }

        // Validador de cedula nicaraguense con fecha real en el centro (DDMMAA)
        public Validator FormatoCedula(string errorMessage = "Error: Formato de cedula invalido. Debe ser 000-ddMMyy-0000X con fecha real")
        {
            _rules.Add(input => (ValidarCedula(input, out _, out _), errorMessage));
            return this;
        }

        public Validator FormatoCorreo(string errorMessage = "Error: El correo electronico no tiene un formato valido")
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            _rules.Add(input => (Regex.IsMatch(input.Trim(), patron), errorMessage));
            return this;
        }

        // Celular formato Nicaragua: 8 digitos e inicia con 8, 7 o 5
        public Validator NumeroCelular(string errorMessage = "Error: El numero celular debe tener 8 digitos y comenzar con 8, 7 o 5")
        {
            _rules.Add(input =>
            {
                string trimmed = input.Trim();
                bool esValido = trimmed.Length == 8 &&
                                trimmed.All(char.IsDigit) &&
                                (trimmed.StartsWith('8') || trimmed.StartsWith('7') || trimmed.StartsWith('5'));
                return (esValido, errorMessage);
            });
            return this;
        }

        // Precios y salarios: valor decimal positivo con punto o coma
        public Validator DecimalPositivo(string errorMessage = "Error: Debe ingresar un valor numerico mayor a cero")
        {
            _rules.Add(input =>
            {
                string limpia = input.Trim().Replace(',', '.');
                bool esValido = decimal.TryParse(limpia, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor) && valor > 0;
                return (esValido, errorMessage);
            });
            return this;
        }

        // Para cantidades a vender (mayor a 0)
        public Validator EnteroPositivo(string errorMessage = "Error: Debe ingresar un numero entero mayor a cero")
        {
            _rules.Add(input =>
            {
                bool esValido = int.TryParse(input.Trim(), out int valor) && valor > 0;
                return (esValido, errorMessage);
            });
            return this;
        }

        // Para stock de productos (permite 0 o mas)
        public Validator EnteroNoNegativo(string errorMessage = "Error: El numero no puede ser negativo")
        {
            _rules.Add(input =>
            {
                bool esValido = int.TryParse(input.Trim(), out int valor) && valor >= 0;
                return (esValido, errorMessage);
            });
            return this;
        }

        // Confirmaciones S/N para reactivaciones o eliminaciones logicas
        public Validator ConfirmacionSN(string errorMessage = "Error: Respuesta invalida. Ingrese 'S' para Si o 'N' para No")
        {
            _rules.Add(input =>
            {
                string resp = input.Trim().ToUpper();
                bool esValido = resp == "S" || resp == "N" || resp == "SI" || resp == "NO";
                return (esValido, errorMessage);
            });
            return this;
        }

        public Validator FormatoFecha(string formato = "dd/MM/yyyy", string errorMessage = "Error: Formato de fecha invalido. Use dd/MM/yyyy")
        {
            _rules.Add(input =>
            {
                bool esValida = DateTime.TryParseExact(input.Trim(), formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
                return (esValida, errorMessage);
            });
            return this;
        }

        public Validator FechaNoFutura(string formato = "dd/MM/yyyy", string errorMessage = "Error: La fecha de nacimiento no puede ser futura")
        {
            _rules.Add(input =>
            {
                if (DateTime.TryParseExact(input.Trim(), formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                {
                    return (fecha.Date <= DateTime.Today, errorMessage);
                }
                return (false, "Error: Formato de fecha invalido para verificar.");
            });
            return this;
        }

        // Para productos cosmeticos en bodega: la fecha de vencimiento debe ser hoy o posterior
        public Validator FechaNoVencida(string formato = "dd/MM/yyyy", string errorMessage = "Error: La fecha de vencimiento no puede ser anterior a la fecha actual")
        {
            _rules.Add(input =>
            {
                if (DateTime.TryParseExact(input.Trim(), formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                {
                    return (fecha.Date >= DateTime.Today, errorMessage);
                }
                return (false, "Error: Formato de fecha invalido (use dd/MM/yyyy).");
            });
            return this;
        }

        public Validator SinEspacios(string errorMessage = "Error: El valor no debe contener espacios en blanco")
        {
            _rules.Add(input => (!input.Any(char.IsWhiteSpace), errorMessage));
            return this;
        }

        public Validator MinLength(int minLength, string errorMessage = "")
        {
            string msg = string.IsNullOrWhiteSpace(errorMessage)
                ? $"Error: Debe contener al menos {minLength} caracteres"
                : errorMessage;

            _rules.Add(input => (input.Trim().Length >= minLength, msg));
            return this;
        }

        public Validator MaxLength(int maxLength, string errorMessage = "")
        {
            string msg = string.IsNullOrWhiteSpace(errorMessage)
                ? $"Error: No debe exceder los {maxLength} caracteres"
                : errorMessage;

            _rules.Add(input => (input.Trim().Length <= maxLength, msg));
            return this;
        }

        public Validator SoloLetras(string errorMessage = "Error: Solo se permiten letras y espacios")
        {
            _rules.Add(input =>
            {
                string trimmed = input.Trim();
                bool esValido = !string.IsNullOrEmpty(trimmed) && trimmed.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
                return (esValido, errorMessage);
            });
            return this;
        }

        public Validator PasswordSegura(string errorMessage = "Error: La contrasenia debe tener al menos 8 caracteres e incluir mayuscula, minuscula, numero y caracter especial")
        {
            _rules.Add(input =>
            {
                bool esValida = input.Length >= 8 &&
                                input.Any(char.IsUpper) &&
                                input.Any(char.IsLower) &&
                                input.Any(char.IsDigit) &&
                                input.Any(c => !char.IsLetterOrDigit(c));
                return (esValida, errorMessage);
            });
            return this;
        }

        // Metodo reutilizable para verificar la estructura y fecha real de una cedula
        public static bool ValidarCedula(string? texto, out string cedulaFormateada, out DateTime fechaNacimiento)
        {
            cedulaFormateada = string.Empty;
            fechaNacimiento = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(texto))
                return false;

            string entrada = texto.Trim().ToUpper();

            if (!CedulaRegex.IsMatch(entrada))
                return false;

            string limpia = entrada.Replace("-", "");
            string parteFecha = limpia.Substring(3, 6);

            // Validamos que sea una fecha de calendario real (DDMMAA)
            if (!DateTime.TryParseExact(parteFecha, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaExtraida))
            {
                return false;
            }

            // Ajuste por si el anio de 2 digitos queda en el futuro
            if (fechaExtraida > DateTime.Today)
            {
                fechaExtraida = fechaExtraida.AddYears(-100);
            }

            fechaNacimiento = fechaExtraida;
            cedulaFormateada = $"{limpia[0..3]}-{limpia[3..9]}-{limpia[9..14]}";
            return true;
        }

        // Formatea cualquier cedula valida a formato estandar con guiones
        public static string FormatearCedula(string raw)
        {
            string limpia = (raw ?? string.Empty).Replace("-", "").Trim().ToUpper();
            if (limpia.Length != 14) return raw ?? string.Empty;
            return $"{limpia[0..3]}-{limpia[3..9]}-{limpia[9..14]}";
        }

        public string ReadAndValidate()
        {
            while (true)
            {
                Console.Write(_promptMessage);
                string input = Console.ReadLine() ?? string.Empty;
                bool allValid = true;

                foreach (var rule in _rules)
                {
                    var result = rule(input);
                    if (!result.IsValid)
                    {
                        Console.WriteLine(result.ErrorMessage);
                        allValid = false;
                        break;
                    }
                }

                if (allValid) return input;
            }
        }

        // Lee la entrada, valida y devuelve la cedula limpia con guiones y letra mayuscula
        public string ReadCedula()
        {
            string raw = ReadAndValidate();
            return FormatearCedula(raw);
        }

        // Lee contrasenia mostrando asteriscos y aplicando las reglas encadenadas
        public string ReadPassword()
        {
            while (true)
            {
                Console.Write(_promptMessage);
                string password = string.Empty;

                while (true)
                {
                    ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

                    if (tecla.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break;
                    }

                    if (tecla.Key == ConsoleKey.Backspace)
                    {
                        if (password.Length > 0)
                        {
                            password = password.Substring(0, password.Length - 1);
                            Console.Write("\b \b");
                        }
                    }
                    else if (!char.IsControl(tecla.KeyChar))
                    {
                        password += tecla.KeyChar;
                        Console.Write("*");
                    }
                }

                bool allValid = true;
                foreach (var rule in _rules)
                {
                    var result = rule(password);
                    if (!result.IsValid)
                    {
                        Console.WriteLine(result.ErrorMessage);
                        allValid = false;
                        break;
                    }
                }

                if (allValid) return password;
            }
        }
    }
}
