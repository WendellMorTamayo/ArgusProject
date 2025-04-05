using Argus.Sync.Data;
using ArgusProject.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ArgusProject.Models;

public class TestDbContext
(
    DbContextOptions<TestDbContext> options,
    IConfiguration configuration
) : CardanoDbContext(options, configuration)
{
    public DbSet<LendTokenDetailsBySubject> LendTokenDetailsBySubject => Set<LendTokenDetailsBySubject>();
    public DbSet<LendTokenDetailsBySlot> LendTokenDetailsBySlot => Set<LendTokenDetailsBySlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LendTokenDetailsBySubject>(entity =>
        {
            entity.HasKey(e => new { e.Subject, e.Slot, e.TxHash, e.TxIndex });
        });

        modelBuilder.Entity<LendTokenDetailsBySlot>(entity =>
        {
            entity.HasKey(e => new { e.Subject, e.Slot, e.TxHash, e.TxIndex, e.UtxoStatus });
        });
    }
}