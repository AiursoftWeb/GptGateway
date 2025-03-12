using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.GptGateway.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestIpAddress = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RequestUserAgent = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    LastQuestion = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    Questions = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    Answer = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    ToolsUsed = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ConversationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PromptTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    PreTokenCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PreTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    AdjustTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalTotal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConversations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserConversations");
        }
    }
}
