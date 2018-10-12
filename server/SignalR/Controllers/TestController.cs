using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SignalR.Hubs;

namespace SignalR.Controllers
{
    [RoutePrefix("test")]
    public class TestController : ApiController
    {
        readonly TestNotificationHub _testHub;

        public TestController(TestNotificationHub testHub)
        {
            _testHub = testHub;
        }

        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping([FromUri] string connectionId)
        {
            _testHub.Broadcast(connectionId, new {Title = "Test"});
            return Ok();
        }
    }
}
