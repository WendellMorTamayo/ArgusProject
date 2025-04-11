using Argus.Sync.Data.Models;

namespace ArgusProject.Models.Entity;

public record PoolParamsBySlot(
    string PrincipalAssetSubject,
    string CollateralAssetSubject,
    string FeeAddress,
    ulong Slot,
    string TxHash,
    ulong TxIndex,
    string PoolSubject,
    byte[]? DatumRaw
) : IReducerModel;