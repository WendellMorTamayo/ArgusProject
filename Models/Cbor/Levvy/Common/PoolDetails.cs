using ArgusProject.Models.Cbor.Levvy.ProtocolParams;
using Chrysalis.Cbor.Serialization.Attributes;
using Chrysalis.Cbor.Types;

namespace ArgusProject.Models.Cbor.Levvy.Common;

[CborSerializable]
[CborConstr(0)]
public partial record PoolDetails(
    [CborOrder(0)]
    Subject PrincipalAsset,

    [CborOrder(1)]
    Subject CollateralAsset
) : CborBase;
