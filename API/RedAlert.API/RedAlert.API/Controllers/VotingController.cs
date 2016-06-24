using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RedAlert.API.BL;

namespace RedAlert.API.Controllers
{
    public class VotingController : ApiController
    {
        [HttpGet]
        [Route("Voting/up")]
        public void UpVote()
        {
            ColorHelper.TotalResponse++;
            ColorHelper.YesAnswer++;
        }
        [HttpGet]
        [Route("Voting/down")]
        public void DownVote()
        {
            ColorHelper.TotalResponse++;
            ColorHelper.NoAnswer++;
        }
    }
}
