using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orca.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Orca.Services
{
    public class SharepointCourseCatalog : ICourseCatalog
    {
        public const string COURSE_CATALOG_LIST_DESCRIPTION = "Course Catalog of Course IDs and their Sharepoint List Names.";
        public const string COURSE_ID_FIELD = "CourseId";
        public const string SHAREPOINT_LIST_NAME_FIELD = "SharepointListName";
        public readonly string COURSE_ID_FIELD_XML_SCHEMA = $"<Field DisplayName='{COURSE_ID_FIELD}' Type='Text' Required='TRUE' />";
        public readonly string SHAREPOINT_LIST_NAME_FIELD_XML_SCHEMA = $"<Field DisplayName='{SHAREPOINT_LIST_NAME_FIELD}' Type='Text' Required='TRUE' />";

        private ILogger<SharepointCourseCatalog> _logger;

        private string _courseCatalogListName;
        private Dictionary<string, string> _courseIdToSharepointListNameMapping;
        private ISharepointManager _sharepointManager;

        /// <summary>
        /// Instantiates a CourseCatalog backed by a Sharepoint List. If the Sharepoint List does not exist, it will be created
        /// </summary>
        /// <param name="sharepointSettings">The settings used to access the Sharepoint List storing the catalog</param>
        /// <param name="createSharepointManagerFunc">A function which returns an ISharepointManager. This will be used whenever the catalog needs to access Sharepoint</param>
        public SharepointCourseCatalog(IOptions<SharepointSettings> sharepointSettings, ILogger<SharepointCourseCatalog> logger, ISharepointManager sharepointManager)
        {
            _logger = logger;
            var settingsValue = sharepointSettings.Value;
            _courseCatalogListName = settingsValue.CourseCatalogListName;

            _courseIdToSharepointListNameMapping = new Dictionary<string, string>();
            _sharepointManager = sharepointManager;

            if (!sharepointManager.CheckListExists(_courseCatalogListName))
            {
                _logger.LogInformation($"The List '{_courseCatalogListName}' used to store the Course Catalog does not exist. Creating it now.");
                _sharepointManager.CreateList(_courseCatalogListName, COURSE_CATALOG_LIST_DESCRIPTION, new List<string> { COURSE_ID_FIELD_XML_SCHEMA, SHAREPOINT_LIST_NAME_FIELD_XML_SCHEMA });
            }
        }


        /// <summary>
        /// Updates the in memory Course ID to Sharepoint List Name Mapping.
        /// This method is thread safe because it simply updates the reference to _courseIdToSharepointListNameMapping
        /// </summary>
        /// <returns></returns>
        public async Task UpdateInMemoryMapping()
        {
            var courseMapping = await _sharepointManager.GetItemsFromList(_courseCatalogListName);
            var updatedCourseMapping = new Dictionary<string, string>();
            foreach (var courseEntry in courseMapping)
            {
                updatedCourseMapping.Add((string)courseEntry[COURSE_ID_FIELD], (string)courseEntry[SHAREPOINT_LIST_NAME_FIELD]);
            }
            _courseIdToSharepointListNameMapping = updatedCourseMapping;


        }

        public string GetListNameForCourse(string courseId)
        {
            // Return course list or throw KeyNotFoundException.
            return _courseIdToSharepointListNameMapping[courseId];
        }

        public bool CheckCourseIdExist(string courseId)
        {
            return _courseIdToSharepointListNameMapping.ContainsKey(courseId);
        }

    }
}
