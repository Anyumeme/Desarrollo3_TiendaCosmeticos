# Guia y Reglas de Negocio - Tienda de Cosmeticos

Este documento explica de forma sencilla y directa como funciona el negocio, que puede hacer cada trabajador y cuales son las reglas que rigen la tienda, sin enredos tecnicos ni explicaciones de codigo.

---

## 1. De que trata el negocio
Es una tienda comercial dedicada a la venta de cosmeticos, maquillaje y productos de cuidado personal para la mujer (marcas como Maybelline, L'Oreal, Revlon, etc.).

El sistema organiza la operacion diaria entre tres roles con responsabilidades bien definidas:
1. Administrador (Gerencia y control general).
2. Encargado de Bodega (Inventario, stock y fechas de vencimiento).
3. Vendedor (Atencion a clientes, facturacion y caja).

---

## 2. Los Roles y sus Permisos

### A. El Administrador
Es el encargado de la supervision de todo el negocio.
- Gestion de Personal:
  - Puede dar de alta a nuevos trabajadores (Vendedores, Bodegueros e incluso otros Administradores).
  - Define el salario mensual de cada empleado y puede consultar la planilla de pagos.
  - Puede actualizar los datos personales de cualquier trabajador (salvo su codigo que es fijo).
- Regla de Oro de Seguridad (Jerarquia de Victor):
  - Victor es el Administrador Principal (Dueño / Cuenta Raiz).
  - Solo Victor tiene la autoridad de desactivar o reactivar a otros Administradores.
  - Ningun administrador secundario puede quitarle acceso a otro administrador.
  - La cuenta de Victor nunca puede ser desactivada por nadie.
- Gestion de Productos y Regla Sanitaria:
  - Puede activar o desactivar productos.
  - Regla Sanitaria: Si un cosmetico ya caduco (fecha de vencimiento vencida), el sistema prohibe terminantemente reactivarlo para proteger la salud de las clientas.
- Gestion de Ventas:
  - Puede facturar ventas, consultar el historial financiero y es el unico que puede anular facturas.

---

### B. El Encargado de Bodega
Es el responsable de mantener abastecida la tienda.
- Registro de Cosmeticos:
  - Ingresa nuevos productos indicando: Nombre, Marca, Categoria (Maquillaje, Cuidado de Piel, etc.), Precio unitario, Cantidad inicial (stock) y Fecha de caducidad.
  - La fecha de vencimiento no puede ser anterior al dia de hoy.
- Edicion:
  - Puede corregir precios, marcas, categorias y reabastecer stock cuando llega nuevo pedido.
- Consultas:
  - Puede revisar todo el inventario, buscar por marca, ver solo vigentes o revisar productos apartados/inactivos.
- Regla Estricta:
  - El bodeguero NO puede desactivar productos (solo el Administrador puede tomar la decision comercial o legal de sacar un producto del catalogo).

---

### C. El Vendedor
Es quien atiende al publico en el mostrador.
- Registro de Clientes:
  - Registra a las clientas con su cedula nicaraguense, nombre, telefono celular y fecha de nacimiento.
  - Edad minima: La clienta debe tener al menos 16 años para ser registrada.
  - No se permiten cedulas ni numeros de celular repetidos.
- Realizar Ventas (Carrito de Compras):
  - Puede buscar productos por su codigo directo o navegando por categoria.
  - Control de Stock en Tiempo Real: No se puede vender mas unidades de las que realmente hay en los estantes de bodega.
  - Limite de Venta al Detalle: Maximo 200 unidades por producto en una sola compra (para evitar desabastecimiento o reventa mayorista no autorizada).
  - Descuentos Automaticos por Volumen:
    - Compras mayores a C$ 1,000 -> 10% de descuento automatico.
    - Compras mayores a C$ 2,000 -> 15% de descuento automatico.
  - Vista previa de Comprobante: Antes de cobrar, se muestra la factura completa y se pide confirmacion final obligatoria (S/N).
- Consulta de Historial:
  - Puede consultar compras anteriores, reimprimir comprobantes y verificar facturas activas o canceladas.

---

## 3. Reglas Especiales de la Tienda

### 1. Anulacion de Facturas y Devolucion de Stock
Si un cliente devuelve una compra o hubo un error al facturar:
- La factura cambia su estado a ANULADA (inactiva).
- Cada producto vendido en esa compra se reincorpora automaticamente al inventario de bodega sumando sus cantidades de vuelta al stock.
- En los reportes de ventas, el dinero de facturas anuladas se separa del dinero real recaudado para no dar cifras infladas o falsas en la contabilidad.

### 2. Busquedas Ultra-Flexibles
El sistema no obliga al usuario a escribir de una forma rigida:
- Cedulas: Se pueden buscar con guiones (001-110997-0041S), sin guiones (0011109970041s), con espacios o en minusculas. El sistema siempre las encuentra.
- Codigos: 
  - Todo el personal usa el formato unico EMP-0001, EMP-0002, EMP-0003, etc.
  - Puedes escribir emp-2, EMP-0002, ven-2 o simplemente el numero 2 o 0002 y el sistema sabra exactamente a quien te refieres.
  - Los productos usan PROD-0001, PROD-0002, etc., y puedes buscarlos por codigo completo, por nombre, por marca o solo por su numero (1, 2).

### 3. Politica de Cero Borrado Fisico (Eliminacion Logica)
Por normativas fiscales, contables y de auditoria:
- Ningun registro se elimina permanentemente de la base de datos.
- Cuando se "elimina" un producto, cliente o trabajador, simplemente pasa a estado INACTIVO.
- Todo registro inactivo puede ser reactivado en el futuro si la administracion asi lo decide.

### 4. Cero Caidas y Confirmaciones Previas
- Ante cualquier dato invalido (letras donde van numeros, fechas falsas, campos vacios), el sistema muestra una advertencia amigable y vuelve a solicitar el dato sin cerrarse jamas.
- Toda accion importante (guardar una venta, dar de baja a alguien o editar datos) pide confirmacion con (S/N) antes de ejecutarse.
