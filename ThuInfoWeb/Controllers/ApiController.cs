using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                var a = await _data.GetAnnounceAsync(id ?? 0);
                if (a is null) return NotFound();
                else return Ok(a);
            }
        }
    }
}
