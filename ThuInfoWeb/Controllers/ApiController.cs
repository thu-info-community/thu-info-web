using Microsoft.AspNetCore.Mvc;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Models;

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

        public ApiController(Data data)
        {
            this._data = data;
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
        /// <param name="vm">a json, has content, appversion, os, nickname(optional)</param>
        /// <returns></returns>
        [Route("Feedback"), HttpPost]
        public async Task<IActionResult> Feedback(FeedbackViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var feedback = new Feedback()
            {
                AppVersion = vm.AppVersion,
                Content = vm.Content,
                CreatedTime = DateTime.Now,
                OS = vm.OS.ToLower(),
                NickName = vm.NickName,
                Contact = vm.Contact
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
        /// <returns></returns>
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
    }
}
