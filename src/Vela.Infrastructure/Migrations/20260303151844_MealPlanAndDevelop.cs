﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vela.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MealPlanAndDevelop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: MealPlan tables were already correctly created by AddMealPlanTables migration.
            // This migration was generated from a stale snapshot after a branch merge.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: corresponds to empty Up().
        }
    }
}
