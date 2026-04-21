using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithRecoverySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration aligns code-first with an existing database that may already include
            // some of these columns (per your recovery SQL). Make it safe to re-run.

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'PRODUCT', N'Category') IS NULL
                    ALTER TABLE [PRODUCT] ADD [Category] nvarchar(max) NULL;

                IF COL_LENGTH(N'PRODUCT', N'ImageUrl') IS NULL
                    ALTER TABLE [PRODUCT] ADD [ImageUrl] nvarchar(max) NULL;

                IF COL_LENGTH(N'PRODUCT', N'Rating') IS NULL
                    ALTER TABLE [PRODUCT] ADD [Rating] float NULL;

                IF COL_LENGTH(N'PRODUCT', N'ImageGalleryJson') IS NULL
                    ALTER TABLE [PRODUCT] ADD [ImageGalleryJson] nvarchar(max) NULL;

                IF COL_LENGTH(N'ORDERS', N'Payment_method') IS NULL
                    ALTER TABLE [ORDERS] ADD [Payment_method] nvarchar(max) NULL;

                IF COL_LENGTH(N'ORDERS', N'Promo_code') IS NULL
                    ALTER TABLE [ORDERS] ADD [Promo_code] nvarchar(max) NULL;

                IF COL_LENGTH(N'ORDERS', N'Discount_amount') IS NULL
                    ALTER TABLE [ORDERS] ADD [Discount_amount] decimal(10,2) NULL;

                IF COL_LENGTH(N'ORDERS', N'Order_notes') IS NULL
                    ALTER TABLE [ORDERS] ADD [Order_notes] nvarchar(max) NULL;

                IF OBJECT_ID(N'[REVIEW]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [REVIEW] (
                        [Rid] int NOT NULL IDENTITY(1,1),
                        [Pid] int NULL,
                        [Uid] int NULL,
                        [Rating] int NULL,
                        [Comment] nvarchar(max) NULL,
                        [CreatedUtc] datetime2 NOT NULL,
                        CONSTRAINT [PK_REVIEW] PRIMARY KEY ([Rid]),
                        CONSTRAINT [FK_REVIEW_PRODUCT_Pid] FOREIGN KEY ([Pid]) REFERENCES [PRODUCT] ([Pid]),
                        CONSTRAINT [FK_REVIEW_USERS_Uid] FOREIGN KEY ([Uid]) REFERENCES [USERS] ([Uid])
                    );

                    CREATE INDEX [IX_REVIEW_Pid] ON [REVIEW] ([Pid]);
                    CREATE INDEX [IX_REVIEW_Uid] ON [REVIEW] ([Uid]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort rollback (idempotent)
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[REVIEW]', N'U') IS NOT NULL
                    DROP TABLE [REVIEW];

                IF COL_LENGTH(N'PRODUCT', N'ImageGalleryJson') IS NOT NULL
                    ALTER TABLE [PRODUCT] DROP COLUMN [ImageGalleryJson];
                """);
        }
    }
}
