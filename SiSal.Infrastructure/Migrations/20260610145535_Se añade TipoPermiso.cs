using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiSal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeañadeTipoPermiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposPermiso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VecesPorMes = table.Column<int>(type: "int", nullable: false),
                    RequiereAnexoSalida = table.Column<bool>(type: "bit", nullable: false),
                    RequiereAnexoRetorno = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPermiso", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TiposPermiso_Nombre",
                table: "TiposPermiso",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiposPermiso");
        }
    }
}
