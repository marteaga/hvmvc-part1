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
    /// Contains error information for a response that has a code other
    /// than <see cref="HealthServiceStatusCode.Ok"/>.
    /// </summary>
    /// 
    [Serializable]
    public class HealthServiceResponseError
    {
        /// <summary>
        /// Gets the error message. 
        /// </summary>
        /// 
        /// <value>
        /// A string representing the error message.
        /// </value>
        /// 
        /// <remarks>
        /// The message contains localized text of why the request failed. 
        /// This text should be added to application context information
        /// and suggestions of what to do before displaying it to the user.
        /// </remarks>
        /// 
        public string Message
        {
            get { return _message; }
            internal set { _message = value; }
        }
        private string _message;

        /// <summary>
        /// Gets the context of the server in which the error occurred.
        /// </summary>
        /// 
        /// <value>
        /// A <see cref="HealthServiceErrorContext"/> representing the server context.
        /// </value>
        /// 
        /// <remarks>
        /// This information is available only when the service is configured
        /// in debugging mode. In all other cases, this property returns 
        /// <b>null</b>.
        /// </remarks>
        /// 
        internal HealthServiceErrorContext Context
        {
            get { return _context; }
            set { _context = value; }
        }
        [NonSerialized]
        private HealthServiceErrorContext _context;

        /// <summary>
        /// Gets the additional information specific to the method failure.
        /// </summary>
        /// 
        /// <value>
        /// A string representing the additional error information.
        /// </value>
        /// 
        /// <remarks>
        /// The text contains specific actionable information related to the failed request.
        /// It may be used in determining possible actions to circumvent the error condition
        /// before displaying an error to the user.
        /// </remarks>
        /// 
        public string ErrorInfo
        {
            get { return _errorInfo; }
            internal set { _errorInfo = value; }
        }
        private string _errorInfo;

        /// <summary>
        /// Gets the string representation of the <see cref="HealthServiceErrorContext"/> 
        /// object.
        /// </summary>
        /// 
        /// <returns> 
        /// A string representing the contents of the <see cref="HealthServiceErrorContext"/> 
        /// object.
        /// </returns>
        /// 
        public override string ToString()
        {
            string result =
                String.Join(" ",
                    new string[] {
                            GetType().ToString(),
                            _message,
                    });
            return result;
        }
    }
}
