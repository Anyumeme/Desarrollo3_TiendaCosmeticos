
namespace TiendaCosmeticos.Models
{
    public interface IAutentication
    {
        string User { get; }
        string Password { get; }
        Rol Rol { get; }
    }
}
