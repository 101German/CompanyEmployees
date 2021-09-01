﻿using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions options):base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Company> Companies { get; set; }
    }
}
