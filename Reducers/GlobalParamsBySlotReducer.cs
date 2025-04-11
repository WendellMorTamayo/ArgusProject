using Argus.Sync.Reducers;
using ArgusProject.Extensions;
using ArgusProject.Models;
using ArgusProject.Models.Cbor.Levvy.ProtocolParams;
using ArgusProject.Models.Entity;
using Chrysalis.Cbor.Extensions.Cardano.Core;
using Chrysalis.Cbor.Extensions.Cardano.Core.Common;
using Chrysalis.Cbor.Extensions.Cardano.Core.Header;
using Chrysalis.Cbor.Extensions.Cardano.Core.Transaction;
using Chrysalis.Cbor.Serialization;
using Chrysalis.Cbor.Types.Cardano.Core;
using Chrysalis.Cbor.Types.Cardano.Core.Transaction;
using Chrysalis.Cbor.Types.Plutus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WalletAddress = Chrysalis.Wallet.Models.Addresses.Address;

namespace ArgusProject.Reducers;

public class GlobalParamsBySlotReducer(
    IDbContextFactory<TestDbContext> dbContextFactory,
    IConfiguration configuration
) : IReducer<GlobalParamsBySlot>
{
    private readonly string _globalParamsSubject = configuration.GetValue("GlobalParamsSubject", "caae00af42cdc71b9b76de4b4611c1de1185eb1866c781d582cbebb1000643b0043fce0f4728689f75a76d15695316cf70ffb51270aaef2540c3e9bb");

    public async Task RollBackwardAsync(ulong slot)
    {
        using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IQueryable<GlobalParamsBySlot> entriesToRemove = dbContext.GlobalParamsBySlot.Where(e => e.Slot >= slot);

        dbContext.RemoveRange(entriesToRemove);
        await dbContext.SaveChangesAsync();
    }

    public async Task RollForwardAsync(Block block)
    {
        using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IEnumerable<TransactionBody> transactions = block.TransactionBodies();
        if (!transactions.Any()) return;

        transactions.ToList().ForEach(tx => ProcessOutputs(tx, block, dbContext));
        await dbContext.SaveChangesAsync();
    }

    public void ProcessOutputs(TransactionBody tx, Block block, TestDbContext dbContext)
    {
        ulong slot = block.Header().HeaderBody().Slot();
        string txHash = tx.Hash().ToLowerInvariant();

        tx.Outputs()
            .Select((output, index) => new { Output = output, Index = (ulong)index })
            .ToList()
            .ForEach(e =>
            {
                ulong? quantity = e.Output.Amount().QuantityOf(_globalParamsSubject);
                if (quantity is null || quantity is not 1) return;

                string? outputBech32Addr = new WalletAddress(e.Output.Address()).ToBech32();
                if (string.IsNullOrEmpty(outputBech32Addr) || !outputBech32Addr.StartsWith("addr")) return;
                Cip68<ProtocolParamsDatum>? globalParamsDetails = null;
                try
                {
                    globalParamsDetails = CborSerializer.Deserialize<Cip68<ProtocolParamsDatum>>(e.Output.Datum());
                }
                catch
                {
                    return;
                }

                ProtocolParamsDatum? protocolParams = globalParamsDetails.Extra switch
                {
                    GlobalParams global => global,
                    _ => null
                };

                if (protocolParams is null) return;

                GlobalParamsBySlot? newEntry = null;
                if (protocolParams is GlobalParams globalParams)
                {
                    newEntry = new GlobalParamsBySlot(
                       Subject: _globalParamsSubject,
                       Slot: slot,
                       TxHash: txHash,
                       TxIndex: e.Index,
                       FeeAddress: outputBech32Addr,
                       PoolParamsPolicy: Convert.ToHexStringLower(globalParams.GlobalParamsDetails.PoolParamsPolicy),
                       NftPrefix: Convert.ToHexStringLower(globalParams.GlobalParamsDetails.NftImage),
                       NftLendImage: Convert.ToHexStringLower(globalParams.GlobalParamsDetails.NftImage),
                       NftBorrowImage: Convert.ToHexStringLower(globalParams.GlobalParamsDetails.NftImage),
                       NftClaimableImage: Convert.ToHexStringLower(globalParams.GlobalParamsDetails.NftImage),
                       DatumRaw: e.Output.Datum()
                   );
                }

                if (newEntry is null) return;
                dbContext.GlobalParamsBySlot.Add(newEntry);
            });
    }
}