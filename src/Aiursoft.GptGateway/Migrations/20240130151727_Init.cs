using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.GptGateway.Migrations
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
                    Questions = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    Answer = table.Column<string>(type: "TEXT", maxLength: 65536, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ConversationTime = table.Column<DateTime>(type: "TEXT", nullable: false)
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
