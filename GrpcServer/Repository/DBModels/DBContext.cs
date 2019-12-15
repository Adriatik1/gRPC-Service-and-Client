using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrpcServer.Repository.DBModels
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customers> Customers { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Sales> Sales { get; set; }
        public virtual DbSet<UserXroles> UserXroles { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //    }
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Roles>(entity =>
        //    {
        //        entity.HasIndex(e => e.Name)
        //            .HasName("UQ__roles__72E12F1BF368E8F7")
        //            .IsUnique();
        //    });

        //    modelBuilder.Entity<Sales>(entity =>
        //    {
        //        entity.HasOne(d => d.Customer)
        //            .WithMany(p => p.Sales)
        //            .HasForeignKey(d => d.CustomerId)
        //            .HasConstraintName("FK__sales__customerI__2AD55B43");

        //        entity.HasOne(d => d.Product)
        //            .WithMany(p => p.Sales)
        //            .HasForeignKey(d => d.ProductId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK__sales__productID__2BC97F7C");
        //    });

        //    modelBuilder.Entity<UserXroles>(entity =>
        //    {
        //        entity.HasOne(d => d.Role)
        //            .WithMany(p => p.UserXroles)
        //            .HasForeignKey(d => d.RoleId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK__userXrole__roleI__70A8B9AE");

        //        entity.HasOne(d => d.User)
        //            .WithMany(p => p.UserXroles)
        //            .HasForeignKey(d => d.UserId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK__userXrole__userI__6FB49575");
        //    });

        //    modelBuilder.Entity<Users>(entity =>
        //    {
        //        entity.HasIndex(e => e.Username)
        //            .HasName("UQ__users__F3DBC572E7536E46")
        //            .IsUnique();
        //    });

        //    OnModelCreatingPartial(modelBuilder);
        //}

        //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
