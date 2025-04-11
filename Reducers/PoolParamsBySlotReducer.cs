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
using Chrysalis.Cbor.Types.Cardano.Core.Common;
using Chrysalis.Cbor.Types.Cardano.Core.Transaction;
using Chrysalis.Cbor.Types.Plutus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WalletAddress = Chrysalis.Wallet.Models.Addresses.Address;

namespace ArgusProject.Reducers;

public class PoolParamsBySlotReducer(
    IDbContextFactory<TestDbContext> dbContextFactory,
    IConfiguration configuration
) : IReducer<PoolParamsBySlot>
{
    private readonly string _poolParamsPolicyId = configuration.GetValue("GlobalParamsSubject", "caae00af42cdc71b9b76de4b4611c1de1185eb1866c781d582cbebb1");
    public async Task RollBackwardAsync(ulong slot)
    {
        using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IQueryable<PoolParamsBySlot> entriesToRemove = dbContext.PoolParamsBySlot.Where(e => e.Slot >= slot);

        dbContext.RemoveRange(entriesToRemove);
        await dbContext.SaveChangesAsync();
    }

    public async Task RollForwardAsync(Block block)
    {
        using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IEnumerable<TransactionBody> transactions = block.TransactionBodies();
        if (!transactions.Any()) return;

        IEnumerable<PoolParamsBySlot> processedOutputs = ProcessOutputs(transactions, block);

        dbContext.PoolParamsBySlot.AddRange(processedOutputs);
        await dbContext.SaveChangesAsync();
    }

    public IEnumerable<PoolParamsBySlot> ProcessOutputs(IEnumerable<TransactionBody> transactions, Block block)
    {
        ulong slot = block.Header().HeaderBody().Slot();

        return transactions.SelectMany(tx =>
        {
            string txHash = tx.Hash().ToLowerInvariant();

            return tx.Outputs()
                .Select((output, index) => new { Output = output, Index = (ulong)index })
                .Where(e =>
                {
                    Dictionary<byte[], TokenBundleOutput> multiAsset = e.Output.Amount().MultiAsset();
                    return multiAsset.Any(asset => Convert.ToHexStringLower(asset.Key) == _poolParamsPolicyId);
                })
                .Where(e =>
                {
                    string? outputBech32Addr = new WalletAddress(e.Output.Address()).ToBech32();
                    return !string.IsNullOrEmpty(outputBech32Addr) && outputBech32Addr.StartsWith("addr");
                })
                .SelectMany(e =>
                {
                    string outputBech32Addr = new WalletAddress(e.Output.Address()).ToBech32();
                    byte[]? datumBytes = e.Output.Datum();

                    KeyValuePair<byte[], TokenBundleOutput> policyEntry = e.Output.Amount().MultiAsset()
                        .First(asset => Convert.ToHexStringLower(asset.Key) == _poolParamsPolicyId);
                    TokenBundleOutput tokenBundle = policyEntry.Value;

                    return tokenBundle.ToDict().Select(assetEntry =>
                    {
                        string assetName = Convert.ToHexStringLower(assetEntry.Key);
                        ulong assetAmount = assetEntry.Value;
                        if (assetAmount == 0 || datumBytes == null || datumBytes.Length == 0) return null;

                        try
                        {
                            Cip68<ProtocolParamsDatum> globalParamsDetails = CborSerializer.Deserialize<Cip68<ProtocolParamsDatum>>(datumBytes);
                            if (globalParamsDetails == null) return null;

                            PoolParams? protocolParams = globalParamsDetails.Extra switch
                            {
                                PoolParams pool => pool,
                                ProtocolParamsDatum ppd when ppd is PoolParams => (PoolParams)ppd,
                                _ => null
                            };

                            if (protocolParams == null) return null;

                            Subject principalAsset = protocolParams.PoolParamsDetails.PoolDetails.PrincipalAsset;
                            Subject collateralAsset = protocolParams.PoolParamsDetails.PoolDetails.CollateralAsset;

                            string principalSubject = Convert.ToHexStringLower(principalAsset.PolicyId) + Convert.ToHexStringLower(principalAsset.AssetName);
                            string collateralSubject = Convert.ToHexStringLower(collateralAsset.PolicyId) + Convert.ToHexStringLower(collateralAsset.AssetName);

                            return new PoolParamsBySlot(
                                PrincipalAssetSubject: principalSubject,
                                CollateralAssetSubject: collateralSubject,
                                FeeAddress: outputBech32Addr,
                                Slot: slot,
                                TxHash: txHash,
                                TxIndex: e.Index,
                                PoolSubject: _poolParamsPolicyId + assetName,
                                DatumRaw: datumBytes
                            );
                        }
                        catch
                        {
                            return null;
                        }
                    });
                })
                .Where(entry => entry != null)
                .Select(entry => entry!);
        });
    }
}