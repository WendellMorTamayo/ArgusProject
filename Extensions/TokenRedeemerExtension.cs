using ArgusProject.Models.Cbor.Redeemers;
using ArgusProject.Models.Enums;

namespace ArgusProject.Extensions;

public static class LendTokenRedeemerExtension
{
    public static TokenAction GetActionType(this TokenRedeemer? self) => self switch
    {
        BorrowTokenAction => TokenAction.BorrowTokenAction,
        RepayTokenAction => TokenAction.RepayTokenAction,
        ClaimTokenAction => TokenAction.ClaimTokenAction,
        ForecloseTokenAction => TokenAction.ForecloseTokenAction,
        CancelTokenAction => TokenAction.CancelTokenAction,
        _ => throw new Exception("Invalid redeemer action")
    };
}