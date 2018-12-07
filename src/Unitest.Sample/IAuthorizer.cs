using System.Threading.Tasks;

namespace Unitest.Sample
{
    public interface IAuthorizer
    {
        Task<bool> IsAuthorizedToPerformTransfer(Account account, User user);
    }
}