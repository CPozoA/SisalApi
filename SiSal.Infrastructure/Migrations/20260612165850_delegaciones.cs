using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiSal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class delegaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Delegaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitularId = table.Column<int>(type: "int", nullable: false),
                    DelegadoId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delegaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Delegaciones_Empleados_DelegadoId",
                        column: x => x.DelegadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Delegaciones_Empleados_TitularId",
                        column: x => x.TitularId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Delegaciones_DelegadoId",
                table: "Delegaciones",
                column: "DelegadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Delegaciones_TitularId",
                table: "Delegaciones",
                column: "TitularId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Delegaciones");
        }
    }
}
