using Chrysalis.Cbor.Types.Cardano.Core.Transaction;
using Chrysalis.Cbor.Types.Plutus.Address;
using Credential = Chrysalis.Cbor.Types.Plutus.Address.Credential;

namespace ArgusProject.Extensions;

public static class CredentialExtension
{
    public static byte[] GetKeyHash(this Credential self) => self switch
    {
        VerificationKey vkey => vkey.VerificationKeyHash,
        Script script => script.ScriptHash,
        _ => throw new Exception("Invalid credential")
    };
}