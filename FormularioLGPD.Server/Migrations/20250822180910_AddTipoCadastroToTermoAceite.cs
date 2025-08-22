using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormularioLGPD.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoCadastroToTermoAceite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoCadastro",
                table: "TermosAceite",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoCadastro",
                table: "TermosAceite");
        }
    }
}
