using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Orca.Entities.Dtos;
using Orca.Services.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orca.Controllers
{
    [Route("api/events/caliper")]
    [ApiController]
    public class CaliperEventsController : ControllerBase
    {
        private readonly MoodleAdapter _moodleAdapter;

        public CaliperEventsController(MoodleAdapter moodleAdapter)
        {
            _moodleAdapter = moodleAdapter;
        }

        [HttpPost]
        public async Task CaliperEvent([FromBody] CaliperEventBatchDto caliperEventBatch)
        {
            await _moodleAdapter.ProcessEvents(caliperEventBatch);
        }
    }
}
