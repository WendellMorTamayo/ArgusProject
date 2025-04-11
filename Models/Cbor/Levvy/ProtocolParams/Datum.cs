using Chrysalis.Cbor.Serialization.Attributes;
using Chrysalis.Cbor.Types;

namespace ArgusProject.Models.Cbor.Levvy.ProtocolParams;

[CborSerializable]
[CborUnion]
public abstract partial record ProtocolParamsDatum : CborBase;

[CborSerializable]
[CborConstr(0)]
public partial record GlobalParams(GlobalParamsDetails GlobalParamsDetails) : ProtocolParamsDatum;

[CborSerializable]
[CborConstr(1)]
public partial record PoolParams(PoolParamsDetails PoolParamsDetails) : ProtocolParamsDatum;