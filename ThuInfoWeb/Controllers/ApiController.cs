using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuInfoWeb.Bots;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Dtos;

namespace ThuInfoWeb.Controllers
{
    /// <summary>
    /// The controller for RESTApi
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly Data _data;
        private readonly VersionManager _versionManager;
        private readonly FeedbackNoticeBot _feedbackNoticeBot;

        public ApiController(Data data, VersionManager versionManager,FeedbackNoticeBot feedbackNoticeBot)
        {
            this._data = data;
            this._versionManager = versionManager;
            this._feedbackNoticeBot = feedbackNoticeBot;
        }
        /// <summary>
        /// Get announce, get the latest announce simply by no query string(just get /api/announce). If needed, you should only enter id or page at one time.
        /// </summary>
        /// <param name="id">if entered, this will return single value</param>
        /// <param name="page">if entered, this will return up to 5 values in an array.</param>
        /// <returns>In json format.</returns>
        [Route("Announce")]
        public async Task<IActionResult> Announce([FromQuery] int? id, [FromQuery] int? page)
        {
            if (page is not null && page <= 0)
                return BadRequest("page必须是正整数");
            if (page is not null)
            {
                var a = await _data.GetActiveAnnouncesAsync(page ?? 1, 5);
                return Ok(a);
            }
            else
            {
                var a = await _data.GetActiveAnnounceAsync(id);
                return Ok(a);
            }
        }
        /// <summary>
        /// Create a feedback
        /// </summary>
        /// <param name="dto">a json, has content, appversion, os, nickname(optional)</param>
        /// <returns></returns>
        [Route("Feedback"), HttpPost]
        public async Task<IActionResult> Feedback(FeedbackDto dto)
        {
            var feedback = new Feedback()
            {
                AppVersion = dto.AppVersion,
                Content = dto.Content,
                CreatedTime = DateTime.Now,
                OS = dto.OS.ToLower(),
                Contact = dto.Contact,
                PhoneModel = dto.PhoneModel
            };
            var result = await _data.CreateFeedbackAsync(feedback);
            if (result != 1) return BadRequest();
            else
            {
                _ = _feedbackNoticeBot.PushNoticeAsync($"收到新反馈\n{dto.Content}\n请前往https://thuinfo.net/Home/Feedback回复");
                return Created("Api/Feedback", null);
            }
        }
        [Route("RepliedFeedback")]
        public async Task<IActionResult> RepliedFeedback()
        {
            return Ok((await _data.GetAllRepliedFeedbacksAsync())
                .Select(x => new
                {
                    content = x.Content,
                    reply = x.Reply,
                    replierName = x.ReplierName ?? "",
                    repliedTime = x.RepliedTime
                }).ToList());
        }
        /// <summary>
        /// Get the url content of Wechat group QRCode.
        /// </summary>
        /// <returns>The url string, NOT in json format.</returns>
        [Route("QRCode")]
        public async Task<IActionResult> QRCode()
        {
            return Ok((await _data.GetMiscAsync()).QrCodeContent);
        }
        /// <summary>
        /// Redirect to the url ok APK.
        /// </summary>
        /// <returns></returns>
        [Route("Apk")]
        public async Task<IActionResult> Apk()
        {
            // when start for the first time, if the apkurl is null or empty, this will generate an exception, so set an apkurl value as soon as possible.
            return Redirect((await _data.GetMiscAsync())?.ApkUrl);
        }
        [Route("Socket")]
        public async Task<IActionResult> Socket([FromQuery] int? sectionId)
        {
            if (sectionId is null)
                return Ok(new List<SocketDto>());
            static string parse(Socket.SocketStatus status) => status switch
            {
                DBModels.Socket.SocketStatus.Available => "available",
                DBModels.Socket.SocketStatus.Unavailable => "unavailable",
                DBModels.Socket.SocketStatus.Unknown => "unknown"
            };
            return Ok((await _data.GetSocketsAsync(sectionId ?? 0)).Select(x => new SocketDto()
            {
                CreatedTime = x.CreatedTime,
                SeatId = x.SeatId,
                SectionId = x.SectionId,
                UpdatedTime = x.UpdatedTime,
                Status = parse(x.Status)
            }).ToList());
        }
        [HttpPost, Route("Socket")]
        public async Task<IActionResult> Socket(SocketDto dto)
        {
            var result = await _data.UpdateSocketAsync(dto.SeatId ?? 0, dto.IsAvailable ?? false);
            if (result != 1) return BadRequest();
            else return Ok();
        }
        [Route("Version/{os}")]
        public IActionResult Version([FromRoute] string os)
        {
            if (os.ToLower() == "android") return Ok(_versionManager.GetCurrentVersion(VersionManager.OS.Android));
            else return Ok(_versionManager.GetCurrentVersion(VersionManager.OS.IOS));
        }
    }
}
