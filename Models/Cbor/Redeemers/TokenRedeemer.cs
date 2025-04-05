using Chrysalis.Cbor.Serialization.Attributes;
using Chrysalis.Cbor.Types;

namespace ArgusProject.Models.Cbor.Redeemers;

[CborSerializable]
[CborUnion]
public abstract partial record TokenRedeemer : CborBase;

[CborSerializable]
[CborConstr(0)]
public partial record BorrowTokenAction : TokenRedeemer;

[CborSerializable]
[CborConstr(1)]
public partial record RepayTokenAction : TokenRedeemer;

[CborSerializable]
[CborConstr(2)]
public partial record ClaimTokenAction : TokenRedeemer;

[CborSerializable]
[CborConstr(3)]
public partial record ForecloseTokenAction : TokenRedeemer;

[CborSerializable]
[CborConstr(4)]
public partial record CancelTokenAction : TokenRedeemer;
