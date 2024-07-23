using Core.Domain;
using Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Infrastructure
{
    public class SwfIdentityContext : IdentityDbContext<IdentityUser>
    {
        private SwfSeeder _seeder;
        public SwfIdentityContext(DbContextOptions<SwfIdentityContext> options)
            : base(options) 
        {
            _seeder = new SwfSeeder();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            List<ApplicationUser> users = _seeder.SeedIdentityUsers();
            List<IdentityRole> roles = _seeder.SeedRoles();

            builder.Entity<ApplicationUser>().HasData(users);
            builder.Entity<IdentityRole>().HasData(roles);

            List<IdentityUserRole<string>> userRoles = _seeder.SeedIdentityUserRoles();
            builder.Entity<IdentityUserRole<string>>().HasData(userRoles);

            base.OnModelCreating(builder);
        }
    }
}