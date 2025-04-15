using Argus.Sync.Reducers;
using ArgusProject.Models;
using ArgusProject.Models.Entity;
using Chrysalis.Cbor.Extensions.Cardano.Core;
using Chrysalis.Cbor.Extensions.Cardano.Core.Header;
using Chrysalis.Cbor.Extensions.Cardano.Core.Transaction;
using Chrysalis.Cbor.Types.Cardano.Core;
using Chrysalis.Cbor.Types.Cardano.Core.Transaction;
using Chrysalis.Wallet.Models.Addresses;
using Microsoft.EntityFrameworkCore;

namespace ArgusProject.Reducers;

public class AlwaysTrueTxBySlotReducer(
    IDbContextFactory<TestDbContext> dbContextFactory
) : IReducer<AlwaysTrueTxBySlot>
{
    private readonly string _alwaysTrueScriptHash = "392dc6ca387fde5ef8afd5b1f23f209b9f22e11c6cf394c9e36fdeda";
    public async Task RollBackwardAsync(ulong slot)
    {
        await using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IQueryable<AlwaysTrueTxBySlot> entriesToRemove = dbContext.AlwaysTrueTxsBySlot.Where(e => e.Slot >= slot);

        dbContext.RemoveRange(entriesToRemove);
        await dbContext.SaveChangesAsync();
    }

    public async Task RollForwardAsync(Block block)
    {
        await using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IEnumerable<TransactionBody> transactions = block.TransactionBodies();
        if (!transactions.Any()) return;

        transactions.SelectMany(e => e.Outputs().Select((output, index) => new { Output = output, Index = (ulong)index, TxHash = e.Hash(), RawData = e.Raw }))
            .ToList().ForEach(entity =>
            {
                string? outputBech32Addr = null;
                try
                {
                    outputBech32Addr = new Address(entity.Output.Address()).ToBech32();
                }
                catch
                {
                    return;
                }
                if (string.IsNullOrEmpty(outputBech32Addr) || !outputBech32Addr.StartsWith("addr")) return;

                string pkh = Convert.ToHexString(new Address(entity.Output.Address()).GetPaymentKeyHash() ?? []).ToLowerInvariant();
                if (!pkh.Equals(_alwaysTrueScriptHash, StringComparison.InvariantCultureIgnoreCase)) return;

                AlwaysTrueTxBySlot newEntry = new(
                    entity.TxHash,
                    entity.Index,
                    block.Header().HeaderBody().Slot(),
                    outputBech32Addr,
                    entity.RawData!.Value.ToArray()
                );

                dbContext.AlwaysTrueTxsBySlot.Add(newEntry);
            });

        await dbContext.SaveChangesAsync();
    }
}