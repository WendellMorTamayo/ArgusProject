using Chrysalis.Wallet.Models.Enums;
using Microsoft.Extensions.Configuration;

namespace ArgusProject.Utils;

public static class NetworkUtils
{
    public static NetworkType GetNetworkType(IConfiguration configuration)
    {
        return configuration.GetValue<int>("CardanoNodeConnection:NetworkMagic") switch
        {
            764824073 => NetworkType.Mainnet,
            1 => NetworkType.Preprod,
            2 => NetworkType.Preview,
            _ => throw new NotImplementedException()
        };
    }
}