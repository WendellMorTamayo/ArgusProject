using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArgusProject.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "LendTokenDetailsBySlot",
                schema: "public",
                columns: table => new
                {
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UtxoStatus = table.Column<int>(type: "integer", nullable: false),
                    OwnerPkh = table.Column<string>(type: "text", nullable: false),
                    BorrowerPkh = table.Column<string>(type: "text", nullable: false),
                    UtxoRaw = table.Column<byte[]>(type: "bytea", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UtxoAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TokenAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OwnerAddress = table.Column<string>(type: "text", nullable: false),
                    BorrowerAddress = table.Column<string>(type: "text", nullable: false),
                    LoanDuration = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LoanEndTime = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OutputRefTxHash = table.Column<string>(type: "text", nullable: false),
                    OutputRefTxIndex = table.Column<long>(type: "bigint", nullable: false),
                    DatumType = table.Column<int>(type: "integer", nullable: false),
                    SpentTxHash = table.Column<string>(type: "text", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    ScriptHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LendTokenDetailsBySlot", x => new { x.Subject, x.Slot, x.TxHash, x.TxIndex, x.UtxoStatus });
                });

            migrationBuilder.CreateTable(
                name: "LendTokenDetailsBySubject",
                schema: "public",
                columns: table => new
                {
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OwnerPkh = table.Column<string>(type: "text", nullable: false),
                    BorrowerPkh = table.Column<string>(type: "text", nullable: false),
                    UtxoRaw = table.Column<byte[]>(type: "bytea", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UtxoAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TokenAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OwnerAddress = table.Column<string>(type: "text", nullable: false),
                    BorrowerAddress = table.Column<string>(type: "text", nullable: false),
                    LoanDuration = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LoanEndTime = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    OutputRefTxHash = table.Column<string>(type: "text", nullable: false),
                    OutputRefTxIndex = table.Column<long>(type: "bigint", nullable: false),
                    DatumType = table.Column<int>(type: "integer", nullable: false),
                    ScriptHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LendTokenDetailsBySubject", x => new { x.Subject, x.Slot, x.TxHash, x.TxIndex });
                });

            migrationBuilder.CreateTable(
                name: "ReducerStates",
                schema: "public",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LatestIntersectionsJson = table.Column<string>(type: "text", nullable: false),
                    StartIntersectionJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReducerStates", x => x.Name);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LendTokenDetailsBySlot",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LendTokenDetailsBySubject",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReducerStates",
                schema: "public");
        }
    }
}
