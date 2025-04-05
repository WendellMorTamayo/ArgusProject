using ArgusProject.Models.CardanoSharp;
using ArgusProject.Models.Cbor;
using ArgusProject.Models.Common;
using ArgusProject.Models.Enums;
using ArgusProject.Utils;
using Chrysalis.Cbor.Types;
using Chrysalis.Cbor.Types.Cardano.Core.Transaction;
using Chrysalis.Cbor.Types.Plutus.Address;
using Chrysalis.Wallet.Models.Addresses;
using Chrysalis.Wallet.Models.Enums;
using Chrysalis.Wallet.Utils;
using Address = Chrysalis.Wallet.Models.Addresses.Address;
using Credential = Chrysalis.Cbor.Types.Plutus.Address.Credential;

namespace ArgusProject.Extensions;

public static class TokenDatumExtension
{
    public static LevvyDatumType? GetDatumType(this TokenDatum self) => self switch
    {
        LendTokenDatum => LevvyDatumType.LendTokenDatum,
        BorrowTokenDatum => LevvyDatumType.BorrowTokenDatum,
        RepayTokenDatum => LevvyDatumType.RepayTokenDatum,
        _ => null
    };

    public static string GetPolicyId(this TokenDatum self)
    {
        byte[] data = self switch
        {
            LendTokenDatum datum => datum.LendTokenDetails.PolicyId,
            BorrowTokenDatum datum => datum.BorrowTokenDetails.PolicyId,
            _ => []
        };

        return Convert.ToHexString(data).ToLowerInvariant();
    }

    public static string GetAssetName(this TokenDatum self)
    {
        byte[] data = self switch
        {
            LendTokenDatum datum => datum.LendTokenDetails.AssetName,
            BorrowTokenDatum datum => datum.BorrowTokenDetails.AssetName,
            _ => []
        };

        return Convert.ToHexString(data).ToLowerInvariant();
    }

    public static Address? GetAdaOwnerAddress(this TokenDatum self, NetworkType networkType)
    {
        (byte[]? paymentKey, byte[]? stakeKey) = self switch
        {
            LendTokenDatum datum => (datum.LendTokenDetails.AdaOwner.PaymentCredential.GetKeyHash(), datum.LendTokenDetails.AdaOwner.StakeCredential switch
            {
                Option<Inline<Credential>> opc => opc switch
                {
                    Some<Inline<Credential>> cred => cred.Value.Value.GetKeyHash(),
                    _ => null
                },
                _ => null
            }),
            BorrowTokenDatum datum => (datum.BorrowTokenDetails.AdaOwner.PaymentCredential.GetKeyHash(), datum.BorrowTokenDetails.AdaOwner.StakeCredential switch
            {
                Option<Inline<Credential>> opc => opc switch
                {
                    Some<Inline<Credential>> cred => cred.Value.Value.GetKeyHash(),
                    _ => null
                },
                _ => null
            }),
            RepayTokenDatum datum => (datum.RepayTokenDetails.AdaOwner.PaymentCredential.GetKeyHash(), datum.RepayTokenDetails.AdaOwner.StakeCredential switch
            {
                Option<Inline<Credential>> opc => opc switch
                {
                    Some<Inline<Credential>> cred => cred.Value.Value.GetKeyHash(),
                    _ => null
                },
                _ => null
            }),
            _ => (null, null)
        };

        if (paymentKey is null || stakeKey is null) return null;

        byte[] addressBody = [.. paymentKey, .. stakeKey];
        AddressHeader header = new(AddressType.BasePayment, NetworkType.Testnet);
        return new([header.ToByte(),  ..addressBody]);
    }

    public static Address? GetAssetOwnerAddress(this TokenDatum self, NetworkType networkType)
    {
        (byte[]? paymentKey, byte[]? stakeKey) = self switch
        {
            BorrowTokenDatum datum => (datum.BorrowTokenDetails.AssetOwner.PaymentCredential.GetKeyHash(), datum.BorrowTokenDetails.AssetOwner.StakeCredential switch
            {
                Option<Inline<Credential>> opc => opc switch
                {
                    Some<Inline<Credential>> cred => cred.Value.Value.GetKeyHash(),
                    _ => []
                },
                _ => []
            }),
            _ => (null, null)
        };

        if (paymentKey is null || stakeKey is null) return null;
        return Address.FromCredentials(networkType, AddressType.BasePayment, paymentKey, stakeKey);
    }

