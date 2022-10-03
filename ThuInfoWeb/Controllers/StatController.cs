using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThuInfoWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatController : ControllerBase
    {
        private readonly Data _data;

        public StatController(Data data)
        {
            this._data = data;
        }
        [Route("[action]/{function}")]
        public async Task<IActionResult> Usage(int function)
        {
            if (!Enum.IsDefined(typeof(DBModels.Usage.FunctionType), function))
                return BadRequest("功能不存在");
            var usage = new DBModels.Usage()
            {
                Function = (DBModels.Usage.FunctionType)function,
                CreatedTime = DateTime.Now
            };
            var result = await _data.CreateUsageAsync(usage);
            if (result != 1)
                return BadRequest();
            else
                return Ok();
        }
        [Route("[action]"),Authorize(Roles = "admin")]
        public async Task<IActionResult> UsageData()
        {
            return Ok(await _data.GetUsageAsync());
        }
    }
}
