using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ThuInfoWeb.Models;

namespace ThuInfoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data _data;

        public HomeController(ILogger<HomeController> logger, Data data)
        {
            _logger = logger;
            this._data = data;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "admin")]
        public IActionResult Admin()
        {
            return Ok("you are admin");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _data.GetUserAsync(vm.Name);
            if (user is null) return BadRequest();
            if (vm.Password != user.Password) return BadRequest();
            await Account.LoginAsync(HttpContext, vm.Name, user.IsAdmin);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Logout()
        {
            await Account.LogoutAsync(HttpContext);
            return RedirectToAction("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}