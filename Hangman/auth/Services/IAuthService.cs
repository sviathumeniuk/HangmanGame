using Auth.Models;
using System.Threading.Tasks;

namespace Auth.Services
{
    public interface IAuthService
    {
        Task<object> RegisterAsync(RegisterModel model);
        Task<object> LoginAsync(LoginModel model);
        Task<object> GetUserProfileAsync(int userId);
    }
}