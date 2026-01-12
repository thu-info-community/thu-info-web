using Microsoft.AspNetCore.Mvc;
using ThuInfoWeb.Bots;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Dtos;

namespace ThuInfoWeb.Controllers;

/// <summary>
///     The controller for RESTApi
/// </summary>
[Route("[controller]")]
[ApiController]
public class ApiController(Data data, VersionManager versionManager, FeedbackNoticeBot feedbackNoticeBot)
    : ControllerBase
{
    /// <summary>
    ///     Get announce, get the latest announce simply by no query string(just get /api/announce). If needed, you should only
    ///     enter id or page at one time.
    /// </summary>
    /// <param name="id">if entered, this will return single value</param>
    /// <param name="page">if entered, this will return up to 5 values in an array.</param>
    /// <param name="version">if entered, this will filter the announcement by version, only return those visible to this version.</param>
    /// <returns>In json format.</returns>
    [Route("Announce")]
    public async Task<IActionResult> Announce([FromQuery] int? id, [FromQuery] int? page, [FromQuery] string? version)
    {
        if (page <= 0)
            return BadRequest("page必须是正整数");

        version ??= "0.0.0";

        if (!version.IsValidVersionNumber())
            return BadRequest("version必须是有效的版本号格式，格式为x.x.x");

        if (page is not null)
        {
            var a = await data.GetActiveAnnouncesAsync((int)page, 5, version);
            return Ok(a);
        }
        else
        {
            var a = await data.GetActiveAnnounceAsync(id);
            return Ok(a);
        }
    }

    /// <summary>
    ///     Create a feedback
    /// </summary>
    /// <param name="dto">a json, has content, appversion, os, nickname(optional)</param>
    /// <returns></returns>
    [Route("Feedback")]
    [HttpPost]
    public async Task<IActionResult> Feedback(FeedbackDto dto)
    {
        var feedback = new Feedback
        {
            AppVersion = dto.AppVersion,
            Content = dto.Content,
            CreatedTime = DateTime.Now,
            OS = dto.OS.ToLower(),
            Contact = dto.Contact,
            PhoneModel = dto.PhoneModel
        };
        var result = await data.CreateFeedbackAsync(feedback);
        if (result != 1)
        {
            return BadRequest();
        }

        _ = feedbackNoticeBot.PushNoticeAsync(
            $"收到新反馈\n{dto.Content}\n请前往http://app.cs.tsinghua.edu.cn/Home/Feedback回复");
        return Created("Api/Feedback", null);
    }

    [Route("RepliedFeedback")]
    public async Task<IActionResult> RepliedFeedback()
    {
        return Ok((await data.GetAllRepliedFeedbacksAsync())
            .Select(x => new
            {
                content = x.Content, reply = x.Reply, replierName = x.ReplierName, repliedTime = x.RepliedTime
            }).ToList());
    }

    /// <summary>
    ///     Get the url content of Wechat group QRCode.
    /// </summary>
    /// <returns>The url string, NOT in json format.</returns>
    [Route("QRCode")]
    public async Task<IActionResult> QRCode()
    {
        return Ok((await data.GetMiscAsync())?.QrCodeContent ?? "");
    }

    /// <summary>
    ///     Redirect to the url ok APK.
    /// </summary>
    /// <returns></returns>
    [Route("Apk")]
    public async Task<IActionResult> Apk()
    {
        // when start for the first time, if the apkurl is null or empty, this will generate an exception, so set an apkurl value as soon as possible.
        return Redirect((await data.GetMiscAsync())?.ApkUrl ?? "");
    }

    [Route("Socket")]
    public async Task<IActionResult> Socket([FromQuery] int? sectionId)
    {
        if (sectionId is null)
            return Ok(new List<SocketDto>());

        return Ok((await data.GetSocketsAsync((int)sectionId)).Select(x => new SocketDto
        {
            CreatedTime = x.CreatedTime,
            SeatId = x.SeatId,
            SectionId = x.SectionId,
            UpdatedTime = x.UpdatedTime,
            Status = Parse(x.Status)
        }).ToList());

        static string Parse(Socket.SocketStatus status)
        {
            return status switch
            {
                DBModels.Socket.SocketStatus.Available => "available",
                DBModels.Socket.SocketStatus.Unavailable => "unavailable",
                _ => "unknown"
            };
        }
    }

    /* [HttpPost]
    [Route("Socket")]
    public async Task<IActionResult> Socket(SocketDto dto)
    {
        var result = await data.UpdateSocketAsync(dto.SeatId ?? 0, dto.IsAvailable ?? false);
        if (result != 1)
            return BadRequest();
        return Ok();
    } */

    [Route("Version/{os}")]
    public IActionResult Version([FromRoute] string os)
    {
        return Ok(os.Equals("android", StringComparison.CurrentCultureIgnoreCase)
            ? versionManager.GetCurrentVersion(VersionManager.OS.Android)
            : versionManager.GetCurrentVersion(VersionManager.OS.IOS));
    }

    [Route("CardIVersion")]
    public async Task<IActionResult> CardIVersion()
    {
        return Ok(new { Version = (await data.GetMiscAsync())?.CardIVersion ?? -1 });
    }
    
    [Route("SchoolCalendarYear")]
    public async Task<IActionResult> SchoolCalendarYear()
    {
        return Ok(new { Year = (await data.GetMiscAsync())?.SchoolCalendarYear ?? -1 });
    }

    [Route("JieliWashers")]
    public async Task<IActionResult> JieliWashers([FromQuery] string? building)
    {
        var washers = await data.GetJieliWashersAsync(building);
        return Ok(washers.ToDictionary(w => w.Id, w => w.Name));
    }
}
