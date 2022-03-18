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
        private readonly VersionManager _versionManager;

        public ApiController(Data data, VersionManager versionManager)
        {
            this._data = data;
            this._versionManager = versionManager;
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
                Contact = dto.Contact,
                PhoneModel = dto.PhoneModel
            };
            var result = await _data.CreateFeedbackAsync(feedback);
            if (result != 1) return BadRequest();
            else return Created("Api/Feedback", null);
        }
        /// <summary>
        /// Get feedback, you should only enter id or page at one time.
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
        /// <summary>
        /// Redirect to the url ok APK.
        /// </summary>
        /// <returns></returns>
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
        [HttpPost, Route("Socket")]
        public async Task<IActionResult> Socket(SocketDto dto)
        {
            var result = await _data.UpdateSocketAsync(dto.SeatId, dto.IsAvailable);
            if (result != 1) return NoContent();
            else return Ok();
        }
        [Route("Version/{os}")]
        public IActionResult Version([FromRoute] string os)
        {
            if (os.ToLower() == "android") return Ok(_versionManager.GetCurrentVersion(VersionManager.OS.Android));
            else return Ok(_versionManager.GetCurrentVersion(VersionManager.OS.IOS));
        }
        //[Route("LostAndFound")]
        //public async Task<IActionResult> LostAndFound()
        //{
        //    return Ok(await _data.GetUnsolveLostAndFoundsAsync());
        //}
        //[HttpPost,Route("LostAndFound/Create")]
        //public async Task<IActionResult> LostAndFoundCreate(LostAndFoundDto dto)
        //{
        //    var time = DateTime.Now;
        //    var l = new LostAndFound()
        //    {
        //        CreatedTime = time,
        //        UpdatedTime = time,
        //        IsSolved = false,
        //        Message = dto.Message,
        //        SenderId = dto.SenderId,
        //        TargetId = dto.TargetId,
        //    };
        //    var result = await _data.CreateLostAndFoundAsync(l);
        //    if (result != 1) return BadRequest();
        //    else return CreatedAtAction(nameof(LostAndFound), null);
        //}
    }
}
