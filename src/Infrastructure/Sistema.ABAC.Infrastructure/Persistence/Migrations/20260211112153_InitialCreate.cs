using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.ABAC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Nombre descriptivo de la acción"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Código único de la acción usado en evaluación"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Descripción detallada de lo que permite hacer esta acción"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Nombre del usuario"),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Apellido del usuario"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha y hora de creación del usuario"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha y hora de última actualización del usuario"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Nombre descriptivo del atributo"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Clave única del atributo usada en evaluación de políticas"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Tipo de dato del atributo (String, Number, Boolean, DateTime)"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Descripción detallada del propósito del atributo"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Nombre descriptivo de la política"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Descripción detallada del propósito de la política"),
                    Effect = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Efecto de la política cuando se cumplen las condiciones (Permit o Deny)"),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 100, comment: "Prioridad numérica para resolver conflictos (mayor = más prioritaria)"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Indica si la política está activa y debe ser evaluada"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Nombre descriptivo del recurso"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Tipo o categoría del recurso (documento, endpoint, vista, etc.)"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Descripción detallada del recurso y su contenido"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador del usuario al que se asigna el atributo"),
                    AttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador del atributo que se asigna"),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Valor concreto del atributo para este usuario"),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha desde la cual este valor es válido (null = desde siempre)"),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha hasta la cual este valor es válido (null = indefinidamente)"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAttributes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAttributes_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador de la política"),
                    ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador de la acción"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyActions_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyActions_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicyConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador de la política a la que pertenece esta condición"),
                    AttributeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Tipo de atributo: Subject, Resource o Environment"),
                    AttributeKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Clave del atributo que se evaluará"),
                    Operator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Operador de comparación (Equals, NotEquals, GreaterThan, etc.)"),
                    ExpectedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Valor esperado contra el cual se comparará el atributo"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyConditions_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador del usuario que intentó acceder"),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identificador del recurso al que se intentó acceder (opcional)"),
                    ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identificador de la acción que se intentó realizar (opcional)"),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Identificador de la política que produjo la decisión (opcional)"),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Resultado de la evaluación (Permit, Deny, Error, NotApplicable)"),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "Razón detallada de la decisión"),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Información contextual adicional en formato JSON"),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true, comment: "Dirección IP desde la cual se realizó la solicitud"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha y hora del intento de acceso (timestamp)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador del recurso al que se asigna el atributo"),
                    AttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador del atributo que se asigna"),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Valor concreto del atributo para este recurso"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha de creación del registro"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha de última actualización del registro"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de eliminación lógica (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceAttributes_Attributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceAttributes_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_ActionId",
                table: "AccessLogs",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_CreatedAt",
                table: "AccessLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_IpAddress",
                table: "AccessLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_IsDeleted",
                table: "AccessLogs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_PolicyId",
                table: "AccessLogs",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_ResourceId",
                table: "AccessLogs",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_ResourceId_CreatedAt",
                table: "AccessLogs",
                columns: new[] { "ResourceId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_Result",
                table: "AccessLogs",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_Result_CreatedAt",
                table: "AccessLogs",
                columns: new[] { "Result", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_UserId",
                table: "AccessLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_UserId_CreatedAt",
                table: "AccessLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Code",
                table: "Actions",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_IsDeleted",
                table: "Actions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Name",
                table: "Actions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "AspNetUsers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FirstName_LastName",
                table: "AspNetUsers",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "AspNetUsers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_IsDeleted",
                table: "Attributes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_Key",
                table: "Attributes",
                column: "Key",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_Name",
                table: "Attributes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_IsActive",
                table: "Policies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_IsActive_IsDeleted_Priority",
                table: "Policies",
                columns: new[] { "IsActive", "IsDeleted", "Priority" },
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_IsDeleted",
                table: "Policies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Name",
                table: "Policies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Priority",
                table: "Policies",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyActions_ActionId",
                table: "PolicyActions",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyActions_IsDeleted",
                table: "PolicyActions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyActions_PolicyId",
                table: "PolicyActions",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyActions_PolicyId_ActionId",
                table: "PolicyActions",
                columns: new[] { "PolicyId", "ActionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyConditions_AttributeKey",
                table: "PolicyConditions",
                column: "AttributeKey");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyConditions_IsDeleted",
                table: "PolicyConditions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyConditions_PolicyId",
                table: "PolicyConditions",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyConditions_PolicyId_AttributeType",
                table: "PolicyConditions",
                columns: new[] { "PolicyId", "AttributeType" });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAttributes_AttributeId",
                table: "ResourceAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAttributes_IsDeleted",
                table: "ResourceAttributes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAttributes_ResourceId",
                table: "ResourceAttributes",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAttributes_ResourceId_AttributeId",
                table: "ResourceAttributes",
                columns: new[] { "ResourceId", "AttributeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_IsDeleted",
                table: "Resources",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Name",
                table: "Resources",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Type",
                table: "Resources",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Type_IsDeleted",
                table: "Resources",
                columns: new[] { "Type", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributes_AttributeId",
                table: "UserAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributes_IsDeleted",
                table: "UserAttributes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributes_UserId",
                table: "UserAttributes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributes_UserId_AttributeId",
                table: "UserAttributes",
                columns: new[] { "UserId", "AttributeId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributes_ValidFrom_ValidTo",
                table: "UserAttributes",
                columns: new[] { "ValidFrom", "ValidTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "PolicyActions");

            migrationBuilder.DropTable(
                name: "PolicyConditions");

            migrationBuilder.DropTable(
                name: "ResourceAttributes");

            migrationBuilder.DropTable(
                name: "UserAttributes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Attributes");
        }
    }
}
