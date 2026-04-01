using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class ArtistaAprobacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artistas_AspNetUsers_UsuarioId",
                table: "Artistas");

            migrationBuilder.AddColumn<int>(
                name: "AprobadoPorId",
                table: "Artistas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsAcademia",
                table: "Artistas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EstadoAprobacion",
                table: "Artistas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PendienteAprobacion");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "Artistas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSolicitud",
                table: "Artistas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "Artistas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Artistas_AprobadoPorId",
                table: "Artistas",
                column: "AprobadoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Artistas_AspNetUsers_AprobadoPorId",
                table: "Artistas",
                column: "AprobadoPorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Artistas_AspNetUsers_UsuarioId",
                table: "Artistas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artistas_AspNetUsers_AprobadoPorId",
                table: "Artistas");

            migrationBuilder.DropForeignKey(
                name: "FK_Artistas_AspNetUsers_UsuarioId",
                table: "Artistas");

            migrationBuilder.DropIndex(
                name: "IX_Artistas_AprobadoPorId",
                table: "Artistas");

            migrationBuilder.DropColumn(
                name: "AprobadoPorId",
                table: "Artistas");

            migrationBuilder.DropColumn(
                name: "EsAcademia",
                table: "Artistas");

            migrationBuilder.DropColumn(
                name: "EstadoAprobacion",
                table: "Artistas");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "Artistas");

            migrationBuilder.DropColumn(
                name: "FechaSolicitud",
                table: "Artistas");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "Artistas");

            migrationBuilder.AddForeignKey(
                name: "FK_Artistas_AspNetUsers_UsuarioId",
                table: "Artistas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
