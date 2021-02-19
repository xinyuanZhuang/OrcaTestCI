﻿using Xunit;
using Orca.Services;
using System;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrcaTests.Tools;

namespace OrcaTests.Services
{
    public class SharepointCourseCatalogTests
    {

        [Fact]
        public void ConstructorCreatesCourseCatalogListIfNotExist()
        {
            var courseCatalogListName = "CourseCatalog";
            var settings = SharepointSettingsWithCourseCatalogName(courseCatalogListName);
            var mockSharepointManager = new MockSharepointManager();

            var courseCatalog = new SharepointCourseCatalog(Options.Create(settings), new InMemoryLogger<SharepointCourseCatalog>(), mockSharepointManager);


            bool catalogListWasCreated = mockSharepointManager.CheckListExists(courseCatalogListName);
            Assert.True(catalogListWasCreated);
        }

        [Fact]
        public async Task UpdateInMemoryMappingGetsLatestCopyOfUnderlyingSharepointList()
        {
            var courseCatalogListName = "CourseCatalog";
            SharepointSettings settings = SharepointSettingsWithCourseCatalogName(courseCatalogListName);
            var mockSharepointManager = new MockSharepointManager();
            mockSharepointManager.CreateList(courseCatalogListName, "", new List<string>());

            string courseIdThatWillBeDeleted = "COMP0100";
            var itemThatWillBeDeleted = ListItemWithCourseIdAndListName(courseIdThatWillBeDeleted, "any");
            await mockSharepointManager.AddItemToList(courseCatalogListName, itemThatWillBeDeleted);

            var courseCatalog = new SharepointCourseCatalog(Options.Create(settings), new InMemoryLogger<SharepointCourseCatalog>(), mockSharepointManager);
            // simulate deleting an item from the sharepoint list
            mockSharepointManager.mockEventList[courseCatalogListName].Remove(itemThatWillBeDeleted);
            // simulate adding an item to the sharepoint list
            string courseIdThatWillBeAdded = "COMP0102";
            string expectedListName = "expectedListName";
            await mockSharepointManager.AddItemToList(courseCatalogListName, ListItemWithCourseIdAndListName(courseIdThatWillBeAdded, expectedListName));
            // update the catalog after modifying the underlying sharepoint list
            await courseCatalog.UpdateInMemoryMapping();

            Assert.Throws<KeyNotFoundException>(() => courseCatalog.GetListNameForCourse(courseIdThatWillBeDeleted));
            string actualListNameOfAddedEntry = courseCatalog.GetListNameForCourse(courseIdThatWillBeAdded);
            Assert.Equal(expectedListName, actualListNameOfAddedEntry);
        }

        public static SharepointSettings SharepointSettingsWithCourseCatalogName(string courseCatalogListName)
        {
            return new SharepointSettings
            {
                CourseCatalogListName = courseCatalogListName,
                AzureAppId = "any",
                SharepointUrl = "https://anysharepointurl.com/sites/X",
                Username = "any",
                Password = "any"
            };
        }

        private static Orca.Tools.SharepointListItem ListItemWithCourseIdAndListName(string courseId, string listName)
        {
            return new Orca.Tools.SharepointListItem()
            {
                [SharepointCourseCatalog.COURSE_ID_FIELD] = courseId,
                [SharepointCourseCatalog.SHAREPOINT_LIST_NAME_FIELD] = listName
            };
        }




    }
}
