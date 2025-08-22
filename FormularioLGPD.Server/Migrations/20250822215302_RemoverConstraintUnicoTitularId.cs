using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormularioLGPD.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoverConstraintUnicoTitularId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TermosAceite_TitularId",
                table: "TermosAceite");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAceite_TitularId",
                table: "TermosAceite",
                column: "TitularId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TermosAceite_TitularId",
                table: "TermosAceite");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAceite_TitularId",
                table: "TermosAceite",
                column: "TitularId",
                unique: true);
        }
    }
}
