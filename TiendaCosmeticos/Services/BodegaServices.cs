
using System;
using System.Collections.Generic;
using System.Linq;
using TiendaCosmeticos.Exceptions;
using TiendaCosmeticos.Models;
using TiendaCosmeticos.Repositories;

namespace TiendaCosmeticos.Services
{
    public class BodegaServices
    {
        private readonly ProductoRepository _productoRepository;

        public BodegaServices(ProductoRepository productoRepository)
        {
            _productoRepository = productoRepository
                ?? throw new ArgumentNullException(nameof(productoRepository));
        }

        // REGISTRAR PRODUCTO
        public Producto RegistrarProducto(
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fechaVencimiento)
        {
            ValidarDatosProducto(
                nombre,
                marca,
                categoria,
                precio,
                stock,
                fechaVencimiento);

            int siguienteNumero = _productoRepository.GetAll().Count + 1;
            string nuevoId;

            // Evitar reutilizar identificadores existentes.
            do
            {
                nuevoId = $"PROD-{siguienteNumero:D4}";
                siguienteNumero++;
            }
            while (_productoRepository.GetById(nuevoId) != null);

            var producto = new Producto(
                nuevoId,
                nombre.Trim(),
                marca.Trim(),
                categoria,
                precio,
                stock,
                fechaVencimiento.Date);

            _productoRepository.Add(producto);

            return producto;
        }

        // EDITAR PRODUCTO
        public Producto EditarProducto(
            string id,
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fechaVencimiento)
        {
            Producto producto = ObtenerPorId(id);

            if (!producto.Estado)
            {
                throw new RegistroInactivoException("producto", id);
            }

            ValidarDatosProducto(
                nombre,
                marca,
                categoria,
                precio,
                stock,
                fechaVencimiento);

            producto.Actualizar(
                nombre.Trim(),
                marca.Trim(),
                categoria,
                precio,
                stock,
                fechaVencimiento.Date);

            return producto;
        }

        // CONSULTAR POR ID
        public Producto ObtenerPorId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new TiendaException(
                    "Debe proporcionar un identificador de producto válido.");
            }

            Producto? producto = _productoRepository.GetById(id.Trim());

            if (producto == null)
            {
                throw new RegistroNoEncontradoException("producto", id);
            }

            return producto;
        }

        // CONSULTAR TODOS LOS PRODUCTOS
        public List<Producto> ObtenerTodos()
        {
            return _productoRepository.GetAll();
        }

        // CONSULTAR POR MARCA
        public List<Producto> ObtenerPorMarca(string marca)
        {
            if (string.IsNullOrWhiteSpace(marca))
            {
                throw new TiendaException(
                    "Debe proporcionar una marca para realizar la búsqueda.");
            }

            return _productoRepository.GetByMarca(marca.Trim());
        }

        // CONSULTAR SOLAMENTE PRODUCTOS ACTIVOS
        public List<Producto> ObtenerSoloActivos()
        {
            return _productoRepository.GetActivos();
        }

        // CONSULTAR SOLAMENTE PRODUCTOS INACTIVOS
        public List<Producto> ObtenerSoloInactivos()
        {
            return _productoRepository.GetInactivos();
        }

        // CONSULTAR PRODUCTOS CADUCADOS O VENCIDOS
        public List<Producto> ObtenerVencidos()
        {
            return _productoRepository.GetAll().Where(p => p.EstaVencido()).ToList();
        }

        // CONSULTAR PRODUCTOS CON STOCK BAJO O AGOTADO (<= 5)
        public List<Producto> ObtenerStockBajo(int limite = 5)
        {
            return _productoRepository.GetAll().Where(p => p.Stock <= limite).ToList();
        }

        // VALIDACIONES CENTRALIZADAS DEL SERVICIO
        private static void ValidarDatosProducto(
            string nombre,
            string marca,
            CategoriaProductos categoria,
            decimal precio,
            int stock,
            DateTime fechaVencimiento)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new TiendaException(
                    "El nombre del producto es obligatorio.");
            }

            if (nombre.Trim().Length < 3 || nombre.Trim().Length > 50)
            {
                throw new TiendaException(
                    "El nombre del producto debe contener entre 3 y 50 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(marca))
            {
                throw new TiendaException(
                    "La marca del producto es obligatoria.");
            }

            if (marca.Trim().Length < 2 || marca.Trim().Length > 40)
            {
                throw new TiendaException(
                    "La marca del producto debe contener entre 2 y 40 caracteres.");
            }

            if (!Enum.IsDefined(typeof(CategoriaProductos), categoria))
            {
                throw new TiendaException(
                    "La categoría seleccionada no es válida.");
            }

            if (precio <= 0)
            {
                throw new TiendaException(
                    "El precio debe ser mayor que cero.");
            }

            if (stock < 0)
            {
                throw new TiendaException(
                    "El stock no puede ser negativo.");
            }

            if (fechaVencimiento.Date < DateTime.Today)
            {
                throw new ProductoVencidoException(nombre.Trim(), fechaVencimiento);
            }
        }
    }
}
