using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiSal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsignacionesPrivilegio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    Privilegio = table.Column<int>(type: "int", nullable: false),
                    OficinaId = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesPrivilegio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dni = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TipoEmpleado = table.Column<int>(type: "int", nullable: false),
                    OficinaId = table.Column<int>(type: "int", nullable: false),
                    JefeInmediatoId = table.Column<int>(type: "int", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DebeCambiarClave = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleados_Empleados_JefeInmediatoId",
                        column: x => x.JefeInmediatoId,
                        principalTable: "Empleados",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Oficinas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Acronimo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    EsOficinaConductores = table.Column<bool>(type: "bit", nullable: false),
                    JefeId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oficinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oficinas_Empleados_JefeId",
                        column: x => x.JefeId,
                        principalTable: "Empleados",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesPrivilegio_EmpleadoId_Privilegio_OficinaId",
                table: "AsignacionesPrivilegio",
                columns: new[] { "EmpleadoId", "Privilegio", "OficinaId" },
                unique: true,
                filter: "[OficinaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesPrivilegio_OficinaId",
                table: "AsignacionesPrivilegio",
                column: "OficinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Dni",
                table: "Empleados",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_JefeInmediatoId",
                table: "Empleados",
                column: "JefeInmediatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_OficinaId",
                table: "Empleados",
                column: "OficinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficinas_Acronimo",
                table: "Oficinas",
                column: "Acronimo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oficinas_JefeId",
                table: "Oficinas",
                column: "JefeId");

            migrationBuilder.CreateIndex(
                name: "IX_Oficinas_Nombre",
                table: "Oficinas",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesPrivilegio_Empleados_EmpleadoId",
                table: "AsignacionesPrivilegio",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesPrivilegio_Oficinas_OficinaId",
                table: "AsignacionesPrivilegio",
                column: "OficinaId",
                principalTable: "Oficinas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Oficinas_OficinaId",
                table: "Empleados",
                column: "OficinaId",
                principalTable: "Oficinas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficinas_Empleados_JefeId",
                table: "Oficinas");

            migrationBuilder.DropTable(
                name: "AsignacionesPrivilegio");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "Oficinas");
        }
    }
}
