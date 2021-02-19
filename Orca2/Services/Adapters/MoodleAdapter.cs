using Orca.Entities;
using Orca.Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orca.Services.Adapters
{
    public class MoodleAdapter
    {
        public const string COURSE_GROUP_TYPE = "http://purl.imsglobal.org/caliper/v1/lis/CourseSection";
        private readonly IEventAggregator _eventAggregator;

        public MoodleAdapter(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public async Task ProcessEvents(CaliperEventBatchDto caliperEventBatch)
        {
            var filteredCaliperEvents = caliperEventBatch.Data.Where(e => IsAboutStudent(e) && IsAboutCourse(e));
            foreach(var caliperEvent in filteredCaliperEvents)
            {
                //TODO cleaner way to get firstname/lastname
                var studentName = caliperEvent.Actor.Name.Split(' ', 2);
                string caliperActivityType = caliperEvent.Object.ObjectType;
                string activityTypeWithoutUrl = caliperActivityType.Substring(caliperActivityType.LastIndexOf('/') + 1);
                var studentEvent = new StudentEvent
                {
                    CourseID = caliperEvent.Group.Name,
                    //TODO don't have this hardcoded in case of live lecture links
                    EventType = EventType.Engagement,
                    ActivityName = caliperEvent.Object.Name,
                    ActivityType = activityTypeWithoutUrl,
                    Student = new Student { ID = caliperEvent.Actor.Id, FirstName = studentName[0], LastName = studentName[1], Email = caliperEvent.Actor.Extensions.Email },
                    Timestamp = caliperEvent.EventTime
                };
                await _eventAggregator.ProcessEvent(studentEvent);

            }
        }

        private static bool IsAboutCourse(CaliperEventDto e)
        {
            return e.Group.GroupType == COURSE_GROUP_TYPE;
        }


        private static bool IsAboutStudent(CaliperEventDto e)
        {
            return e.Actor.ActorType == "http://purl.imsglobal.org/caliper/v1/lis/Person" && e.Membership.Roles.Any(role => role.Contains("Learner"));
        }


    }
}
