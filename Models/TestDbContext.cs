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
    public DbSet<GlobalParamsBySlot> GlobalParamsBySlot => Set<GlobalParamsBySlot>();
    public DbSet<PoolParamsBySlot> PoolParamsBySlot => Set<PoolParamsBySlot>();

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

        modelBuilder.Entity<GlobalParamsBySlot>(entity =>
        {
            entity.HasKey(e => new { e.TxHash, e.TxIndex, e.Slot });
        });

        modelBuilder.Entity<PoolParamsBySlot>(entity =>
        {
            entity.HasKey(e => new { e.PoolSubject, e.TxHash, e.TxIndex, e.Slot });
        });
    }
}