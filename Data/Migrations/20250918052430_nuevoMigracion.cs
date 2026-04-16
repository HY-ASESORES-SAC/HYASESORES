using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoIngSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class nuevoMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "T_Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CargoLaboral",
                table: "T_Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmarPassword",
                table: "T_Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Distrito",
                table: "T_Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dni",
                table: "T_Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "T_Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "T_Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "T_Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ubigeo",
                table: "T_Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "CargoLaboral",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "ConfirmarPassword",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "Distrito",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "Dni",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "T_Usuarios");

            migrationBuilder.DropColumn(
                name: "Ubigeo",
                table: "T_Usuarios");
        }
    }
}
