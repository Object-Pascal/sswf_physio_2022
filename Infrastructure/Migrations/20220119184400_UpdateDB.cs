using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    public partial class UpdateDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0748e56b-f084-43d0-95df-e2387b0b5ac3", "5997c33b-8df3-4c80-bb4a-2a3d093ead51", "Administrator", "ADMINISTRATOR" },
                    { "ebac858e-ca4d-487f-8687-bd14f185b48f", "46714472-55d7-4ba1-b654-60be0c30f9b1", "Therapist", "THERAPIST" },
                    { "9a95fa39-7f16-4487-86d8-ae1e16b1b935", "e7b8b64b-749c-4aa8-8c06-0c337aa5518c", "Student", "STUDENT" },
                    { "098a88a1-47b8-4c68-a1d9-869f3813f03c", "163959ec-0b8c-419d-a7c2-15d589aa0f91", "Patient", "PATIENT" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DataId", "Discriminator", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "620ba96f-15fb-4a48-994e-af6aa7258218", 0, "f6a05d9d-4dad-4020-80b1-8974fb31f582", 1, "ApplicationUser", null, false, false, null, null, "THERAPIST01", "AQAAAAEAACcQAAAAEKuQglGfpdjUKHYgAArtvcEmqBqAneNhuSMBm57OOCKAmk1UxrJy3xrMiXxImvBUdg==", null, false, "b70f9e58-253e-4dbb-a731-d9768534ebfb", false, "Therapist01" },
                    { "fbed071c-76c4-4d5c-8435-4b61cc42c8f7", 0, "6b82ba2d-0ed1-4b29-b673-4dc6a3914dbb", 2, "ApplicationUser", null, false, false, null, null, "THERAPIST02", "AQAAAAEAACcQAAAAEHPEdOkAGqhDuit1Aa2dVN0BNVNhI6yV0BuO4vp7oJ+hhmtoRsOvqANAVUVkG3qaUQ==", null, false, "b1285c54-6835-4030-96bd-1db19bb2ae9b", false, "Therapist02" },
                    { "628ee35c-a396-457b-8a74-e75d8f96c73e", 0, "edc7e1bb-3078-4816-b588-0a9ba3e2c00f", 3, "ApplicationUser", null, false, false, null, null, "THERAPIST03", "AQAAAAEAACcQAAAAEDpp6crIVviV1jt+47TtXF2lOYQ4rXXAkB4Lj7ZiqctL30SHvrw8zyLS9Mahba3l2g==", null, false, "d1215fc8-809e-4b50-9683-3c335744dae8", false, "Therapist03" },
                    { "f577b715-1611-4b73-b98e-c7940f3f763b", 0, "f7742122-8b29-4944-9ee5-56cad093ab87", 4, "ApplicationUser", null, false, false, null, null, "STUDENT01", "AQAAAAEAACcQAAAAEDp1EC8lYv7X3vQNCDMlHiUoR9uSoDSIGDYlweCGyoo2OVj/dhiA3IvIteElvLzLsg==", null, false, "6b53294d-2a14-43f8-80ac-f93f864734ba", false, "Student01" },
                    { "63858e13-f11a-4f98-a63a-9d5d7e77ac36", 0, "4a6b9a92-88ca-4453-b569-cae4b79210c1", 1, "ApplicationUser", null, false, false, null, null, "PATIENT01", "AQAAAAEAACcQAAAAEFFWZuNxMR2cmgTm2Htjzv/bYCJOM89GyOEzfJM3teHw3uBYdkhcISrrrEG94g2YXg==", null, false, "64b963a8-af34-4404-a3d4-e3298db600f4", false, "Patient01" },
                    { "ec85f013-c18b-4472-b358-9c94d21fcb18", 0, "8518dfd8-2c55-4052-9267-8ae0cf13a884", 2, "ApplicationUser", null, false, false, null, null, "PATIENT02", "AQAAAAEAACcQAAAAEImQ5BlYY/RE6cGu0jSS51mSTYYFfswEgSCKwPaMU1i7UYUYnX0z64k6eljRh1Bj+A==", null, false, "7348367c-753d-47cf-a900-8540002f2586", false, "Patient02" },
                    { "088063e3-dd7a-4e19-b67b-ec910b3f8e3f", 0, "49052eb5-e060-4a3f-8f01-68be33eee8b0", 3, "ApplicationUser", null, false, false, null, null, "PATIENT03", "AQAAAAEAACcQAAAAELKXdWiRIaaH0oI8Uqylfsc0gKhRODCRBYaM4Ds42YhIRGyFtx9fyTNGugJoxCjFTA==", null, false, "0cddebdf-c997-42ef-a28a-c054bf51c34b", false, "Patient03" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "ebac858e-ca4d-487f-8687-bd14f185b48f", "620ba96f-15fb-4a48-994e-af6aa7258218" },
                    { "ebac858e-ca4d-487f-8687-bd14f185b48f", "fbed071c-76c4-4d5c-8435-4b61cc42c8f7" },
                    { "ebac858e-ca4d-487f-8687-bd14f185b48f", "628ee35c-a396-457b-8a74-e75d8f96c73e" },
                    { "9a95fa39-7f16-4487-86d8-ae1e16b1b935", "f577b715-1611-4b73-b98e-c7940f3f763b" },
                    { "098a88a1-47b8-4c68-a1d9-869f3813f03c", "63858e13-f11a-4f98-a63a-9d5d7e77ac36" },
                    { "098a88a1-47b8-4c68-a1d9-869f3813f03c", "ec85f013-c18b-4472-b358-9c94d21fcb18" },
                    { "098a88a1-47b8-4c68-a1d9-869f3813f03c", "088063e3-dd7a-4e19-b67b-ec910b3f8e3f" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
