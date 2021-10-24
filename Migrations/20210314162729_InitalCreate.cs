using Microsoft.EntityFrameworkCore.Migrations;

namespace EDFab_Telemetry_Server.Migrations
{
    public partial class InitalCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorInfo",
                columns: table => new
                {
                    SID = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    IP = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorInfo", x => x.SID);
                });

            migrationBuilder.CreateTable(
                name: "UserInfo",
                columns: table => new
                {
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Priv = table.Column<string>(type: "TEXT", nullable: true),
                    Token = table.Column<string>(type: "TEXT", nullable: true),
                    Hash = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfo", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "sensorValues",
                columns: table => new
                {
                    SIDKey = table.Column<string>(type: "TEXT", nullable: false),
                    DateTime = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensorValues", x => x.SIDKey);
                    table.ForeignKey(
                        name: "FK_sensorValues_SensorInfo_SIDKey",
                        column: x => x.SIDKey,
                        principalTable: "SensorInfo",
                        principalColumn: "SID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userPerms",
                columns: table => new
                {
                    EmailKey = table.Column<string>(type: "TEXT", nullable: false),
                    SID = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userPerms", x => x.EmailKey);
                    table.ForeignKey(
                        name: "FK_userPerms_UserInfo_EmailKey",
                        column: x => x.EmailKey,
                        principalTable: "UserInfo",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sensorValues");

            migrationBuilder.DropTable(
                name: "userPerms");

            migrationBuilder.DropTable(
                name: "SensorInfo");

            migrationBuilder.DropTable(
                name: "UserInfo");
        }
    }
}
