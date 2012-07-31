﻿// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Health;
using Microsoft.Health.Web.Authentication;
using Microsoft.Health.Certificate;
using Microsoft.Health.ItemTypes;
using Microsoft.Health.Web;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Text;

namespace Microsoft.Health
{
    /// <summary>
    /// Represents a HealthVault client application.
    /// </summary>
    /// 
    /// <remarks>
    /// Use this class for creating a Windows client application
    /// for connecting to HealthVault. 
    /// </remarks>
    /// 
    public class HealthClientApplication : IDisposable
    {
        #region Private variables

        /// <summary>
        /// Certificate for the child application
        /// </summary>
        private ApplicationCertificate _childCert;
        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor for <see cref="HealthClientApplication"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// Constructor is private. For creating an instance of 
        /// <see cref="HealthClientApplication"/> use Create.
        /// </remarks>
        /// 
        private HealthClientApplication()
        {
        }

        /// <summary>
        /// Create the application using values stored in the app.config file.
        /// </summary>
        /// 
        /// <param name="applicationId">
        /// Unique identifier of the new local client application.
        /// </param>
        /// 
        /// <param name="masterApplicationId">
        /// Unique identifier of an existing HealthVault master application.
        /// The client application will be created as a child application using
        /// the specified application as a parent.
        /// </param>
        /// 
        /// <remarks>
        /// App.config entries are as follows:
        /// ShellUrl - The url of the HealthVault shell
        /// HealthServiceUrl - the url of the HealthVault platform
        /// </remarks>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The configuration file does not 
        /// contain an entry for either ShellUrl or HealthServiceUrl.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// Either the <paramref name="applicationId "/> or
        /// <paramref name="masterApplicationId"/> parameter is <see cref="Guid.Empty"/>. 
        /// </exception>
        /// 
        /// <exception ref="CryptographicException">
        /// If the certificate cannot be created or could not be added to the store.
        /// </exception>
        /// 
        /// <remarks>
        /// This method will lookup the certificate in the
        /// user certificate store by applicationId. If the
        /// certificate does not exist, then a new certificate will
        /// be created.
        /// </remarks>
        /// 
        public static HealthClientApplication Create(
            Guid applicationId, 
            Guid masterApplicationId)
        {
            if (HealthApplicationConfiguration.Current.HealthVaultShellUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            if (HealthApplicationConfiguration.Current.HealthVaultMethodUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            return Create(applicationId, 
                masterApplicationId,
                HealthApplicationConfiguration.Current.HealthVaultShellUrl,
                HealthApplicationConfiguration.Current.HealthVaultMethodUrl);
        }


        /// <summary>
        /// Create an application based on the passed-in values.
        /// </summary>
        /// 
        /// <param name="applicationId">
        /// Unique identifier of the new local client application.
        /// </param>
        /// 
        /// <param name="masterApplicationId">
        /// Unique identifier of an existing HealthVault master application.
        /// The client application will be created as a child application using
        /// the specified application as a parent.
        /// </param>
        /// 
        /// <param name="shellUrl">
        /// The URL of the HealthVault shell service.
        /// </param>
        /// 
        /// <param name="healthServiceUrl">
        /// The URL of the HealthVault platform service.
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        /// The value of <paramref name="shellUrl"/> or 
        /// <paramref name="healthServiceUrl"/> is 
        /// <b>null</b>, or the value of <paramref name="applicationId"/> or 
        /// <paramref name="masterApplicationId"/> is <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <exception ref="CryptographicException">
        /// If the certificate cannot be created or could not be added to the store.
        /// </exception>
        /// 
        /// <remarks>
        /// This method looks up the certificate in the
        /// user certificate store using applicationId. If the
        /// certificate does not exist, then it will create
        /// a new certificate.
        /// </remarks>
        /// 
        public static HealthClientApplication Create(
            Guid applicationId,
            Guid masterApplicationId,
            Uri shellUrl, 
            Uri healthServiceUrl)
        {
            HealthClientApplication healthClientApplication =
                new HealthClientApplication();

            Validator.ThrowArgumentExceptionIf(
                applicationId == Guid.Empty,
                "applicationId",
                "InvalidApplicationIdConfiguration");

            healthClientApplication._applicationId = applicationId;

            Validator.ThrowArgumentExceptionIf(
                masterApplicationId == Guid.Empty,
                "masterApplicationId",
                "InvalidMasterApplicationIdConfiguration");

            healthClientApplication._masterApplicationId = masterApplicationId;

            Validator.ThrowArgumentExceptionIf(
                shellUrl == null,
                "shellUrl",
                "InvalidRequestUrlConfiguration");

            healthClientApplication._shellUrl = shellUrl;

            Validator.ThrowArgumentExceptionIf(
                healthServiceUrl == null,
                "healthServiceUrl",
                "InvalidRequestUrlConfiguration");

            healthClientApplication._healthServiceUrl = healthServiceUrl;

            healthClientApplication.CreateApplication();

            return healthClientApplication;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of the local client application.
        /// </summary>
        public Guid ApplicationId
        {
            get { return _applicationId; }
        }
        /// <summary>
        /// ApplicationId for the child application
        /// </summary>
        private Guid _applicationId;

        /// <summary>
        /// Gets the URL of the HealthVault shell service.
        /// </summary>
        public Uri ShellUrl
        {
            get { return _shellUrl; }
        }

        private Uri _shellUrl;

        /// <summary>
        /// Gets the URL of the HealthVault platform service.
        /// </summary>
        public Uri HealthServiceUrl
        {
            get
            {
                return _healthServiceUrl;
            }
        }

        private Uri _healthServiceUrl;

        /// <summary>
        /// Gets the ID of the master application.
        /// </summary>
        public Guid MasterApplicationId
        {
            get
            {
                return _masterApplicationId;
            }
        }

        private Guid _masterApplicationId;

        /// <summary>
        /// Gets an <see cref="ApplicationConnection"/> that represents the connection to HealthVault.
        /// </summary>
        /// 
        /// <exception ref="SecurityException">
        /// The required application-specific certificate is not found.
        /// </exception>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The application Id or the certificate, or the healthServiceUrl
        /// are incorrect.
        /// </exception>
        /// 
        /// <remarks>
        /// This method could cause a request to the network to retrieve the 
        /// cryptographic object identifier of the certificate used by the 
        /// application. For example in case the hosting machine is joined to 
        /// a domain, resolving or retrieving the cryptographic object 
        /// identifier could result in an LDAP query.
        /// </remarks>
        public ApplicationConnection ApplicationConnection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = Connect();
                }
                return _connection;
            }
        }
        
