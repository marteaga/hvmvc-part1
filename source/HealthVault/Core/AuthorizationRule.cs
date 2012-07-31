// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health
{
    /// <summary>
    /// This class defines an authorization rule in the HealthVault service.
    /// </summary>
    /// 
    /// <remarks>
    /// Authorization rules are applied to authorized records to state what
    /// permissions the person or group being authorized has on the data in
    /// that record. See the HealthVault Developer's Guide for more information
    /// on how authorization works in HealthVault.
    /// 
    /// This rule does not necessarily represent a rule that is present
    /// on the server. It can be used to generate the necessary XML when 
    /// using the Shell pages to authorize records.
    /// </remarks>
    /// 
    public class AuthorizationRule
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationRule"/> class
        /// with the specified permissions.
        /// </summary>
        /// 
        /// <param name="permissions">
        /// The permissions granted.
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="permissions"/> parameter is 
        /// <see cref="Microsoft.Health.HealthRecordItemPermissions.None"/>.
        /// </exception>
        /// 
        public AuthorizationRule(
            HealthRecordItemPermissions permissions)
            : this(permissions, null, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationRule"/> class 
        /// with the specified permissions, target and exception sets.
        /// </summary>
        /// 
        /// <param name="permissions">
        /// The permissions granted. 
        /// </param>
        /// 
        /// <param name="targetSets">
        /// The set or sets of health record items to which this rule applies.
        /// </param>
        /// 
        /// <param name="exceptionSets">
        /// The set or sets of health record items to which this rule does not 
        /// apply even if contained in a set defined by 
        /// <paramref name="targetSets"/>.
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="permissions"/> parameter is 
        /// <see cref="Microsoft.Health.HealthRecordItemPermissions.None"/>.
        /// </exception>
        ///         
        public AuthorizationRule(
            HealthRecordItemPermissions permissions,
            IList<AuthorizationSetDefinition> targetSets,
            IList<AuthorizationSetDefinition> exceptionSets)
            : this(null, null, permissions, targetSets, exceptionSets, false,
                AuthorizationRuleDisplayFlags.None)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationRule"/> class 
        /// with the specified name, reason, permissions, target, exception sets,
        /// optional and display flags.
        /// </summary>
        /// 
        /// <param name="name">
        /// The name uniquely identifying this rule in a set
        /// </param>
        ///
        /// <param name="reason">
        /// The reason why an application wants this access
        /// </param>
        ///
        /// <param name="permissions">
        /// The permissions granted. 
        /// </param>
        /// 
        /// <param name="targetSets">
        /// The set or sets of health record items to which this rule applies.
        /// </param>
        /// 
        /// <param name="exceptionSets">
        /// The set or sets of health record items to which this rule does not 
        /// apply even if contained in a set defined by 
        /// <paramref name="targetSets"/>.
        /// </param>
        ///
        /// <param name="isOptional">
        /// Flag indicating whether or not this rule is optional
        /// </param>
        ///
        /// <param name="displayFlags">
        /// Flags controlling how to display this rule
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="permissions"/> parameter is 
        /// <see cref="Microsoft.Health.HealthRecordItemPermissions.None"/>.
        /// </exception>
        ///         
        public AuthorizationRule(
            string name,
            string reason, 
            HealthRecordItemPermissions permissions,
            IList<AuthorizationSetDefinition> targetSets,
            IList<AuthorizationSetDefinition> exceptionSets,
            bool isOptional,
            AuthorizationRuleDisplayFlags displayFlags)
        {
            _name = name;
            _isOptional = isOptional;
            _displayFlags = displayFlags;

            if (!String.IsNullOrEmpty(reason))
            {
                CultureSpecificReasons.DefaultValue = reason;
            }

            Validator.ThrowArgumentExceptionIf(
                permissions == HealthRecordItemPermissions.None,
                "permissions",
                "AuthorizationRuleBadPermissions");
                
            _permissions = permissions;

            if (targetSets != null)
            {
                _targetSets =
                    new ReadOnlyCollection<AuthorizationSetDefinition>(
                        targetSets);
            }
            else
            {
                _targetSets =
                    new ReadOnlyCollection<AuthorizationSetDefinition>(
                        new AuthorizationSetDefinition[0]);
            }

            if (exceptionSets != null)
            {
                _exceptionSets =
                    new ReadOnlyCollection<AuthorizationSetDefinition>(
                        exceptionSets);
            }
            else
            {
                _exceptionSets = 
                    new ReadOnlyCollection<AuthorizationSetDefinition>(
                        new AuthorizationSetDefinition[0]);
            }
        }

        /// <summary>
        /// Gets the name uniquely identifying the rule within the rule set.
        /// </summary>
        /// 
        public string Name
        {
            get { return _name; }
        }
        private string _name;

        /// <summary>
        /// Gets a value indicating whether the authorization rule is optional
        /// </summary>
        /// 
        /// <remarks>
        /// Optional rules are not required for application authorization.
        /// </remarks>
        /// 
        public bool IsOptional
        {
            get { return _isOptional; }
        }
        private bool _isOptional;

        /// <summary>
        /// Gets the reason the application wants the access represented by this rule.
        /// </summary>
        /// 
        public string Reason
        {
            get 
            {
                return CultureSpecificReasons.BestValue;
            }
        }

        /// <summary>
        /// Gets a dictionary of language specifiers and reasons.
        /// </summary>
        /// <remarks>
        ///  Each entry is a localized version of the same reason.
        /// </remarks>
        /// 
        public CultureSpecificStringDictionary CultureSpecificReasons
        {
            get { return _cultureSpecificReasons; }
        }
        private CultureSpecificStringDictionary _cultureSpecificReasons = 
            new CultureSpecificStringDictionary();

        /// <summary>
        /// Gets flags controlling display behavior of rules.
        /// </summary>
        /// 
        public AuthorizationRuleDisplayFlags DisplayFlags
        {
            get { return _displayFlags; }
        }
        private AuthorizationRuleDisplayFlags _displayFlags
            = AuthorizationRuleDisplayFlags.None;


        /// <summary>
        /// Gets the permissions that the rule grants.
        /// </summary>
        /// 
        /// <value>
        /// An instance of <see cref="HealthRecordItemPermissions"/>.
        /// </value>
        /// 
        public HealthRecordItemPermissions Permissions
        {
            get { return _permissions; }
        }
        private HealthRecordItemPermissions _permissions 
            = HealthRecordItemPermissions.None;

        /// <summary>
        /// Gets the sets of health record items to which this rule 
        /// grants permission.
        /// </summary>
        /// 
        /// <value>
        /// A read-only collection of type <see cref="AuthorizationSetDefinition"/>
        /// representing the items.
        /// </value>
        /// 
        public ReadOnlyCollection<AuthorizationSetDefinition> TargetSets
        {
            get { return _targetSets; }
        }
        private ReadOnlyCollection<AuthorizationSetDefinition> _targetSets;

        /// <summary>
        /// Gets the sets of health record items that are excluded by this 
        /// rule even if they are part of the <see cref="TargetSets"/>.
        /// </summary>
        /// 
        /// <value>
        /// A read-only collection of type <see cref="AuthorizationSetDefinition"/>
        /// representing the items.
        /// </value>
        /// 
        public ReadOnlyCollection<AuthorizationSetDefinition> ExceptionSets
        {
            get { return _exceptionSets; }
        }
        private ReadOnlyCollection<AuthorizationSetDefinition> _exceptionSets;

        /// <summary>
        /// Gets the XML representation of the <see cref="AuthorizationRule"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The XML representation of the <see cref="AuthorizationRule"/>.
        /// </returns>
        /// 
        public override string ToString()
        {
            return GetXml();
        }

        /// <summary>
        /// Retrieves the authorization XML for the specified rules.
        /// </summary>
        /// 
        /// <param name="rules">
        /// The authorization rules to be included in the authorization XML.
        /// </param>
        /// 
        /// <returns>
        /// A string containing the XML representation of the specified
        /// authorization rules.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="rules"/> is <b>null</b>.
        /// </exception>
        /// 
        public static string GetRulesXml(IList<AuthorizationRule> rules)
        {
            Validator.ThrowIfArgumentNull(rules, "rules", "GetRulesXmlNullRules");

            StringBuilder result = new StringBuilder();
            XmlWriterSettings settings = SDKHelper.XmlUnicodeWriterSettings;

            using (XmlWriter writer = XmlWriter.Create(result, settings))
            {
                writer.WriteStartElement("auth");
                {
                    writer.WriteStartElement("rules");

                    foreach (AuthorizationRule rule in rules)
                    {
                        writer.WriteRaw(rule.GetXml());
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            return result.ToString();
        }

        /// <summary>
        /// Gets the base-64 encoding of the authorization XML for the 
        /// specified rules.
        /// </summary>
        /// 
        /// <param name="rules">
        /// The authorization rules to be included in the authorization XML.
        /// </param>
        /// 
        /// <returns>
        /// A string containing the base-64 encoding of the XML representation 
        /// of the specified authorization rules.
        /// </returns>
        /// 
        /// <remarks>
        /// The base-64 encoding of the authorization rules can be used when
        /// calling the Shell web pages to authorize records, register an
        /// application, or set permissions on a record for a person or group.
        /// The base-64 encoding of the authorization rules is passed as a 
        /// value in the appropriate query string argument to the web page.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="rules"/> parameter is <b>null</b> or empty.
        /// </exception>
        /// 
        public static string GetBase64EncodedRulesXml(
            IList<AuthorizationRule> rules)
        {
            string authXml = GetRulesXml(rules);
            UTF8Encoding utf8 = new UTF8Encoding(false, true);
            Byte[] encodedBytes = utf8.GetBytes(authXml);
            string base64AuthXml = Convert.ToBase64String(encodedBytes);
            return base64AuthXml;
        }

        /// <summary>
        /// Gets the XML representation of the rule.
        /// </summary>
        /// 
        /// <returns>
        /// A string containing XML representing the rule.
        /// </returns>
        /// 
        public string GetXml()
        {
            StringBuilder result = new StringBuilder();
            XmlWriterSettings settings = SDKHelper.XmlUnicodeWriterSettings;

            using (XmlWriter writer = XmlWriter.Create(result, settings))
            {
                writer.WriteStartElement("rule");

                if (IsOptional)
                {
                    writer.WriteAttributeString("is-optional", "true");
                }

                if (!String.IsNullOrEmpty(Name))
                {
                    writer.WriteAttributeString("name", Name);
                }


                AppendReasons(writer);
                AppendDisplayFlags(writer);
                AppendPermissions(writer);
                AppendSets(writer, "target-set", this.TargetSets);
                AppendSets(writer, "exception-set", this.ExceptionSets);

                writer.WriteEndElement();
            }

            return result.ToString();
        }

        private void AppendReasons(XmlWriter writer)
        {
            CultureSpecificReasons.AppendLocalizedElements(
                writer,
                "reason");
        }


        private void AppendDisplayFlags(XmlWriter writer)
        {
            if (_displayFlags != AuthorizationRuleDisplayFlags.None)
            {
                writer.WriteStartElement("display-flags");
                writer.WriteValue((uint) _displayFlags);
                writer.WriteEndElement();
            }
        }

        private static void AppendSets(
            XmlWriter writer,
            string elementName,
            IList<AuthorizationSetDefinition> sets)
        {
            if (sets != null && sets.Count > 0)
            {
                bool wroteStartElement = false;

                // Need to process the set types in order to conform with
                // the schema

                SetProcessingState processingState = 
                    SetProcessingState.DateRange;

                for (;
                    (int)processingState <= (int)SetProcessingState.TypeId;
                    ++processingState)
                {
                    foreach (AuthorizationSetDefinition set in sets)
                    {
                        bool appendXml = false;
                        switch (set.SetType)
                        {
                            case SetType.DateRangeSet:
                                appendXml =
                                    processingState ==
                                    SetProcessingState.DateRange;
                                break;

                            case SetType.TypeIdSet:
                                appendXml =
                                    processingState ==
                                    SetProcessingState.TypeId;
                                break;

                            case SetType.UserTagSet:
                                appendXml =
                                    processingState ==
                                    SetProcessingState.UserTag;
                                break;

                            default:
                                // For forwards compatibility.
                                System.Diagnostics.Debug.Assert(
                                    false,
                                    "Got an AuthorizationSetDefinition type" +
                                    "that wasn't expected.");
                                break;
                        }

                        if (appendXml)
                        {
                            if (!wroteStartElement)
                            {
                                writer.WriteStartElement(elementName);
                                wroteStartElement = true;
                            }
                            writer.WriteRaw(set.GetXml());
                        }
                    }
                }

                if (wroteStartElement)
                {
                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Defines the states of a state machine that ensures that the sets 
        /// are processed in the appropriate order to match the XML schema 
        /// defined for target and exception sets.
        /// </summary>
        /// 
        private enum SetProcessingState
        {
            DateRange = 0,
            TypeId = 1,
            UserTag = 2,
        }

        private void AppendPermissions(XmlWriter writer)
        {
            foreach (HealthRecordItemPermissions permissionValue in
                Enum.GetValues(typeof(HealthRecordItemPermissions)))
            {
                if (permissionValue != HealthRecordItemPermissions.All &&
                    permissionValue != HealthRecordItemPermissions.None)
                {
                    if ((permissionValue & this.Permissions) != 0)
                    {
                        writer.WriteElementString("permission",
                            permissionValue.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Creates an instance of an AuthorizationRule object using
        /// the specified XML.
        /// </summary>
        /// 
        /// <param name="authNav">
        /// The XML containing the auth rules information.
        /// </param>
        /// 
        /// <returns>
        /// A Collection of AuthorizationRule objects parsed out from 
        /// the XML.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="authNav"/> is null.
        /// </exception>
        /// 
        public static Collection<AuthorizationRule> CreateFromXml(
            XPathNavigator authNav)
        {
            if (authNav == null)
            {
                throw new ArgumentNullException(
                    "authNav",
                    "The authNav can't be null."); 
            }

            XPathNodeIterator rulesIter = authNav.Select(
                    "auth/rules/rule");

            return CreateRules(rulesIter); 
        }

        internal static Collection<AuthorizationRule> CreateRules(
            XPathNodeIterator rulesNav)
        {
            return CreateRules(rulesNav, false);
        }

        internal static Collection<AuthorizationRule> CreateRules(
            XPathNodeIterator rulesNav,
            bool onlyAllowTypeIdSets)
        {
            Collection<AuthorizationRule> result =
                new Collection<AuthorizationRule>();

            foreach (XPathNavigator ruleNav in rulesNav)
            {
                AuthorizationRule rule 
                    = CreateRule(ruleNav, onlyAllowTypeIdSets);
                if (rule != null)
                {
                    result.Add(rule);
                }
            }
            return result;
        }

        internal static AuthorizationRule CreateRule(
            XPathNavigator ruleNav,
            bool onlyAllowTypeIdSets)
        {
            bool isOptional = false;

            string isOptionalStr
                = ruleNav.GetAttribute("is-optional", String.Empty);
            if (!String.IsNullOrEmpty(isOptionalStr))
            {
                try
                {
                    isOptional = XmlConvert.ToBoolean(isOptionalStr);
                }
                catch (FormatException)
                {
                }
            }
            string ruleName = ruleNav.GetAttribute("name", String.Empty);
                
            // <displayflags>
            AuthorizationRuleDisplayFlags displayFlags = AuthorizationRuleDisplayFlags.None;
            XPathNavigator displayFlagsNav = ruleNav.SelectSingleNode("display-flags");
            if (displayFlagsNav != null)
            {
                try
                {
                    UInt32 flags = XmlConvert.ToUInt32(displayFlagsNav.Value);
                    displayFlags = (AuthorizationRuleDisplayFlags)flags;
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
            }
                
            HealthRecordItemPermissions permissions 
                = HealthRecordItemPermissions.None;
            XPathNodeIterator permissionsIterator =
                ruleNav.Select("permission");

            foreach (XPathNavigator permissionNav in permissionsIterator)
            {
                permissions |=
                    (HealthRecordItemPermissions)Enum.Parse(
                            typeof(HealthRecordItemPermissions), 
                            permissionNav.Value);
            }

            List<AuthorizationSetDefinition> targetSets =
                GetSets(ruleNav, "target-set");

            List<AuthorizationSetDefinition> exceptionSets =
                GetSets(ruleNav, "exception-set");

            // only do this if only type ID sets are allowed 
            //                      AND 
            // we have either some target set or exception set data
            // if we enter the if block w/o any target or exception set data to 
            // begin with, then we'll incorrectly return <b>null</b> when the XML
            // specified that permissions were allowed on all record data.
            if (onlyAllowTypeIdSets
                && (targetSets != null || exceptionSets != null))
            {
                if (targetSets != null)
                    for (int i = targetSets.Count - 1; i >= 0; --i)
                        if (!(targetSets[i] is TypeIdSetDefinition))
                            targetSets.RemoveAt(i);

                if (exceptionSets != null)
                    for (int i = exceptionSets.Count - 1; i >= 0; --i)
                        if (!(exceptionSets[i] is TypeIdSetDefinition))
                            exceptionSets.RemoveAt(i);

                if ((targetSets == null || targetSets.Count == 0)
                    && (exceptionSets == null || exceptionSets.Count ==0))
                    return null;
            }

            AuthorizationRule rule = new AuthorizationRule(
                    ruleName,
                    null,
                    permissions,
                    targetSets,
                    exceptionSets,
                    isOptional,
                    displayFlags);
            
            // <reason>
            // Do this after create the rule so that the CultureSpecificReasons dictionary is available.
            rule.CultureSpecificReasons.PopulateFromXml(ruleNav, "reason");

            return rule;
        }

        internal static List<AuthorizationSetDefinition> GetSets(
            XPathNavigator ruleNav,
            string elementName)
        {
            List<AuthorizationSetDefinition> result = null;

            XPathNodeIterator setsIterator =
                ruleNav.Select(elementName);

            foreach (XPathNavigator set in setsIterator)
            {
                if (result == null)
                {
                    result = new List<AuthorizationSetDefinition>();
                }
                result.AddRange(CreateSets(set));
            }
            return result;
        }

        internal static List<AuthorizationSetDefinition> CreateSets(
            XPathNavigator setNav)
        {
            List<AuthorizationSetDefinition> sets =
                new List<AuthorizationSetDefinition>();

            // date range
            XPathNodeIterator dateSets = setNav.Select("date-range");
            foreach (XPathNavigator dateSet in dateSets)
            {
                DateTime dateMin = DateTime.MinValue;
                XPathNavigator dateMinNav =
                    dateSet.SelectSingleNode("date-min");
                if (dateMinNav != null)
                {
                    dateMin = dateMinNav.ValueAsDateTime;
                }

                DateTime dateMax = DateTime.MaxValue;
                XPathNavigator dateMaxNav =
                    dateSet.SelectSingleNode("date-max");
                if (dateMaxNav != null)
                {
                    dateMax = dateMaxNav.ValueAsDateTime;
                }

                sets.Add(new DateRangeSetDefinition(dateMin, dateMax));
            }

            // TypeId
            XPathNodeIterator typeSets = setNav.Select("type-id");
            foreach (XPathNavigator typeSet in typeSets)
            {
                Guid setId = new Guid(typeSet.Value);
                sets.Add(new TypeIdSetDefinition(setId));
            }

            return sets;
        }

    }
}
