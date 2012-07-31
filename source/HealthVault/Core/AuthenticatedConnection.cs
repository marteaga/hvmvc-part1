// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Health.Authentication;

namespace Microsoft.Health
{

    /// <summary>
    /// Represents an authenticated interface to the HealthVault service. Most
    /// operations performed against the service require authentication.
    /// </summary>
    /// 
    /// <remarks>
    /// You must connect to the HealthVault service to access its
    /// web methods. This class does not maintain
    /// an open connection to the service, but uses XML over HTTP to 
    /// make requests and receive responses from the service. The connection
    /// only maintains the data necessary for the request.
    /// <br/><br/>
    /// An authenticated connection takes the user name and password, 
    /// authenticates them against the HealthVault service, and then stores an 
    /// authentication token which is then passed to the service on each 
    /// subsequent request. An authenticated connection is required for 
    /// accessing a person's health record. 
    /// <br/><br/>
    /// For operations that do not require user or application authentication, 
    /// use the <see cref="AnonymousConnection"/> class.
    /// </remarks>
    /// 
    public class AuthenticatedConnection : ApplicationConnection
    {
        #region ctors

        /// <summary>
        /// Uses the specified Live ID ticket to authenticate the user with HealthVault.
        /// </summary>
        /// 
        /// <param name="liveIdTicket">
        /// A Live ID ticket that was retrieved using the Live ID client APIs (IDCRL).
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="AuthenticatedConnection"/> to HealthVault for the user specified in the
        /// <paramref name="liveIdTicket"/>.
        /// </returns>
        /// 
        /// <remarks>
        /// The calling ApplicationId, HealthServiceUrl, and ShellUrl are retrieved from the
        /// app.config file for the application.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="liveIdTicket"/> or is <b>null</b> or empty.
        /// </exception>
        /// 
        /// <exception cref ="HealthServiceException">
        /// If verification of the passport ticket fails or there is a failure
        /// in finding a HealthVault account for the specified <paramref name="liveIdTicket"/>.
        /// </exception>
        /// 
        public static AuthenticatedConnection LogOn(string liveIdTicket)
        {
            return LogOn(liveIdTicket, false);
        }

        /// <summary>
        /// Uses the specified Live ID ticket to authenticate the user with HealthVault.
        /// </summary>
        /// 
        /// <param name="liveIdTicket">
        /// A Live ID ticket that was retrieved using the Live ID client APIs (IDCRL).
        /// </param>
        /// 
        /// <param name="isMra">
        /// True if the application is a multi-record application, or false otherwise. Multi-record
        /// applications can work with many user records at one time and does not rely on the
        /// selected record when performing operations.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="AuthenticatedConnection"/> to HealthVault for the user specified in the
        /// <paramref name="liveIdTicket"/>.
        /// </returns>
        /// 
        /// <remarks>
        /// The calling ApplicationId, HealthServiceUrl, and ShellUrl are retrieved from the
        /// app.config file for the application.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="liveIdTicket"/> or is <b>null</b> or empty.
        /// </exception>
        /// 
        /// <exception cref ="HealthServiceException">
        /// If verification of the passport ticket fails or there is a failure
        /// in finding a HealthVault account for the specified <paramref name="liveIdTicket"/>.
        /// </exception>
        /// 
        public static AuthenticatedConnection LogOn(
            string liveIdTicket,
            bool isMra)
        {
            return LogOn(liveIdTicket, isMra, null);
        }

