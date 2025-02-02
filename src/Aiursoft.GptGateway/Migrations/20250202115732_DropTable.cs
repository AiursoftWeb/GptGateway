using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.GptGateway.Migrations
{
    /// <inheritdoc />
    public partial class DropTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenAiRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenAiRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdjustTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    CompletionTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    ConversationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    FinalTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    LastQuestion = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    PreTokenCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PreTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    PromptTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    Questions = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    TotalTokens = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenAiRequests", x => x.Id);
                });
        }
    }
}
