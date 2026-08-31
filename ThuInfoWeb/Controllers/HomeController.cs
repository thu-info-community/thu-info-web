using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Models;

namespace ThuInfoWeb.Controllers;

[AutoValidateAntiforgeryToken]
public class HomeController(
    ILogger<HomeController> logger,
    Data data,
    UserManager userManager,
    VersionManager versionManager,
    LoginAttemptService loginAttemptService,
    IPasswordHasher<User> passwordHasher,
    TimeProvider timeProvider)
    : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly TimeProvider _timeProvider = timeProvider;

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // Prohibit registration
        ModelState.AddModelError(nameof(vm.Name), "禁止注册新用户");
        return View(vm);
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid || vm.Name is null || vm.Password is null)
            return View(vm);
        if (loginAttemptService.IsBlocked(vm.Name))
        {
            ModelState.AddModelError(nameof(vm.Name), "Your account is temporarily locked due to multiple failed login attempts.");
            return View(vm);
        }
        // get the user and check if the password is correct
        var user = vm.Name != null ? await data.GetUserAsync(vm.Name) : null;
        if (user is null || !VerifyPassword(user, vm.Password!, out var shouldRehash))
        {
            ModelState.AddModelError(nameof(vm.Name), "用户名或密码错误");
            ModelState.AddModelError(nameof(vm.Password), "用户名或密码错误");
            loginAttemptService.RecordAttempt(vm.Name!);
            return View(vm);
        }

        if (shouldRehash)
        {
            var rehashResult = await data.ChangeUserPasswordAsync(
                user.Name,
                _passwordHasher.HashPassword(user, vm.Password!));
            if (rehashResult != 1)
                ApplicationLog.PasswordHashMigrationFailed(_logger, user.Name);
        }

        loginAttemptService.ClearAttempts(user.Name);
        await userManager.DoLoginAsync(user.Name, user.IsAdmin);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "admin,guest")]
    public async Task<IActionResult> Logout()
    {
        await userManager.DoLogoutAsync();
        return RedirectToAction("Login");
    }

    [Authorize(Roles = "admin,guest")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "admin,guest")]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);
        if (HttpContext.User.Identity!.Name != vm.Name)
            return BadRequest();

        var user = await data.GetUserAsync(HttpContext.User.Identity.Name!);
        if (user is null || vm.OldPassword is null || !VerifyPassword(user, vm.OldPassword, out _))
        {
            ModelState.AddModelError(nameof(vm.OldPassword), "旧密码错误");
            return View(vm);
        }

        var result = await data.ChangeUserPasswordAsync(user.Name, _passwordHasher.HashPassword(user, vm.NewPassword!));
        if (result != 1)
        {
            ModelState.AddModelError(nameof(vm.NewPassword), "发生未知错误");
            return View(vm);
        }

        await userManager.DoLogoutAsync();
        return RedirectToAction(nameof(Login));
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
        var list = await data.GetAnnouncesAsync(page, 10);
        return View(list.Select(a => new AnnounceViewModel
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

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateAnnounce(AnnounceViewModel vm)
    {
        if (vm.Title is null || vm.Content is null)
            return BadRequest("标题或内容为空");
        var visibleNotAfter = vm.VisibleNotAfter?.Trim() ?? "9.9.9";
        var visibleExact = vm.VisibleExact ?? "";

        if (!visibleNotAfter.IsValidVersionNumber())
            return BadRequest("\"在不晚于以下版本生效\"中的版本号格式错误");

        var visibleExactList = visibleExact.Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (visibleExactList.Any(x => !x.IsValidVersionNumber()))
            return BadRequest("\"在以下版本生效\"中的版本号格式错误");

        visibleExact = string.Join(',', visibleExactList);

        var user = HttpContext.User.Identity!.Name!;
        var a = new Announce
        {
            Title = vm.Title,
            Content = vm.Content,
            Author = user,
            CreatedTime = _timeProvider.GetLocalNow().DateTime,
            IsActive = vm.IsActive,
            VisibleNotAfter = visibleNotAfter,
            VisibleExact = visibleExact
        };
        var result = await data.CreateAnnounceAsync(a);
        if (result != 1)
            return BadRequest(ModelState);
        return CreatedAtAction(nameof(Announce), null);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ChangeAnnounceStatus([FromRoute] int id, [FromQuery] int returnpage)
    {
        var a = await data.GetAnnounceAsync(id);
        if (a is null)
            return BadRequest("找不到对应公告");
        var result = await data.UpdateAnnounceStatusAsync(id, !a.IsActive);
        if (result != 1)
            return BadRequest();
        return RedirectToAction(nameof(Announce), new { page = returnpage == 0 ? 1 : returnpage });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteAnnounce([FromRoute] int id, [FromQuery] int returnpage)
    {
        var result = await data.DeleteAnnounceAsync(id);
        if (result != 1)
            return NoContent();
        return RedirectToAction(nameof(Announce), new { page = returnpage });
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Feedback([FromQuery] int page = 1)
    {
        var list = (await data.GetFeedbacksAsync(page, 10)).Select(x =>
            new FeedbackViewModel
            {
                AppVersion = x.AppVersion,
                Contact = x.Contact,
                Content = x.Content,
                CreatedTime = x.CreatedTime,
                Id = x.Id,
                OS = x.OS,
                PhoneModel = x.PhoneModel,
                Reply = x.Reply,
                ReplierName = x.ReplierName,
                RepliedTime = x.RepliedTime
            }).ToList();
        ViewData["page"] = page;
        return View(list);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteFeedback([FromRoute] int id, [FromQuery] int returnpage = 1)
    {
        var result = await data.DeleteFeedbackAsync(id);
        if (result != 1)
            return NoContent();
        return RedirectToAction(nameof(Feedback), new { page = returnpage });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ReplyFeedback([FromForm] int id, [FromForm] string reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
            return BadRequest("回复不能为空");
        var user = HttpContext.User.Identity!.Name!;
        var result = await data.ReplyFeedbackAsync(id, reply, user);
        if (result != 1)
            return BadRequest("未知错误");
        return Ok();
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Misc()
    {
        var misc = await data.GetMiscAsync() ?? new Misc();
        return View(new MiscViewModel
        {
            QrCodeContent = misc.QrCodeContent,
            CardIVersion = misc.CardIVersion,
            SchoolCalendarYear = misc.SchoolCalendarYear
        });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Misc(MiscViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);
        var existingMisc = await data.GetMiscAsync() ?? new Misc();
        var misc = new Misc
        {
            // Keep the legacy value until its database column can be removed in a dedicated migration.
            ApkUrl = existingMisc.ApkUrl,
            QrCodeContent = vm.QrCodeContent ?? "",
            CardIVersion = vm.CardIVersion,
            SchoolCalendarYear = vm.SchoolCalendarYear
        };
        var result = await data.UpdateMiscAsync(misc);
        if (result != 1)
            return BadRequest();
        return RedirectToAction(nameof(Misc));
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> JieliWashers()
    {
        var list = await data.GetJieliWashersAsync();
        return View(list.Select(x => new JieliWasherViewModel
        {
            Id = x.Id,
            Building = x.Building,
            Name = x.Name,
            CreatedTime = x.CreatedTime
        }).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateJieliWasher(JieliWasherViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var washer = new JieliWasher
        {
            Id = vm.Id!.Trim(),
            Building = vm.Building!.Trim(),
            Name = vm.Name!.Trim(),
            CreatedTime = _timeProvider.GetLocalNow().DateTime
        };

        try
        {
            var result = await data.CreateJieliWasherAsync(washer);
            if (result != 1)
                return BadRequest("创建失败");
        }
        catch (Exception ex)
        {
            return BadRequest("创建失败，可能是ID已存在：" + ex.Message);
        }

        return RedirectToAction(nameof(JieliWashers));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateJieliWasher(JieliWasherViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await data.UpdateJieliWasherAsync(vm.Id!.Trim(), vm.Building!.Trim(), vm.Name!.Trim());
        if (result != 1)
            return BadRequest("更新失败");
        return RedirectToAction(nameof(JieliWashers));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteJieliWasher([FromForm] string Id)
    {
        if (string.IsNullOrWhiteSpace(Id))
            return BadRequest("id不能为空");
        var result = await data.DeleteJieliWasherAsync(Id.Trim());
        if (result != 1)
            return BadRequest("删除失败");
        return RedirectToAction(nameof(JieliWashers));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [Route("Home/CheckUpdate/{os}")]
    public async Task<IActionResult> CheckUpdate([FromRoute] string os)
    {
        var target = string.Equals(os, "android", StringComparison.OrdinalIgnoreCase)
            ? VersionManager.OS.Android
            : string.Equals(os, "ios", StringComparison.OrdinalIgnoreCase)
                ? VersionManager.OS.IOS
                : (VersionManager.OS?)null;
        if (target is null)
            return BadRequest("Unsupported operating system.");

        await versionManager.CheckUpdateAsync(target.Value, HttpContext.RequestAborted);
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
        throw new InvalidOperationException("Generated exception in DEBUG build");
    }
#endif

    private bool VerifyPassword(User user, string password, out bool shouldRehash)
    {
        shouldRehash = false;
        if (IsLegacyHash(user.PasswordHash))
        {
            var expected = Encoding.UTF8.GetBytes(user.PasswordHash);
            var actual = Encoding.UTF8.GetBytes(password.ToSHA256Hex());
            if (expected.Length != actual.Length || !CryptographicOperations.FixedTimeEquals(actual, expected))
                return false;

            shouldRehash = true;
            return true;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        shouldRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
        return result != PasswordVerificationResult.Failed;
    }

    private static bool IsLegacyHash(string hash)
    {
        return hash.Length == 64 && hash.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');
    }
}