        /// <summary>
        /// Uses the specified Live ID ticket to authenticate the user with HealthVault.
        /// </summary>
        /// 
        /// <param name="liveIdTicket">
        /// A Live ID ticket that was retrieved using the IDCRL.
        /// </param>
        /// 
        /// <param name="isMra">
        /// True if the application is a multi-record application, or false otherwise. Multi-record
        /// applications can work with many user records at one time and does not rely on the
        /// selected record when performing operations.
        /// </param>
        /// 
        /// <param name="cancelTrigger">
        /// If the event gets triggered the log on request will be cancelled resulting in an
        /// <see cref="HealthServiceRequestCancelledException"/>.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="AuthenticatedConnection"/> to HealthVault for the user specified in the
        /// <paramref name="liveIdTicket"/>.
        /// </returns>
        /// 
        /// <remarks>
        /// The calling ApplicationId, HealthServiceUrl, and ShellUrl are retrieved from the
        /// app.config file for the application.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="liveIdTicket"/> or is <b>null</b> or empty.
        /// </exception>
        /// 
        /// <exception cref="WebException">
        /// If the request to the HealthVault Shell to verify the <paramref name="liveIdTicket"/>
        /// fails.
        /// </exception>
        /// 
        /// <exception cref="HealthServiceAccessDeniedException">
        /// If the user specified in the <paramref name="liveIdTicket"/> could not be authenticated
        /// to HealthVault.
        /// </exception>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// If either the application identifier, platform URL or the shell URL is
        /// missing from the configuration.
        /// </exception>
        /// 
        public static AuthenticatedConnection LogOn(
            string liveIdTicket,
            bool isMra,
            ManualResetEvent cancelTrigger)
        {
            Guid appId = HealthApplicationConfiguration.Current.ApplicationId;
            Uri healthServiceUrl = HealthApplicationConfiguration.Current.HealthVaultMethodUrl;
            Uri shellUrl = HealthApplicationConfiguration.Current.HealthVaultShellUrl;

            if (appId == Guid.Empty)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationIdConfiguration");
            }
            
            if (healthServiceUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            if ((shellUrl == null)||(String.IsNullOrEmpty(shellUrl.ToString())))
            {
                throw Validator.InvalidConfigurationException("InvalidShellUrlConfiguration");
            }

            return LogOn(liveIdTicket, isMra, false, cancelTrigger, appId, shellUrl, healthServiceUrl);
        }

        
        /// <summary>
        /// Uses the specified Live ID ticket to authenticate the user with HealthVault.
        /// </summary>
        /// 
        /// <param name="liveIdTicket">
        /// A Live ID ticket that was retrieved using the IDCRL.
        /// </param>
        /// 
        /// <param name="isMra">
        /// True if the application is a multi-record application, or false otherwise. Multi-record
        /// applications can work with many user records at one time and does not rely on the
        /// selected record when performing operations.
        /// </param>
        /// 
        /// <param name="cancelTrigger">
        /// If the event gets triggered the log on request will be cancelled resulting in an
        /// <see cref="HealthServiceRequestCancelledException"/>.
        /// </param>
        /// 
        /// <param name="applicationId">
        /// The unique HealthVault application identifier that the user is being logged into.
        /// </param>
        /// 
        /// <param name="healthServiceUrl">
        /// The URL of the HealthVault service. Note, this must include the web service handler, 
        /// "wildcat.ashx".
        /// </param>
        /// 
        /// <param name="shellUrl">
        /// The HealthVault Shell redirector URL. This is used to to verify the Live ID ticket before 
        /// authenticating the user to HealthVault.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="AuthenticatedConnection"/> to HealthVault for the user specified in the
        /// <paramref name="liveIdTicket"/>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="liveIdTicket"/> or is <b>null</b> or empty, or
        /// <paramref name="applicationId"/> is <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="shellUrl"/> or <paramref name="healthServiceUrl"/> is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="WebException">
        /// If the request to the HealthVault Shell to verify the <paramref name="liveIdTicket"/>
        /// fails.
        /// </exception>
        /// 
        /// <exception cref="HealthServiceAccessDeniedException">
        /// If the user specified in the <paramref name="liveIdTicket"/> could not be authenticated
        /// to HealthVault.
        /// </exception>
        /// 
        public static AuthenticatedConnection LogOn(
            string liveIdTicket,
            bool isMra,
            ManualResetEvent cancelTrigger,
            Guid applicationId,
            Uri shellUrl,
            Uri healthServiceUrl)
        {
            return LogOn(liveIdTicket, isMra, false, cancelTrigger, applicationId,
                shellUrl, healthServiceUrl);
        }

