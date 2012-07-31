// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.Health.Web
{
    /// <summary> our template for building action links </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [SecurityCritical]
    internal class ActionTemplate : ITemplate
    {
        [SecurityCritical]
        internal ActionTemplate(HealthRecordItemDataGrid grid)
        {
            _grid = grid;
        }

        /// <summary>
        /// Defines the Control object that child controls and templates belong
        /// to. These child controls are in turn defined within an inline
        /// template.
        /// </summary>
        /// 
        /// <param name="container">
        /// The <see cref="Control"/> object to contain the instances of the 
        /// controls from the inline template.
        /// </param>
        /// 
        [SecurityCritical]
        public void InstantiateIn(Control container)
        {
            Validator.ThrowIfArgumentNull(container, "container", "ArgumentNull");
            _grid.AddActionLinksToContainer(container, -1, this);
        }

        [SecurityCritical]
        internal void OnLinkDataBinding(Object sender, System.EventArgs e)
        {
            LinkButton link = (LinkButton)sender;
            GridViewRow gridRow = (GridViewRow)link.NamingContainer;

            string key = 
                (string)DataBinder.Eval(gridRow.DataItem, "wc-id") + "," +
                (string)DataBinder.Eval(gridRow.DataItem, "wc-version");

            link.CommandArgument = key;

            if (!String.IsNullOrEmpty(link.OnClientClick))
                link.OnClientClick =  link.OnClientClick.Replace("[KEY]", key);
        }

        private HealthRecordItemDataGrid _grid;
    }
}
