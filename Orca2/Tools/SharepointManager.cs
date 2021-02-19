using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Orca.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Orca.Entities;

namespace Orca.Tools
{
    public class SharepointManager : IDisposable, ISharepointManager
    {
        private readonly string _azureAppId;
        private readonly string _sharepointUrl;
        private bool _disposedValue;
        private PnP.Framework.AuthenticationManager _authenticationManager;
        public SharepointManager(IOptions<SharepointSettings> sharepointSettings)
        {
            var settingsVal = sharepointSettings.Value;
            _azureAppId = settingsVal.AzureAppId;
            _sharepointUrl = settingsVal.SharepointUrl;
            SecureString securePassword = new SecureString();
            foreach(char c in settingsVal.Password)
            {
                securePassword.AppendChar(c);
            }
            _authenticationManager = new PnP.Framework.AuthenticationManager(_azureAppId, settingsVal.Username, securePassword);
        }

        public async Task<bool> AddItemToList(string listName, SharepointListItem item)
        {

            try
            {
                // Authentication.
                using (var context = _authenticationManager.GetContext(_sharepointUrl))
                {
                    Microsoft.SharePoint.Client.List eventList = context.Web.Lists.GetByTitle(listName);
                    ListItemCreationInformation itemInfo = new ListItemCreationInformation();

                    ListItem listItemToAdd = eventList.AddItem(itemInfo);
                    foreach (var field in item)
                    {
                        listItemToAdd[field.Key] = field.Value;
                    }
                    listItemToAdd.Update();
                    await context.ExecuteQueryAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error occurred while adding an item.");
                Console.WriteLine(e.Message);
                return false;
            }
            // When add item to list, the target list may not exist.
            // Then it should be created and set privilege.
            // Should modify code here and with the help of create list function.
        }

        public async Task<List<SharepointListItem>> GetItemsFromList(string listName)
        {
            var itemsToReturn = new List<SharepointListItem>();
            using (var context = _authenticationManager.GetContext(_sharepointUrl))
            {
                var list = context.Web.Lists.GetByTitle(listName);
                context.Load(list);
                context.Load(list.Fields);
                await context.ExecuteQueryAsync();

                CamlQuery query = CamlQuery.CreateAllItemsQuery();

                var items = list.GetItems(query);
                context.Load(items);
                await context.ExecuteQueryAsync();

                foreach (var item in items)
                {
                    itemsToReturn.Add(new SharepointListItem(item.FieldValues));
                }
                return itemsToReturn;
            }
        }

        public bool CheckListExists(string listName)
        {
            using (var context = _authenticationManager.GetContext(_sharepointUrl))
            {
                Web orcaSite = context.Web;
                return orcaSite.ListExists(listName);
            }
        }

        /// <inheritdoc/>
        public void CreateList(string listName, string description, List<string> fieldsAsXml)
        {
            // Need to modify to set privilege.
            using (var context = _authenticationManager.GetContext(_sharepointUrl))
            {
                Web orcaSite = context.Web;

                ListCreationInformation listCreationInfo = new ListCreationInformation();
                listCreationInfo.Title = listName;
                listCreationInfo.TemplateType = (int)ListTemplateType.GenericList;
                listCreationInfo.Description = description;
                List catalogList = orcaSite.Lists.Add(listCreationInfo);
                foreach (var fieldXml in fieldsAsXml)
                {
                    catalogList.Fields.AddFieldAsXml(fieldXml, true, AddFieldOptions.DefaultValue);
                }

                // hide the default title field
                Field titleField = orcaSite.Lists.GetByTitle(listName).Fields.GetByTitle("Title");
                titleField.Hidden = true;
                titleField.Required = false;
                titleField.Update();
                var defaultListView = catalogList.DefaultView;
                context.Load(defaultListView);
                context.Load(defaultListView.ViewFields);
                context.ExecuteQuery();
                defaultListView.ViewFields.Remove("LinkTitle");
                defaultListView.Update();

                // add a navigation link to the newly created list
                orcaSite.AddNavigationNode(
                    listName,
                    new Uri($"{_sharepointUrl}/Lists/{listName}"),
                    string.Empty,
                    PnP.Framework.Enums.NavigationType.QuickLaunch);

                context.ExecuteQuery();
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _authenticationManager?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

    }
    
    public class SharepointListItem : Dictionary<string, object>
    {

        public SharepointListItem() : base()
        {
        }

        /// <summary>
        /// Creates a SharepointListItem based on a dictionary, where the keys are the field names and values are the field values
        /// </summary>
        /// <param name="dictionary">A dictionary whose keys/values represent the item's field names/field values</param>
        public SharepointListItem(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }


    }

}
