using ArgusProject.Models.Cbor.Levvy.Common;
using Chrysalis.Cbor.Serialization.Attributes;
using Chrysalis.Cbor.Types;
using Chrysalis.Cbor.Types.Plutus.Address;

namespace ArgusProject.Models.Cbor.Levvy.ProtocolParams;

[CborSerializable]
[CborConstr(0)]
public partial record GlobalParamsDetails(
    [CborOrder(0)]
    Rational Fee,

    [CborOrder(1)]
    Address FeeAddress,

    [CborOrder(2)]
    MultisigScript Admin,

    [CborOrder(3)]
    byte[] PoolParamsPolicy,

    [CborOrder(4)]
    byte[] NftPositionPrefix,

    [CborOrder(5)]
    byte[] NftImage,

    [CborOrder(6)]
    byte[] ForeclosedNftImage
) : CborBase;

[CborSerializable]
[CborConstr(0)]
public partial record PoolParamsDetails(
    [CborOrder(0)]
    Rational Fee,

    [CborOrder(1)]
    Address FeeAddress,

    [CborOrder(2)]
    PoolDetails PoolDetails
) : CborBase;


[CborSerializable]
[CborConstr(0)]
public partial record Subject(
    [CborOrder(0)]
    byte[] PolicyId,

    [CborOrder(1)]
    byte[] AssetName
) : CborBase;


[CborSerializable]
[CborConstr(0)]
public partial record Rational(
    [CborOrder(0)]
    ulong Numerator,

    [CborOrder(1)]
    ulong Denominator
) : CborBase;