        /// <summary>
        /// Uses the specified Live ID ticket to authenticate the user with HealthVault.
        /// </summary>
        /// 
        /// <param name="liveIdTicket">
        /// A Live ID ticket that was retrieved using the IDCRL.
        /// </param>
        /// 
        /// <param name="isMra">
        /// True if the application is a multi-record application, or false otherwise. Multi-record
        /// applications can work with many user records at one time and does not rely on the
        /// selected record when performing operations.
        /// </param>
        /// 
        /// <param name="isPersistent">
        /// True if creating a persistent token, or false otherwise.  Persistent connections
        /// remain valid for the duration specified in the application's configuration within
        /// HealthVault.  Typically, persistent tokens are valid for up to one year.
        /// </param>
        /// 
        /// <param name="cancelTrigger">
        /// If the event gets triggered the log on request will be cancelled resulting in an
        /// <see cref="HealthServiceRequestCancelledException"/>.
        /// </param>
        /// 
        /// <param name="applicationId">
        /// The unique HealthVault application identifier that the user is being logged into.
        /// </param>
        /// 
        /// <param name="healthServiceUrl">
        /// The URL of the HealthVault service. Note, this must include the web service handler, 
        /// "wildcat.ashx".
        /// </param>
        /// 
        /// <param name="shellUrl">
        /// The HealthVault Shell redirector URL. This is used to to verify the Live ID ticket before 
        /// authenticating the user to HealthVault.
        /// </param>
        /// 
        /// <returns>
        /// An <see cref="AuthenticatedConnection"/> to HealthVault for the user specified in the
        /// <paramref name="liveIdTicket"/>.
        /// </returns>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="liveIdTicket"/> or is <b>null</b> or empty, or
        /// <paramref name="applicationId"/> is <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="shellUrl"/> or <paramref name="healthServiceUrl"/> is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="WebException">
        /// If the request to the HealthVault Shell to verify the <paramref name="liveIdTicket"/>
        /// fails.
        /// </exception>
        /// 
        /// <exception cref="HealthServiceAccessDeniedException">
        /// If the user specified in the <paramref name="liveIdTicket"/> could not be authenticated
        /// to HealthVault.
        /// </exception>
        /// 
        public static AuthenticatedConnection LogOn(
            string liveIdTicket,
            bool isMra,
            bool isPersistent,
            ManualResetEvent cancelTrigger,
            Guid applicationId,
            Uri shellUrl,
            Uri healthServiceUrl)
        {
            Validator.ThrowIfStringNullOrEmpty(liveIdTicket, "liveIdTicket");
            Validator.ThrowArgumentExceptionIf(
                applicationId == Guid.Empty,
                "applicationId",
                "AuthenticatedConnectionLogonAppId");

            Validator.ThrowIfArgumentNull(shellUrl, "shellUrl", "StringNull");
            Validator.ThrowIfArgumentNull(healthServiceUrl, "healthServiceUrl", "StringNull");

            PassportCredential cred = PassportCredential.Create(applicationId);

            // Verify with Shell to produce authToken

            string authToken = 
                VerifyTicketWithShell(
                    applicationId,
                    liveIdTicket,
                    cred.SharedSecret,
                    isMra,
                    isPersistent,
                    cancelTrigger,
                    shellUrl);

            cred.AuthenticationToken = authToken;

            return new AuthenticatedConnection(applicationId, healthServiceUrl, cred);
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="appId"></param>
        /// <param name="ticket"></param>
        /// <param name="sharedSecret"></param>
        /// 
        /// <param name="isMra">
        /// True if the application is a multi-record application, or false otherwise. Multi-record
        /// applications can work with many user records at one time and does not rely on the
        /// selected record when performing operations.
        /// </param>
        /// 
        /// <param name="isPersistent">
        /// True if creating a persistent token, or false otherwise.  Persistent connections
        /// remain valid for the duration specified in the application's configuration within
        /// HealthVault.  Typically, persistent tokens are valid for up to one year.
        /// </param>
        /// 
        /// <param name="cancelTrigger"></param>
        /// <param name="shellUrl"></param>
        /// 
        /// <returns></returns>
        /// 
        /// <exception cref ="HealthServiceException">
        /// If the request results in an error being returned from the Shell.  The 
        /// <see cref="HealthServiceException.ErrorCode"/> can give more details about the error
        /// that occurred.
        /// </exception>
        /// 
        /// <exception cref="HealthRecordAuthorizationRequiredException">
        /// If the user could not be logged in because they have not authorized the application to
        /// a health record. The application should direct the user to the APPAUTH target of the
        /// Shell.
        /// </exception>
        /// 
        /// <exception cref="HealthRecordAuthorizationNotPossible">
        /// If the user does not have access to a health record that meets the minimum authorization
        /// requirements of the application. The user will need to request more access from the 
        /// custodian to use this application.
        /// </exception>
        /// 
        /// <exception cref="HealthRecordReauthorizationRequired">
        /// If the user had authorized a health record for this application but the application
        /// changed its required base authorizations such that the user must reauthorize the
        /// application. The application should direct the user to the APPAUTH target of the Shell.
        /// </exception>
        /// 
        private static string VerifyTicketWithShell(
            Guid appId,
            string ticket,
            CryptoHash sharedSecret,
            bool isMra,
            bool isPersistent,
            ManualResetEvent cancelTrigger,
            Uri shellUrl)
        {
            ShellResponseHandler handler = new ShellResponseHandler();
            EasyWebRequest request = new EasyWebRequest();
            request.ForceAsyncRequest = true;
            request.RequestCompressionMethod = String.Empty;

            if (cancelTrigger != null)
            {
                request.RequestCancelTrigger = cancelTrigger;
            }

            // Add the Authorization header for passport verification
            string ticketHeader =
                Convert.ToBase64String(
                    new UTF8Encoding().GetBytes("WLID1.0 t=" + ticket));
            request.Headers.Add(
                "LiveIdTicket", 
                ticketHeader);

            // Add the shared secret header for creating the session token
            string sharedSecretHeader =
                Convert.ToBase64String(
                    new UTF8Encoding().GetBytes(sharedSecret.GetInfoXml()));
            request.Headers.Add(
                "SharedSecret",
                sharedSecretHeader);

            string authToken = null;
            try
            {
                Uri liveIdTicketVerifierUrl =
                    new Uri(
                        shellUrl,
                        "redirect.aspx?target=verifyliveid&targetqs=" +
                            HttpUtility.UrlEncode(
                                "?appid=" + appId + "&ismra=" + SDKHelper.XmlFromBool(isMra) +
                                "&persistwctoken=" + SDKHelper.XmlFromBool(isPersistent)));
                
                request.Fetch(liveIdTicketVerifierUrl,handler);

                ApplicationRecordAuthorizationAction action =
                    ApplicationRecordAuthorizationAction.Unknown;

                XmlReader infoReader = handler.Response.InfoReader;
                if (SDKHelper.ReadUntil(infoReader, "token"))
                { 
                    if (infoReader.MoveToAttribute("app-record-auth-action"))
                    {
                        try
                        {
                            action =
                                (ApplicationRecordAuthorizationAction)Enum.Parse(
                                    typeof(ApplicationRecordAuthorizationAction),
                                    infoReader.Value);
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                }

                switch (action)
                {
                    case ApplicationRecordAuthorizationAction.NoActionRequired :
                        break;

                    case ApplicationRecordAuthorizationAction.AuthorizationRequired :
                        throw new HealthRecordAuthorizationRequiredException();

                    case ApplicationRecordAuthorizationAction.ReauthorizationNotPossible :
                        throw new HealthRecordAuthorizationNotPossible();

                    case ApplicationRecordAuthorizationAction.ReauthorizationRequired :
                        throw new HealthRecordReauthorizationRequired();

                    default :
                        throw new HealthServiceException(
                            HealthServiceStatusCode.RecordAuthorizationFailure);
                }

                infoReader.MoveToElement(); 
                authToken = infoReader.ReadElementContentAsString();
            }
            catch (WebException webException)
            {
                if (((HttpWebResponse)webException.Response).StatusCode ==
                    HttpStatusCode.Forbidden)
                {
                    throw new HealthServiceAccessDeniedException(
                        webException.Message,
                        webException);
                }
                throw;
            }
            finally
            {
                if (request != null)
                {
                    request.Dispose();
                    request = null;
                }
            }
            return authToken;
        }

        private class ShellResponseHandler : IEasyWebResponseHandler
        {
            public void HandleResponseStream(Stream stream)
            {
                _response = HealthServiceRequest.HandleResponseStreamResult(stream);
            }

            public HealthServiceResponseData Response
            {
                get { return _response; }
            }
            private HealthServiceResponseData _response;
        }

        internal AuthenticatedConnection()
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="AuthenticatedConnection"/>
        /// class with the specified credential.
        /// </summary>
        /// 
        /// <param name="credential">
        /// The credential of the user to authenticate for access to HealthVault.
        /// </param>
        /// <remarks>
        /// The base class, <see cref="ApplicationConnection"/>, obtains an 
        /// application identifier and a service URL from the configuration file.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="credential"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The web or application configuration file does not contain 
        /// configuration entries for "ApplicationID" or "HealthServiceUrl".
        /// </exception>
        /// 
        public AuthenticatedConnection(
            Credential credential)
            : base()
        {
            Validator.ThrowIfArgumentNull(credential, "credential", "CtorUsernameNullOrEmpty");
            Credential = credential;
        }

        /// <summary>
        /// Creates an instance of the <see cref="AuthenticatedConnection"/>
        /// class with the specified application identifier, and 
        /// credential.
        /// </summary>
        /// 
        /// <param name="callingApplicationId">
        /// The HealthVault application identifier.
        /// </param>
        /// 
        /// <param name="credential">
        /// The credential of the user to authenticate for access to HealthVault.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="credential"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        public AuthenticatedConnection(
            Guid callingApplicationId,
            Credential credential)
            : base(callingApplicationId)
        {
            Validator.ThrowIfArgumentNull(credential, "credential", "CtorUsernameNullOrEmpty");
            Credential = credential;
        }


        /// <summary>
        /// Creates an instance of the <see cref="AuthenticatedConnection"/>
        /// class with the specified application identifier, URL, and 
        /// credential.
        /// </summary>
        /// 
        /// <param name="callingApplicationId">
        /// The HealthVault application identifier.
        /// </param>
        /// 
        /// <param name="healthServiceUrl">
        /// The URL of the HealthVault web service.
        /// </param>
        /// 
        /// <param name="credential">
        /// The credential of the user to authenticate for access to HealthVault.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="healthServiceUrl"/> or 
        /// <paramref name="credential"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        public AuthenticatedConnection(
            Guid callingApplicationId,
            Uri healthServiceUrl,
            Credential credential)
            : base(
                callingApplicationId,
                healthServiceUrl)
        {
            Validator.ThrowIfArgumentNull(credential, "credential", "CtorUsernameNullOrEmpty");
            Credential = credential;
        }

        /// <summary>
        /// Creates an instance of the <see cref="AuthenticatedConnection"/>
        /// class with the specified application identifier, a string 
        /// representing the URL, and credential.
        /// </summary>
        /// 
        /// <param name="callingApplicationId">
        /// The HealthVault application identifier.
        /// </param>
        /// 
        /// <param name="healthServiceUrl">
        /// A string representing the URL of the HealthVault web service.
        /// </param>
        /// 
        /// <param name="credential">
        /// The credential of the user to authenticate for access to HealthVault.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="healthServiceUrl"/> or 
        /// <paramref name="credential"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        public AuthenticatedConnection(
            Guid callingApplicationId,
            string healthServiceUrl,
            Credential credential)
            : this(
                callingApplicationId,
                new Uri(healthServiceUrl),
                credential)
        {
        }

