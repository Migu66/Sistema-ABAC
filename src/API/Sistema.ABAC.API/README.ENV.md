# Configuración de Variables de Entorno

Este proyecto utiliza variables de entorno para gestionar configuraciones sensibles de forma segura.

## Configuración Inicial

1. **Copia el archivo de ejemplo:**
   ```bash
   cp .env.example .env
   ```

2. **Edita el archivo `.env`** con tus credenciales reales:
   - Contraseña de PostgreSQL
   - Clave secreta JWT (mínimo 32 caracteres)
   - Otros parámetros según tu entorno

## Variables Disponibles

### Base de Datos Azure SQL Server
- `ConnectionStrings__DefaultConnection`: Cadena de conexión completa a Azure SQL Server
  - **Formato:** `Server=tcp:your-server.database.windows.net,1433;Initial Catalog=your-database;User ID=username;Password=password;Encrypt=True;`
  - Incluye encriptación SSL requerida por Azure

### JWT (JSON Web Tokens)
- `JwtSettings__SecretKey`: Clave secreta para firmar tokens (mínimo 32 caracteres)
- `JwtSettings__Issuer`: Identificador del emisor del token
- `JwtSettings__Audience`: Identificador del consumidor del token
- `JwtSettings__ExpirationInMinutes`: Tiempo de expiración del token en minutos
- `JwtSettings__RefreshTokenExpirationInDays`: Tiempo de expiración del refresh token en días

## Formato de Variables de Entorno

.NET usa el formato de doble guión bajo (`__`) para representar secciones anidadas en JSON:

```bash
# JSON: { "JwtSettings": { "SecretKey": "..." } }
JwtSettings__SecretKey=tu_clave_secreta
```

## Seguridad

⚠️ **IMPORTANTE:**
- El archivo `.env` está en `.gitignore` y NO debe subirse a Git
- Nunca compartas tu archivo `.env` en repositorios públicos
- Usa `.env.example` como plantilla para nuevos desarrolladores
- En producción, usa variables de entorno del sistema o servicios como Azure Key Vault

## Para Producción

En producción, configura las variables de entorno directamente en tu plataforma de hosting:
- Azure App Service → Configuration → Application Settings
- AWS → Environment Variables
- Docker → archivo docker-compose.yml o `-e` flags
- Kubernetes → ConfigMaps y Secrets
