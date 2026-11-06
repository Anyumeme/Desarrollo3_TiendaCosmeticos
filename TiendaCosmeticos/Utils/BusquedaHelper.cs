using System;
using System.Text.RegularExpressions;

namespace TiendaCosmeticos.Util
{
    // Utilidad centralizada para normalizar y comparar IDs y Cedulas de forma flexible
    public static class BusquedaHelper
    {
        // Normaliza cualquier formato de cedula quitando guiones y espacios
        public static string NormalizarCedula(string? cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return string.Empty;
            }
            return cedula.Replace("-", "").Replace(" ", "").Trim().ToUpper();
        }

        // Comprueba si dos cedulas coinciden independientemente de guiones, espacios o mayusculas
        public static bool CoincideCedula(string? cedulaAlmacenada, string? criterio)
        {
            if (string.IsNullOrWhiteSpace(cedulaAlmacenada) || string.IsNullOrWhiteSpace(criterio))
            {
                return false;
            }

            return string.Equals(NormalizarCedula(cedulaAlmacenada), NormalizarCedula(criterio), StringComparison.OrdinalIgnoreCase);
        }

        // Comprueba si un ID coincide con el criterio ingresado
        // Admite:
        // - Mayusculas/minusculas variadas: "vEn-0001", "Ven-0001", "veN-0001", "ven-0001"
        // - Sin guion: "ven0001", "ven 0001"
        // - Ceros variables o sin relleno: "ven-1", "ven-01", "ven-001"
        // - Solo el numero o formato D4: "0001", "1"
        public static bool CoincideId(string? idAlmacenado, string? criterio)
        {
            if (string.IsNullOrWhiteSpace(idAlmacenado) || string.IsNullOrWhiteSpace(criterio))
            {
                return false;
            }

            string idLimpio = idAlmacenado.Trim().ToUpper();
            string critLimpio = criterio.Trim().ToUpper();

            // 1. Coincidencia directa ignorando mayusculas
            if (string.Equals(idLimpio, critLimpio, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 2. Coincidencia sin guiones ni espacios intermedios
            string idSinSep = idLimpio.Replace("-", "").Replace(" ", "");
            string critSinSep = critLimpio.Replace("-", "").Replace(" ", "");
            if (string.Equals(idSinSep, critSinSep, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 3. Descomposicion en prefijo alfabetico y sufijo numerico
            Match matchAlmacenado = Regex.Match(idLimpio, @"^([A-Z]+)[-_ ]*(\d+)$");
            if (matchAlmacenado.Success)
            {
                string prefijoAlm = matchAlmacenado.Groups[1].Value;
                int numeroAlm = int.Parse(matchAlmacenado.Groups[2].Value);

                // Caso A: El usuario ingreso prefijo y numero con ceros variables (ej. "ven-1" o "ven-01")
                Match matchCrit = Regex.Match(critLimpio, @"^([A-Z]+)[-_ ]*(\d+)$");
                if (matchCrit.Success)
                {
                    string prefijoCrit = matchCrit.Groups[1].Value;
                    int numeroCrit = int.Parse(matchCrit.Groups[2].Value);

                    if (prefijoAlm == prefijoCrit && numeroAlm == numeroCrit)
                    {
                        return true;
                    }
                }

                // Caso B: El usuario ingreso solamente el numero (ej. "0001" o "1")
                if (int.TryParse(critSinSep, out int soloNumeroCrit))
                {
                    if (numeroAlm == soloNumeroCrit)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

