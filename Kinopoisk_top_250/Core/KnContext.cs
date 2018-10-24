using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Kinopoisk_top_250.Core
{
    public class KnContext : DbContext
    {
        public KnContext()
        {
        }

        public DbSet<KnFilm> Films { get; set; }
    }
}