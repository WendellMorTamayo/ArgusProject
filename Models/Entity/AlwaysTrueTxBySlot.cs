using Argus.Sync.Data.Models;

namespace ArgusProject.Models.Entity;

public record AlwaysTrueTxBySlot(
    string TxHash,
    ulong TxIndex,
    ulong Slot,
    string Address,
    byte[] RawData
) : IReducerModel;