// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health
{
    /// <summary>
    /// Contains the response information from the HealthVault service after
    /// processing a request.
    /// </summary>
    /// 
    public class HealthServiceResponseData
    {
        //
        // Prevents creation of an instance.
        //
        internal HealthServiceResponseData() { }

        /// <summary>
        /// Gets the status code of the response.
        /// </summary>
        /// 
        /// <value>
        /// A <see cref="HealthServiceStatusCode"/> value.
        /// </value>
        /// 
        /// <remarks>
        /// Any code other than <see cref="HealthServiceStatusCode.Ok"/> indicates
        /// an error. Use the <see cref="HealthServiceResponseData.Error"/> property
        /// to get more information about the error.
        /// </remarks>
        /// 
        public HealthServiceStatusCode Code
        {
            get
            {
                return
                    HealthServiceStatusCodeManager.GetStatusCode(_codeId);
            }
        }

        /// <summary>
        /// Gets the integer identifier of the status code in the HealthVault
        /// response.
        /// </summary>
        /// 
        /// <value>
        /// An integer representing the status code ID.
        /// </value>
        /// 
        /// <remarks>
        /// Use this property when the SDK is out of sync with the HealthVault 
        /// status code set. You can look up the actual integer value of the 
        /// status code for further investigation.
        /// </remarks>
        /// 
        public int CodeId
        {
            get { return _codeId; }
            internal set { _codeId = value; }
        }

        private int _codeId;

        /// <summary>
        /// Gets the information about an error that occurred while processing
        /// the request.
        /// </summary>
        /// 
        /// <value>
        /// A <see cref="HealthServiceResponseError"/>.
        /// </value>
        /// <remarks>
        /// This property is <b>null</b> if 
        /// <see cref="HealthServiceResponseData.Code"/> returns 
        /// <see cref="HealthServiceStatusCode.Ok"/>.
        /// </remarks>
        /// 
        public HealthServiceResponseError Error
        {
            get { return _error; }
            internal set { _error = value; }
        }
        private HealthServiceResponseError _error;

        /// <summary>
        /// Gets the info section of the response XML.
        /// </summary>
        /// 
        public XPathNavigator InfoNavigator
        {
            get
            {
                if (_infoNavigator == null && InfoReader != null)
                {
                    _infoNavigator = new XPathDocument(NewInfoReader).CreateNavigator();
                    _infoNavigator.MoveToFirstChild();
                }
                return _infoNavigator;
            }
            internal set { _infoNavigator = value; }
        }
        XPathNavigator _infoNavigator;

        /// <summary>
        /// Gets the info section of the response XML.
        /// </summary>
        /// 
        public XmlReader InfoReader
        {
            get
            {
                if (_infoReader == null)
                {
                    _infoReader = NewInfoReader;
                }
                return _infoReader;
            }
            internal set { _infoReader = value; }
        }
        XmlReader _infoReader;

        internal XmlReader NewInfoReader
        {
            get
            {
                XmlReader reader = null;
                if (_responseText.Array != null && _responseText.Count > 0)
                {
                    MemoryStream ms = 
                        new MemoryStream(
                            _responseText.Array, 
                            _responseText.Offset, 
                            _responseText.Count, 
                            false);
                    
                    reader = XmlReader.Create(ms, SDKHelper.XmlReaderSettings);
                    reader.NameTable.Add("wc");
                    reader.ReadToFollowing("wc:info");
                }
                return reader;
            }
        }

        /// <summary>
        /// Gets or sets the cached response results text (UTF8 encoded).
        /// </summary>
        /// 
        internal ArraySegment<Byte> ResponseText
        {
            get { return _responseText; }
            set { _responseText = value; }
        }
        private ArraySegment<Byte> _responseText;
    }
}
