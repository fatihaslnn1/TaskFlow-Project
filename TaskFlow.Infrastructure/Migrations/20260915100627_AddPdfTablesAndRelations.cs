using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfTablesAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IssuePriorityId",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "IssueStatusId",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "IssueTypeId",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "IssuePriorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuePriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mentions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IssueId = table.Column<int>(type: "int", nullable: true),
                    CommentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mentions_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mentions_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mentions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SprintIssues",
                columns: table => new
                {
                    SprintId = table.Column<int>(type: "int", nullable: false),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprintIssues", x => new { x.SprintId, x.IssueId });
                    table.ForeignKey(
                        name: "FK_SprintIssues_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SprintIssues_Sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "Sprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "IssuePriorities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Low" },
                    { 2, "Medium" },
                    { 3, "High" },
                    { 4, "Critical" }
                });

            migrationBuilder.InsertData(
                table: "IssueStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Todo" },
                    { 2, "In Progress" },
                    { 3, "Done" }
                });

            migrationBuilder.InsertData(
                table: "IssueTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Task" },
                    { 2, "Bug" },
                    { 3, "Feature" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_IssuePriorityId",
                table: "Issues",
                column: "IssuePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_IssueStatusId",
                table: "Issues",
                column: "IssueStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_IssueTypeId",
                table: "Issues",
                column: "IssueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_CommentId",
                table: "Mentions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_IssueId",
                table: "Mentions",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Mentions_UserId",
                table: "Mentions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SprintIssues_IssueId",
                table: "SprintIssues",
                column: "IssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_IssuePriorities_IssuePriorityId",
                table: "Issues",
                column: "IssuePriorityId",
                principalTable: "IssuePriorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_IssueStatuses_IssueStatusId",
                table: "Issues",
                column: "IssueStatusId",
                principalTable: "IssueStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_IssueTypes_IssueTypeId",
                table: "Issues",
                column: "IssueTypeId",
                principalTable: "IssueTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_IssuePriorities_IssuePriorityId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_IssueStatuses_IssueStatusId",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_IssueTypes_IssueTypeId",
                table: "Issues");

            migrationBuilder.DropTable(
                name: "IssuePriorities");

            migrationBuilder.DropTable(
                name: "IssueStatuses");

            migrationBuilder.DropTable(
                name: "IssueTypes");

            migrationBuilder.DropTable(
                name: "Mentions");

            migrationBuilder.DropTable(
                name: "SprintIssues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_IssuePriorityId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_IssueStatusId",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_IssueTypeId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IssuePriorityId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IssueStatusId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IssueTypeId",
                table: "Issues");
        }
    }
}
