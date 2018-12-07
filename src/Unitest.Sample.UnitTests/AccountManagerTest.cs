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

        public class TransferMoneyTests : TestWith<AccountManagerFixture, AccountManager>
        {
            public TransferMoneyTests(AccountManagerFixture fixture)
                : base(fixture)
            {
            }

            [Theory]
            [InlineData(-1000)]
            [InlineData(-1)]
            [InlineData(0)]
            public async Task WhenAmountIsLessThenMinimumAllowed_ShouldThrowArgumentException(decimal amount)
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);

                var whenITransferMoney = new Action(() =>
                {
                    SUT.TransferMoney(amount, FromAccount, ToAccount, User).GetAwaiter().GetResult();
                });

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because trying to transfer with minimum amount less than 1$")
                    .WithMessage($"Minimum amount for transfer is 1$");
            }

            [Fact]
            public async Task WhenTransferringFromAndToAreTheSameAccount_ThenShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);

                var whenITransferMoney = new Action(() =>
                {
                    SUT.TransferMoney(WithdrawalAmount, FromAccount, FromAccount, User).GetAwaiter().GetResult();
                });

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because cannot transfer money to same account")
                    .WithMessage($"Cannot transfer to same account number '{FromAccount}'");
            }

            [Fact]
            public async Task WhenFromAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.AccountDoesNotExist(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);

                var whenITransferMoney = new Action(() =>
                {
                    SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User).GetAwaiter().GetResult();
                });

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because account doesn't exist")
                    .WithMessage($"Account '{FromAccount}' not found");
            }

            [Fact]
            public async Task WhenToAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.AccountDoesNotExist(ToAccount);

                var whenITransferMoney = new Action(() =>
                {
                    SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User).GetAwaiter().GetResult();
                });

                whenITransferMoney
                    .Then()
                    .Throw<ArgumentException>("because account doesn't exist")
                    .WithMessage($"Account '{ToAccount}' not found");
            }

            [Fact]
            public async Task WhenUserDoesNotHaveAccessToAccount_ShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);
                And.UserIsNotAuthorisedToWithdraw(FromAccount, User);

                var whenITransferMoney = new Action(() =>
                {
                    SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User).GetAwaiter().GetResult();
                });

                whenITransferMoney
                    .Then()
                    .Throw<NotAuthorizedException>("because user have not being authorized to withdraw")
                    .Where(x => x.AccountNumber == FromAccount && x.UserName == User.Name);
            }

            [Fact]
            public async Task WhenAccountDoesNotHaveEnoughFunds_ShouldInvalidOperationException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.ValidAccountExistsInDatabase(ToAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountDoesNotHaveEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                var whenITransferMoney = new Action(() =>
                {
                    SUT.TransferMoney(WithdrawalAmount, FromAccount, ToAccount, User).GetAwaiter().GetResult();
                });

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

        public class WithdrawTests : TestWith<AccountManagerFixture, AccountManager>
        {
            public WithdrawTests(AccountManagerFixture fixture)
                : base(fixture)
            {
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            [InlineData(-1000)]
            public async Task WhenAmountIsLessThenMinimumAllowed_ShouldThrowArgumentException(int amount)
            {
                Given.ValidAccountExistsInDatabase(FromAccount);

                var whenIWithdraw = new Action(() =>
                {
                    SUT.Withdraw(amount, FromAccount, User).GetAwaiter().GetResult();
                });

                whenIWithdraw
                    .Then()
                    .Throw<ArgumentException>("because trying to withdraw with minimum amount less than 1$")
                    .WithMessage($"Minimum amount for withdrawal is 1$");
            }

            [Fact]
            public async Task WhenAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.AccountDoesNotExist(FromAccount);

                var whenIWithdraw = new Action(() =>
                {
                    SUT.Withdraw(WithdrawalAmount, FromAccount, User).GetAwaiter().GetResult();
                });

                whenIWithdraw
                    .Then()
                    .Throw<ArgumentException>("because account doesn't exist")
                    .WithMessage($"Account '{FromAccount}' not found");
            }

            [Fact]
            public async Task WhenUserDoesNotHaveAccessToAccount_ShouldThrowArgumentException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.UserIsNotAuthorisedToWithdraw(FromAccount, User);

                var whenIWithdraw = new Action(() =>
                {
                    SUT.Withdraw(10, FromAccount, User).GetAwaiter().GetResult();
                });

                whenIWithdraw
                    .Then()
                    .Throw<NotAuthorizedException>("because user have not being authorized to withdraw")
                    .Where(x => x.AccountNumber == FromAccount && x.UserName == User.Name);
            }

            [Fact]
            public async Task WhenAccountDoesNotHaveEnoughFunds_ShouldInvalidOperationException()
            {
                Given.ValidAccountExistsInDatabase(FromAccount);
                And.UserIsAuthorisedToWithdraw(FromAccount, User);
                And.AccountDoesNotHaveEnoughFundsForWithdraw(FromAccount, WithdrawalAmount);

                var whenIWithdraw = new Action(() =>
                {
                    SUT.Withdraw(WithdrawalAmount, FromAccount, User).GetAwaiter().GetResult();
                });

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

        public class DepositTests : TestWith<AccountManagerFixture, AccountManager>
        {
            public DepositTests(AccountManagerFixture fixture)
                : base(fixture)
            {
            }

            [Fact]
            public async Task WhenAccountDoesNotExist_ShouldThrowArgumentException()
            {
                Given.AccountDoesNotExist(ToAccount);

                var whenIDeposit = new Action(() =>
                {
                    SUT.Deposit(DepositAmount, ToAccount).GetAwaiter().GetResult();
                });

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
