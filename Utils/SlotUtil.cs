using ArgusProject.Models.CardanoSharp;
using Chrysalis.Wallet.Models.Enums;

namespace ArgusProject.Utils;

public static class SlotUtil
{
    public static SlotNetworkConfig Mainnet { get; set; } = new SlotNetworkConfig(1596059091000L, 4492800L, 1000);

    public static SlotNetworkConfig Preprod { get; set; } = new SlotNetworkConfig(1655769600000L, 86400L, 1000);

    public static SlotNetworkConfig Preview { get; set; } = new SlotNetworkConfig(1666656000000L, 0L, 1000);

    public static SlotNetworkConfig GetSlotNetworkConfig(NetworkType networkType)
    {
        return networkType switch
        {
            NetworkType.Mainnet => Mainnet,
            NetworkType.Preprod => Preprod,
            NetworkType.Preview => Preview,
            _ => new SlotNetworkConfig(),
        };
    }

    public static long GetPosixTimeSecondsFromSlot(SlotNetworkConfig config, long slot) => config.ZeroTime / 1000 + (slot - config.ZeroSlot);
}