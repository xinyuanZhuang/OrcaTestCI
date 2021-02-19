using Microsoft.Extensions.Logging;
using Orca.Database;
using Orca.Entities;
using Orca.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orca.Services
{

    public class EventAggregator : IEventAggregator
    {

        private readonly ISharepointManager _sharePointManager;
        private readonly ICourseCatalog _courseCatalog;
        private ILogger<EventAggregator> _logger;
        public EventAggregator(ISharepointManager sharePointManager, ICourseCatalog courseCatalog, ILogger<EventAggregator> logger)
        {
            _sharePointManager = sharePointManager;
            _courseCatalog = courseCatalog;
            _logger = logger;
        }

        public async Task ProcessEvent(StudentEvent studentEvent)
        {

            //Check courseId exist in the coursecatalog
            if (_courseCatalog.CheckCourseIdExist(studentEvent.CourseID))
            {
                if (studentEvent.EventType == EventType.Attendance)
                {
                    // Check the corresponding list of this course according to Catalog.
                    string targetList = _courseCatalog.GetListNameForCourse(studentEvent.CourseID);
                    _logger.LogInformation("Event aggregator will send event to list {0}.", targetList);

                    SharepointListItem eventItem = new SharepointListItem();
                    // Event Detailed Information.
                    eventItem["Title"] = "Event by " + studentEvent.Student.Email;
                    eventItem["CourseID"] = studentEvent.CourseID.ToUpper();
                    eventItem["StudentName"] = studentEvent.Student.FirstName + " (" + studentEvent.Student.LastName + ")";
                    eventItem["StudentID"] = studentEvent.Student.ID;
                    eventItem["StudentEmail"] = studentEvent.Student.Email;
                    eventItem["EventType"] = studentEvent.EventType.ToString();
                    eventItem["ActivityType"] = studentEvent.ActivityType;
                    eventItem["ActivityName"] = studentEvent.ActivityName;
                    eventItem["Timestamp"] = studentEvent.Timestamp;

                    // Assign to different list by course ID.
                    await _sharePointManager.AddItemToList(targetList, eventItem);
                }
                
                StoreInDatabase(studentEvent);
            }
            else
            {
                _logger.LogInformation($"Cannot find the courseId '{studentEvent.CourseID}', event aggregator has cancelled current event.");
            }
        }


        private void StoreInDatabase(StudentEvent studentEvent)
        {
            _logger.LogInformation(studentEvent.ToString());
            DatabaseConnect connect = new DatabaseConnect();
            connect.StoreStudentToDatabase(studentEvent);
            connect.StoreEventToDatabase(studentEvent);
        }
    }
}
