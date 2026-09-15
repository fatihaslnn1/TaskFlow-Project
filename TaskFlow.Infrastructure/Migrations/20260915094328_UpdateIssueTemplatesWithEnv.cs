using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssueTemplatesWithEnv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "IssueTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Content",
                value: "• **Beklenen Davranış:**\n\n• **Gerçekleşen Davranış:**\n\n• **Tekrarlama Adımları:**\n1. \n2. \n3. \n\n• **Ortam:**\n\n• **Browser:**\n\n• **OS:**\n");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "IssueTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Content",
                value: "• **Beklenen Davranış:**\n\n• **Gerçekleşen Davranış:**\n\n• **Tekrarlama Adımları:**\n1. \n2. \n3. ");
        }
    }
}
