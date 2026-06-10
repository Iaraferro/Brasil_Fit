using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrasilFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Kcal = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Carboidratos = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Proteinas = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Lipidios = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    CodigoBarrasExterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alimentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Papel = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Crn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Especialidade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sexo = table.Column<int>(type: "int", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HistoricoClinico = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NutricionistaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_NutricionistaId",
                        column: x => x.NutricionistaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ObservacoesClinicas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    NutricionistaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Usuarios_NutricionistaId",
                        column: x => x.NutricionistaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Usuarios_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enderecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cep = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Logradouro = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Complemento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    PacienteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enderecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enderecos_Usuarios_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Operacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Entidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Detalhes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    EnderecoIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsAuditoria_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Metas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    ValorAlvo = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Unidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Prazo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CriadaEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PacienteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metas_Usuarios_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lida = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosAlimentares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Objetivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracaoDias = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    NutricionistaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosAlimentares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosAlimentares_Usuarios_NutricionistaId",
                        column: x => x.NutricionistaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanosAlimentares_Usuarios_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComposicoesCorporais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Imc = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PercentualGordura = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MassaMagra = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Classificacao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AvaliacaoAntropometricaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComposicoesCorporais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComposicoesCorporais_Avaliacoes_AvaliacaoAntropometricaId",
                        column: x => x.AvaliacaoAntropometricaId,
                        principalTable: "Avaliacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedidasCorporais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Peso = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Altura = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    CircunferenciasJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DobrasCutaneasJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvaliacaoAntropometricaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedidasCorporais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedidasCorporais_Avaliacoes_AvaliacaoAntropometricaId",
                        column: x => x.AvaliacaoAntropometricaId,
                        principalTable: "Avaliacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressosMeta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataVerificacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorAtual = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressosMeta_Metas_MetaId",
                        column: x => x.MetaId,
                        principalTable: "Metas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Refeicoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Horario = table.Column<TimeOnly>(type: "time", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PlanoAlimentarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refeicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refeicoes_PlanosAlimentares_PlanoAlimentarId",
                        column: x => x.PlanoAlimentarId,
                        principalTable: "PlanosAlimentares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensRefeicao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantidade = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Unidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RefeicaoId = table.Column<int>(type: "int", nullable: false),
                    AlimentoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensRefeicao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensRefeicao_Alimentos_AlimentoId",
                        column: x => x.AlimentoId,
                        principalTable: "Alimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItensRefeicao_Refeicoes_RefeicaoId",
                        column: x => x.RefeicaoId,
                        principalTable: "Refeicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alimentos_CodigoBarrasExterno",
                table: "Alimentos",
                column: "CodigoBarrasExterno",
                unique: true,
                filter: "[CodigoBarrasExterno] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Alimentos_Nome",
                table: "Alimentos",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_NutricionistaId",
                table: "Avaliacoes",
                column: "NutricionistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_PacienteId",
                table: "Avaliacoes",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ComposicoesCorporais_AvaliacaoAntropometricaId",
                table: "ComposicoesCorporais",
                column: "AvaliacaoAntropometricaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_PacienteId",
                table: "Enderecos",
                column: "PacienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensRefeicao_AlimentoId",
                table: "ItensRefeicao",
                column: "AlimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensRefeicao_RefeicaoId",
                table: "ItensRefeicao",
                column: "RefeicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_DataHora",
                table: "LogsAuditoria",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_Operacao",
                table: "LogsAuditoria",
                column: "Operacao");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_UsuarioId",
                table: "LogsAuditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MedidasCorporais_AvaliacaoAntropometricaId",
                table: "MedidasCorporais",
                column: "AvaliacaoAntropometricaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metas_PacienteId",
                table: "Metas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_UsuarioId",
                table: "Notificacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosAlimentares_NutricionistaId",
                table: "PlanosAlimentares",
                column: "NutricionistaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosAlimentares_PacienteId",
                table: "PlanosAlimentares",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosMeta_MetaId",
                table: "ProgressosMeta",
                column: "MetaId");

            migrationBuilder.CreateIndex(
                name: "IX_Refeicoes_PlanoAlimentarId",
                table: "Refeicoes",
                column: "PlanoAlimentarId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Crn",
                table: "Usuarios",
                column: "Crn",
                unique: true,
                filter: "[Crn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NutricionistaId",
                table: "Usuarios",
                column: "NutricionistaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComposicoesCorporais");

            migrationBuilder.DropTable(
                name: "Enderecos");

            migrationBuilder.DropTable(
                name: "ItensRefeicao");

            migrationBuilder.DropTable(
                name: "LogsAuditoria");

            migrationBuilder.DropTable(
                name: "MedidasCorporais");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "ProgressosMeta");

            migrationBuilder.DropTable(
                name: "Alimentos");

            migrationBuilder.DropTable(
                name: "Refeicoes");

            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "Metas");

            migrationBuilder.DropTable(
                name: "PlanosAlimentares");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
