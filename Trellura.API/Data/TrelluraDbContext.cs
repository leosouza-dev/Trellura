using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trellura.API.Models;

namespace Trellura.API.Data
{
    public class TrelluraDbContext : DbContext
    {
        public TrelluraDbContext(DbContextOptions<TrelluraDbContext> options) : base(options)
        {

        }

        public DbSet<Card> Cards { get; set; }
    }
}
