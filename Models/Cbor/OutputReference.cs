using Chrysalis.Cbor.Serialization.Attributes;
using Chrysalis.Cbor.Types;

namespace ArgusProject.Models.Cbor;

[CborSerializable]
[CborConstr(0)]
public partial record OutputReference(
    [CborOrder(0)]
    byte[] TransactionId,

    [CborOrder(1)]
    ulong Index
) : CborBase;