using Xunit;
using Orca.Services.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orca.Services;
using OrcaTests.Tools;
using Orca.Entities;
using Orca.Entities.Dtos;

namespace OrcaTests.Services.Adapters
{
    public class MoodleAdapterTests
    {
        [Fact]
        public async Task ProcessEventsAcceptsEventsAboutStudentsInteractingWithCourses()
        {
            var eventAggregator = new MockEventAggregator();
            var moodleAdapter = new MoodleAdapter(eventAggregator);

            CaliperActor caliperActor = new CaliperActor
            {
                Id = "1",
                ActorType = "http://purl.imsglobal.org/caliper/v1/lis/Person",
                Name = "John Doe",
                Extensions = new CaliperActorExtensions { Email = "john.doe@example.com" }
            };
            string activityType = "survey";
            string studentRole = "http://purl.imsglobal.org/vocab/lis/v2/membership#Learner";
            CaliperEventBatchDto eventsWithStudentRole = EventBatch(caliperActor, "any", "http://purl.imsglobal.org/caliper/v1/lis/" + activityType, "COMP0101", MoodleAdapter.COURSE_GROUP_TYPE, studentRole);

            await moodleAdapter.ProcessEvents(eventsWithStudentRole);

            Assert.Single(eventAggregator.processedEvents);
            var processedEvent = eventAggregator.processedEvents[0];
            CaliperEventDto submittedEvent = eventsWithStudentRole.Data[0];
            Assert.Equal(submittedEvent.Actor.Name, processedEvent.Student.FirstName + " " + processedEvent.Student.LastName);
            Assert.Equal(submittedEvent.Actor.Extensions.Email, processedEvent.Student.Email);
            Assert.Equal(submittedEvent.Actor.Id, processedEvent.Student.ID);
            Assert.Equal(submittedEvent.Object.Name, processedEvent.ActivityName);
            Assert.Equal(activityType, processedEvent.ActivityType);
            Assert.Equal(submittedEvent.Group.Name, processedEvent.CourseID);
            Assert.Equal(submittedEvent.EventTime, processedEvent.Timestamp);
        }

        [Fact]
        public async Task ProcessEventsIgnoresEventsIfActorNotStudent()
        {
            var eventAggregator = new MockEventAggregator();
            var moodleAdapter = new MoodleAdapter(eventAggregator);

            CaliperActor caliperActor = new CaliperActor
            {
                Id = "1",
                ActorType = "http://purl.imsglobal.org/caliper/v1/lis/Person",
                Name = "John Doe",
                Extensions = new CaliperActorExtensions { Email = "john.doe@example.com" }
            };
            string instructorRole = "http://purl.imsglobal.org/vocab/lis/v2/membership#Instructor";
            CaliperEventBatchDto eventsWithInstructorRole = EventBatch(caliperActor, "any", "http://purl.imsglobal.org/caliper/v1/lis/url", "COMP0101", MoodleAdapter.COURSE_GROUP_TYPE, instructorRole);

            await moodleAdapter.ProcessEvents(eventsWithInstructorRole);

            Assert.Empty(eventAggregator.processedEvents);
        }

        private static CaliperEventBatchDto EventBatch(CaliperActor actor, string objectName, string objectType, string groupName, string groupType, string actorRole)
        {
            return new CaliperEventBatchDto
            {
                Data = new List<CaliperEventDto>
                {
                    new CaliperEventDto
                    {
                        Actor = actor,
                        Object = new CaliperObject
                        {
                            Id = "any",
                            ObjectType = objectType,
                            Name = objectName
                        },
                        Group = new CaliperGroup
                        {
                            Id = "any",
                            Name = groupName,
                            GroupType = groupType
                        },
                        Membership = new CaliperActorMembership {Roles = new List<string>{ actorRole }},
                        Action = "any",
                        Type = "any",
                        EventTime = DateTime.UtcNow
                    }
                }
            };
        }
    }


    class MockEventAggregator : IEventAggregator
    {
        public List<StudentEvent> processedEvents = new List<StudentEvent>();
        public async Task ProcessEvent(StudentEvent studentEvent)
        {
            processedEvents.Add(studentEvent);
        }
    }
}
