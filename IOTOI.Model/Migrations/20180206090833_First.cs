using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace IOTOI.Model.Migrations
{
    public partial class First : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CCTV",
                columns: table => new
                {
                    CCTVId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<string>(nullable: true),
                    AccountPass = table.Column<string>(nullable: true),
                    CCTVName = table.Column<string>(nullable: false),
                    CCTVType = table.Column<string>(nullable: true),
                    IpAddress = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CCTV", x => x.CCTVId);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZigBeeEndDevice",
                columns: table => new
                {
                    MacAddress = table.Column<ulong>(nullable: false),
                    IsConnected = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NetworkAddress = table.Column<ushort>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZigBeeEndDevice", x => x.MacAddress);
                });

            migrationBuilder.CreateTable(
                name: "ZWaveNode",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    HomeId = table.Column<uint>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Manufacturer = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Product = table.Column<string>(nullable: true),
                    Value = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZWaveNode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZigBeeEndPoint",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommanProfileId = table.Column<ushort>(nullable: false),
                    CustomName = table.Column<string>(nullable: true),
                    DeviceId = table.Column<ushort>(nullable: false),
                    EpNum = table.Column<byte>(nullable: false),
                    IsActivated = table.Column<bool>(nullable: false),
                    MacAddress = table.Column<ulong>(nullable: false),
                    Model = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ProtocolTypeId = table.Column<int>(nullable: false),
                    Vendor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZigBeeEndPoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZigBeeEndPoint_ZigBeeEndDevice_MacAddress",
                        column: x => x.MacAddress,
                        principalTable: "ZigBeeEndDevice",
                        principalColumn: "MacAddress",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZigBeeEndPoint_ProtocolType_ProtocolTypeId",
                        column: x => x.ProtocolTypeId,
                        principalTable: "ProtocolType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZigBeeInCluster",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClusterId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZigBeeInCluster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZigBeeInCluster_ZigBeeEndPoint_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ZigBeeEndPoint",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZigBeeOutCluster",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClusterId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZigBeeOutCluster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZigBeeOutCluster_ZigBeeEndPoint_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ZigBeeEndPoint",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZigBeeInClusterAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttrValue = table.Column<byte[]>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: false),
                    ZigBeeType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZigBeeInClusterAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZigBeeInClusterAttribute_ZigBeeInCluster_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ZigBeeInCluster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZigBeeOutClusterAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttrValue = table.Column<byte[]>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: false),
                    ZigBeeType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZigBeeOutClusterAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZigBeeOutClusterAttribute_ZigBeeOutCluster_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ZigBeeOutCluster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolType_Name",
                table: "ProtocolType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeEndDevice_MacAddress",
                table: "ZigBeeEndDevice",
                column: "MacAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeEndPoint_MacAddress",
                table: "ZigBeeEndPoint",
                column: "MacAddress");

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeEndPoint_ProtocolTypeId",
                table: "ZigBeeEndPoint",
                column: "ProtocolTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeEndPoint_EpNum_MacAddress",
                table: "ZigBeeEndPoint",
                columns: new[] { "EpNum", "MacAddress" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeInCluster_ParentId",
                table: "ZigBeeInCluster",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeInClusterAttribute_ParentId",
                table: "ZigBeeInClusterAttribute",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeOutCluster_ParentId",
                table: "ZigBeeOutCluster",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ZigBeeOutClusterAttribute_ParentId",
                table: "ZigBeeOutClusterAttribute",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ZWaveNode_Id",
                table: "ZWaveNode",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CCTV");

            migrationBuilder.DropTable(
                name: "ZigBeeInClusterAttribute");

            migrationBuilder.DropTable(
                name: "ZigBeeOutClusterAttribute");

            migrationBuilder.DropTable(
                name: "ZWaveNode");

            migrationBuilder.DropTable(
                name: "ZigBeeInCluster");

            migrationBuilder.DropTable(
                name: "ZigBeeOutCluster");

            migrationBuilder.DropTable(
                name: "ZigBeeEndPoint");

            migrationBuilder.DropTable(
                name: "ZigBeeEndDevice");

            migrationBuilder.DropTable(
                name: "ProtocolType");
        }
    }
}
