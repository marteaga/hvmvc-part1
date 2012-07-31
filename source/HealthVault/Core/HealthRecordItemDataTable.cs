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
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health
{
    /// <summary> 
    /// Represents a data table that populates itself with HealthVault data. 
    /// </summary>
    /// 
    [Serializable]
    public class HealthRecordItemDataTable : DataTable
    {
        /// <summary> 
        /// Creates a new instance of the <see cref="HealthRecordItemDataTable"/> 
        /// class with the specified table view and filter.
        /// </summary>
        /// 
        /// <param name="view">
        /// The view that the data table should take on the data.
        /// </param>
        /// 
        /// <param name="filter">
        /// The filter used to gather health record items from the HealthVault 
        /// service.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="filter"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="view"/> parameter is 
        /// <see cref="HealthRecordItemDataTableView.SingleTypeTable"/> and 
        /// the <paramref name="filter"/> parameter contains more than one type 
        /// identifier.
        /// </exception>
        /// 
        public HealthRecordItemDataTable(
            HealthRecordItemDataTableView view,
            HealthRecordFilter filter)
        {
            Validator.ThrowIfArgumentNull(filter, "filter", "DataTableFilterNull");

            Validator.ThrowArgumentExceptionIf(
                view == HealthRecordItemDataTableView.SingleTypeTable &&
                filter.TypeIds.Count > 1,
                "view",
                "DataTableViewInvalid");

            _filter = filter;
            _view = view;
        }

        #region Serialization

        /// <summary>
        /// Creates a new instance of the <see cref="HealthRecordItemDataTable"/> 
        /// class with the specified serialization information.
        /// </summary>
        /// 
        /// <param name="info">
        /// Serialized information about this data table.
        /// </param>
        /// 
        /// <param name="context">
        /// The stream context of the serialized information.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        protected HealthRecordItemDataTable(
            SerializationInfo info,
            StreamingContext context) 
            : base(info, context)
        {
            Validator.ThrowIfArgumentNull(info, "info", "ExceptionSerializationInfoNull");

            string filterXml = info.GetString("filter");
            if (!String.IsNullOrEmpty(filterXml))
            {
                XPathDocument filterDoc =new XPathDocument(new StringReader(filterXml));
                _filter = HealthRecordFilter.CreateFromXml(filterDoc.CreateNavigator());
            }

            _view =
                (HealthRecordItemDataTableView)
                Enum.Parse(typeof(HealthRecordItemDataTableView), info.GetString("view"));
        }

        /// <summary>
        /// Serializes the data table.
        /// </summary>
        /// 
        /// <param name="info">
        /// The serialization information.
        /// </param>
        /// 
        /// <param name="context">
        /// The serialization context.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        [SecurityCritical]
        [SecurityPermissionAttribute(
            SecurityAction.Demand,
            SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            Validator.ThrowIfArgumentNull(info, "info", "ExceptionSerializationInfoNull");

            base.GetObjectData(info, context);
            info.AddValue("filter", _filter.GetXml());
            info.AddValue("view", _view.ToString());
        }
        #endregion Serialization

        /// <summary> 
        /// Fills in the data table with data from a list of HealthRecordItem.
        /// </summary>
        public void GetData(
            HealthRecordAccessor record, IList<HealthRecordItem> items, int startIndex, int count)
        {
            HealthRecordItemDataTableView effectiveView = 
                this.ApplyEffectiveView(record.Connection);

            IDictionary<Guid, HealthRecordItemTypeDefinition> typeDefDict = 
                ItemTypeManager.GetHealthRecordItemTypeDefinition(_filter.TypeIds, 
                    record.Connection);
            HealthRecordItemTypeDefinition sttTypeDef = 
                typeDefDict.Count == 1 ? typeDefDict[_filter.TypeIds[0]] : null;
            
            bool firstRow = true;
            string transformName = 
                (effectiveView == HealthRecordItemDataTableView.SingleTypeTable) ? "stt" : "mtt";

            for (int i = startIndex; i < items.Count && i < count; ++i)
            {
                HealthRecordItem item = items[i];

                XPathNavigator itemTransformNav;
                IDictionary<string, XmlDocument> transformedXmlData = item.TransformedXmlData;
                if (transformedXmlData.ContainsKey(transformName))
                {
                    itemTransformNav =
                        transformedXmlData[transformName].CreateNavigator().SelectSingleNode(
                            "//data-xml/row");
                }
                else
                {
                    string transform = (sttTypeDef == null) ? 
                        typeDefDict[item.TypeId].TransformItem(transformName, item) : 
                        sttTypeDef.TransformItem(transformName, item);
                
                    itemTransformNav = new XPathDocument(XmlReader.Create(new StringReader(
                        transform))).CreateNavigator();

                    if (!itemTransformNav.MoveToFirstChild())
                    {
                        continue;
                    }
                }

                if (firstRow)
                {
                    SetupColumns(itemTransformNav.Clone());
                    firstRow = false;
                }
                AddRow(itemTransformNav);
            }
        }


        /// <summary> 
        /// Fills in the data table with data from the HealthVault service.
        /// </summary>
        /// 
        /// <param name="recordId"> 
        /// The unique health record identifier to get the data from.
        /// </param>
        /// 
        /// <param name="connection"> 
        /// The connection to the HealthVault service to use.
        /// </param>
        /// 
        /// <remarks>
        /// This method makes a web-method call to the HealthVault service.
        /// </remarks>
        /// 
        /// <exception cref="HealthServiceException">
        /// An error occurred while accessing the HealthVault service.
        /// </exception>
        /// 
        public void GetData(
            Guid recordId,
            ApplicationConnection connection)
        {
            HealthRecordAccessor record = 
                new HealthRecordAccessor(connection, recordId);
            GetData(record);
        }

        /// <summary> 
        /// Fills in the data table with data from the HealthVault service.
        /// </summary>
        /// 
        /// <param name="record"> 
        /// The health record to get the data from.
        /// </param>
        /// 
        /// <remarks>
        /// This method makes a web-method call to the HealthVault service.
        /// </remarks>
        /// 
        /// <exception cref="HealthServiceException">
        /// An error occurred while accessing the HealthVault service.
        /// </exception>
        /// 
        public void GetData(HealthRecordAccessor record)
        {
            GetData(record, 0, Int32.MaxValue);
        }


        /// <summary> 
        /// Fills in the data table with data from the HealthVault service 
        /// starting at the specific index for the count specified.
        /// </summary>
        /// 
        /// <param name="record"> 
        /// The health record to get the data from.
        /// </param>
        /// 
        /// <param name="startIndex">
        /// The index to start retrieving full data from HealthVault.
        /// </param>
        /// 
        /// <param name="count">
        /// The count of full items to retrieve.
        /// </param>
        /// 
        /// <remarks>
        /// This method makes a web-method call to the HealthVault service.
        /// 
        /// The default <see cref="GetData(HealthRecordAccessor)"/> implementation
        /// fills the data with complete information for all items matching
        /// the filter. If the <see cref="HealthRecordItemDataTable"/> is being
        /// bound to a HealthServiceDataGrid or other such control that supports
        /// paging, this may not be the desired result as many calls to 
        /// HealthVault may be required to fetch all the data.  This overload
        /// of GetData allows the caller to specify the index and the count of
        /// the full items to retrieve to match the page that is currently visible.
        /// The <see cref="HealthRecordItemDataTable"/> will be filled with
        /// empty values except for the rows specified.
        /// </remarks>
        /// 
        /// <exception cref="HealthServiceException">
        /// An error occurred while accessing the HealthVault service.
        /// </exception>
        /// 
        public void GetData(
            HealthRecordAccessor record,
            int startIndex,
            int count)
        {
            HealthRecordItemDataTableView effectiveView =
                this.ApplyEffectiveView(record.Connection);

            // Need to specify the type version to ensure that the columns match when the app
            // supports multiple versions.
            if (effectiveView == HealthRecordItemDataTableView.SingleTypeTable)
            {
                for (int index = 0; index < this.Filter.TypeIds.Count; ++index)
                {
                    this.Filter.View.TypeVersionFormat.Add(this.Filter.TypeIds[index]);
                }
            }

            HealthRecordSearcher searcher = record.CreateSearcher();
            searcher.Filters.Add(this.Filter);


            XPathNavigator nav = searcher.GetMatchingItemsRaw();

            _hasData = true;


            XPathNavigator navFiltered =
                nav.SelectSingleNode("//group/filtered");

            if (navFiltered != null)
            {
                _wasFiltered = navFiltered.ValueAsBoolean;
            }

            int numberOfFullThingsToRetrieve = AddRows(nav);

            List<HealthRecordItemKey> partialThingKeys = 
                GetPartialThingKeys(nav);

            int thingIndex = numberOfFullThingsToRetrieve;
            int partialThingsCurrentIndex = 0;

            while (thingIndex < startIndex &&
                    partialThingsCurrentIndex < partialThingKeys.Count)
            {
                AddPartialThingRow(partialThingKeys[partialThingsCurrentIndex++]);
                ++thingIndex;
            }

            while (thingIndex < startIndex + count &&
                   partialThingsCurrentIndex < partialThingKeys.Count)
            {
                nav = 
                    GetPartialThings(
                        record, 
                        partialThingKeys, 
                        partialThingsCurrentIndex,
                        numberOfFullThingsToRetrieve);

                // Note, not all partial things may still exist when doing
                // the next query so AddRows may return less than 
                // numberOfFullThingsToRetrieve. Just skip anything that is
                // missing.
                AddRows(nav);

                partialThingsCurrentIndex += numberOfFullThingsToRetrieve;
                thingIndex += numberOfFullThingsToRetrieve;
            }


            while (partialThingsCurrentIndex < partialThingKeys.Count)
            {
                AddPartialThingRow(partialThingKeys[partialThingsCurrentIndex++]);
                ++thingIndex;
            }
        }
        private bool _isFirstRow = true;

        /// <summary>
        /// Gets a value indicating whether there is any signed health data in the table.
        /// </summary>
        public bool HasSignedData
        {
            get { return _hasSignedData; }
        }
        private bool _hasSignedData;

        /// <summary>
        /// Gets a value indicating whether there is any personal health data in the table.
        /// </summary>
        public bool HasPersonalData
        {
            get { return _hasPersonalData; }
        }
        private bool _hasPersonalData; 

        private int AddRows(XPathNavigator nav)
        {
            int rowsAdded = 0;

            XPathNodeIterator rowIterator = nav.Select("//data-xml/row");
            foreach (XPathNavigator rowNav in rowIterator)
            {
                if (_isFirstRow)
                {
                    SetupColumns(rowNav.Clone());
                    _isFirstRow = false;
                }
                AddRow(rowNav);
                ++rowsAdded;
            }
            return rowsAdded;
        }

        private void AddRow(XPathNavigator rowNav)
        {
            DataRow row = this.NewRow();
            foreach (DataColumn column in this.Columns)
            {
                string columnName = column.ColumnName;
                string columnValue = rowNav.GetAttribute(columnName, String.Empty);

                if (columnValue.Length == 0)
                {
                    row[column.Ordinal] = ItemTypeDataColumn.GetNotPresentValue(column.DataType);
                }
                else
                {
                    row[column.Ordinal] = columnValue;
                    if (!this._hasSignedData && columnName == "wc-issigned" &&
                        columnValue.Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase))
                    {
                        this._hasSignedData = true;
                    }
                    if (!this._hasPersonalData && columnName == "wc-ispersonal" &&
                        columnValue.Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase))
                    {
                        this._hasPersonalData = true;
                    }
                }
            }

            this.Rows.Add(row);
        }

        private void AddPartialThingRow(HealthRecordItemKey key)
        {
            DataRow row = this.NewRow();
            foreach (DataColumn column in this.Columns)
            {
                switch (column.ColumnName)
                {
                    case "wc-id" :
                        row[column.Ordinal] = key.Id.ToString();
                        break;

                    case "wc-version" :
                        row[column.Ordinal] = key.VersionStamp.ToString();
                        break;

                    default :
                        row[column.Ordinal] =
                            ItemTypeDataColumn.GetNotPresentValue(column.DataType);
                        break;
                }
            }
            this.Rows.Add(row);
        }

        private void SetupColumns(XPathNavigator rowNav)
        {
            if (rowNav.MoveToFirstAttribute())
            {
                do
                {
                    if (_displayColumns.ContainsKey(rowNav.Name))
                    {
                        this.Columns.Add(_displayColumns[rowNav.Name]);
                    }
                    else
                    {
                        this.Columns.Add(
                            new DataColumn(rowNav.Name, typeof(String)));
                    }
                } while (rowNav.MoveToNextAttribute());

            }
        }

        private HealthRecordItemDataTableView ApplyEffectiveView(
            ApplicationConnection connection)
        {
            HealthRecordItemDataTableView effectiveView =
                HealthRecordItemDataTableView.MultipleTypeTable;

            HealthRecordItemTypeDefinition typeDefinition = null;

            if (Filter.TypeIds.Count == 1 &&
                View != HealthRecordItemDataTableView.MultipleTypeTable)
            {
                typeDefinition =
                    ItemTypeManager.GetHealthRecordItemTypeDefinition(
                        this.Filter.TypeIds[0],
                        connection);

                if (typeDefinition != null &&
                    typeDefinition.ColumnDefinitions.Count > 0)
                {
                    effectiveView
                        = HealthRecordItemDataTableView.SingleTypeTable;
                    _singleTypeDefinition = typeDefinition;

                    foreach (
                        ItemTypeDataColumn column in
                        typeDefinition.ColumnDefinitions)
                    {
                        _displayColumns.Add(
                            column.ColumnName, column.Clone());
                    }

                    this.Filter.View.TransformsToApply.Clear();
                    this.Filter.View.TransformsToApply.Add("stt");
                }
            }

            if (_singleTypeDefinition == null)
            {
                typeDefinition =
                    ItemTypeManager.GetBaseHealthRecordItemTypeDefinition(
                        connection);

                effectiveView
                    = HealthRecordItemDataTableView.MultipleTypeTable;

                foreach (
                    ItemTypeDataColumn column in
                    typeDefinition.ColumnDefinitions)
                {
                    _displayColumns.Add(column.ColumnName, column.Clone());
                }

                this.Filter.View.TransformsToApply.Clear();
                this.Filter.View.TransformsToApply.Add("mtt");
            }

            return effectiveView;
        }

        private static List<HealthRecordItemKey> GetPartialThingKeys(
            XPathNavigator nav)
        {
            List<HealthRecordItemKey> partialThingKeys
                = new List<HealthRecordItemKey>();

            XPathNodeIterator partialThingIterator =
                nav.Select("//unprocessed-thing-key-info/thing-id");
            foreach (XPathNavigator partialThingNav in partialThingIterator)
            {
                string versionStamp
                    = partialThingNav.GetAttribute(
                        "version-stamp", String.Empty);
                HealthRecordItemKey key
                    = new HealthRecordItemKey(
                        new Guid(partialThingNav.Value),
                        new Guid(versionStamp));
                partialThingKeys.Add(key);
            }
            return partialThingKeys;
        }

        private XPathNavigator GetPartialThings(
            HealthRecordAccessor record,
            IList<HealthRecordItemKey> thingKeys,
            int currentThingKeyIndex,
            int numberOfFullThingsToRetrieve)
        {
            HealthRecordSearcher searcher = record.CreateSearcher();
            HealthRecordFilter filter = new HealthRecordFilter();

            for (int i = currentThingKeyIndex; 
                 i < thingKeys.Count && 
                    i < currentThingKeyIndex + numberOfFullThingsToRetrieve;
                 i++)
            {
                filter.ItemKeys.Add(thingKeys[i]);
            }
            filter.View = this.Filter.View;
            filter.States = this.Filter.States;
            filter.CurrentVersionOnly = this.Filter.CurrentVersionOnly;

            searcher.Filters.Add(filter);

            return searcher.GetMatchingItemsRaw();
        }

        /// <summary>
        /// Gets the definition for the type of items in the data table.
        /// </summary>
        /// 
        /// <remarks>
        /// This value is set only if the single type table view is being
        /// shown.
        /// </remarks>
        /// 
        public HealthRecordItemTypeDefinition SingleTypeDefinition
        {
            get { return _singleTypeDefinition; }
        }
        private HealthRecordItemTypeDefinition _singleTypeDefinition;

        /// <summary>
        /// Gets the display columns for the table.
        /// </summary>
        /// 
        /// <remarks>
        /// This collection will be empty until 
        /// <see cref="GetData(HealthRecordAccessor)"/> or
        /// <see cref="GetData(Guid,ApplicationConnection)"/> is called.
        /// </remarks>
        /// 
        public Dictionary<string, ItemTypeDataColumn> DisplayColumns
        {
            get { return _displayColumns; }
        }
        private Dictionary<string, ItemTypeDataColumn> _displayColumns =
            new Dictionary<string, ItemTypeDataColumn>();


        /// <summary>
        /// Gets the view of the data that the table will show.
        /// </summary>
        /// 
        public HealthRecordItemDataTableView View
        {
            get { return _view; }
        }
        private HealthRecordItemDataTableView _view;

        /// <summary>
        /// Gets or sets the filter to use when getting data from the
        /// health record.
        /// </summary>
        /// 
        public HealthRecordFilter Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                Validator.ThrowIfArgumentNull(value, "Filter", "ArgumentNull");
                _filter = value;
            }
        }
        private HealthRecordFilter _filter = new HealthRecordFilter();

        /// <summary> 
        /// Gets a value indicating whether the data was filtered by the 
        /// HealthVault service.
        /// </summary>
        /// 
        /// <remarks> 
        /// This value is set only after 
        /// <see cref="GetData(HealthRecordAccessor)"/> 
        /// or <see cref="GetData(Guid,ApplicationConnection)"/> is called.
        /// </remarks>
        /// 
        public bool WasFiltered
        {
            get { return _wasFiltered; }
        }
        private bool _wasFiltered;

        /// <summary>
        /// <b>true</b> if the data has been retrieved from the HealthVault service; 
        /// otherwise, <b>false</b>.
        /// </summary>
        /// 
        /// <remarks>
        /// This value is set only after 
        /// <see cref="GetData(HealthRecordAccessor)"/> 
        /// or <see cref="GetData(Guid,ApplicationConnection)"/> is called.
        /// <br/><br/>
        /// This property returns <b>false</b> if there was an error contacting 
        /// the HealthVault service, but returns <b>true</b> if the call was 
        /// made successfully but the filter produced no results.
        /// </remarks>
        /// 
        public bool HasData
        {
            get { return _hasData; }
        }
        private bool _hasData;
    }

}

