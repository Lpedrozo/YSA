using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class Actividades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecursosActividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoEntidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoRecurso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiereEntrega = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosActividades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntregasActividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecursoActividadId = table.Column<int>(type: "int", nullable: false),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<int>(type: "int", nullable: true),
                    UrlArchivoEntrega = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComentarioEstudiante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Calificacion = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ObservacionInstructor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCalificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregasActividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntregasActividades_Artistas_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Artistas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntregasActividades_AspNetUsers_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntregasActividades_RecursosActividades_RecursoActividadId",
                        column: x => x.RecursoActividadId,
                        principalTable: "RecursosActividades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntregasActividades_EstudianteId",
                table: "EntregasActividades",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasActividades_InstructorId",
                table: "EntregasActividades",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasActividades_RecursoActividadId",
                table: "EntregasActividades",
                column: "RecursoActividadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntregasActividades");

            migrationBuilder.DropTable(
                name: "RecursosActividades");
        }
    }
}
