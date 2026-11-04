using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TiendaCosmeticos.Util
{
    // Utilidad especializada para la estructura y fechas reales de cedulas en Nicaragua
    public static class CedulaHelper
    {
        private static readonly Regex CedulaRegex = new(@"^\d{3}-?\d{6}-?\d{4}[A-Za-z]$", RegexOptions.Compiled);

        // Valida que cumpla el patron y que los 6 digitos centrales sean una fecha real de calendario (DDMMAA)
        public static bool ValidarCedula(string? texto, out string cedulaFormateada, out DateTime fechaNacimiento)
        {
            cedulaFormateada = string.Empty;
            fechaNacimiento = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }

            string entrada = texto.Trim().ToUpper();

            if (!CedulaRegex.IsMatch(entrada))
            {
                return false;
            }

            string limpia = entrada.Replace("-", "");
            string parteFecha = limpia.Substring(3, 6);

            // Validacion de fecha real (evita meses invalidos o dias inexistentes como 31 de febrero)
            if (!DateTime.TryParseExact(parteFecha, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaExtraida))
            {
                return false;
            }

            // Ajuste por si el anio de 2 digitos queda interpretado en el futuro
            if (fechaExtraida > DateTime.Today)
            {
                fechaExtraida = fechaExtraida.AddYears(-100);
            }

            fechaNacimiento = fechaExtraida;
            cedulaFormateada = $"{limpia[0..3]}-{limpia[3..9]}-{limpia[9..14]}";
            return true;
        }

        // Formatea cualquier cedula valida a formato estandar con guiones y letra mayuscula
        public static string FormatearCedula(string raw)
        {
            string limpia = (raw ?? string.Empty).Replace("-", "").Trim().ToUpper();
            if (limpia.Length != 14)
            {
                return raw ?? string.Empty;
            }
            return $"{limpia[0..3]}-{limpia[3..9]}-{limpia[9..14]}";
        }
    }
}