        #endregion ctors

        #region public methods

        /// <summary>
        /// Logs on to the HealthVault service using the username and password.
        /// </summary>
        /// 
        /// <remarks>
        /// It is not necessary to explicitly call this method before calling
        /// any of the methods that access the service. Those methods will 
        /// call this method if the user has not already been authenticated. 
        /// This method is provided as a convenience to allow for separate 
        /// error handling for authorization errors.
        /// </remarks>
        /// 
        /// <exception cref="SecurityException">
        /// The caller does not have permission to connect to the requested
        /// URI or a URI to which the request is redirected.
        /// </exception>
        /// 
        /// <exception cref="UriFormatException">
        /// The authorization URL specified to the constructor is not a 
        /// valid URI.
        /// </exception>
        /// 
        /// <exception cref="HealthServiceException">
        /// The authorization was not returned in the response from the 
        /// server.
        /// </exception>
        /// 
        public void Authenticate()
        {
            Credential.AuthenticateIfRequired(
                this,
                this.ApplicationId);
        }

        #region Impersonation
        /// <summary>
        /// Sets the identifier for the person being impersonated.
        /// </summary>
        /// 
        /// <param name="targetPersonId">
        /// The unique identifier for the person to impersonate.
        /// </param>
        /// 
        /// <remarks>
        /// Impersonation occurs when the authenticated person wants to make
        /// a request to the HealthVault service on behalf of another person. This
        /// should not occur in most applications.
        /// <br/><br/>
        /// The authenticated person must have the rights to call the requested
        /// method for the person being impersonated. If that right exists, all
        /// security processing occurs using the impersonated person's 
        /// identity. If the right does not exist, the caller receives an
        /// <see cref="HealthServiceAccessDeniedException"/> upon the first 
        /// invocation of a method that accesses the HealthVault service.
        /// <br/><br/>
        /// To start impersonating, call the <see cref="Impersonate"/> method.
        /// To stop impersonating, call the <see cref="StopImpersonating"/>
        /// method.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="targetPersonId"/> parameter is Guid.Empty.
        /// </exception>
        /// 
        /// <seealso cref="StopImpersonating"/>
        /// <seealso cref="IsImpersonating"/>
        /// 
        public void Impersonate(Guid targetPersonId)
        {
            Validator.ThrowArgumentExceptionIf(
                targetPersonId == Guid.Empty,
                "targetPersonId",
                "ImpersonateEmptyGuid");
            _targetPersonId = targetPersonId;
        }
        private Guid _targetPersonId = Guid.Empty;

