using Argus.Sync.Data.Models;
using ArgusProject.Models.Enums;

namespace ArgusProject.Models.Entity;

public record LendTokenDetailsBySubject : IReducerModel
{
    public string Subject { get; set; }
    public ulong Slot { get; init; }
    public string TxHash { get; init; }
    public ulong TxIndex { get; init; }
    public string OwnerPkh { get; init; }
    public string BorrowerPkh { get; set; }
    public byte[] UtxoRaw { get; init; }
    public ulong LoanAmount { get; init; }
    public ulong InterestAmount { get; init; }
    public ulong UtxoAmount { get; init; }
    public ulong TokenAmount { get; init; }
    public string OwnerAddress { get; init; }
    public string BorrowerAddress { get; set; }
    public ulong LoanDuration { get; set; }
    public ulong LoanEndTime { get; set; }
    public string OutputRefTxHash { get; init; }
    public uint OutputRefTxIndex { get; init; }
    public LevvyDatumType DatumType { get; init; }
    public string? ScriptHash { get; set; } = null;

    public LendTokenDetailsBySubject(
        string Subject,
        ulong Slot,
        string TxHash,
        ulong TxIndex,
        string OwnerPkh,
        string BorrowerPkh,
        byte[] UtxoRaw,
        ulong LoanAmount,
        ulong InterestAmount,
        ulong UtxoAmount,
        ulong TokenAmount,
        string OwnerAddress,
        string BorrowerAddress,
        ulong LoanDuration,
        ulong LoanEndTime,
        string OutputRefTxHash,
        uint OutputRefTxIndex,
        LevvyDatumType DatumType,
        string? ScriptHash = null
    )
    {
        this.Subject = Subject;
        this.Slot = Slot;
        this.TxHash = TxHash;
        this.TxIndex = TxIndex;
        this.OwnerPkh = OwnerPkh;
        this.BorrowerPkh = BorrowerPkh;
        this.UtxoRaw = UtxoRaw;
        this.LoanAmount = LoanAmount;
        this.InterestAmount = InterestAmount;
        this.UtxoAmount = UtxoAmount;
        this.TokenAmount = TokenAmount;
        this.OwnerAddress = OwnerAddress;
        this.BorrowerAddress = BorrowerAddress;
        this.LoanDuration = LoanDuration;
        this.LoanEndTime = LoanEndTime;
        this.OutputRefTxHash = OutputRefTxHash;
        this.OutputRefTxIndex = OutputRefTxIndex;
        this.DatumType = DatumType;
        this.ScriptHash = ScriptHash;
    }

    // public TransactionOutput? Utxo => CborSerializer.Deserialize<TransactionOutput>(UtxoRaw);
    // public TokenDatum? TokenDatum => Utxo switch
    // {
    //     BabbageTransactionOutput babbage => babbage.Datum switch
    //     {
    //         InlineDatumOption inlineDatum => CborSerializer.Deserialize<TokenDatum>(inlineDatum.Data.Value),
    //         _ => null
    //     },
    //     _ => null
    // };
}