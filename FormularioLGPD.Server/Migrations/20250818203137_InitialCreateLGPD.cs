using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormularioLGPD.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateLGPD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Titulares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CPF = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    RG = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoCivil = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naturalidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Escolaridade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerieSemestre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QualificacaoLegal = table.Column<int>(type: "int", nullable: false),
                    IsAtivo = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titulares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dependentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitularId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CPF = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrauParentesco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAtivo = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dependentes_Titulares_TitularId",
                        column: x => x.TitularId,
                        principalTable: "Titulares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermosAceite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitularId = table.Column<int>(type: "int", nullable: false),
                    NumeroTermo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConteudoTermo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AceiteConfirmado = table.Column<bool>(type: "bit", nullable: false),
                    DataHoraAceite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpOrigem = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HashIntegridade = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CaminhoArquivoPDF = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VersaoTermo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StatusTermo = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermosAceite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermosAceite_Titulares_TitularId",
                        column: x => x.TitularId,
                        principalTable: "Titulares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TermoAceiteId = table.Column<int>(type: "int", nullable: true),
                    TipoOperacao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IpOrigem = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataHoraOperacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DadosAntes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DadosDepois = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusOperacao = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsAuditoria_TermosAceite_TermoAceiteId",
                        column: x => x.TermoAceiteId,
                        principalTable: "TermosAceite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dependentes_TitularId_CPF",
                table: "Dependentes",
                columns: new[] { "TitularId", "CPF" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_DataHoraOperacao",
                table: "LogsAuditoria",
                column: "DataHoraOperacao");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_TermoAceiteId",
                table: "LogsAuditoria",
                column: "TermoAceiteId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_TipoOperacao",
                table: "LogsAuditoria",
                column: "TipoOperacao");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAceite_DataHoraAceite",
                table: "TermosAceite",
                column: "DataHoraAceite");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAceite_NumeroTermo",
                table: "TermosAceite",
                column: "NumeroTermo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermosAceite_TitularId",
                table: "TermosAceite",
                column: "TitularId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Titulares_CPF",
                table: "Titulares",
                column: "CPF",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dependentes");

            migrationBuilder.DropTable(
                name: "LogsAuditoria");

            migrationBuilder.DropTable(
                name: "TermosAceite");

            migrationBuilder.DropTable(
                name: "Titulares");
        }
    }
}
