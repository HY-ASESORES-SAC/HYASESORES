using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proyectoIngSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class usuarioMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DbSetRegyLog",
                table: "DbSetRegyLog");

            migrationBuilder.RenameTable(
                name: "DbSetRegyLog",
                newName: "T_Usuarios");

            migrationBuilder.AddPrimaryKey(
                name: "PK_T_Usuarios",
                table: "T_Usuarios",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_T_Usuarios",
                table: "T_Usuarios");

            migrationBuilder.RenameTable(
                name: "T_Usuarios",
                newName: "DbSetRegyLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DbSetRegyLog",
                table: "DbSetRegyLog",
                column: "Id");
        }
    }
}
