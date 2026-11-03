namespace TiendaCosmeticos.Models
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public interface IController
    {
        void DisplayMenu();
    }
}
