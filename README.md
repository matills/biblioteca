# 📚 Sistema de Gestión de Biblioteca

Un sistema completo de gestión de biblioteca desarrollado en **ASP.NET Core MVC** con **Entity Framework Core** que permite administrar libros, autores, categorías, usuarios y préstamos.

## 🚀 Características Principales

### ✅ Funcionalidades Implementadas
- **Gestión de Libros**: CRUD completo con upload de imágenes e importación desde Excel
- **Gestión de Autores**: Creación y administración de autores
- **Gestión de Categorías**: Organización por categorías (descripción opcional)
- **Gestión de Usuarios**: Registro y administración de usuarios de la biblioteca
- **Sistema de Préstamos**: Control completo de préstamos, devoluciones y renovaciones
- **Reportes**: Estadísticas y reportes de usuarios y préstamos
- **Validaciones**: DataAnnotations y validaciones personalizadas
- **Relaciones**: One-to-One y One-to-Many implementadas
- **Inyección de Dependencias**: Configurada correctamente
- **UI Moderna**: Interfaz responsive con Bootstrap y estilos personalizados

### 🎯 Requisitos del Proyecto Cumplidos
- ✅ Framework .NET MVC instalado y configurado
- ✅ Proyecto creado con plantilla MVC
- ✅ Navegación completa por estructura de carpetas MVC
- ✅ Entity Framework configurado con NuGet
- ✅ Conexión a base de datos y DbContext configurados
- ✅ Database First y Code First implementados
- ✅ Clases Modelo con DataAnnotations
- ✅ Migrations para generación de tablas
- ✅ Relaciones One-to-One y One-to-Many
- ✅ Controladores MVC con Entity Framework
- ✅ ActionResult e IActionResult implementados
- ✅ Estilos personalizados en las vistas
- ✅ Upload de imágenes en ABM de libros
- ✅ Importación desde Excel en libros
- ✅ Inyección de dependencias aplicada

## 🛠️ Tecnologías Utilizadas

- **Framework**: ASP.NET Core 6.0 MVC
- **ORM**: Entity Framework Core
- **Base de Datos**: SQL Server
- **Frontend**: HTML5, CSS3, Bootstrap 5, JavaScript
- **Iconos**: Font Awesome
- **Validaciones**: DataAnnotations + jQuery Validation

## 📋 Prerrequisitos

Antes de ejecutar el proyecto, asegúrate de tener instalado:

1. **Visual Studio 2022** (Community, Professional o Enterprise)
2. **.NET 6.0 SDK** o superior
3. **SQL Server** (LocalDB, Express, o versión completa)
4. **Git** (opcional, para clonar el repositorio)

## 🔧 Instalación y Configuración

### 1. Clonar o Descargar el Proyecto

```bash
git clone [URL_DEL_REPOSITORIO]
cd biblioteca
```

### 2. Configurar la Cadena de Conexión

Edita el archivo `appsettings.json` y configura la cadena de conexión según tu entorno:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BibliotecaDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Opciones de conexión:**

- **LocalDB**: `Server=(localdb)\\mssqllocaldb;Database=BibliotecaDB;Trusted_Connection=true;MultipleActiveResultSets=true`
- **SQL Server Express**: `Server=.\\SQLEXPRESS;Database=BibliotecaDB;Trusted_Connection=true;MultipleActiveResultSets=true`
- **SQL Server con autenticación**: `Server=SERVIDOR;Database=BibliotecaDB;User Id=usuario;Password=contraseña;MultipleActiveResultSets=true`

### 3. Restaurar Paquetes NuGet

Abre una terminal en la carpeta del proyecto y ejecuta:

```bash
dotnet restore
```

O desde Visual Studio: `Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes` y ejecuta:

```powershell
Update-Package
```

### 4. Crear y Configurar la Base de Datos

#### Opción A: Usando Migrations (Recomendado)

1. Abre la **Consola del Administrador de Paquetes** en Visual Studio
2. Ejecuta los siguientes comandos:

```powershell
# Crear una nueva migración (si no existe)
Add-Migration InitialCreate

# Aplicar las migraciones a la base de datos
Update-Database
```

#### Opción B: Usando .NET CLI

```bash
# Crear migración
dotnet ef migrations add InitialCreate

# Actualizar base de datos
dotnet ef database update
```

### 5. Verificar la Instalación

1. Compila el proyecto: `Ctrl + Shift + B`
2. Ejecuta el proyecto: `F5` o `Ctrl + F5`
3. El navegador debería abrir en `https://localhost:xxxx`

## 🗃️ Estructura de la Base de Datos

El sistema creará automáticamente las siguientes tablas:

### Tablas Principales

- **Authors**: Información de autores
- **Categorias**: Categorías de libros
- **Libros**: Catálogo de libros con relaciones
- **Usuarios**: Usuarios registrados en el sistema
- **Prestamos**: Registro de préstamos y devoluciones

### Relaciones Implementadas

- **Autor → Libros**: One-to-Many (Un autor puede tener muchos libros)
- **Categoría → Libros**: One-to-Many (Una categoría puede tener muchos libros)
- **Usuario → Préstamos**: One-to-Many (Un usuario puede tener muchos préstamos)
- **Libro → Préstamos**: One-to-Many (Un libro puede tener muchos préstamos)

## 🎮 Uso del Sistema

