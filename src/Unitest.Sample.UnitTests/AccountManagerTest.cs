using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutofacContrib.NSubstitute;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using Xunit;

namespace Unitest.Sample.UnitTests
{
    public class AccountManagerTest
    {
        private static readonly string FromAccount = "12345";
        private static readonly string ToAccount = "54321";
        private static readonly decimal DepositAmount = 1000;
        private static readonly decimal WithdrawalAmount = 1000;
        private static readonly User User = new User
        {
            AccountNumbers = new List<string> { FromAccount },
            Name = "Mr. Haacker"
        };

        public class TransferMoneyTests : TestWith<AccountManagerFixture, AccountManager>, IClassFixture<AccountManagerFixture>
        {
            public TransferMoneyTests(AccountManagerFixture fixture)
                : base(fixture)
            {
            }

            [Theory]
            [InlineData(-1000)]
            [InlineData(-1)]
            [InlineData(0)]
            public void WhenAmountIsLessThenMinimumAllowed_ShouldThrowArgumentException(decimal amount)
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);

                Func<Task> whenITransferMoney = async () =>
                {
                    await SUT.TransferMoney(amount, FromAccount, ToAccount, User);
                };

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because trying to transfer with minimum amount less than 1$")
                    .WithMessage($"Minimum amount for transfer is 1$");
            }

