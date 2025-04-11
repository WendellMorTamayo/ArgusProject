using Argus.Sync.Data.Models;

namespace ArgusProject.Models.Entity;

public record GlobalParamsBySlot(
    string Subject,
    string FeeAddress,
    ulong Slot,
    string TxHash,
    ulong TxIndex,
    string PoolParamsPolicy,
    string NftPrefix,
    string NftLendImage,
    string NftBorrowImage,
    string NftClaimableImage,
    byte[]? DatumRaw
) : IReducerModel;