### 1. Configuración Inicial

1. **Crear Categorías**: Ve a `Categorías > Nueva Categoría`
2. **Registrar Autores**: Ve a `Autores > Nuevo Autor`
3. **Agregar Libros**: Ve a `Libros > Nuevo Libro`
4. **Registrar Usuarios**: Ve a `Usuarios > Nuevo Usuario`

### 2. Gestión de Préstamos

1. **Crear Préstamo**: `Préstamos > Nuevo Préstamo`
2. **Devolver Libro**: Desde la lista de préstamos o detalles
3. **Renovar Préstamo**: Botón de renovación en préstamos activos
4. **Consultar Historial**: Ver detalles de usuario o préstamo

### 3. Funcionalidades Avanzadas

#### Upload de Imágenes
- Al crear/editar libros, puedes subir una imagen de portada
- Formatos soportados: JPG, PNG, GIF
- Tamaño máximo recomendado: 2MB

#### Importación desde Excel
- Ve a `Libros > Importar Excel`
- Descarga la plantilla o usa el formato especificado
- Columnas requeridas: Título, ISBN, Autor, Categoría, Año, Páginas, Cantidad, Descripción

#### Reportes
- **Reporte de Usuarios**: Estadísticas completas de préstamos por usuario
- **Filtros Avanzados**: Por estado, fechas, categorías
- **Exportación**: Los reportes se pueden imprimir o exportar

## 🔍 Solución de Problemas Comunes

### Error de Conexión a Base de Datos

**Problema**: `Cannot open database "BibliotecaDB" requested by the login`

**Solución**:
1. Verifica que SQL Server esté ejecutándose
2. Confirma la cadena de conexión en `appsettings.json`
3. Ejecuta `Update-Database` nuevamente

### Error de Migrations

**Problema**: `No migrations configuration type was found`

**Solución**:
```powershell
# Eliminar migraciones existentes
Remove-Migration

# Crear nueva migración
Add-Migration InitialCreate

# Aplicar a la base de datos
Update-Database
```

### Problemas con Paquetes NuGet

**Problema**: Errores de paquetes faltantes

**Solución**:
```bash
# Limpiar y restaurar
dotnet clean
dotnet restore
dotnet build
```

### Error de Certificados HTTPS

**Problema**: Advertencias de certificado en desarrollo

**Solución**:
```bash
# Confiar en el certificado de desarrollo
dotnet dev-certs https --trust
```

## 📁 Estructura del Proyecto

```
Biblioteca/
├── Controllers/           # Controladores MVC
│   ├── AutoresController.cs
│   ├── CategoriasController.cs
│   ├── HomeController.cs
│   ├── LibrosController.cs
│   ├── PrestamosController.cs
│   └── UsuariosController.cs
├── Data/                 # Contexto de Entity Framework
│   └── BibliotecaContext.cs
├── Models/               # Modelos de datos
│   ├── Author.cs
│   ├── Categoria.cs
│   ├── Libro.cs
│   ├── Prestamo.cs
│   └── Usuario.cs
├── Views/                # Vistas Razor
│   ├── Autores/
│   ├── Categorias/
│   ├── Home/
│   ├── Libros/
│   ├── Prestamos/
│   ├── Shared/
│   └── Usuarios/
├── wwwroot/              # Archivos estáticos
│   ├── css/
│   ├── js/
│   ├── lib/
│   └── uploads/          # Imágenes subidas
├── Migrations/           # Migraciones de EF
├── Service/              # Servicios de la aplicación
└── appsettings.json      # Configuración
```

## 🎨 Personalización

### Estilos Personalizados

El proyecto incluye estilos personalizados en cada vista:
- **Gradientes**: Botones con efectos de gradiente
- **Animaciones**: Hover effects y transiciones
- **Responsive**: Diseño adaptable a dispositivos móviles
- **Iconos**: Font Awesome integrado

### Colores del Sistema

- **Primario**: Azul (#007bff)
- **Éxito**: Verde (#28a745)
- **Advertencia**: Amarillo (#ffc107)
- **Peligro**: Rojo (#dc3545)
- **Info**: Cian (#17a2b8)

## 🤝 Contribución

Para contribuir al proyecto:

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Notas Adicionales

### Validaciones Implementadas

- **Email único**: Los usuarios no pueden tener emails duplicados
- **ISBN único**: Los libros deben tener ISBN únicos
- **Disponibilidad**: Control de stock en préstamos
- **Fechas**: Validación de fechas lógicas
- **Campos requeridos**: Validación en cliente y servidor

### Características de Seguridad

- **Anti-forgery tokens**: Protección CSRF en formularios
- **Validación de entrada**: Sanitización de datos
- **Manejo de errores**: Páginas de error personalizadas

### Performance

- **Lazy Loading**: Carga perezosa de relaciones
- **Include**: Carga explícita cuando es necesario
- **Paginación**: Implementada en listas grandes
- **Caché**: Configurado para recursos estáticos

## 📞 Soporte

Si encuentras algún problema o tienes preguntas:

1. Revisa la sección de **Solución de Problemas**
2. Verifica que todos los **Prerrequisitos** estén instalados
3. Consulta la documentación de [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
4. Consulta la documentación de [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**¡Disfruta usando el Sistema de Gestión de Biblioteca! 📚✨**
