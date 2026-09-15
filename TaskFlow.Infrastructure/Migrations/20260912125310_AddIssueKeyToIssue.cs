using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueKeyToIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IssueKey",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_IssueId",
                table: "WorkLogs",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_SubTasks_IssueId",
                table: "SubTasks",
                column: "IssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubTasks_Issues_IssueId",
                table: "SubTasks",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkLogs_Issues_IssueId",
                table: "WorkLogs",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubTasks_Issues_IssueId",
                table: "SubTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkLogs_Issues_IssueId",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_WorkLogs_IssueId",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_SubTasks_IssueId",
                table: "SubTasks");

            migrationBuilder.DropColumn(
                name: "IssueKey",
                table: "Issues");
        }
    }
}
