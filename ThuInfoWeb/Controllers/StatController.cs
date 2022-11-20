using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuInfoWeb.DBModels;

namespace ThuInfoWeb.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class StatController : ControllerBase
    {
        private readonly Data _data;

        public StatController(Data data)
        {
            _data = data;
        }

        [Route("{function:int}")]
        public async Task<IActionResult> Usage(int function)
        {
            if (!Enum.IsDefined(typeof(Usage.FunctionType), function))
                return BadRequest("功能不存在");
            var usage = new Usage
            {
                Function = (Usage.FunctionType)function,
                CreatedTime = DateTime.Now
            };
            var result = await _data.CreateUsageAsync(usage);
            if (result != 1)
                return BadRequest();
            return Ok();
        }

        [Route(""), Authorize(Roles = "admin")]
        public async Task<IActionResult> UsageData()
        {
            return Ok(await _data.GetUsageAsync());
        }

        public async Task<IActionResult> Startup()
        {
            var s = new Startup { CreatedTime = DateTime.Now };
            var result = await _data.CreateStartupAsync(s);
            if (result != 1)
                return BadRequest();
            return Ok();
        }

        [Route(""), Authorize(Roles = "admin")]
        public async Task<IActionResult> StartupData()
        {
            return Ok(await _data.GetStartupDataAsync());
        }
#if DEBUG
        public async Task<IActionResult> GenStartupData()
        {
            await _data.GenStartupDataAsync();
            return Ok();
        }
#endif
    }
}