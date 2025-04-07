using UserService.DTO;
using UserService.Models;

namespace UserService.Repository
{
    public interface IRegisterRepository
    {
        public User AddUser(UserDTO user);
    }
}
