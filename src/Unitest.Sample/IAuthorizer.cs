using System.Threading.Tasks;

namespace HockyTest.Sample
{
    public interface IAuthorizer
    {
        Task<bool> IsAuthorizedToPerformTransfer(Account account, User user);
    }
}