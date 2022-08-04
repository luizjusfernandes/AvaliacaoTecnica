using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Registration.Models
{
    public partial class RegistrationContext : DbContext
    {
        public RegistrationContext()
        {
        }

        public RegistrationContext(DbContextOptions<RegistrationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Record> Records { get; set; } = null!;
        public virtual DbSet<UserInfo> UserInfos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Filename=Registration.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasOne<Record>(d => d.Record)
                    .WithOne(p => p.User)
                    .HasForeignKey<Record>(d => d.UserId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
