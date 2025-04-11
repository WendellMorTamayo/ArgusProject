using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgusProject.Migrations
{
    /// <inheritdoc />
    public partial class AddPoolParamsBySlotEntity1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoolParamsBySlot",
                schema: "public",
                columns: table => new
                {
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PoolSubject = table.Column<string>(type: "text", nullable: false),
                    PrincipalAssetSubject = table.Column<string>(type: "text", nullable: false),
                    CollateralAssetSubject = table.Column<string>(type: "text", nullable: false),
                    FeeAddress = table.Column<string>(type: "text", nullable: false),
                    DatumRaw = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolParamsBySlot", x => new { x.PoolSubject, x.TxHash, x.TxIndex, x.Slot });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoolParamsBySlot",
                schema: "public");
        }
    }
}
