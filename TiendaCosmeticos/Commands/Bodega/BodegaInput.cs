
using System;
using TiendaCosmeticos.Util;

namespace TiendaCosmeticos.Commands.Bodega
{
    public class BodegaInput : Validator
    {
        private readonly string _mensaje;

        public BodegaInput(string mensaje) : base(mensaje)
        {
            _mensaje = mensaje;
        }

        public string ReadOptional(string valorActual)
        {
            while (true)
            {
                Console.Write(_mensaje);

                string entrada = Console.ReadLine() ?? string.Empty;

                // Si presiona Enter, conserva el valor actual.
                if (entrada.Length == 0)
                    return valorActual;

                bool valido = true;

                foreach (var regla in _rules)
                {
                    var resultado = regla(entrada);

                    if (!resultado.IsValid)
                    {
                        Console.WriteLine(resultado.ErrorMessage);
                        valido = false;
                        break;
                    }
                }

                if (valido)
                    return entrada;
            }
        }
    }
}


