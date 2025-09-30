# 📚 Sistema de Gestión de Biblioteca

Un sistema completo de gestión de biblioteca desarrollado en **ASP.NET Core MVC** con **Entity Framework Core** que permite administrar libros, autores, categorías, usuarios y préstamos.

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
dotnet ef database update
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Verificar la Instalación

1. Compila el proyecto: `Ctrl + Shift + B`
2. Ejecuta el proyecto: `F5` o `Ctrl + F5`
3. El navegador debería abrir en `https://localhost:xxxx`

## Solución de Problemas Comunes

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
