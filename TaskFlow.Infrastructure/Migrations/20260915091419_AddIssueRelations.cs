using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentIssueId",
                table: "Issues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IssueRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceIssueId = table.Column<int>(type: "int", nullable: false),
                    TargetIssueId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueRelations_Issues_SourceIssueId",
                        column: x => x.SourceIssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IssueRelations_Issues_TargetIssueId",
                        column: x => x.TargetIssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentIssueId",
                table: "Issues",
                column: "ParentIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueRelations_SourceIssueId",
                table: "IssueRelations",
                column: "SourceIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueRelations_TargetIssueId",
                table: "IssueRelations",
                column: "TargetIssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Issues_ParentIssueId",
                table: "Issues",
                column: "ParentIssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Issues_ParentIssueId",
                table: "Issues");

            migrationBuilder.DropTable(
                name: "IssueRelations");

            migrationBuilder.DropIndex(
                name: "IX_Issues_ParentIssueId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ParentIssueId",
                table: "Issues");
        }
    }
}