        /// <summary>
        /// Unsets the target person identifier for all requests.
        /// </summary>
        /// 
        /// <remarks>
        /// All future requests will act as the authenticated person.
        /// <br/><br/>
        /// Note, to change the person that is being impersonated,
        /// StopImpersonating does not have to be called. 
        /// <see cref="Impersonate"/> can be called directly with a
        /// new target person identifier.
        /// <br/><br/>
        /// Impersonation occurs when the authenticated person wants to make
        /// a request to the HealthVault service on behalf of another person. This
        /// should not occur in most applications.
        /// <br/><br/>
        /// The authenticated person must have the rights to call the requested
        /// method for the person being impersonated. If that right exists, all
        /// security processing occurs using the impersonated person's 
        /// identity. If the right does not exist, the caller receives a
        /// <see cref="HealthServiceAccessDeniedException"/> upon the first 
        /// invocation of a method that accesses the HealthVault service.
        /// <br/><br/>
        /// To start impersonating, call the <see cref="Impersonate"/> method.
        /// To stop impersonating, call the <see cref="StopImpersonating"/>
        /// method.
        /// </remarks>
        /// 
        /// <seealso cref="Impersonate"/>
        /// <seealso cref="IsImpersonating"/>
        /// 
        public void StopImpersonating()
        {
            _targetPersonId = Guid.Empty;
        }

