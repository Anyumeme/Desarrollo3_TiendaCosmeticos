using TiendaCosmeticos.Models;
using System.Collections.Generic;

namespace TiendaCosmeticos.Util
{
    public interface IConsoleWriter
    {
        void DisplayInitialMessage(string name);
        void DisplayMenu(string headMessage, params string[] options);
        void ShowHeader(string title);
        void ShowMessage(string message);
        void ShowError(string error);
        void WaitForKey(string prompt = "Presione cualquier tecla para continuar...");
        void Clear();
    }
}