            [Fact]
            public void WhenTransferringFromAndToAreTheSameAccount_ThenShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);

                Func<Task> whenITransferMoney = async () =>
                {
                    await SUT.TransferMoney(WithdrawalAmount, FromAccount, FromAccount, User);
                };

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because cannot transfer money to same account")
                    .WithMessage($"Cannot transfer to same account number '{FromAccount}'");
            }

            [Fact]
            public void WhenFromAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.AccountDoesNotExist(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);

                Func<Task> whenITransferMoney = async () =>
                {
                    await SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);
                };

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because account doesn't exist")
                    .WithMessage($"Account '{FromAccount}' not found");
            }

            [Fact]
            public void WhenToAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.AccountDoesNotExist(ToAccount);

                Func<Task> whenITransferMoney = async () =>
                {
                    await SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);
                };

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because account doesn't exist")
                    .WithMessage($"Account '{ToAccount}' not found");
            }

            [Fact]
            public void WhenUserDoesNotHaveAccessToAccount_ShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);
                And.UserIsNotAuthorisedToWithdraw(FromAccount, User);

                Func<Task> whenITransferMoney = async () =>
                {
                    await SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);
                };

                whenITransferMoney
                    .Then()
                    .Throw<NotAuthorizedException>("because user have not being authorized to withdraw")
                    .Where(x => x.AccountNumber == FromAccount && x.UserName == User.Name);
            }

            [Fact]
            public void WhenAccountDoesNotHaveEnoughFunds_ShouldInvalidOperationException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountDoesNotHaveEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                Func<Task> whenITransferMoney = async () =>
                {
                    await SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);
                };

                whenITransferMoney
                    .Then()
                    .Throw<InvalidOperationException>("because account's balance is less then the withdrawal amount")
                    .WithMessage($"Not enough money on account '{FromAccount}'");
            }

            [Fact]
            public async Task WhenAllConditionsAreMet_ShouldPerformWithdrawal()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountHasEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                await When.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);

                await Then.DidWithdrawalForAccount(FromAccount, WithdrawalAmount);
            }

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
            public async Task AfterSuccessfulWithdrawal_ShouldCommitDatabbaseTransaction()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountHasEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                await When.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);

                await Then.TransactionWasCommited();
            }

            [Fact]
            public async Task SuccessfulWithdrawal_ShouldReturnTrue()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountHasEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                // When
                var transactionResult = await SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User);

                // Then
                transactionResult.Should().BeTrue();
            }
        }

        public class WithdrawTests : TestWith<AccountManagerFixture, AccountManager>, IClassFixture<AccountManagerFixture>
        {
            public WithdrawTests(AccountManagerFixture fixture)
                : base(fixture)
            {
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            [InlineData(-1000)]
            public void WhenAmountIsLessThenMinimumAllowed_ShouldThrowArgumentException(int amount)
            {
                Given.ValidAccountExistsInDatabase(FromAccount);

                Func<Task> whenIWithdraw = async () =>
                {
                    await SUT.Withdraw(amount, FromAccount, User);
                };

                whenIWithdraw
                    .Then()
                    .Throw<ArgumentException>("because trying to withdraw with minimum amount less than 1$")
                    .WithMessage($"Minimum amount for withdrawal is 1$");
            }

            [Fact]
            public void WhenAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.AccountDoesNotExist(FromAccount);

                Func<Task> whenIWithdraw = async () =>
                {
                    await SUT.Withdraw(WithdrawalAmount, FromAccount, User);
                };

                whenIWithdraw
                    .Then()
                    .Throw<ArgumentException>("because account doesn't exist")
                    .WithMessage($"Account '{FromAccount}' not found");
            }

            [Fact]
            public void WhenUserDoesNotHaveAccessToAccount_ShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.UserIsNotAuthorisedToWithdraw(FromAccount, User);

                Func<Task> whenIWithdraw = async () =>
                {
                    await SUT.Withdraw(10, FromAccount, User);
                };

                whenIWithdraw
                    .Then()
                    .Throw<NotAuthorizedException>("because user have not being authorized to withdraw")
                    .Where(x => x.AccountNumber == FromAccount && x.UserName == User.Name);
            }

            [Fact]
            public void WhenAccountDoesNotHaveEnoughFunds_ShouldInvalidOperationException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountDoesNotHaveEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                Func<Task> whenIWithdraw = async () =>
                {
                    await SUT.Withdraw(WithdrawalAmount, FromAccount, User);
                };

                whenIWithdraw
                    .Then()
                    .Throw<InvalidOperationException>("because account's balance is less then the withdrawal amount")
                    .WithMessage($"Not enough money on account '{FromAccount}'");
            }

            [Fact]
            public async Task WhenAllConditionsAreMet_ShouldPerformWithdrawal()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountHasEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                await When.Withdraw(WithdrawalAmount, FromAccount, User);

                await Then.DidWithdrawalForAccount(FromAccount, WithdrawalAmount);
            }

            [Fact]
            public async Task AfterSuccessfulWithdrawal_ShouldCommitDatabbaseTransaction()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountHasEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                await When.Withdraw(WithdrawalAmount, FromAccount, User);

                await Then.TransactionWasCommited();
                // or
                // await Mock<IAccountRepository>().Received().CommitTransaction(Arg.Any<int>());
            }

            [Fact]
            public async Task SuccessfulWithdrawal_ShouldReturnTrue()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountHasEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                // When
                var transactionResult = await SUT.Withdraw(WithdrawalAmount, FromAccount, User);

                // Then
                transactionResult.Should().BeTrue();
            }
        }

        public class DepositTests : TestWith<AccountManagerFixture, AccountManager>, IClassFixture<AccountManagerFixture>
        {
            public DepositTests(AccountManagerFixture fixture)
                : base(fixture)
            {
            }

            [Fact]
            public void WhenAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.AccountDoesNotExist(ToAccount);

                Func<Task> whenIDeposit = async () => 
                {
                    await SUT.Deposit(DepositAmount, ToAccount);
                };

                whenIDeposit
                    .Then()
                    .Throw<ArgumentException>("because account doesn't exist")
                    .WithMessage($"Account '{ToAccount}' not found");
            }

            [Fact]
            public async Task WhenAllConditionsAreMet_ShouldPerformDeposit()
            {
                Given.ValidAccountExistsInDatabase(ToAccount);

                await When.Deposit(DepositAmount, ToAccount);

                await Then.DidDepositToAccount(ToAccount, DepositAmount);
            }

            [Fact]
            public async Task AfterSuccessfulDeposit_ShouldCommitDatabbaseTransaction()
            {
                Given.ValidAccountExistsInDatabase(ToAccount);

                await When.Deposit(DepositAmount, ToAccount);

                await Then.TransactionWasCommited();
            }

            [Fact]
            public async Task SuccessfulDeposit_ShouldReturnTrue()
            {
                Given.ValidAccountExistsInDatabase(ToAccount);

                // When
                var transactionResult = await SUT.Deposit(DepositAmount, ToAccount);

                // Then
                transactionResult.Should().BeTrue();
            }
        }
    }
}
