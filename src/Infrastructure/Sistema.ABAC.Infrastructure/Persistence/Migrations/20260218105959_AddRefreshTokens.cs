using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.ABAC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Token de actualización único"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Identificador del usuario propietario del token"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Fecha y hora de creación del token"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Fecha y hora de expiración del token"),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indica si el token ha sido revocado manualmente"),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Fecha y hora en que se revocó el token"),
                    CreatedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true, comment: "Dirección IP desde la cual se creó el token"),
                    ReplacedByTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "ID del token que reemplazó a este cuando se renovó")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");
        }
    }
}
