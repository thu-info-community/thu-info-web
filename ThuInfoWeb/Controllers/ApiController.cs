using Microsoft.AspNetCore.Mvc;
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
        private readonly SecretManager _secretManager;

        public ApiController(Data data,SecretManager secretManager)
        {
            this._data = data;
            this._secretManager = secretManager;
        }
        /// <summary>
        /// Get announce, you should only enter id or page at once.
        /// </summary>
        /// <param name="id">if entered, this will return single value</param>
        /// <param name="page">if entered, this will return up to 5 values in an array.</param>
        /// <returns></returns>
        [Route("Announce")]
        public async Task<IActionResult> Announce([FromQuery] int? id, [FromQuery] int? page)
        {
            if (page is not null)
            {
                var a = await _data.GetAnnouncesAsync(page ?? 0, 5);
                return Ok(a);
            }
            else
            {
                var a = await _data.GetAnnounceAsync(id);
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
                NickName = dto.NickName,
                Contact = dto.Contact
            };
            var result = await _data.CreateFeedbackAsync(feedback);
            if (result != 1) return BadRequest();
            else return Created("Api/Feedback", null);
        }
        /// <summary>
        /// Get feedback, you should only enter id or page at once.
        /// </summary>
        /// <param name="id">if entered, return a single value</param>
        /// <param name="page">if entered, return up to 5 values in an array</param>
        /// <returns>In json format.</returns>
        [Route("Feedback")]
        public async Task<IActionResult> Feedback([FromQuery] int? id, [FromQuery] int? page)
        {
            if (page is not null)
            {
                return Ok(await _data.GetFeedbacksAsync(page ?? 0, 5));
            }
            else
            {
                return Ok(await _data.GetFeedbackAsync(id));
            }
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
        [Route("Apk")]
        public async Task<IActionResult> Apk()
        {
            return Redirect((await _data.GetMiscAsync())?.ApkUrl);
        }
        [Route("Socket")]
        public async Task<IActionResult> Socket([FromQuery] int sectionId)
        {
            return Ok(await _data.GetSocketsAsync(sectionId));
        }
        [Route("Socket")]
        public async Task<IActionResult> Socket([FromQuery] int? seatid,[FromQuery]bool? isAvailable)
        {
            if (seatid is null || isAvailable is null) return BadRequest();
            var result = await _data.UpdateSocketAsync(seatid ?? 0, isAvailable ?? false);
            if (result != 1) return NoContent();
            else return Ok();
        }
        [Route("Version")]
        public async Task<IActionResult> Version([FromRoute]string os)
        {
            if (os.ToLower() == "android") return Ok(await _data.GetVersionAsync(true));
            else return Ok(await _data.GetVersionAsync(false));
        }
        [Route("Version"),HttpPost]
        public async Task<IActionResult> Version([FromQuery] string key,VersionDto dto)
        {
            if (key != _secretManager.CreateVersionKey) return Forbid();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _data.CreateVersionAsync(new DBModels.Version()
            {
                CreatedTime = DateTime.Now,
                IsAndroid = dto.IsAndroid,
                ReleaseNote = dto.ReleaseNote,
                VersionName = dto.VersionName
            });
            if (result != 1) return BadRequest();
            else return CreatedAtAction("LatestVersion", dto.IsAndroid ? "android" : "ios");
        }
    }
}
