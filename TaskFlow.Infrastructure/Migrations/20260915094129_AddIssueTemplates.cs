using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueTemplates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "IssueTemplates",
                columns: new[] { "Id", "Content", "IssueType", "Name" },
                values: new object[,]
                {
                    { 1, "• **Beklenen Davranış:**\n\n• **Gerçekleşen Davranış:**\n\n• **Tekrarlama Adımları:**\n1. \n2. \n3. ", "Bug", "Standart Bug Şablonu" },
                    { 2, "• **Kullanıcı Hikayesi (User Story):**\n\n• **Kabul Kriterleri (Acceptance Criteria):**\n- \n- \n- ", "Feature", "Standart Feature Şablonu" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueTemplates");
        }
    }
}
