using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Models;

namespace ThuInfoWeb.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data _data;
        private readonly UserManager _userManager;
        private readonly VersionManager _versionManager;

        [GeneratedRegex(@"^\d+\.\d+\.\d+$")]
        private static partial Regex VersionRegex();

        public HomeController(ILogger<HomeController> logger, Data data, UserManager userManager, VersionManager versionManager)
        {
            _logger = logger;
            _data = data;
            _userManager = userManager;
            _versionManager = versionManager;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Prohibit registration
            ModelState.AddModelError(nameof(vm.Name), "禁止注册新用户");
            return View(vm);

            // if (await _data.CheckUserAsync(vm.Name))
            // {
            //     ModelState.AddModelError(nameof(vm.Name), "用户名已被注册");
            //     return View(vm);
            // }
            // var user = new User()
            // {
            //     Name = vm.Name,
            //     PasswordHash = vm.Password.ToSHA256Hex(),
            //     CreatedTime = DateTime.Now,
            //     IsAdmin = false
            // };
            // var result = await _data.CreateUserAsync(user);
            // if (result == 1)
            // {
            //     await _userManager.DoLoginAsync(vm.Name, false);
            //     return RedirectToAction("Index");
            // }
            // else
            // {
            //     ModelState.AddModelError(nameof(vm.Name), "发生未知错误");
            //     return View(vm);
            // }
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            // get the user and check if the password is correct
            var user = await _data.GetUserAsync(vm.Name);
            if (user is null || vm.Password.ToSHA256Hex() != user.PasswordHash)
            {
                ModelState.AddModelError(nameof(vm.Name), "用户名或密码错误");
                ModelState.AddModelError(nameof(vm.Password), "用户名或密码错误");
                return View(vm);
            }
            await _userManager.DoLoginAsync(user.Name, user.IsAdmin);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin,guest")]
        public async Task<IActionResult> Logout()
        {
            await _userManager.DoLogoutAsync();
            return RedirectToAction("Login");
        }
        [Authorize(Roles = "admin,guest")]
        public IActionResult ChangePassword() => View();
        [HttpPost, Authorize(Roles = "admin,guest")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            if (HttpContext.User.Identity.Name != vm.Name) BadRequest();
            if (vm.OldPassword.ToSHA256Hex() != (await _data.GetUserAsync(HttpContext.User.Identity.Name)).PasswordHash)
            {
                ModelState.AddModelError(nameof(vm.OldPassword), "旧密码错误");
                return View(vm);
            }
            var result = await _data.ChangeUserPasswordAsync(HttpContext.User.Identity.Name, vm.NewPassword.ToSHA256Hex());
            if (result != 1)
            {
                ModelState.AddModelError(nameof(vm.NewPassword), "发生未知错误");
                return View(vm);
            }
            else
            {
                await _userManager.DoLogoutAsync();
                return RedirectToAction(nameof(Login));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = "admin,guest")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Announce([FromQuery] int page = 1)
        {
            ViewData["page"] = page;
            var list = await _data.GetAnnouncesAsync(page, 10);
            return View(list.Select(a => new AnnounceViewModel()
            {
                Id = a.Id,
                Content = a.Content,
                Title = a.Title,
                Author = a.Author,
                CreatedTime = a.CreatedTime,
                IsActive = a.IsActive,
                VisibleNotAfter = a.VisibleNotAfter,
                VisibleExact = a.VisibleExact
            }).ToList());
        }

        [HttpPost, Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateAnnounce(AnnounceViewModel vm)
        {
            if (vm.Title is null || vm.Content is null) return BadRequest("标题或内容为空");
            var visibleNotAfter = vm.VisibleNotAfter?.Trim() ?? "9.9.9";
            var visibleExact = vm.VisibleExact ?? "";
            
            if (!VersionRegex().IsMatch(visibleNotAfter))
            {
                return BadRequest("\"在不晚于以下版本生效\"中的版本号格式错误");
            }
            
            var visibleExactList = visibleExact.Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            
            if (visibleExactList.Any(x => !VersionRegex().IsMatch(x)))
            {
                return BadRequest("\"在以下版本生效\"中的版本号格式错误");
            }
            
            visibleExact = string.Join(',', visibleExactList);

            var user = HttpContext.User.Identity!.Name!;
            var a = new Announce
            {
                Title = vm.Title,
                Content = vm.Content,
                Author = user,
                CreatedTime = DateTime.Now,
                IsActive = vm.IsActive,
                VisibleNotAfter = visibleNotAfter,
                VisibleExact = visibleExact
            };
            var result = await _data.CreateAnnounceAsync(a);
            if (result != 1) return BadRequest(ModelState);
            else return CreatedAtAction(nameof(Announce), null);
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeAnnounceStatus([FromRoute] int id, [FromQuery] int returnpage)
        {
            var a = await _data.GetAnnounceAsync(id);
            if (a is null) return BadRequest("找不到对应公告");
            var result = await _data.UpdateAnnounceStatusAsync(id, !a.IsActive);
            if (result != 1) return BadRequest();
            else return RedirectToAction(nameof(Announce), new { page = returnpage == 0 ? 1 : returnpage });
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAnnounce([FromRoute] int id, [FromQuery] int returnpage)
        {
            var result = await _data.DeleteAnnounceAsync(id);
            if (result != 1) return NoContent();
            else return RedirectToAction(nameof(Announce), new { page = returnpage });
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Feedback([FromQuery] int page = 1)
        {
            var list = (await _data.GetFeedbacksAsync(page, 10)).Select(x =>
                new FeedbackViewModel()
                {
                    AppVersion = x.AppVersion,
                    Contact = x.Contact,
                    Content = x.Content,
                    CreatedTime = x.CreatedTime,
                    Id = x.Id,
                    OS = x.OS,
                    PhoneModel = x.PhoneModel,
                    Reply = x.Reply,
                    ReplyerName = x.ReplierName,
                    RepliedTime = x.RepliedTime
                }).ToList();
            ViewData["page"] = page;
            return View(list);
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteFeedback([FromRoute] int id, [FromQuery] int returnpage = 1)
        {
            var result = await _data.DeleteFeedbackAsync(id);
            if (result != 1) return NoContent();
            else return RedirectToAction(nameof(Feedback), new { page = returnpage });
        }
        [HttpPost, Authorize(Roles = "admin")]
        public async Task<IActionResult> ReplyFeedback([FromForm] int id, [FromForm] string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                return BadRequest("回复不能为空");
            }
            var user = HttpContext.User.Identity.Name;
            var result = await _data.ReplyFeedbackAsync(id, reply, user);
            if (result != 1) return BadRequest("未知错误");
            else return Ok();
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Misc()
        {
            var misc = await _data.GetMiscAsync();
            return View(new MiscViewModel()
            {
                ApkUrl = misc.ApkUrl,
                QrCodeContent = misc.QrCodeContent,
                CardIVersion = misc.CardIVersion
            });
        }
        [HttpPost, Authorize(Roles = "admin")]
        public async Task<IActionResult> Misc(MiscViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var misc = new Misc()
            {
                ApkUrl = vm.ApkUrl,
                QrCodeContent = vm.QrCodeContent,
                CardIVersion = vm.CardIVersion
            };
            var result = await _data.UpdateMiscAsync(misc);
            if (result != 1) return BadRequest();
            else return RedirectToAction(nameof(Misc));
        }
        [Authorize(Roles = "admin"), Route("Home/CheckUpdate/{os}")]
        public IActionResult CheckUpdate([FromRoute] string os)
        {
            if (!_versionManager.IsRunning)
                _ = _versionManager.CheckUpdateAsync(os.ToLower() == "android" ? VersionManager.OS.Android : VersionManager.OS.IOS);
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "admin")]
        public IActionResult Stat()
        {
            return View();
        }

#if DEBUG
        [Route("Home/Exception")]
        public IActionResult Exception()
        {
            throw new Exception("Generated exception in DEBUG build");
        }
#endif
    }
}