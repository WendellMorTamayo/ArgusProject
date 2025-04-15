using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgusProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAlwaysTrueTxBySlotReducer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlwaysTrueTxsBySlot",
                schema: "public",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlwaysTrueTxsBySlot", x => new { x.TxHash, x.TxIndex, x.Slot });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlwaysTrueTxsBySlot",
                schema: "public");
        }
    }
}
