// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Security.Permissions;

namespace Microsoft.Health
{


    /// <summary>
    /// The exception indicating that the application requested the HealthVault request be cancelled.
    /// </summary>
    /// 
    /// <remarks>
    /// This exception is raised on the thread in which a <see cref="HealthServiceRequest.Execute()"/>
    /// was called and was subsequently cancelled using <see cref="HealthServiceRequest.CancelRequest()"/>
    /// from another thread.
    /// </remarks>
    /// 
    [Serializable]
    public sealed class HealthServiceRequestCancelledException : HealthServiceException
    {
        /// <summary>
        /// Creates an instance of the <see cref="HealthServiceRequestCancelledException"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This constructor is internal to the SDK. Application developers 
        /// using the SDK should catch instances of this exception instead of 
        /// throwing new exceptions of this type.
        /// </remarks>
        /// 
        internal HealthServiceRequestCancelledException()
            : base(ResourceRetriever.GetResourceString("RequestCancelled"))
        {
        }
        #region FxCop required ctors

        /// <summary>
        /// Creates an instance of the <see cref="HealthServiceRequestCancelledException"/> 
        /// class with the specified message.
        /// </summary>
        /// 
        /// <param name="message">
        /// The error message.
        /// </param>
        /// 
        public HealthServiceRequestCancelledException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="HealthServiceRequestCancelledException"/> 
        /// class with the specified message and inner exception.
        /// </summary>
        /// 
        /// <param name="message">
        /// The error message.
        /// </param>
        /// 
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        /// 
        public HealthServiceRequestCancelledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #region Serialization

        /// <summary>
        /// Creates an instance of the <see cref="HealthServiceRequestCancelledException"/> 
        /// class with the specified serialization information and context.
        /// </summary>
        /// 
        /// <param name="info">
        /// Serialized information about this exception.
        /// </param>
        /// 
        /// <param name="context">
        /// The source and destination of the serialized information.
        /// </param>
        /// 
        private HealthServiceRequestCancelledException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Serializes the exception.
        /// </summary>
        /// 
        /// <param name="info">
        /// The serialization information.
        /// </param>
        /// 
        /// <param name="context">
        /// The source and destination of the serialized information.
        /// </param>
        /// 
        [SecurityCritical]
        [SecurityPermissionAttribute(
            SecurityAction.Demand,
            SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
        #endregion Serialization

        #endregion FxCop required ctors
    }

}