        /// <summary>
        /// Gets the value which states whether or not the current connection
        /// is impersonating a different user than who is authenticated.
        /// </summary>
        /// 
        /// <value>
        /// <b>true</b> if <see cref="Impersonate"/> has been called with a 
        /// valid target person identifier and <see cref="StopImpersonating"/> 
        /// has not been called; otherwise, <b>false</b>.
        /// </value>
        /// 
        /// <remarks>
        /// Impersonation occurs when the authenticated person wants to make
        /// a request to the HealthVault service on behalf of another person. This
        /// should not occur in most applications.
        /// <br/><br/>
        /// The authenticated person must have the rights to call the requested
        /// method for the person being impersonated. If that right exists, all
        /// security processing occurs using the impersonated person's 
        /// identity. If the right does not exist, the caller receives a
        /// <see cref="HealthServiceAccessDeniedException"/> upon the first 
        /// invocation of a method that accesses the HealthVault service.
        /// <br/><br/>
        /// To start impersonating, call the <see cref="Impersonate"/> method.
        /// To stop impersonating, call the <see cref="StopImpersonating"/>
        /// method.
        /// </remarks>
        ///
        /// <seealso cref="Impersonate"/>
        /// <seealso cref="StopImpersonating"/>
        /// 
        public bool IsImpersonating
        {
            get { return _targetPersonId != Guid.Empty; }
        }

