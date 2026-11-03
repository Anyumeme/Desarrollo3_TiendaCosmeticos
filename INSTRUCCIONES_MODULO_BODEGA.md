# Guia de Implementacion: Modulo de Bodega

## 1. Descripcion General del Modulo
El modulo de Bodega permite al usuario con rol Bodeguero gestionar el catalogo de productos cosmeticos de la tienda: registrar nuevos productos, editar informacion de productos existentes y consultar el inventario bajo diferentes criterios de busqueda y filtrado.

### Regla de Control de Acceso
El Bodeguero tiene estrictamente prohibido desactivar o dar de baja productos. La eliminacion logica (desactivacion) es una potestad exclusiva del Administrador.

---

## 2. Estructura de Archivos a Desarrollar
Siguiendo las directrices del proyecto (un archivo por accion, separacion de responsabilidades y un limite estricto de menos de 300 lineas por archivo), los componentes a crear y modificar son:

```text
TiendaCosmeticos/
├── Services/
│   └── ProductoService.cs
├── Controllers/
│   └── BodegueroController.cs
└── Commands/Bodega/
    ├── AgregarProductoCommand.cs
    ├── EditarProductoCommand.cs
    └── BuscarProductoCommand.cs
```

---

## 3. Especificacion Tecnica por Componente

### 3.1. Services/ProductoService.cs
Capa encargada de la logica de negocio y el control de reglas sobre productos. Debe recibir e inyectar `ProductoRepository`.

Metodos requeridos:
- `RegistrarProducto(string nombre, string marca, CategoriaProductos categoria, decimal precio, int stock, DateTime fechaVencimiento)`:
  - Genera el identificador automatico con prefijo 'PROD-' y numero secuencial de dos digitos (ejemplo: PROD-01, PROD-02) basandose en la cantidad total de productos registrados (activos e inactivos).
  - Valida que el precio sea mayor que cero.
  - Valida que el stock sea mayor o igual a cero.
  - Valida que la fecha de vencimiento sea igual o posterior a la fecha actual; si ya paso, lanza `ProductoVencidoException`.
  - Agrega la instancia al repositorio.

- `EditarProducto(string id, string nuevoNombre, string nuevaMarca, CategoriaProductos nuevaCategoria, decimal nuevoPrecio, int nuevoStock, DateTime nuevaFechaVencimiento)`:
  - Busca el producto por ID. Si no existe, lanza `RegistroNoEncontradoException`.
  - Si el producto esta en estado inactivo, lanza `RegistroInactivoException` (no se puede editar un producto inactivo sin reactivarlo previamente).
  - Actualiza los campos mediante el metodo `Actualizar(...)` de la entidad Producto.
  - Bajo ninguna circunstancia se permite modificar el campo `Id`.

- Metodos de consulta:
  - `ObtenerPorId(string id)`: Devuelve el producto o lanza `RegistroNoEncontradoException`.
  - `ObtenerTodos()`: Devuelve todos los productos (activos e inactivos indicando su estado).
  - `ObtenerPorMarca(string marca)`: Filtra productos que coincidan con la marca especificada.
  - `ObtenerSoloActivos()`: Filtra unicamente los productos con `Estado == true`.

---

### 3.2. Controllers/BodegueroController.cs
Controlador de interfaz de consola que coordina el menu del bodeguero usando el patron Command.

Estructura:
- Recibe en su constructor `Empleado bodeguero`, `ProductoService productoService` e `IConsoleWriter writer`.
- Inicializa un `Dictionary<int, ICommand>` con las siguientes opciones:
  - Opcion 1: `AgregarProductoCommand`
  - Opcion 2: `EditarProductoCommand`
  - Opcion 3: `BuscarProductoCommand`
  - Opcion 4: Salir / Cerrar sesion (retorna al login)
- El bucle `DisplayMenu()` limpia pantalla, muestra el encabezado con el nombre del bodeguero, despliega el menu, lee la opcion mediante `Validator` con rango 1 a 4 y ejecuta el comando seleccionado.

