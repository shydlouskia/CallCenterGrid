using CallCenter.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CallCenter.Api.Data;

public class AppDbContext : DbContext
{
    public DbSet<Contact> Contacts => Set<Contact>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var c = modelBuilder.Entity<Contact>();
        c.ToTable("Contacts");
        c.HasKey(x => x.Id);
        c.Property(x => x.Id).ValueGeneratedNever();

        c.HasIndex(x => x.LastName);
        c.HasIndex(x => x.FirstName);
        c.HasIndex(x => x.Email);
        c.HasIndex(x => x.City);
        c.HasIndex(x => x.State);
        c.HasIndex(x => x.Status);
        c.HasIndex(x => x.Age);
    }
}
