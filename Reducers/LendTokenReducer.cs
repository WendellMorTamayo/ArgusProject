using System.Linq.Expressions;
using Argus.Sync.Data.Models.Enums;
using Argus.Sync.Reducers;
using Argus.Sync.Utils;
using ArgusProject.Extensions;
using ArgusProject.Models;
using ArgusProject.Models.Cbor;
using ArgusProject.Models.Cbor.Redeemers;
using ArgusProject.Models.Entity;
using ArgusProject.Models.Enums;
using ArgusProject.Utils;
using Chrysalis.Cbor.Extensions.Cardano.Core;
using Chrysalis.Cbor.Extensions.Cardano.Core.Common;
using Chrysalis.Cbor.Extensions.Cardano.Core.Header;
using Chrysalis.Cbor.Extensions.Cardano.Core.Transaction;
using Chrysalis.Cbor.Serialization;
using Chrysalis.Cbor.Types.Cardano.Core;
using Chrysalis.Cbor.Types.Cardano.Core.Transaction;
using Chrysalis.Cbor.Types.Cardano.Core.TransactionWitness;
using Chrysalis.Wallet.Models.Addresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetworkType = Chrysalis.Wallet.Models.Enums.NetworkType;

namespace ArgusProject.Reducers;

public class LendTokenReducer(
    IDbContextFactory<TestDbContext> dbContextFactory,
    IConfiguration configuration
) : IReducer<LendTokenDetailsBySubject>
{
    private readonly string _newLendTokenValidatorSh = "05e0dae2a16c474ffee7fa5004e6b04fd408f35bdaffe0e690d8dd7c";

    private readonly NetworkType _networkType = NetworkUtils.GetNetworkType(configuration);

    public async Task RollBackwardAsync(ulong slot)
    {
        using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        IQueryable<LendTokenDetailsBySubject> rollbackEntriesUtxo = dbContext.LendTokenDetailsBySubject
            .Where(ltdbs => ltdbs.Slot > slot);

        IQueryable<LendTokenDetailsBySlot> rollbackEntriesHistorical = dbContext.LendTokenDetailsBySlot
            .Where(ltdbs => ltdbs.Slot > slot);

        IEnumerable<string> rollbackEntriesHistoricalOutRefs = await rollbackEntriesHistorical
           .AsNoTracking()
           .Select(e => e.TxHash + e.TxIndex)
           .ToListAsync();

        IEnumerable<(string TxHash, ulong TxIndex)> historicalOutRefsTuple = rollbackEntriesHistoricalOutRefs
            .Select(e =>
                {
                    var txHash = e[..^1];
                    var txIndex = ulong.Parse(e.Last().ToString());
                    return (txHash, txIndex);
                }
            );

        Expression<Func<LendTokenDetailsBySlot, bool>> predicate = PredicateBuilder.False<LendTokenDetailsBySlot>();
        historicalOutRefsTuple.ToList().ForEach(inputTuple =>
            predicate = predicate.Or(e => e.TxHash == inputTuple.TxHash && e.TxIndex == inputTuple.TxIndex)
        );

        IEnumerable<LendTokenDetailsBySlot> unspentEntries = await dbContext.LendTokenDetailsBySlot
            .AsNoTracking()
            .Where(e => e.Slot <= slot)
            .Where(predicate)
            .ToListAsync();

        dbContext.RemoveRange(rollbackEntriesUtxo);
        dbContext.RemoveRange(rollbackEntriesHistorical);

        IEnumerable<LendTokenDetailsBySubject> rollbackEntries = unspentEntries.Select(e => new LendTokenDetailsBySubject(
            Subject: e.Subject,
            Slot: e.Slot,
            TxHash: e.TxHash,
            TxIndex: e.TxIndex,
            OwnerPkh: e.OwnerPkh,
            BorrowerPkh: e.BorrowerPkh,
            UtxoRaw: e.UtxoRaw,
            DatumType: e.DatumType,
            LoanAmount: e.LoanAmount,
            InterestAmount: e.InterestAmount,
            UtxoAmount: e.UtxoAmount,
            TokenAmount: e.TokenAmount,
            OwnerAddress: e.OwnerAddress,
            BorrowerAddress: e.BorrowerAddress,
            LoanDuration: e.LoanDuration,
            LoanEndTime: e.LoanEndTime,
            OutputRefTxHash: e.OutputRefTxHash,
            OutputRefTxIndex: e.OutputRefTxIndex,
            ScriptHash: e.ScriptHash
        ));

        dbContext.LendTokenDetailsBySubject.AddRange(rollbackEntries);

        await dbContext.SaveChangesAsync();
    }

    public async Task RollForwardAsync(Block block)
    {
        using TestDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        IEnumerable<TransactionBody> transactions = block.TransactionBodies();
        if (!transactions.Any()) return;

        transactions.ToList().ForEach(tx => ProcessOutputs(tx, block, dbContext));

        Expression<Func<LendTokenDetailsBySlot, bool>> repayOutRefPredicate = PredicateBuilder.False<LendTokenDetailsBySlot>();

        dbContext.LendTokenDetailsBySlot.Local
            .Where(e => e.DatumType == LevvyDatumType.RepayTokenDatum)
            .Select(e => (e.OutputRefTxHash, e.OutputRefTxIndex))
            .ToList()
            .ForEach(outputRef =>
            {
                if (!string.IsNullOrEmpty(outputRef.OutputRefTxHash) && !outputRef.OutputRefTxHash.Equals("00"))
                {
                    repayOutRefPredicate = repayOutRefPredicate.Or(e => e.TxHash == outputRef.OutputRefTxHash && e.TxIndex == outputRef.OutputRefTxIndex);
                }
            });

        List<LendTokenDetailsBySlot> remoteRepayOutRefEntries = await dbContext.LendTokenDetailsBySlot
            .AsNoTracking()
            .Where(e => e.DatumType == LevvyDatumType.BorrowTokenDatum)
            .Where(repayOutRefPredicate)
            .ToListAsync();

        List<LendTokenDetailsBySlot> repayOutRefEntries = [.. remoteRepayOutRefEntries, .. dbContext.LendTokenDetailsBySlot.Local];

        dbContext.LendTokenDetailsBySlot.Local
            .Where(e => e.DatumType == LevvyDatumType.RepayTokenDatum)
            .ToList()
            .ForEach(e =>
            {
                LendTokenDetailsBySlot? parentBorrowEntry = repayOutRefEntries
                    .Where(parent => parent.TxHash == e.OutputRefTxHash && parent.TxIndex == e.OutputRefTxIndex)
                    .FirstOrDefault();
                if (parentBorrowEntry is null) return;

                e.Subject = parentBorrowEntry.Subject;
                e.BorrowerPkh = parentBorrowEntry.BorrowerPkh;
                e.BorrowerAddress = parentBorrowEntry.BorrowerAddress;
                e.LoanDuration = parentBorrowEntry.LoanDuration;
                e.LoanEndTime = parentBorrowEntry.LoanEndTime;
            });

        // Get all input outrefs
        List<((string SpentTxHash, string TxHash, ulong TxIndex) Id, RedeemerEntry? Redeemer)> inputsTuple = [.. transactions
            .SelectMany(
                tx => tx.Inputs(),
                (tx, input) =>
                    (
                        (tx.Hash(), Convert.ToHexStringLower(input.TransactionId), input.Index),
                        input.Redeemer(block)
                    )
            )];

        IEnumerable<string> inputOutRefs = inputsTuple.Select(inputTuple => inputTuple.Id.TxHash + inputTuple.Id.TxIndex);

        Expression<Func<LendTokenDetailsBySlot, bool>> predicate = PredicateBuilder.False<LendTokenDetailsBySlot>();

        inputsTuple.ForEach(inputTuple =>
            predicate = predicate.Or(ltdbs => ltdbs.TxHash == inputTuple.Id.TxHash && ltdbs.TxIndex == inputTuple.Id.TxIndex)
        );


        // Fetch all the corresponding entry from the database, if there's any
        // Note: an output can be spent within the same block, so we also need to check
        // all outputs processed in this block
        List<LendTokenDetailsBySlot> lendTokenDetailsBySlotEntries = await dbContext.LendTokenDetailsBySlot
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();

        List<LendTokenDetailsBySlot> lendTokenDetailsBySlotLocalEntries = [.. dbContext.LendTokenDetailsBySlot.Local.Where(ltdbs => inputOutRefs.Contains(ltdbs.TxHash + ltdbs.TxIndex))];

        // Merge
        lendTokenDetailsBySlotEntries.AddRange(lendTokenDetailsBySlotLocalEntries);

        IEnumerable<(LendTokenDetailsBySlot ltdbs, (string SpentTxHash, RedeemerEntry? Redeemer) ParentTx)> lendTokenDetailsBySlotRedeemerTuple = lendTokenDetailsBySlotEntries
            .Select(ltdbs => (
                    ltdbs,
                    inputsTuple
                        .Where(it => it.Id.TxHash == ltdbs.TxHash && it.Id.TxIndex == ltdbs.TxIndex)
                        .Select(it => (it.Id.SpentTxHash, it.Redeemer))
                        .FirstOrDefault()
                )
            );

        ProcessInputs(lendTokenDetailsBySlotRedeemerTuple, block, dbContext);

        List<string> spentLendTokenDetailsBySlotOutrefs = [.. dbContext.LendTokenDetailsBySlot.Local
            .Where(ltdbs => ltdbs.UtxoStatus == UtxoStatus.Spent)
            .Select(ltdbs => ltdbs.TxHash + ltdbs.TxIndex)];

        IQueryable<LendTokenDetailsBySubject> spentLendTokenDetailsBySubjectEntries = dbContext.LendTokenDetailsBySubject
            .Where(ldtdbs => spentLendTokenDetailsBySlotOutrefs.Contains(ldtdbs.TxHash + ldtdbs.TxIndex));

        dbContext.LendTokenDetailsBySubject.RemoveRange(spentLendTokenDetailsBySubjectEntries);

        List<LendTokenDetailsBySlot> unspentLendTokenDetailsBySlotEntries = [.. dbContext.LendTokenDetailsBySlot.Local
            .GroupBy(ldtdbs => new { ldtdbs.TxHash, ldtdbs.TxIndex })
            .Where(g => g.Count() < 2)
            .Select(g => g.First())
            .Where(g => g.UtxoStatus != UtxoStatus.Spent)];

        IEnumerable<LendTokenDetailsBySubject> lendTokenDetailsBySubjectEntries = unspentLendTokenDetailsBySlotEntries.Select(ldtdbs => new LendTokenDetailsBySubject(
            Subject: ldtdbs.Subject,
            Slot: ldtdbs.Slot,
            TxHash: ldtdbs.TxHash,
            TxIndex: ldtdbs.TxIndex,
            OwnerPkh: ldtdbs.OwnerPkh,
            BorrowerPkh: ldtdbs.BorrowerPkh,
            UtxoRaw: ldtdbs.UtxoRaw,
            DatumType: ldtdbs.DatumType,
            LoanAmount: ldtdbs.LoanAmount,
            InterestAmount: ldtdbs.InterestAmount,
            UtxoAmount: ldtdbs.UtxoAmount,
            TokenAmount: ldtdbs.TokenAmount,
            OwnerAddress: ldtdbs.OwnerAddress,
            BorrowerAddress: ldtdbs.BorrowerAddress,
            LoanDuration: ldtdbs.LoanDuration,
            LoanEndTime: ldtdbs.LoanEndTime,
            OutputRefTxHash: ldtdbs.OutputRefTxHash,
            OutputRefTxIndex: ldtdbs.OutputRefTxIndex,
            ScriptHash: ldtdbs.ScriptHash
        ));

        dbContext.LendTokenDetailsBySubject.AddRange(lendTokenDetailsBySubjectEntries);
        await dbContext.SaveChangesAsync();
    }

    private static void ProcessInputs(IEnumerable<(LendTokenDetailsBySlot Ltdbs, (string SpentTxHash, RedeemerEntry? Redeemer) ParentTx)> lendTokenDetailsBySlotRedeemerTuple, Block block, TestDbContext dbContext)
    {
        lendTokenDetailsBySlotRedeemerTuple.ToList().ForEach(ltdbsTuple =>
        {

            // Check if there's a redeemer
            
            byte[]? redeemer = CborSerializer.Serialize(ltdbsTuple.ParentTx.Redeemer!.Data);
            TokenAction actionType = TokenAction.CreateAction;

            if (redeemer is not null)
            {
                try
                {
                    TokenRedeemer? tokenRedeemer = CborSerializer.Deserialize<TokenRedeemer>(redeemer);
                    actionType = tokenRedeemer.GetActionType();
                }
                catch (Exception)
                {

                }
            }

            LendTokenDetailsBySlot updatedEntry = ltdbsTuple.Ltdbs with
            {
                Slot = block.Header().HeaderBody().Slot(),
                UtxoStatus = UtxoStatus.Spent,
                ActionType = actionType,
                SpentTxHash = ltdbsTuple.ParentTx.SpentTxHash
            };

            dbContext.Add(updatedEntry);
        });
    }

    public void ProcessOutputs(TransactionBody tx, Block block, TestDbContext dbContext)
    {
        ulong slot = block.Header().HeaderBody().Slot();
        string txHash = tx.Hash().ToLowerInvariant();

        tx.Outputs().Select((output, index) => new { Output = output, Index = (ulong)index })
            .ToList().ForEach(e =>
            {
                string? outputBech32Addr = new Address(e.Output.Address()).ToBech32();
                if (string.IsNullOrEmpty(outputBech32Addr) || !outputBech32Addr.StartsWith("addr")) return;

                string pkh = Convert.ToHexString(new Address(e.Output.Address()).GetPaymentKeyHash() ?? []).ToLowerInvariant();
                if (!pkh.Equals(_newLendTokenValidatorSh, StringComparison.InvariantCultureIgnoreCase)) return;

                if (e.Output.Datum() is null) return;
                try
                {
                    TokenDatum? datum = CborSerializer.Deserialize<TokenDatum>(e.Output.Datum());
                    if (datum is null) return;

                    LevvyDatumType datumType = datum switch
                    {
                        LendTokenDatum => LevvyDatumType.LendTokenDatum,
                        BorrowTokenDatum => LevvyDatumType.BorrowTokenDatum,
                        RepayTokenDatum => LevvyDatumType.RepayTokenDatum,
                        _ => throw new Exception("Invalid datum")
                    };

                    TokenAction actionType = TokenAction.CreateAction;

                    //   string poolPolicyId = datum
                    string poolPolicyId = datum.GetPolicyId();
                    string poolAssetName = datum.GetAssetName();
                    string poolSubject = poolPolicyId + poolAssetName;
                    Address? adaOwnerAddr = datum.GetAdaOwnerAddress(_networkType);
                    Address? assetOwnerAddr = datum.GetAssetOwnerAddress(_networkType);

                    ulong loanAmount = datum.GetLoanAmount();
                    ulong tokenAmount = datum.GetTokenAmount();
                    ulong interestAmount = datum.GetInterestAmount();
                    ulong utxoAmount = e.Output.Amount().Lovelace(); // TODO

                    ulong loanDuration = datum.GetLoanDuration(slot, _networkType);
                    ulong loanEndTime = (ulong)datum.GetLoanEndTimePosix();


                    string outputRefTxHash = datum.GetOutref().TxHash;
                    ulong outRefTxIndex = datum.GetOutref().TxIndex;

                    string ownerPkh = string.Empty;
                    string borrowerPkh = string.Empty;
                    string ownerAddr = string.Empty;
                    string borrowerAddr = string.Empty;

                    try
                    {
                        ownerPkh = Convert.ToHexString(adaOwnerAddr?.GetPaymentKeyHash() ?? []).ToLowerInvariant();
                        ownerAddr = adaOwnerAddr?.ToBech32() ?? string.Empty;
                    }
                    catch { }

                    try
                    {
                        borrowerPkh = Convert.ToHexString(assetOwnerAddr?.GetPaymentKeyHash() ?? []).ToLowerInvariant();
                        borrowerAddr = assetOwnerAddr?.ToBech32() ?? string.Empty;
                    }
                    catch { }

                    LendTokenDetailsBySlot entry = new(
                        Subject: poolSubject,
                        Slot: slot,
                        TxHash: tx.Hash(),
                        TxIndex: (uint)e.Index,
                        OwnerPkh: ownerPkh,
                        BorrowerPkh: borrowerPkh,
                        UtxoRaw: e.Output.Raw.HasValue ? e.Output.Raw.Value.ToArray() : [],
                        LoanAmount: loanAmount,
                        InterestAmount: interestAmount,
                        UtxoAmount: utxoAmount,
                        TokenAmount: tokenAmount,
                        OwnerAddress: ownerAddr,
                        BorrowerAddress: borrowerAddr,
                        LoanDuration: loanDuration,
                        LoanEndTime: loanEndTime,
                        OutputRefTxHash: outputRefTxHash,
                        OutputRefTxIndex: (uint)outRefTxIndex,
                        ActionType: actionType,
                        DatumType: datumType,
                        SpentTxHash: string.Empty,
                        ScriptHash: pkh
                    );

                    dbContext.LendTokenDetailsBySlot.Add(entry);
                }
                catch
                {

                }
            });
    }
}