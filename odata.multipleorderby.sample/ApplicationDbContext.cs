using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OData.MultipleOrderBy.Sample
{
    public class DbUser
    {
        public Guid Id { get; set; }
        public Name Name { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public ICollection<DbUserRole> UserRoles { get; set; }
    }

    public class DbUserRole
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DbUser User { get; set; }
        public DbRole Role { get; set; }
    }

    public class DbRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<DbUserRole> UserRoles { get; set; }
    }

    

    public class ApplicationDbContext : DbContext
    {
        public const string SchemaName = "identity";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<DbUser> Users { get; set; }
        public DbSet<DbRole> Roles { get; set; }
        public DbSet<DbUserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(SchemaName);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }

    public class ApplicationUserConfiguration : IEntityTypeConfiguration<DbUser>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<DbUser> builder)
        {
            // add indexes for fields we will look up the most
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.UserName).IsUnique();

            builder.OwnsOne(x => x.Name, m =>
            {
                m.Property(p => p.FirstName).HasColumnName("FirstName").IsRequired();
                m.Property(p => p.LastName).HasColumnName("LastName").IsRequired();
            });

            // Each User can have many entries in the UserRole join table
            builder
                .HasMany(user => user.UserRoles)
                .WithOne(userRole => userRole.User)
                .HasForeignKey(userRole => userRole.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // builder.Metadata.FindNavigation(nameof(DbUser.UserRoles)).SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<DbRole>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<DbRole> builder)
        {
            // add indexes for fields we will look up the most
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Name).IsUnique();

            // Each Role can have many entries in the UserRole join table
            builder
                .HasMany(role => role.UserRoles)
                .WithOne(userRole => userRole.Role)
                .HasForeignKey(userRole => userRole.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // builder.Metadata.FindNavigation(nameof(ApplicationRole.UserRoles)).SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
