using Microsoft.AspNetCore.Mvc;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Models;

namespace ThuInfoWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Data _data;

        public UserController(Data data)
        {
            this._data = data;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(UserViewModel viewModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = new User() { Name = viewModel.Name, Password = viewModel.Password };
            var result = await _data.CreateUserAsync(user);
            if (result != 1) return BadRequest(new { msg = "user allready exists" });
            else return Ok(viewModel);
        }
    }
}
