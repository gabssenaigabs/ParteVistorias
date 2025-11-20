using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondoHub.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CondoHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<NoticeFoto> NoticeFotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar comportamento de exclusão para Property
            modelBuilder.Entity<Notice>()
                .HasOne(n => n.Property)
                .WithMany()
                .HasForeignKey(n => n.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Property)
                .WithMany()
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}