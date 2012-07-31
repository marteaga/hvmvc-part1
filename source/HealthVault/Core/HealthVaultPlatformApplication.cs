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
using System.Security;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Health.Authentication;

namespace Microsoft.Health.PlatformPrimitives
{
    /// <summary>
    /// Provides low-level access to the HealthVault message operations.
    /// </summary>
    /// <remarks>
    /// <see cref="HealthVaultPlatform"/> uses this class to perform operations. Set 
    /// HealthVaultPlatformApplication.Current to a derived class to intercept all message calls.
    /// </remarks>

    public class HealthVaultPlatformApplication
    {
        /// <summary>
        /// Enables mocking of calls to this class.
        /// </summary>
        /// 
        /// <remarks>
        /// The calling class should pass in a class that derives from this
        /// class and overrides the calls to be mocked. 
        /// </remarks>
        /// 
        /// <param name="mock">The mocking class.</param>
        /// 
        /// <exception cref="InvalidOperationException">
        /// There is already a mock registered for this class.
        /// </exception>
        /// 
        public static void EnableMock(HealthVaultPlatformApplication mock)
        {
            Validator.ThrowInvalidIf(_saved != null, "ClassAlreadyMocked");

            _saved = _current;
            _current = mock;
        }

        /// <summary>
        /// Removes mocking of calls to this class.
        /// </summary>
        /// 
        /// <exception cref="InvalidOperationException">
        /// There is no mock registered for this class.
        /// </exception>
        /// 
        public static void DisableMock()
        {
            Validator.ThrowInvalidIfNull(_saved, "ClassIsntMocked");

            _current = _saved;
            _saved = null;
        }
        internal static HealthVaultPlatformApplication Current
        {
            get { return _current; }
        }
        private static HealthVaultPlatformApplication _current = new HealthVaultPlatformApplication();
        private static HealthVaultPlatformApplication _saved;

        #region GetAuthorizedPeople

        /// <summary>
        /// Gets information about people authorized for an application.
        /// </summary>                
        /// 
        /// <remarks>
        /// The returned IEnumerable iterator will access the HealthVault service 
        /// across the network. See <see cref="GetAuthorizedPeopleSettings"/> for applicable 
        /// settings.
        /// </remarks>
        /// 
        /// <param name="connection">The connection to use to perform the operation. This connection
        /// must be application-level. </param>
        ///
        /// <param name="settings">
        /// The <see cref="GetAuthorizedPeopleSettings" /> object used to configure the 
        /// IEnumerable iterator returned by this method.
        /// </param>
        /// 
        /// <returns>
        /// An IEnumerable iterator of <see cref="PersonInfo"/> objects representing 
        /// people authorized for the application.
        /// </returns>        
        /// 
        /// <exception cref="HealthServiceException">
        /// The HealthVault service returned an error. The retrieval can be retried from the 
        /// current position by calling this method again and using the last successfully 
        /// retrieved person Id for <see cref="GetAuthorizedPeopleSettings.StartingPersonId"/>.        
        /// </exception>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settings"/> is null.
        /// </exception>
        /// 
        public virtual IEnumerable<PersonInfo> GetAuthorizedPeople(
            ApplicationConnection connection,
            GetAuthorizedPeopleSettings settings)
        {
            Validator.ThrowIfArgumentNull(settings, "settings", "GetAuthorizedPeopleSettingsNull");

            Boolean moreResults = true;
            Guid cursor = settings.StartingPersonId;
            DateTime authCreatedSinceDate = settings.AuthorizationsCreatedSince;
            Int32 batchSize = settings.BatchSize;

            while (moreResults)
            {
                Collection<PersonInfo> personInfos =
                    GetAuthorizedPeople(
                        connection,
                        cursor,
                        authCreatedSinceDate,
                        batchSize,
                        out moreResults);

                if (personInfos.Count > 0)
                {
                    cursor = personInfos[personInfos.Count - 1].PersonId;
                }

                for (int i = 0; i < personInfos.Count; i++)
                {
                    yield return personInfos[i];
                }
            }
        }

