# Unitest

A simple very narrow case project to leverage [AutofacContrib.NSubstitute](https://github.com/MRCollective/AutofacContrib.NSubstitute) for creating small, easy maintainable and readable unit tests.

Examples can be found in src/Unitest.Sample.UnitTests project.

With `xUnit.net` and `IClassFixture` that provides a test fixture that contain the test setup code.

Then tests look something like this:

```
[Fact]
public async Task WhenAllConditionsAreMet_ShouldPerformDeposit()
{
    Given.ValidAccountExistsInDatabase(FromAccount);
    And.ValidAccountExistsInDatabase(ToAccount);
    And.UserIsAuthorisedToWithdraw(FromAccount, User);
    And.AccountHasEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

    await When.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);

    await Then.DidDepositToAccount(ToAccount, WithdrawalAmount);
}

[Fact]
public async Task SuccessfulWithdrawal_ShouldReturnTrue()
{
    Given.ValidAccountExistsInDatabase(FromAccount);
    And.ValidAccountExistsInDatabase(ToAccount);
    And.UserIsAuthorisedToWithdraw(FromAccount, User);
    And.AccountHasEnoughFundsForWithdrawal(FromAccount, WithdrawalAmount);

    // When
    var transactionResult = await SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);

    // Then
    transactionResult.Should().BeTrue();
}
```

And the Fixture leveraging `AutofacContrib.NSubstitute` looks like:

```
public class AccountManagerFixture : Fixture
{
    public AccountManagerFixture ValidAccountExistsInDatabase(string accountNumber)
    {
        SubstituteFor<IAccountRepository>()
            .GetAccount(Arg.Is(accountNumber))
            .Returns(new Account
            {
                Number = accountNumber
            });
        return this;
    }

    public AccountManagerFixture AccountDoesNotExist(string accountNumber)
    {
        SubstituteFor<IAccountRepository>()
            .GetAccount(Arg.Is(accountNumber))
            .Returns((Account)null);
        return this;
    }

    public async Task<AccountManagerFixture> DidDepositToAccount(string account, decimal amount)
    {
        await SubstituteFor<IAccountRepository>()
            .Received()
            .AddToAccount(amount, Arg.Is<Account>(x => x.Number == account), Arg.Any<int>());
        return this;
    }

    ...
}
```