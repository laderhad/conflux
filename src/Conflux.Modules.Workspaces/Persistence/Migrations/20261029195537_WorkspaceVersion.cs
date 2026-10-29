using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conflux.Modules.Workspaces.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "workspaces",
                table: "workspaces",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                schema: "workspaces",
                table: "workspaces");
        }
    }
}
