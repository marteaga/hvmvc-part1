// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health
{
    /// <summary>
    /// Represents data, typically binary data, that extends the XML of the 
    /// health record item.
    /// </summary>
    /// 
    /// <remarks>
    /// Other data for a health record item is usually auxiliary to the 
    /// health record item. This data might either be large or not 
    /// easily or efficiently stored as XML. Examples include 
    /// binary data such as images, videos, or binary files.
    /// </remarks>
    /// 
    public class OtherItemData
    {
        /// <summary>
        /// Creates a new instance of the <see cref="OtherItemData"/> class 
        /// using default values.
        /// </summary>
        /// 
        public OtherItemData()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="OtherItemData"/> class 
        /// with the specified data, encoding, and content type.
        /// </summary>
        /// 
        /// <param name="data">
        /// The data to store in the other data section of the health record
        /// item.
        /// </param>
        /// 
        /// <param name="contentEncoding">
        /// The type of encoding that was done on the data. Usually this will
        /// be "base64" but other encodings are acceptable.
        /// </param>
        /// 
        /// <param name="contentType">
        /// The MIME-content type of the data.
        /// </param>
        /// 
        public OtherItemData(
            string data, 
            string contentEncoding, 
            string contentType)
        {
            _data = data;
            _otherDataEncoding = contentEncoding;
            _otherDataType = contentType;
        }
        
        /// <summary>
        /// Populate the <see cref="OtherItemData"/> instance from the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        ///
        /// <param name="otherDataNavigator">
        /// The <see cref="XPathNavigator"/> to read the data from.
        /// </param>
        public void ParseXml(XPathNavigator otherDataNavigator)
        {
            _data = otherDataNavigator.Value;

            _otherDataType =
                otherDataNavigator.GetAttribute("content-type", String.Empty);

            _otherDataEncoding =
                otherDataNavigator.GetAttribute("content-encoding", String.Empty);
        }

        /// <summary>
        /// Writes the other data section to the passed-in XmlWriter.
        /// </summary>
        ///
        /// <remarks>
        /// Classes that override this method must call the base method to do the actual conversion. 
        /// </remarks>
        /// <param name="writer">
        /// The XmlWriter to write the data to.
        /// </param>
        /// 
        public virtual void WriteXml(XmlWriter writer)
        {
            // <data-other>
            writer.WriteStartElement("data-other");

            if (!String.IsNullOrEmpty(this.ContentType))
            {
                writer.WriteAttributeString(
                    "content-type",
                    this.ContentType);
            }

            if (!String.IsNullOrEmpty(this.ContentEncoding))
            {
                writer.WriteAttributeString(
                    "content-encoding",
                    this.ContentEncoding);
            }

            writer.WriteString(_data);

            // </data-other>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// 
        /// <returns>
        /// A string representing the data.
        /// </returns>
        /// 
        /// <remarks>
        /// Binary data can be base64 encoded to be stored as a string.
        /// <br/><br/>
        /// The <see cref="ContentType"/> and <see cref="ContentEncoding"/>
        /// properties should be set to appropriate values for the data.
        /// </remarks>
        /// 
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }
        private string _data;

        /// <summary>
        /// Gets or sets the MIME content type of other data for the health
        /// record item.
        /// </summary>
        /// 
        /// <returns>
        /// A string representing the content type.
        /// </returns>
        /// 
        public string ContentType
        {
            get { return _otherDataType; }
            set { _otherDataType = value; }
        }
        private string _otherDataType;

        /// <summary>
        /// Gets or sets the content encoding of other data for the health
        /// record item.
        /// </summary>
        /// 
        /// <returns>
        /// A string representing the encoding.
        /// </returns>
        /// 
        /// <remarks>
        /// In most cases, this should be set to base64, but other 
        /// encodings are acceptable, such as XML.
        /// </remarks>
        /// 
        public string ContentEncoding
        {
            get { return _otherDataEncoding; }
            set { _otherDataEncoding = value; }
        }
        private string _otherDataEncoding;
    }
}
