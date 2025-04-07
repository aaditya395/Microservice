using UserService.DatabaseContext;
using UserService.DTO;
using UserService.Models;

namespace UserService.Repository
{
    public class RegisterRepository : IRegisterRepository
    {
        private readonly UserContext _context;

        public RegisterRepository(UserContext context)
        {
            _context = context;
        }
        public User AddUser(UserDTO user)
        {
            var newUser = new User()
            {
                Name = user.Name,
                Username = user.Username,
                Password = user.Password,
                Role = user.Role,
            };
            _context.user.Add(newUser);
            _context.SaveChanges();
            return newUser;
        }
    }
}