        static internal Collection<PersonInfo> GetAuthorizedPeople(
            ApplicationConnection connection,
            Guid personIdCursor,
            DateTime authCreatedSinceDate,
            Int32 numResults,
            out Boolean moreResults)
        {
            Validator.ThrowArgumentOutOfRangeIf(
                numResults < 0,
                "numResults",
                "GetAuthorizedPeopleNumResultsNegative");

            HealthServiceRequest request =
                new HealthServiceRequest(connection, "GetAuthorizedPeople", 1);
            StringBuilder requestParameters = new StringBuilder(256);
            XmlWriterSettings settings = SDKHelper.XmlUnicodeWriterSettings;
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            using (XmlWriter writer = XmlWriter.Create(requestParameters, settings))
            {
                writer.WriteStartElement("parameters");

                if (personIdCursor != Guid.Empty)
                {
                    writer.WriteElementString("person-id-cursor", personIdCursor.ToString());
                }

                if (authCreatedSinceDate != DateTime.MinValue)
                {
                    writer.WriteElementString(
                        "authorizations-created-since",
                        SDKHelper.XmlFromDateTime(authCreatedSinceDate));
                }

                if (numResults != 0)
                {
                    writer.WriteElementString("num-results", numResults.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteEndElement(); // parameters                
                writer.Flush();
            }
            request.Parameters = requestParameters.ToString();

            request.Execute();

            Collection<PersonInfo> personInfos = new Collection<PersonInfo>();

            XPathExpression navExp =
                 SDKHelper.GetInfoXPathExpressionForMethod(
                     request.Response.InfoNavigator, "GetAuthorizedPeople");
            XPathNavigator infoNav = request.Response.InfoNavigator.SelectSingleNode(navExp);
            XPathNavigator nav = infoNav.SelectSingleNode("response-results/person-info");

            if (nav != null)
            {
                do
                {
                    PersonInfo personInfo = PersonInfo.CreateFromXml(connection, nav);
                    personInfos.Add(personInfo);

                } while (nav.MoveToNext("person-info", String.Empty));

                nav.MoveToNext();
            }
            else
            {
                nav = infoNav.SelectSingleNode("response-results/more-results");
            }

            moreResults = nav.ValueAsBoolean;

            return personInfos;
        }

        #endregion GetAuthorizedPeople

        #region GetApplicationInfo

        /// <summary>
        /// Gets the application configuration information for the calling application.
        /// </summary>
        /// 
        /// <param name="connection">The connection to use to perform the operation. This connection
        /// must be application level. </param>
        ///
        /// <param name="allLanguages">
        /// A boolean value indicating whether the localized values all languages should be 
        /// returned, just one language. This affects all properties which can have multiple 
        /// localized values, including <see cref="ApplicationInfo.CultureSpecificNames"/>, 
        /// <see cref="ApplicationInfo.CultureSpecificDescriptions"/>,
        /// <see cref="ApplicationInfo.CultureSpecificAuthorizationReasons"/>, 
        /// <see cref="ApplicationInfo.LargeLogo"/>,
        /// <see cref="ApplicationInfo.SmallLogo"/>,
        /// <see cref="ApplicationInfo.PrivacyStatement"/>,
        /// <see cref="ApplicationInfo.TermsOfUse"/>,
        /// and <see cref="ApplicationInfo.DtcSuccessMessage"/>
        /// </param>
        /// 
        /// <returns>
        /// An ApplicationInfo object for the calling application.
        /// </returns>
        /// 
        /// <remarks>
        /// This method always calls the HealthVault service to get the latest 
        /// information. It returns installation configuration about the calling 
        /// application.
        /// </remarks>
        /// 
        /// <exception cref="HealthServiceException">
        /// The HealthVault service returns an error.
        /// </exception>
        /// 
        public virtual ApplicationInfo GetApplicationInfo(
            HealthServiceConnection connection,
            Boolean allLanguages)
        {
            HealthServiceRequest request =
                new HealthServiceRequest(connection, "GetApplicationInfo", 2);

            if (allLanguages)
            {
                request.Parameters += "<all-languages>true</all-languages>";
            }

            request.Execute();

            XPathExpression xPathExpression = SDKHelper.GetInfoXPathExpressionForMethod(
                    request.Response.InfoNavigator, "GetApplicationInfo");

            XPathNavigator infoNav
                = request.Response.InfoNavigator
                    .SelectSingleNode(xPathExpression);

            XPathNavigator appInfoNav = infoNav.SelectSingleNode("application");

            ApplicationInfo appInfo = null;
            if (appInfoNav != null)
            {
                appInfo = ApplicationInfo.CreateFromInfoXml(appInfoNav);
            }

            return appInfo;
        }

        #endregion

        #region GetUpdatedRecordsForApplication


        /// <summary>
        /// Gets a list of health record IDs for the current application, 
        /// that optionally have been updated since a specified date.
        /// </summary>
        /// 
        /// <param name="connection">The connection to use to perform the operation. This connection
        /// must be application level. </param>
        ///
        /// <param name="updatedDate">
        /// Date that is used to filter health record IDs according to whether or not they have
        /// been updated since the specified date.
        /// </param>
        /// 
        /// <returns>
        /// List of health record IDs filtered by any specified input parameters.
        /// </returns>
        /// 
        public virtual IList<Guid> GetUpdatedRecordsForApplication(
            HealthServiceConnection connection,
            DateTime? updatedDate)
        {
            HealthServiceRequest request =
                CreateGetUpdateRecordsForApplicationRequest(connection, updatedDate);

            request.Execute();
            IList<Guid> results;
            ParseGetUpdatedRecordsForApplicationResponse(request.Response, out results);
            return results;
        }

        /// <summary>
        /// Gets a list of <see cref="HealthRecordUpdateInfo"/> objects for the current application, 
        /// that optionally have been updated since a specified date.
        /// </summary>
        /// 
        /// <param name="connection">The connection to use to perform the operation. This connection
        /// must be application level. </param>
        ///
        /// <param name="updatedDate">
        /// Date that is used to filter health record IDs according to whether or not they have
        /// been updated since the specified date.
        /// </param>
        /// 
        /// <returns>
        /// List of <see cref="HealthRecordUpdateInfo"/> objects filtered by any specified input parameters.
        /// </returns>
        /// 
        public virtual IList<HealthRecordUpdateInfo> GetUpdatedRecordInfoForApplication(
            HealthServiceConnection connection,
            DateTime? updatedDate)
        {
            HealthServiceRequest request =
                CreateGetUpdateRecordsForApplicationRequest(connection, updatedDate);

            request.Execute();
            IList<HealthRecordUpdateInfo> results;
            ParseGetUpdatedRecordsForApplicationResponse(request.Response, out results);
            return results;
        }

        private static HealthServiceRequest CreateGetUpdateRecordsForApplicationRequest(
            HealthServiceConnection connection,
            DateTime? updateDate)
        {
            HealthServiceRequest request =
                new HealthServiceRequest(connection, "GetUpdatedRecordsForApplication", 1);

            StringBuilder parameters = new StringBuilder();

            if (updateDate != null)
            {
                parameters.Append("<update-date>");
                parameters.Append(SDKHelper.XmlFromDateTime(updateDate.Value));
                parameters.Append("</update-date>");
            }

            request.Parameters = parameters.ToString();

            return request;
        }

        private static void ParseGetUpdatedRecordsForApplicationResponse(
            HealthServiceResponseData response, out IList<Guid> recordIds)
        {
            recordIds = new List<Guid>();

            XPathNodeIterator iterator = response.InfoNavigator.Select("record-id");
            foreach (XPathNavigator navigator in iterator)
            {
                recordIds.Add(ParseHealthRecordUpdateInfo(navigator).RecordId);
            }
        }
        private static void ParseGetUpdatedRecordsForApplicationResponse(
            HealthServiceResponseData response,
            out IList<HealthRecordUpdateInfo> healthRecordUpdateInfos)
        {
            healthRecordUpdateInfos = new List<HealthRecordUpdateInfo>();

            XPathNodeIterator iterator = response.InfoNavigator.Select("record-id");
            foreach (XPathNavigator navigator in iterator)
            {
                healthRecordUpdateInfos.Add(ParseHealthRecordUpdateInfo(navigator));
            }
        }

        private static HealthRecordUpdateInfo ParseHealthRecordUpdateInfo(XPathNavigator navigator)
        {
            Guid recordId = new Guid(navigator.Value);
            DateTime lastUpdateDate =
                DateTime.Parse(
                    navigator.GetAttribute("update-date", String.Empty),
                    DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal);
            return new HealthRecordUpdateInfo(recordId, lastUpdateDate);
        }

        #endregion

        #region NewSignupCode

        /// <summary>
        /// Generates a new signup code that should be passed to HealthVault Shell in order
        /// to create a new user account.
        /// </summary>
        /// 
        /// <param name="connection">The connection to use to perform the operation. This connection
        /// must be application level. </param>
        ///
        /// <returns>
        /// A signup code that can be used to create an account.
        /// </returns>
        /// 
        public virtual string NewSignupCode(HealthServiceConnection connection)
        {
            HealthServiceRequest request =
                new HealthServiceRequest(connection, "NewSignupCode", 1);
            request.Execute();

            XPathExpression infoPath =
                SDKHelper.GetInfoXPathExpressionForMethod(
                    request.Response.InfoNavigator,
                    "NewSignupCode");

            XPathNavigator infoNav = request.Response.InfoNavigator.SelectSingleNode(infoPath);
            return infoNav.SelectSingleNode("signup-code").Value;
        }

        #endregion
    }
}

