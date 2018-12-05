using System.Threading.Tasks;

namespace Unitest.Sample
{
    public interface IAccountRepository
    {
        Task<Account> GetAccount(string number);
        Task<decimal> GetBalance(Account account);
        Task<int> AddToAccount(decimal amount, Account account, int transaction);
        Task<int> SubtractFromAccount(decimal amount, Account account, int transaction);
        Task CommitTransaction(int transaction);
        Task RollbackTransaction(int transaction);
    }
}