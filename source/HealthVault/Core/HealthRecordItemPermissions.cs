// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Xml.XPath;


namespace Microsoft.Health
{
    /// <summary>
    /// Provides values that indicate the rights associated with access to a 
    /// health record item.
    /// </summary>
    /// 
    [Flags]
    public enum HealthRecordItemPermissions
    {
        /// <summary>
        /// The person or group has no permissions.
        /// This is not the same as denial of permissions. It means that the 
        /// rule is not currently granting any permissions.
        /// </summary>
        /// 
        None = 0x0,

        /// <summary>
        /// The person or group has read access to the set of health record
        /// items.
        /// </summary>
        /// 
        Read = 0x1,

        /// <summary>
        /// The person or group has update access to the set of health record
        /// items.
        /// </summary>
        /// 
        Update = 0x2,

        /// <summary>
        /// The person or group can create health record items in the set.
        /// </summary>
        /// 
        Create = 0x4,

        /// <summary>
        /// The person or group can delete health record items in the set.
        /// </summary>
        /// 
        Delete = 0x8,

        /// <summary>
        /// The person or group has all permissions on the set.
        /// </summary>
        /// 
        All = Read | Update | Create | Delete
    }
}