        private ApplicationConnection _connection;

        #endregion

        #region Public methods


        /// <summary>
        /// Gets an <see cref="ApplicationInfo"/> from the HealthVault server that
        /// describes the client application.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>This method makes a call to HealthVault to get the ApplicationInfo.
        /// If the application exists, then the ApplicationInfo is created and returned.</para>
        /// <para>If application does not exist, the method returns <b>null</b>.
        /// To create the application on the server, 
        /// call <see cref="HealthClientApplication.StartApplicationCreationProcess()" />.</para>
        /// </remarks>
        /// 
        /// <returns>
        /// An <see cref="ApplicationInfo"/> that describes the application,
        /// or <b>null</b> if the application does not exist on the server.
        /// </returns>
        /// 
        /// <exception cref="HealthServiceException">
        /// The HealthVault service returned an error. This exception does not indicate
        /// that the application does not exist on the server: the method returns <b>null</b>
        /// in that case and no exception is thrown.
        /// </exception>
        /// 
        public ApplicationInfo GetApplicationInfo()
        {
            ApplicationInfo applicationInfo = null;
           
            try
            {
                applicationInfo = HealthVaultPlatform.GetApplicationInfo(ApplicationConnection);
            }
            catch (HealthServiceException e)
            {
                // If the exception's error code is not InvalidApp
                // then rethrow the exception.
                //
                if (e.ErrorCode != HealthServiceStatusCode.InvalidApp)
                {
                    throw;
                }
            }

            return applicationInfo;
        }


        /// <summary>
        /// Gets the URL of a web page that will direct the user to create the application.
        /// </summary>
        /// <remarks>
        /// Open this URL in a browser window to allow the user to create the application.
        /// By default, the client name will be set to the local machine name. 
        /// </remarks>
        /// <exception cref="InvalidConfigurationException">
        /// The value of <see cref="HealthClientApplication.ShellUrl"/> or 
        /// <see cref="HealthClientApplication.HealthServiceUrl"/> is 
        /// <b>null</b>, or the value of <see cref="HealthClientApplication.ApplicationId"/> or 
        /// <see cref="HealthClientApplication.MasterApplicationId"/> is <see cref="Guid.Empty"/> or
        /// the required application-specific certificate is not found.
        /// </exception>
        /// 
        /// <returns>
        /// The URL of the application creation web page.
        /// </returns>
        /// 
        public Uri GetApplicationCreationUrl()
        {
            string clientName = Environment.MachineName;

            return GetApplicationCreationUrl(clientName, String.Empty);
        }

