using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.GptGateway.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrackMoreProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdjustTotal",
                table: "UserConversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                table: "UserConversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FinalTotal",
                table: "UserConversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreTokenCount",
                table: "UserConversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreTotal",
                table: "UserConversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                table: "UserConversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                table: "UserConversations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustTotal",
                table: "UserConversations");

            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                table: "UserConversations");

            migrationBuilder.DropColumn(
                name: "FinalTotal",
                table: "UserConversations");

            migrationBuilder.DropColumn(
                name: "PreTokenCount",
                table: "UserConversations");

            migrationBuilder.DropColumn(
                name: "PreTotal",
                table: "UserConversations");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                table: "UserConversations");

            migrationBuilder.DropColumn(
                name: "TotalTokens",
                table: "UserConversations");
        }
    }
}
