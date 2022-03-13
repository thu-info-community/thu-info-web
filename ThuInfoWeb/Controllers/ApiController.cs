using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThuInfoWeb.DBModels;
using ThuInfoWeb.Models;

namespace ThuInfoWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly Data _data;

        public ApiController(Data data)
        {
            this._data = data;
        }
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
        [Route("Feedback"), HttpPost]
        public async Task<IActionResult> Feedback(FeedbackViewModel vm)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var feedback = new Feedback()
            {
                AppVersion = vm.Content,
                Content = vm.AppVersion,
                CreatedTime = DateTime.Now,
                OS = vm.OS.ToLower()
            };
            var result = await _data.CreateFeedbackAsync(feedback);
            if (result != 1) return BadRequest();
            else return Created("/Feedback", null);
        }
        [Route("Feedback")]
        public async Task<IActionResult> Feedback([FromQuery]int? id,[FromQuery]int? page)
        {
            if(page is not null)
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
    