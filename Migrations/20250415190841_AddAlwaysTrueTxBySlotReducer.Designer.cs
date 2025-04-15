﻿// <auto-generated />
using System;
using ArgusProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArgusProject.Migrations
{
    [DbContext(typeof(TestDbContext))]
    [Migration("20250415190841_AddAlwaysTrueTxBySlotReducer")]
    partial class AddAlwaysTrueTxBySlotReducer
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Argus.Sync.Data.Models.ReducerState", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LatestIntersectionsJson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("StartIntersectionJson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.ToTable("ReducerStates", "public");
                });

            modelBuilder.Entity("ArgusProject.Models.Entity.AlwaysTrueTxBySlot", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TxHash", "TxIndex", "Slot");

                    b.ToTable("AlwaysTrueTxsBySlot", "public");
                });

            modelBuilder.Entity("ArgusProject.Models.Entity.GlobalParamsBySlot", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<byte[]>("DatumRaw")
                        .HasColumnType("bytea");

                    b.Property<string>("FeeAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NftBorrowImage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NftClaimableImage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NftLendImage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NftPrefix")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PoolParamsPolicy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TxHash", "TxIndex", "Slot");

                    b.ToTable("GlobalParamsBySlot", "public");
                });

            modelBuilder.Entity("ArgusProject.Models.Entity.LendTokenDetailsBySlot", b =>
                {
                    b.Property<string>("Subject")
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("UtxoStatus")
                        .HasColumnType("integer");

                    b.Property<int>("ActionType")
                        .HasColumnType("integer");

                    b.Property<string>("BorrowerAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BorrowerPkh")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DatumType")
                        .HasColumnType("integer");

                    b.Property<decimal>("InterestAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LoanAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LoanDuration")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LoanEndTime")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("OutputRefTxHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("OutputRefTxIndex")
                        .HasColumnType("bigint");

                    b.Property<string>("OwnerAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OwnerPkh")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ScriptHash")
                        .HasColumnType("text");

                    b.Property<string>("SpentTxHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("TokenAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("UtxoAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<byte[]>("UtxoRaw")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.HasKey("Subject", "Slot", "TxHash", "TxIndex", "UtxoStatus");

                    b.ToTable("LendTokenDetailsBySlot", "public");
                });

            modelBuilder.Entity("ArgusProject.Models.Entity.LendTokenDetailsBySubject", b =>
                {
                    b.Property<string>("Subject")
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("BorrowerAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BorrowerPkh")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DatumType")
                        .HasColumnType("integer");

                    b.Property<decimal>("InterestAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LoanAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LoanDuration")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("LoanEndTime")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("OutputRefTxHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("OutputRefTxIndex")
                        .HasColumnType("bigint");

                    b.Property<string>("OwnerAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OwnerPkh")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ScriptHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TokenAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("UtxoAmount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<byte[]>("UtxoRaw")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.HasKey("Subject", "Slot", "TxHash", "TxIndex");

                    b.ToTable("LendTokenDetailsBySubject", "public");
                });

            modelBuilder.Entity("ArgusProject.Models.Entity.PoolParamsBySlot", b =>
                {
                    b.Property<string>("PoolSubject")
                        .HasColumnType("text");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("CollateralAssetSubject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("DatumRaw")
                        .HasColumnType("bytea");

                    b.Property<string>("FeeAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PrincipalAssetSubject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PoolSubject", "TxHash", "TxIndex", "Slot");

                    b.ToTable("PoolParamsBySlot", "public");
                });
#pragma warning restore 612, 618
        }
    }
}
