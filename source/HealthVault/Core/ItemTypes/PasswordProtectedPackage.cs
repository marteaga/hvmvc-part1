// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health.ItemTypes
{
    /// <summary>
    /// Provides metadata about the encryption algorithm and parameters used to 
    /// protect some data with a password.
    /// </summary>
    /// 
    /// <remarks>
    /// The <see cref="PasswordProtectedPackage"/> item type defines the metadata for the
    /// encryption algorithm used to protect data with a password. The 
    /// application should generate a password (or take it from the user) and
    /// encrypt the desired data. This data should be set in a <see cref="Blob"/> created off the
    /// <see cref="BlobStore"/> retrieved from the 
    /// <see cref="HealthRecordItem.GetBlobStore(Microsoft.Health.HealthRecordAccessor)"/>.
    /// The properties of the Blob should be set with the parameters required 
    /// to decrypt the data. These parameters are application dependant but 
    /// should adhere to standard practices in dealing with PKCS5v2 data.
    /// </remarks>
    /// 
    public class PasswordProtectedPackage : HealthRecordItem
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PasswordProtectedPackage"/> class 
        /// with default values.
        /// </summary>
        /// 
        /// <remarks>
        /// The item is not added to the health record until the
        /// <see cref="Microsoft.Health.HealthRecordAccessor.NewItem(HealthRecordItem)"/> 
        /// method is called.
        /// </remarks>
        /// 
        public PasswordProtectedPackage()
            : base(TypeId)
        {
        }
 
        /// <summary>
        /// Creates a new instance of the <see cref="PasswordProtectedPackage"/> class 
        /// specifying the mandatory values.
        /// </summary>
        /// 
        /// <param name="algorithm">
        /// The name of the algorithm used to protect the data.
        /// </param>
        /// 
        /// <param name="salt">
        /// A string representing the encoding of the bytes that were used as
        /// the salt when protecting the data.
        /// </param>
        /// 
        /// <param name="keyLength">
        /// The number of bits used by the algorithm.
        /// </param>
        /// 
        /// <remarks>
        /// In general, the salt is a series of bytes encoded in an 
        /// application-dependent way. The length of the salt must match the 
        /// algorithm. It is recommended that the salt encoding be base64.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The <paramref name="salt"/> parameter is <b>null</b> or empty.
        /// </exception>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="keyLength"/> parameter is negative or zero.
        /// </exception>
        /// 
        public PasswordProtectedPackage(
            PasswordProtectAlgorithm algorithm, 
            string salt, 
            int keyLength)
            : base(TypeId)
        {
            this.PasswordProtectAlgorithm = algorithm;
            this.Salt = salt;
            this.KeyLength = keyLength;
        }
        
        /// <summary>
        /// Retrieves the unique identifier for the item type.
        /// </summary>
        /// 
        /// <value>
        /// A GUID.
        /// </value>
        /// 
        public new static readonly Guid TypeId =
            new Guid("c9287326-bb43-4194-858c-8b60768f000f");

        /// <summary>
        /// Populates this PasswordProtectedPackage instance from the data 
        /// in the XML.
        /// </summary>
        /// 
        /// <param name="typeSpecificXml">
        /// The XML to get the file data from.
        /// </param>
        /// 
        /// <exception cref="InvalidOperationException">
        /// The first node in the <paramref name="typeSpecificXml"/> 
        /// parameter is not a file node.
        /// </exception>
        /// 
        protected override void ParseXml(IXPathNavigable typeSpecificXml)
        {
            XPathNavigator packageNav =
                typeSpecificXml.CreateNavigator().SelectSingleNode(
                    "password-protected-package/encrypt-algorithm");

            Validator.ThrowInvalidIfNull(packageNav, "PackageUnexpectedNode");

            _algorithmName =
                packageNav.SelectSingleNode("algorithm-name").Value;

            switch (_algorithmName)
            {
                case "none":
                    _algorithm = PasswordProtectAlgorithm.None;
                    break;

                case "hmac-sha1-3des":
                    _algorithm = PasswordProtectAlgorithm.HmacSha13Des;
                    break;

                case "hmac-sha256-aes256":
                    _algorithm = PasswordProtectAlgorithm.HmacSha256Aes256;
                    break;

                default:
                    _algorithm = PasswordProtectAlgorithm.Unknown;
                    break;
            }

            _salt = packageNav.SelectSingleNode("parameters/salt").Value;
            _hashIterations = 
                packageNav.SelectSingleNode(
                    "parameters/iteration-count").ValueAsInt;

            _keyLength =
                packageNav.SelectSingleNode(
                    "parameters/key-length").ValueAsInt;

        }

        /// <summary>
        /// Writes the file data to the specified XmlWriter.
        /// </summary>
        /// 
        /// <param name="writer">
        /// The XmlWriter to write the password protected package to.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="writer"/> is <b>null</b>.
        /// </exception>
        /// 
        public override void WriteXml(XmlWriter writer)
        {
            Validator.ThrowIfWriterNull(writer);

            Validator.ThrowSerializationIf(
                _algorithm == PasswordProtectAlgorithm.Unknown &&
                _algorithmName == null,
                "PackageAlgorithmNotSet");

            Validator.ThrowSerializationIf(String.IsNullOrEmpty(_salt), "PackageSaltNotSet");

            Validator.ThrowSerializationIf(_keyLength < 1, "PackageKeyLengthNotSet");

            // <password-protected-package>
            writer.WriteStartElement("password-protected-package");

            // <encrypt-algorithm>
            writer.WriteStartElement("encrypt-algorithm");

            switch (_algorithm)
            {
                case PasswordProtectAlgorithm.None:
                    writer.WriteElementString("algorithm-name", "none");
                    break;

                case PasswordProtectAlgorithm.HmacSha13Des:
                    writer.WriteElementString("algorithm-name", "hmac-sha1-3des");
                    break;

                case PasswordProtectAlgorithm.HmacSha256Aes256:
                    writer.WriteElementString("algorithm-name", "hmac-sha256-aes256");
                    break;

                case PasswordProtectAlgorithm.Unknown:
                    writer.WriteElementString("algorithm-name", _algorithmName);
                    break;
            }

            // <parameters>
            writer.WriteStartElement("parameters");

            writer.WriteElementString("salt", _salt);
            writer.WriteElementString(
                "iteration-count", 
                _hashIterations.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString(
                "key-length",
                _keyLength.ToString(CultureInfo.InvariantCulture));

            // </parameters>
            writer.WriteEndElement();

            // </encrypt-algorithm>
            writer.WriteEndElement();

            // </password-protected-package>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets or sets the algorithm used to encrypt the package.
        /// </summary>
        /// 
        /// <value>
        /// An instance of <see cref="PasswordProtectAlgorithm"/> 
        /// representing the algorithm.
        /// </value>
        /// 
        public PasswordProtectAlgorithm PasswordProtectAlgorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; }
        }
        private PasswordProtectAlgorithm _algorithm = 
            PasswordProtectAlgorithm.None;
        private string _algorithmName;

        /// <summary>
        /// Gets or sets the salt used when encrypting the package.
        /// </summary>
        /// 
        /// <value>
        /// A string representing the salt.
        /// </value>
        /// 
        /// <remarks>
        /// In general, the salt is a series of bytes encoded in an 
        /// application-dependent way. The length of the salt must match the 
        /// algorithm. It is recommended that the salt encoding be base64.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="value"/> parameter is <b>null</b>, empty, or contains only
        /// whitespace on set.
        /// </exception>
        /// 
        public string Salt
        {
            get { return _salt; }
            set
            {
                Validator.ThrowIfStringNullOrEmpty(value, "Salt");
                Validator.ThrowIfStringIsWhitespace(value, "Salt");
                _salt = value;
            }
        }
        private string _salt;

        /// <summary>
        /// Gets or sets the number of hash iterations taken when protecting
        /// the package.
        /// </summary>
        /// 
        /// <value>
        /// An integer representing the number of iterations. The default value 
        /// is 20000 iterations.
        /// </value>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="value"/> parameter is less than or equal to zero.
        /// </exception>
        /// 
        public int HashIterations
        {
            get
            {
                return _hashIterations;
            }
            set
            {
                Validator.ThrowArgumentOutOfRangeIf(value <= 0, "HashIterations", "PackageHashIterationOutOfRange");
                _hashIterations = value;
            }
        }
        // Default to 20k (same as PFX in Windows)
        private int _hashIterations = 20000;

        /// <summary>
        /// Gets or sets the key length in bits.
        /// </summary>
        /// 
        /// <value>
        /// An integer representing the key length.
        /// </value>
        /// 
        /// <remarks>
        /// The value should match that of the algorithm, for example, 168 bits
        /// for 3DES and 256 bits for AES256.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="value"/> parameter is less than one.
        /// </exception>
        /// 
        public int KeyLength
        {
            get { return _keyLength; }
            set 
            {
                Validator.ThrowArgumentOutOfRangeIf(
                    value < 1,
                    "KeyLength", 
                    "PackageKeyLengthOutOfRange");
                _keyLength = value; 
            }
        }
        private int _keyLength;

        /// <summary>
        /// Gets a string representation of the password protected package definition.
        /// </summary>
        /// 
        /// <returns>
        /// A string representation of the password protected package.
        /// </returns>
        /// 
        public override string ToString()
        {
            string result = null;
            switch (PasswordProtectAlgorithm)
            {
                case PasswordProtectAlgorithm.HmacSha13Des:
                    result = "hmac-sha1-3des";
                    break;

                case PasswordProtectAlgorithm.HmacSha256Aes256:
                    result = "hmac-sha256-aes256";
                    break;

                case PasswordProtectAlgorithm.None:
                    result = "none";
                    break;

                default:
                    result = "unknown";
                    break;
            }
            return result;
        }
    }
}
