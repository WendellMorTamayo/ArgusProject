using Chrysalis.Cbor.Serialization.Attributes;
using Chrysalis.Cbor.Types;
using Chrysalis.Cbor.Types.Plutus.Address;

namespace ArgusProject.Models.Cbor;

[CborSerializable]
[CborUnion]
public abstract partial record TokenDatum : CborBase;

[CborSerializable]
[CborConstr(0)]
public partial record LendTokenDatum(LendTokenDetails LendTokenDetails) : TokenDatum;

[CborSerializable]
[CborConstr(1)]
public partial record BorrowTokenDatum(BorrowTokenDetails BorrowTokenDetails) : TokenDatum;

[CborSerializable]
[CborConstr(2)]
public partial record RepayTokenDatum(RepayTokenDetails RepayTokenDetails) : TokenDatum;

[CborSerializable]
[CborConstr(0)]
public partial record LendTokenDetails(
    [CborOrder(0)]
    Address AdaOwner,

    [CborOrder(1)]
    byte[] PolicyId,

    [CborOrder(2)]
    byte[] AssetName,

    [CborOrder(3)]
    ulong TokenAmount,

    [CborOrder(4)]
    ulong LoanAmount,

    [CborOrder(5)]
    ulong InterestAmount,

    [CborOrder(6)]
    ulong LoanDuration,

    [CborOrder(7)]
    OutputReference OutputReference
) : CborBase;

[CborSerializable]
[CborConstr(1)]
public partial record BorrowTokenDetails(
    [CborOrder(0)]
    Address AdaOwner,

    [CborOrder(1)]
    Address AssetOwner,

    [CborOrder(2)]
    byte[] PolicyId,

    [CborOrder(3)]
    byte[] AssetName,

    [CborOrder(4)]
    ulong TokenAmount,

    [CborOrder(5)]
    ulong LoanAmount,

    [CborOrder(6)]
    ulong InterestAmount,

    [CborOrder(7)]
    ulong LoanEndTime,

    [CborOrder(8)]
    OutputReference OutputReference
) : CborBase;

[CborSerializable]
[CborConstr(2)]
public partial record RepayTokenDetails(
    [CborOrder(0)]
    Address AdaOwner,

    [CborOrder(1)]
    ulong TokenAmount,

    [CborOrder(2)]
    ulong LoanAmount,

    [CborOrder(3)]
    ulong InterestAmount,

    [CborOrder(4)]
    OutputReference OutputReference
) : CborBase;