        /// <summary>
        /// Gets the URL of a web page that will direct the user to create the application.
        /// </summary>
        /// <remarks>
        /// Open this URL in a browser window to allow the user to create the application.
        /// </remarks>
        /// 
        /// <param name="clientName">The client name to use.</param>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The value of <see cref="HealthClientApplication.ShellUrl"/> or 
        /// <see cref="HealthClientApplication.HealthServiceUrl"/> is 
        /// <b>null</b>, or the value of <see cref="HealthClientApplication.ApplicationId"/> or 
        /// <see cref="HealthClientApplication.MasterApplicationId"/> is <see cref="Guid.Empty"/> or
        /// the required application-specific certificate is not found.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="clientName"/> is empty or <b>null</b>.
        /// </exception>
        /// 
        /// <returns>
        /// The URL of the application creation web page.
        /// </returns>
        public Uri GetApplicationCreationUrl(string clientName)
        {
            return GetApplicationCreationUrl(clientName, String.Empty);
        }

        /// <summary>
        /// Gets the URL of a web page that will direct the user to create the application.
        /// </summary>
        /// <remarks>
        /// Open this URL in a browser window to allow the user to create the application.
        /// </remarks>
        /// 
        /// <param name="clientName">The client name to use.</param>
        /// 
        /// <param name="optionalQueryParameters">
        /// Optional parameters to be added to the creation URL:
        /// 
        /// <ul>
        ///     <li>ismra - the application can use multiple records for the same user at one time.</li>
        ///     <li>extrecordid - record identifier.</li>
        ///     <li>forceappauth - force redirect to APPAUTH target once user is authenticated.</li>
        ///     <li>onopt# - A sequence of online optional authorization rule names
        ///                 identifying which rules to present.  The sequence begins with 1.</li>
        ///     <li>offopt# - A sequence of offline optional authorization rule names  
        ///                 identifying which rules to present.  The sequence begins with 1.</li>
        /// </ul>
        /// </param>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The value of <see cref="HealthClientApplication.ShellUrl"/> or 
        /// <see cref="HealthClientApplication.HealthServiceUrl"/> is 
        /// <b>null</b>, or the value of <see cref="HealthClientApplication.ApplicationId"/> or 
        /// <see cref="HealthClientApplication.MasterApplicationId"/> is <see cref="Guid.Empty"/> or
        /// the required application-specific certificate is not found.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="clientName"/> is empty or <b>null</b>.
        /// </exception>
        /// 
        /// <returns>
        /// The URL of the application creation web page.
        /// </returns>
        public Uri GetApplicationCreationUrl(string clientName, string optionalQueryParameters)
        {
            Validator.ThrowIfStringNullOrEmpty(clientName, "clientName");

            if (this._shellUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            if (this._healthServiceUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            if (this._masterApplicationId == Guid.Empty)
            {
                throw Validator.InvalidConfigurationException("InvalidMasterApplicationIdConfiguration");
            }

            if (this._applicationId == Guid.Empty)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationIdConfiguration");
            }

            if (this._childCert == null)
            {
                throw Validator.InvalidConfigurationException("InvalidCertificate");
            }

            // Create the query string that passes the master app id, the app id, the client name, and
            // the certificate public key to shell. 
            Uri uri = HealthServiceLocation.GetShellRedirectorUrl(_shellUrl);

            StringBuilder queryString = new StringBuilder(1024);
            queryString.Append(string.Format(CultureInfo.InvariantCulture,
                    "appId={0}&instanceId={1}&instanceName={2}&publicKey={3}",
                    _masterApplicationId,
                    _applicationId,
                    clientName,
                    System.Web.HttpUtility.UrlEncode(
                        Convert.ToBase64String(this.Certificate.Export(X509ContentType.Cert)))));

            if (!String.IsNullOrEmpty(optionalQueryParameters))
            {
                queryString.Append("&");
                queryString.Append(optionalQueryParameters);
            }

            return HealthServiceLocation.GetHealthServiceShellUrl(uri, "CreateApplication", queryString.ToString());
        }

        /// <summary>
        /// Gets the URL of a web page that will direct the user to authorize the application.
        /// </summary>
        /// <remarks>
        /// Open this URL in a browser window to allow the user to authorize the application.
        /// </remarks>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The value of <see cref="HealthClientApplication.ShellUrl"/> is <b>null</b>, 
        /// or the value of <see cref="HealthClientApplication.ApplicationId"/> is 
        /// <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <returns>
        /// The URL of the application authorization web page.
        /// </returns>

        public Uri GetUserAuthorizationUrl()
        {
            return GetUserAuthorizationUrl(string.Empty);
        }

        /// <summary>
        /// Gets the URL of a web page that will direct the user to authorize the application, 
        /// including optional APPAUTH parameters.
        /// </summary>
        /// <remarks>
        /// Open this URL in a browser window to allow the user to authorize the application.
        /// </remarks>
        /// 
        /// <param name="optionalQueryParameters">
        /// Optional parameters to be added to the authorization URL:
        /// 
        /// <ul>
        ///     <li>ismra - the application can use multiple records for the same user at one time.</li>
        ///     <li>extrecordid - record identifier.</li>
        ///     <li>forceappauth - force redirect to APPAUTH target once user is authenticated.</li>
        ///     <li>onopt# - A sequence of online optional authorization rule names
        ///                 identifying which rules to present.  The sequence begins with 1.</li>
        ///     <li>offopt# - A sequence of offline optional authorization rule names  
        ///                 identifying which rules to present.  The sequence begins with 1.</li>
        /// </ul>
        /// </param>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The value of <see cref="HealthClientApplication.ShellUrl"/> is <b>null</b>, 
        /// or the value of <see cref="HealthClientApplication.ApplicationId"/> is 
        /// <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <returns>
        /// The URL of the application authorization web page.
        /// </returns>

        public Uri GetUserAuthorizationUrl(string optionalQueryParameters)
        {
            if (_shellUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            if (_applicationId == Guid.Empty)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationIdConfiguration");
            }

            StringBuilder queryString = new StringBuilder(256);
            queryString.Append("appid=");
            queryString.Append(_applicationId);
            if (!String.IsNullOrEmpty(optionalQueryParameters))
            {
                queryString.Append("&");
                queryString.Append(optionalQueryParameters);
            }

            Uri redirectUri = HealthServiceLocation.GetShellRedirectorUrl(_shellUrl);
            return HealthServiceLocation.GetHealthServiceShellUrl(redirectUri , "appauth", queryString.ToString());
        }

        /// <summary>
        /// Start the authorization process.
        /// </summary>
        /// 
        /// <remarks>
        /// Starts the authorization process by opening an authorization page in the 
        /// user's default browser.
        /// After this call, the application is responsible for waiting 
        /// until the authorization process is completed before continuing.  A typical 
        /// implementation will create a UI element that allows the user to indicate that
        /// authorization is complete.
        /// </remarks>
        /// 
        /// <exception cref="Win32Exception">
        /// There was an error opening the authorization URL.
        /// </exception>
        /// 
        public void StartUserAuthorizationProcess()
        {
            Uri uri = GetUserAuthorizationUrl();

            LaunchBrowser(uri);
        }


        /// <summary>
        /// Start the authorization process with optional APPAUTH parameters.
        /// </summary>
        /// 
        /// <remarks>
        /// Starts the authorization process by opening an authorization page in the 
        /// user's default browser with optional APPAUTH parameters.
        /// After this call, the application is responsible for waiting 
        /// until the authorization process is completed before continuing.  A typical 
        /// implementation will create a UI element that allows the user to indicate that
        /// authorization is complete.
        /// </remarks>
        /// 
        /// <param name="optionalQueryParameters">
        /// Optional parameters to be added to the authorization URL:
        /// 
        /// <ul>
        ///     <li>ismra - the application can use multiple records for the same user at one time.</li>
        ///     <li>extrecordid - record identifier.</li>
        ///     <li>forceappauth - force redirect to APPAUTH target once user is authenticated.</li>
        ///     <li>onopt# - A sequence of online optional authorization rule names
        ///                 identifying which rules to present.  The sequence begins with 1.</li>
        ///     <li>offopt# - A sequence of offline optional authorization rule names  
        ///                 identifying which rules to present.  The sequence begins with 1.</li>
        /// </ul>
        /// </param>
        /// 
        /// <exception cref="Win32Exception">
        /// There was an error opening the uri.
        /// </exception>
        /// 
        public void StartUserAuthorizationProcess(string optionalQueryParameters)
        {
            Uri uri = GetUserAuthorizationUrl(optionalQueryParameters);

            LaunchBrowser(uri);
        }


        /// <summary>
        /// Start the application creation process with a default application name.
        /// </summary>
        /// 
        /// <remarks>
        /// Starts the application creation process by opening a URL in the 
        /// user's default browser.
        /// After this call, the application is responsible for waiting 
        /// until the authorization process is completed before continuing.  A typical 
        /// implementation will create a UI element that allows the user to indicate that
        /// application creation is complete.
        /// The name of the child application is set to the local machine name.
        /// </remarks>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The value of <see cref="HealthClientApplication.ShellUrl"/> or 
        /// <see cref="HealthClientApplication.HealthServiceUrl"/> is 
        /// <b>null</b>, or the value of <see cref="HealthClientApplication.ApplicationId"/> or 
        /// <see cref="HealthClientApplication.MasterApplicationId"/> is <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <exception cref="Win32Exception">
        /// There was an error opening the URL.
        /// </exception>
        /// 
        public void StartApplicationCreationProcess()
        {
            string clientName = Environment.MachineName;

            StartApplicationCreationProcess(clientName);
        }

        /// <summary>
        /// Start the application creation process with a specified application name.
        /// </summary>
        /// 
        /// <remarks>
        /// Starts the application creation process by opening a URL in the 
        /// user's default browser.
        /// After this call, the application is responsible for waiting 
        /// until the authorization process is completed before continuing.  A typical 
        /// implementation will create a UI element that allows the user to indicate that
        /// application creation is complete.
        /// </remarks>
        /// 
        /// <param name="clientName">
        /// The unique client name to use. The client name specifies the instance name
        /// of the application.
        /// </param>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The value of <see cref="HealthClientApplication.ShellUrl"/> or 
        /// <see cref="HealthClientApplication.HealthServiceUrl"/> is 
        /// <b>null</b>, or the value of <see cref="HealthClientApplication.ApplicationId"/> or 
        /// <see cref="HealthClientApplication.MasterApplicationId"/> is <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="clientName"/> is empty or <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="Win32Exception">
        /// There was an error opening the URL.
        /// </exception>
        /// 
        public void StartApplicationCreationProcess(string clientName)
        {
            Validator.ThrowIfStringNullOrEmpty(clientName, "clientName");

            Uri uri = GetApplicationCreationUrl(clientName);
            LaunchBrowser(uri);
        }

        /// <summary>
        /// Creates an authorized client connection to the application.
        /// </summary>
        /// 
        /// <param name="personId">
        /// ID of the person for the connection.
        /// </param>
        /// 
        /// <exception ref="SecurityException">
        /// The required application-specific certificate is invalid.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="personId"/> parameter is empty.
        /// </exception>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The required application-specific certificate is not found,
        /// the value of <see cref="HealthClientApplication.HealthServiceUrl"/> is 
        /// <b>null</b>, or the value of <see cref="HealthClientApplication.ApplicationId"/>
        /// is <see cref="Guid.Empty"/>.
        /// </exception>
        /// 
        /// <returns>
        /// A <see cref="HealthClientAuthorizedConnection"/> instance.
        /// </returns>
        /// 
        public HealthClientAuthorizedConnection CreateAuthorizedConnection(Guid personId)
        {
            Validator.ThrowArgumentExceptionIf(
                personId == Guid.Empty,
                "personId",
                "InvalidPersonId");

            if (_applicationId == Guid.Empty)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationIdConfiguration");
            }

            if (Certificate == null)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationCertificate");
            }