        /// <summary>
        /// Gets the ID of the person being impersonated.
        /// </summary>
        /// 
        /// <remarks>
        /// This is Guid.Empty if <see cref="Impersonate"/> has not been
        /// called.
        /// </remarks>
        /// 
        internal Guid ImpersonatedPersonId
        {
            get { return _targetPersonId; }
        }

        #endregion Impersonation


        #region ApplicationSettings

        /// <summary>
        /// Gets the application settings for the current application and person.
        /// </summary>
        /// 
        /// <returns>
        /// A complete set of application settings including the XML, selected record ID, etc.
        /// </returns>
        /// 
        ////[Obsolete("Use HealthServicePlatform.GetApplicationSettings() instead.")]
        public ApplicationSettings GetAllApplicationSettings()
        {
            return HealthVaultPlatform.GetApplicationSettings(this);
        }

        /// <summary>
        /// Gets the application settings for the current application and
        /// person.
        /// </summary>
        /// 
        /// <returns>
        /// The application settings XML.
        /// </returns>
        /// 
        /// <remarks>
        /// This might be <b>null</b> if no application settings have been 
        /// stored for the application or user.
        /// </remarks>
        /// 
        ////[Obsolete("Use HealthServicePlatform.GetApplicationSettingsAsXml() instead.")]
        public IXPathNavigable GetApplicationSettings()
        {
            return HealthVaultPlatform.GetApplicationSettingsAsXml(this);
        }


        /// <summary>
        /// Sets the application settings for the current application and
        /// person.
        /// </summary>
        /// 
        /// <param name="applicationSettings">
        /// The application settings XML.
        /// </param>
        /// 
        /// <remarks>
        /// This might be <b>null</b> if no application settings have been stored
        /// for the application or user.
        /// </remarks>
        /// 
        ////[Obsolete("Use HealthServicePlatform.SetApplicationSettings() instead.")]
        public void SetApplicationSettings(
                IXPathNavigable applicationSettings)
        {
            HealthVaultPlatform.SetApplicationSettings(this, applicationSettings);
        }

        #endregion ApplicationSettings


        #endregion public methods

        #region CreateRequest

        internal override HealthServiceRequest CreateRequest(
            string methodName,
            int methodVersion,
            bool forAuthentication)
        {
            Validator.ThrowIfStringNullOrEmpty(methodName, "methodName");

            HealthServiceRequest request = 
                new HealthServiceRequest(
                    this,
                    methodName,
                    methodVersion);

            if (IsImpersonating)
            {
                request.ImpersonatedPersonId = _targetPersonId;
            }

            return request;
        }

        #endregion CreateRequest
    }
}

