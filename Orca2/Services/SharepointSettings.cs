using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Orca.Services
{
    /// <summary>
    /// Stores various pieces of information needed to connect to Sharepoint 
    /// </summary>
    public class SharepointSettings
    {
        /// 
        /// <summary>
        /// The App ID registered on Azure AD
        /// </summary>
        public string AzureAppId { get; set; }

        /// <summary>
        /// The URL to the sharepoint site
        /// </summary>
        public string SharepointUrl { get; set; }

        /// <summary>
        /// The name of the Sharepoint List where the course id to list name mapping is stored
        /// </summary>
        public string CourseCatalogListName { get; set; }

        /// <summary>
        /// The username through which we will authenticate
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password through which we will authenticate
        /// </summary>
        public string Password { get; set; }

    }
}
