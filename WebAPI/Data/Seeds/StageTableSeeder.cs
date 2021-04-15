using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Data.Seeds
{
    public static class StageTableSeeder
    {
        public static void StageSeed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stage>().HasData(
                new { Id = 1, Name = "Grade 9 / Sorani" },
                new { Id = 2, Name = "Grade 9 / Badini" },
                new { Id = 3, Name = "Grade 12 Literature / Sorani" },
                new { Id = 4, Name = "Grade 12 Literature / Badini" },
                new { Id = 5, Name = "Grade 12 Science / Sorani" },
                new { Id = 6, Name = "Grade 12 Science / Badini" }
            );
        }
    }
}