            if (_healthServiceUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            WebApplicationCredential webApplicationCredential =
                new WebApplicationCredential(ApplicationId,
                    StoreLocation.CurrentUser,
                    Certificate.Subject);

            return new HealthClientAuthorizedConnection(
                webApplicationCredential, 
                ApplicationId, 
                _healthServiceUrl, 
                personId);
        }

        /// <summary>
        /// Deletes the certificate created as part of application creation
        /// </summary>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The required application-specific certificate is not found,
        /// </exception>
        /// 
        public void DeleteCertificate()
        {
            if (_childCert == null)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationCertificate");
            }

            using (CertificateStore store = new CertificateStore(StoreLocation.CurrentUser))
            {
                store.RemoveCert(_childCert.Certificate);
                _childCert.Dispose();
                _childCert = null;
            }

            ApplicationCertificate.DeleteKeyContainer(_applicationId);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes the request.
        /// </summary>
        /// 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up the cancel request trigger.
        /// </summary>
        /// 
        /// <param name="disposing"></param>
        /// 
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_childCert != null)
                {
                    _childCert.Dispose();
                }
            } 
        }

        #endregion IDisposable

        #region Private Helpers

        /// <summary>
        /// Gets the certificate associated with the application
        /// </summary>
        private X509Certificate2 Certificate
        {
            get
            {
                Debug.Assert(_childCert != null);

                X509Certificate2 x509Certificate2 = _childCert.Certificate;
                return x509Certificate2;
            }

        }

        /// <summary>
        /// Creates the application.
        /// </summary>
        /// 
        /// <remarks>
        /// The caller should store the value of the ApplicationId property so it can re-use this 
        /// application. This method will lookup the certificate by application Id. If no 
        /// certificate is found, it creates one in the current user store.
        /// </remarks>
        /// 
        /// <exception ref="CryptographicException">
        /// If the certificate cannot be created or could not be added to the store.
        /// </exception>
        /// ///
        private void CreateApplication()
        {
            // If no child certificate exists, create a new ApplicationCertificate
            // ApplicationCertificate will find the certificate name HVClient-appId
            // in the local user store. If one does not exist, a new certificate will be created
            if (_childCert == null)
            {
                _childCert = new ApplicationCertificate(_applicationId);
            }

        }


        /// <summary>
        /// Create a connection to the application.
        /// </summary>
        /// 
        /// <remarks>
        /// The ApplicationId must be set before calling this method.
        /// 
        /// This method could cause a request to the network to retrieve the 
        /// cryptographic object identifier of the certificate used by the 
        /// application. For example in case the hosting machine is joined to 
        /// a domain, resolving or retrieving the cryptographic object 
        /// identifier could result in an LDAP query.
        /// </remarks>
        /// 
        /// <exception ref="SecurityException">
        /// The required application-specific certificate is not found.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// The person Id is empty.
        /// </exception>
        /// 
        /// <exception cref="InvalidConfigurationException">
        /// The application Id or the certificate, or the healthServiceUrl
        /// are incorrect.
        /// </exception>
        /// 
        /// <returns>
        /// An HealthClientAuthorizedConnection instance
        /// </returns>
        private HealthClientAuthorizedConnection Connect()
        {
            if (this._applicationId == Guid.Empty)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationIdConfiguration");
            }

            if (this.Certificate == null)
            {
                throw Validator.InvalidConfigurationException("InvalidApplicationCertificate");
            }

            if (_healthServiceUrl == null)
            {
                throw Validator.InvalidConfigurationException("InvalidRequestUrlConfiguration");
            }

            WebApplicationCredential webApplicationCredential =
                new WebApplicationCredential(ApplicationId,
                    StoreLocation.CurrentUser,
                    this.Certificate.Subject);

            // create a connection based on the app-id and certificate...
            return new HealthClientAuthorizedConnection(
                webApplicationCredential,
                ApplicationId,
                _healthServiceUrl);
        }

        #endregion

        #region static
        /// <summary>
        /// Launches the browser process with the specified Uri.
        /// </summary>
        /// 
        /// <param name="uri">
        /// Uri to be launched.
        /// </param>
        /// 
        /// <exception cref="Win32Exception">
        /// There was an error opening the uri.
        /// </exception>
        /// 
        private static void LaunchBrowser(Uri uri)
        {
            //
            // The request is executed by launching it in the registered browser
            // The user then walks through the process of authorizing the application
            //
            ProcessStartInfo startInfo = new ProcessStartInfo(uri.AbsoluteUri);
            startInfo.ErrorDialog = true;
            startInfo.UseShellExecute = true;
            startInfo.CreateNoWindow = false; // create in a new window, which we can control
            Process process = Process.Start(startInfo);
            if (process != null)
            {
                process.Dispose();
            }
        }


        #endregion
    }
}