---

### 3.3. Commands/Bodega/AgregarProductoCommand.cs
Implementa `ICommand`. Responsable de capturar y confirmar el alta de un nuevo cosmetico.

Flujo de ejecucion:
1. Limpiar pantalla y mostrar titulo: "REGISTRO DE NUEVO PRODUCTO".
2. Captura de datos con `Validator`:
   - Nombre: `.NotEmpty().MaxLength(50)`
   - Marca: `.NotEmpty().MaxLength(40)`
   - Categoria: Desplegar menu con los valores de `CategoriaProductos` (1. Maquillaje, 2. CuidadoPiel, 3. CuidadoCabello, 4. Fragancias, 5. Accesorios, 6. Otros) y validar rango 1 a 6.
   - Precio: `.NotEmpty().DecimalPositivo()`
   - Stock Inicial: `.NotEmpty().EnteroNoNegativo()`
   - Fecha de Vencimiento: `.NotEmpty().FormatoFecha().FechaNoVencida()`
3. Flujo de confirmacion previa:
   - Mostrar en pantalla el resumen de todos los datos ingresados en espera.
   - Presentar menu de decision:
     [1] Confirmar y guardar producto
     [2] Editar algun campo en espera
     [3] Anular accion y descartar cambios
4. Si el usuario confirma, llamar a `_productoService.RegistrarProducto(...)`.
5. Manejar todo el bloque dentro de un `try/catch (TiendaException ex)` mostrando el mensaje de error con `_writer.ShowError(ex.Message)` sin interrumpir la ejecucion de la aplicacion.

---

### 3.4. Commands/Bodega/EditarProductoCommand.cs
Implementa `ICommand`. Responsable de la actualizacion de datos de productos ya registrados.

Flujo de ejecucion:
1. Limpiar pantalla y mostrar titulo: "EDICION DE PRODUCTO".
2. Solicitar el identificador del producto (ejemplo: PROD-01).
3. Obtener el producto mediante `_productoService.ObtenerPorId(...)`. Si no existe o esta inactivo, capturar la excepcion correspondiente y mostrar el mensaje explicativo.
4. Mostrar los valores actuales de cada campo y permitir ingresar el nuevo valor o presionar Enter para conservar el dato actual.
5. El identificador `Id` se muestra como dato bloqueado de solo lectura.
6. Presentar pantalla de vista previa con los datos modificados y solicitar confirmacion antes de aplicar los cambios en el servicio.

---

### 3.5. Commands/Bodega/BuscarProductoCommand.cs
Implementa `ICommand`. Coordina el submenú de busquedas y reportes de inventario.

Opciones del submenu:
1. Buscar producto por ID:
   - Pide el codigo del producto.
   - Muestra la ficha detallada: ID, Nombre, Marca, Categoria, Precio, Stock, Fecha de Vencimiento y Estado (ACTIVO o INACTIVO).
2. Listar todos los productos:
   - Muestra en formato tabular todos los productos registrados sin excepcion, indicando el estado de cada uno.
3. Listar productos por Marca:
   - Solicita la marca (ejemplo: Maybelline, Revlon).
   - Muestra la lista de productos que coinciden con dicha marca.
4. Listar solo productos activos:
   - Muestra en formato tabular unicamente los productos disponibles para la venta.
5. Volver al menu anterior.

---

## 4. Criterios de Evaluacion y Calidad
Para cumplir con la rubrica de calificacion del proyecto:
1. Prevencion de caidas: No utilizar `Console.ReadLine()` directamente para datos numericos o fechas. Emplear exclusivamente las validaciones de `Validator`.
2. Captura de errores: Centralizar el control de excepciones de negocio con `catch (TiendaException ex)`.
3. Ningun archivo debe superar las 300 lineas de codigo.
4. Conservar el estado de los datos: Los productos inactivos siguen existiendo y su ID no puede ser reutilizado para un producto nuevo.