    public static ulong GetLoanAmount(this TokenDatum self)
    {
        ulong amount = self switch
        {
            LendTokenDatum datum => datum.LendTokenDetails.LoanAmount,
            BorrowTokenDatum datum => datum.BorrowTokenDetails.LoanAmount,
            RepayTokenDatum datum => datum.RepayTokenDetails.LoanAmount,
            _ => 0
        };

        return amount;
    }

    public static ulong GetLoanDuration(this TokenDatum self, ulong slot = 0, NetworkType networkType = NetworkType.Preview)
    {

        ulong amount = self switch
        {
            LendTokenDatum datum => datum.LendTokenDetails.LoanDuration,
            BorrowTokenDatum datum => (ulong)GetBorrowLoanDuration(datum, (long)slot, networkType),
            _ => 0
        };

        return amount;
    }

    private static long GetBorrowLoanDuration(BorrowTokenDatum datum, long slot, NetworkType networkType)
    {
        SlotNetworkConfig slotNetworkConfig = SlotUtil.GetSlotNetworkConfig(networkType);
        long slotTime = SlotUtil.GetPosixTimeSecondsFromSlot(slotNetworkConfig, slot) * 1_000;
        long duration = datum.GetLoanEndTimePosix() - slotTime;

        long days14millis = 1209600000;
        long days28millis = 2419200000;
        List<long> durations = [days14millis, days28millis];
        long closest = durations.OrderBy(v => Math.Abs(v - duration)).First();

        return closest;
    }

    public static ulong GetInterestAmount(this TokenDatum self)
    {
        ulong amount = self switch
        {
            LendTokenDatum datum => datum.LendTokenDetails.InterestAmount,
            BorrowTokenDatum datum => datum.BorrowTokenDetails.InterestAmount,
            RepayTokenDatum datum => datum.RepayTokenDetails.InterestAmount,
            _ => 0
        };

        return amount;
    }

    public static ulong GetTokenAmount(this TokenDatum self)
    {
        ulong amount = self switch
        {
            LendTokenDatum datum => datum.LendTokenDetails.TokenAmount,
            BorrowTokenDatum datum => datum.BorrowTokenDetails.TokenAmount,
            RepayTokenDatum datum => datum.RepayTokenDetails.TokenAmount,
            _ => 0
        };

        return amount;
    }

    public static DateTimeOffset? GetLoanEndTime(this TokenDatum self)
    {
        DateTimeOffset? endTime = self switch
        {
            BorrowTokenDatum datum => DateTimeOffset.FromUnixTimeSeconds((long)datum.BorrowTokenDetails.LoanEndTime / 1000),
            _ => null
        };

        return endTime;
    }

    public static long GetLoanEndTimePosix(this TokenDatum self)
    {
        long endTime = self switch
        {
            BorrowTokenDatum datum => (long)datum.BorrowTokenDetails.LoanEndTime,
            _ => 0
        };

        return endTime;
    }

    public static OutRef GetOutref(this TokenDatum self) => self switch
    {
        LendTokenDatum datum => new()
        {
            TxHash = Convert.ToHexStringLower(datum.LendTokenDetails.OutputReference.TransactionId),
            TxIndex = datum.LendTokenDetails.OutputReference.Index
        },
        BorrowTokenDatum datum => new()
        {
            TxHash = Convert.ToHexStringLower(datum.BorrowTokenDetails.OutputReference.TransactionId),
            TxIndex = datum.BorrowTokenDetails.OutputReference.Index
        },
        RepayTokenDatum datum => new()
        {
            TxHash = Convert.ToHexStringLower(datum.RepayTokenDetails.OutputReference.TransactionId),
            TxIndex = datum.RepayTokenDetails.OutputReference.Index
        },
        _ => new()
    };
}