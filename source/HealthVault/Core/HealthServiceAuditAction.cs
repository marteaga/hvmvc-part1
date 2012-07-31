// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace Microsoft.Health
{
    /// <summary>
    /// Indicates the operation that was performed.
    /// </summary>
    /// 
    public enum HealthServiceAuditAction
    {
        /// <summary>
        /// The action returned from the server is not understood by this
        /// client.
        /// </summary>
        /// 
        Unknown = 0,

        /// <summary>
        /// A creation occurred.
        /// </summary>
        /// 
        Created = 1,

        /// <summary>
        /// An update occurred.
        /// </summary>
        /// 
        Updated = 2,

        /// <summary>
        /// A deletion occurred.
        /// </summary>
        /// 
        Deleted = 3,
    }
}
