# PROMPT PARA ARQUITECTURA DE SISTEMA ABAC (ATTRIBUTE-BASED ACCESS CONTROL) EN .NET

**Actúa como un Senior Software Architect especializado en el ecosistema .NET y Seguridad de Aplicaciones.**

Necesito diseñar y desarrollar el núcleo de un sistema de **Control de Acceso Basado en Atributos (ABAC)**. Este sistema debe permitir definir permisos dinámicos basados en atributos del Usuario (Subject), el Recurso (Resource) y el Entorno (Environment).

## 1. REQUISITOS DE ARQUITECTURA
El proyecto debe seguir estrictamente los principios de **Clean Architecture** y **Domain-Driven Design (DDD)**:
- **Core (Domain):** Entidades puras, lógica de negocio y definiciones de abstracciones.
- **Application:** Casos de uso, DTOs, Mappers y el "Policy Evaluation Engine".
- **Infrastructure:** Persistencia con PostgreSQL, Implementación de Identity, Interceptores de EF Core y Servicios de Terceros.
- **API:** Controladores RESTful, Middleware de manejo de excepciones globales y Documentación con Swagger.

## 2. STACK TÉCNICO ESPECÍFICO
- **Runtime:** .NET 8/9.
- **ORM:** Entity Framework Core (PostgreSQL Provider).
- **Seguridad:** ASP.NET Core Identity + JWT con Custom Claims.
- **Evaluación Dinámica:** Utiliza **Dynamic LINQ** o **NCalc** para procesar las reglas guardadas como texto.
- **Observabilidad:** Serilog para logging estructurado y auditoría.

## 3. COMPONENTES CLAVE A DESARROLLAR (DETALLE TÉCNICO)

### A. Modelo de Datos (PostgreSQL)
Define una estructura que soporte:
- `Policy`: Id, Nombre, `RuleExpression` (string que guarda la lógica, ej: `User.Level >= Resource.RequiredLevel`), Prioridad y Efecto (Permit/Deny).
- `UserAttributes` & `ResourceAttributes`: Tablas o JSONB en Postgres para flexibilidad total de atributos.

### B. Motor de Evaluación (Policy Engine)
Implementa un servicio `IPolicyEvaluator` que:
- Reciba un `EvaluationContext` (User, Resource, Environment).
- Busque las políticas aplicables en la base de datos.
- Ejecute la expresión lógica dinámicamente.
- Devuelva un `AuthorizationResult`.

### C. Seguridad a Nivel de Datos (Global Query Filters)
Configura EF Core para que, mediante un `ISessionService`, se aplique automáticamente un filtro `.HasQueryFilter()` en las consultas para asegurar que el usuario nunca reciba datos que no le corresponden, incluso si el desarrollador olvida el `.Where()`.

### D. Auditoría y Trazabilidad
Implementa un **SaveChangesInterceptor** en EF Core que detecte cambios en entidades sensibles y registre en una tabla de `AuditLogs`:
- Quién hizo el cambio.
- Qué atributos de seguridad tenía en ese momento.
- Timestamp y IP del entorno.

## 4. ENTREGABLES ESPERADOS
1. **Estructura de carpetas** del proyecto siguiendo Clean Architecture.
2. **Entidades de Dominio** para `Policy` y `User`.
3. El código del **AuthorizationHandler** personalizado que integre el `IPolicyEvaluator`.
4. Ejemplo de configuración de **Global Query Filter** en el `DbContext`.
5. Un ejemplo de **Controller** decorado con una política ABAC personalizada.

## 5. RESTRICCIONES DE CALIDAD
- Aplica principios **SOLID** y **DRY**.
- Usa **Inyección de Dependencias** de forma limpia.
- El código debe ser altamente testeable (usa interfaces).
- Manejo de errores mediante un Middleware global que devuelva `ProblemDetails` (RFC 7807).

**Por favor, comienza generando la estructura del proyecto y las entidades del Dominio.**