using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NewGarasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestModel
    {
        public string JobStatus { get; set; }
        public string UserId { get; set; }
    }
    public class ZapierTestController : ControllerBase
    {
        [HttpPost("RecieveDataFromZapier")]
        public IActionResult RecieveData([FromBody]TestModel model)
        {
            return Ok(model.JobStatus);
        }
    }
}
