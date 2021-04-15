using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class ApplicationDbContext
    {
        public DbSet<Stage> Stages { get; set; }
    }
}
