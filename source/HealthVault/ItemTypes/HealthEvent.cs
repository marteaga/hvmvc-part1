// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health.ItemTypes
{
    /// <summary>
    /// A health event that occured for the owner of the record.
    /// </summary>
    ///
    /// <remarks>
    /// A health event is a health-related occurence for the owner of the record.  For 
    /// children, it might be used to record the date that the child first crawls.
    /// For adults, it might be used to record the date of an accident or progress through a rehabilitation.
    /// </remarks>
    ///
    public class HealthEvent : HealthRecordItem
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HealthEvent"/> class with default values.
        /// </summary>
        ///
        /// <remarks>
        /// This item is not added to the health record until the
        /// <see cref="Microsoft.Health.HealthRecordAccessor.NewItem(HealthRecordItem)"/> method
        /// is called.
        /// </remarks>
        ///
        public HealthEvent()
            : base(TypeId)
        {
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="HealthEvent"/> class
        /// specifying mandatory values.
        /// </summary>
        ///
        /// <remarks>
        /// This item is not added to the health record until the
        /// <see cref="Microsoft.Health.HealthRecordAccessor.NewItem(HealthRecordItem)"/> method
        /// is called.
        /// </remarks>
        ///
        /// <param name="when">
        /// The date and time the event occurred.
        /// </param>
        /// <param name="eventValue">
        /// The name of the health event.
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="when"/> is <b>null</b>.
        /// If <paramref name="eventValue"/> is <b>null</b>.
        /// </exception>
        ///
        public HealthEvent(
            ApproximateDateTime when,
            CodableValue eventValue)
        : base(TypeId)
        {
            When = when;
            Event = eventValue;
        }
        
        /// <summary>
        /// Retrieves the unique identifier for the HealthEvent type.
        /// </summary>
        ///
        /// <value>
        /// A GUID.
        /// </value>
        ///
        public static new readonly Guid TypeId =
            new Guid("1572af76-1653-4c39-9683-9f9ca6584ba3");
        
        /// <summary>
        /// Populates this <see cref="HealthEvent"/> instance from the data in the specified XML.
        /// </summary>
        ///
        /// <param name="typeSpecificXml">
        /// The XML to get the HealthEvent data from.
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="typeSpecificXml"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="InvalidOperationException">
        /// If the first node in <paramref name="typeSpecificXml"/> is not
        /// a HealthEvent node.
        /// </exception>
        /// 
        protected override void ParseXml(IXPathNavigable typeSpecificXml)
        {
            Validator.ThrowIfArgumentNull(typeSpecificXml, "typeSpecificXml", "ParseXmlNavNull");
            
            XPathNavigator itemNav =
                typeSpecificXml.CreateNavigator().SelectSingleNode("health-event");

            Validator.ThrowInvalidIfNull(itemNav, "HealthEventUnexpectedNode");            
            
            _when = new ApproximateDateTime();
            _when.ParseXml(itemNav.SelectSingleNode("when"));
            _event = new CodableValue();
            _event.ParseXml(itemNav.SelectSingleNode("event"));
            _category = XPathHelper.GetOptNavValue<CodableValue>(itemNav, "category");
        }
        
        /// <summary>
        /// Writes the XML representation of the HealthEvent into
        /// the specified XML writer.
        /// </summary>
        ///
        /// <param name="writer">
        /// The XML writer into which the HealthEvent should be
        /// written.
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="writer"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="HealthRecordItemSerializationException">
        /// If <see cref="When"/> is <b>null</b>.
        /// If <see cref="Event"/> is <b>null</b>.
        /// </exception>
        ///
        public override void WriteXml(XmlWriter writer)
        {
            Validator.ThrowIfWriterNull(writer);

            Validator.ThrowSerializationIfNull(_when, "WhenNullValue");
            Validator.ThrowSerializationIfNull(_event, "EventNullValue");
            
            writer.WriteStartElement("health-event");
            
            _when.WriteXml("when", writer);
            _event.WriteXml("event", writer);
            XmlWriterHelper.WriteOpt<CodableValue>(writer, "category", _category);
            writer.WriteEndElement();
        }
        
        /// <summary>
        /// Gets or sets the date and time the event occurred.
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="value"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        public ApproximateDateTime When
        {
            get
            {
                return _when;
            }
            
            set
            {
                Validator.ThrowIfArgumentNull(value, "value", "WhenNullValue");
                
                _when = value;
            }
        }
        
        private ApproximateDateTime _when;
        
        /// <summary>
        /// Gets or sets the name of the health event.
        /// </summary>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="value"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "FXCop thinks that CodableValue is a collection, so it throws this error.")]
        public CodableValue Event
        {
            get
            {
                return _event;
            }
            
            set
            {
                Validator.ThrowIfArgumentNull(value, "value", "EventNullValue");
                
                _event = value;
            }
        }
        
        private CodableValue _event;
        
        /// <summary>
        /// Gets or sets the category of the health event.
        /// </summary>
        /// 
        /// <remarks>
        /// The category can be used to group related events together. For example, 'pediatric'.
        /// If there is no information about category the value should be set to <b>null</b>.
        /// </remarks>
        ///
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "FXCop thinks that CodableValue is a collection, so it throws this error.")]
        public CodableValue Category
        {
            get
            {
                return _category;
            }
            
            set
            {
                _category = value;
            }
        }
        
        private CodableValue _category;
        
        /// <summary>
        /// Gets a string representation of the HealthEvent.
        /// </summary>
        /// 
        /// <returns>
        /// A string representation of the HealthEvent.
        /// </returns>
        ///
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.Append(_event.Text);

            if (_category != null)
            {
                result.Append(ResourceRetriever.GetSpace("resources"));
                result.Append(ResourceRetriever.GetResourceString("OpenParen"));
                result.Append(_category.Text);
                result.Append(ResourceRetriever.GetResourceString("CloseParen"));
            }

            return result.ToString();
        }
    }
}
