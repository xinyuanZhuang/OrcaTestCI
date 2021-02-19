using Orca.Services;
using Microsoft.Extensions.Logging;
using Orca.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Orca.Database;

namespace Orca.Controllers
{
    // -- /api/events
    [ApiController]
    [Route("api/events")]
    public class EventController : ControllerBase
    {
        private readonly IEventAggregator _eventAggregator;
        // IEventAggregator is the interface of event aggregator.

        public EventController(IEventAggregator eventAggregator)
        {
            // Passing event aggregator to event controller.
            _eventAggregator = eventAggregator;
        }

        [HttpGet]
        public StudentEvent GenerateEvent()
        {
            StudentEvent testEvent = new StudentEvent
            {
                CourseID = "COMP0101", // Course ID Upper case.
                Timestamp = DateTime.UtcNow,
                EventType = EventType.Attendance,
                ActivityType = "Video",
                ActivityName = "Weekly Lecture",
                Student = new Student 
                { 
                    Email = "vcd.zard@example.com",
                    FirstName = "Vcd",
                    LastName = "Zard",
                    ID = "2020019878"
                }
            };
             _eventAggregator.ProcessEvent(testEvent);
            // Print event info in console.
            return testEvent;
            // Get response on the swagger UI.
        }

    }
}
