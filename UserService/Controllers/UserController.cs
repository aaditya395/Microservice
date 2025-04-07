using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.DTO;
using UserService.Repository;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IRegisterRepository _userRepository;

        public UserController(IRegisterRepository userRepository)
        {

            _userRepository = userRepository;

        }

        [HttpPost]
        public IActionResult RegisterUser(UserDTO user)
        {
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            var newUser = _userRepository.AddUser(user);


            return Ok(newUser);
        }
    }
}
