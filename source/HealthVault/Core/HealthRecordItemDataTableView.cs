// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health
{
    /// <summary>
    /// Defines the possible views for the <see cref="HealthRecordItemDataTable"/>.
    /// </summary>
    /// 
    public enum HealthRecordItemDataTableView
    {
        /// <summary>
        /// The default view uses SingleTypeTable if the filter contains
        /// only a single type and there is a single type table view defined.
        /// Otherwise, the MultipleTypeTable view is used.
        /// </summary>
        /// 
        Default = 0,

        /// <summary>
        /// A view for the <see cref="HealthRecordItemDataTable"/> that shows 
        /// columns that are specific to a single item type. However, if the 
        /// type does not contain a specific single type table view, the base 
        /// type is used, which is the same as the multiple type table.
        /// </summary>
        /// 
        SingleTypeTable = 1,

        /// <summary>
        /// A view that shows a common set of columns for all item types.
        /// </summary>
        /// 
        MultipleTypeTable = 2
    }
}

