// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Health
{
    /// <summary>
    /// Uniquely identifies a health record item in the system.
    /// </summary>
    /// 
    public class HealthRecordItemKey
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HealthRecordItemKey"/>
        /// class with the specified globally unique ID for the 
        /// <see cref="HealthRecordItem"/> and globally unique version stamp.
        /// </summary>
        /// 
        /// <param name="id">
        /// A globally unique identifier for the <see cref="HealthRecordItem"/>
        /// in the system.
        /// </param>
        /// 
        /// <param name="versionStamp">
        /// A globally unique identifier for the version of the <see cref="HealthRecordItem"/>
        /// in the system.
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="id"/> or <paramref name="versionStamp"/> 
        /// parameter is Guid.Empty.
        /// </exception>
        /// 
        public HealthRecordItemKey(Guid id, Guid versionStamp)
        {
            Validator.ThrowArgumentExceptionIf(
                id == Guid.Empty,
                "id",
                "ThingIdInvalid");

            Validator.ThrowArgumentExceptionIf(
                versionStamp == Guid.Empty,
                "versionStamp",
                "ThingVersionInvalid");

            _thingId = id;
            _versionStamp = versionStamp;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="HealthRecordItemKey"/>
        /// class with the specified globally unique ID for the 
        /// <see cref="HealthRecordItem"/>.
        /// </summary>
        /// 
        /// <param name="id">
        /// A globally unique identifier for the <see cref="HealthRecordItem"/>
        /// in the system.
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="id"/> is Guid.Empty.
        /// </exception>
        /// 
        public HealthRecordItemKey(Guid id)
        {
            Validator.ThrowArgumentExceptionIf(
                id == Guid.Empty,
                "id",
                "ThingIdInvalid");

            _thingId = id;
        }
        
        /// <summary>
        /// Gets the unique identifier of the <see cref="HealthRecordItem"/>.
        /// </summary>
        /// 
        /// <value>
        /// A globally unique identifier for the <see cref="HealthRecordItem"/>, 
        /// issued when the item is created.
        /// </value>
        ///
        public Guid Id
        {
            get { return _thingId; }
        }
        private Guid _thingId;

        /// <summary>
        /// Gets the unique version stamp of the <see cref="HealthRecordItem"/>.
        /// </summary>
        /// 
        ///<value>
        /// A globally unique identifier that represents the version of the
        /// <see cref="HealthRecordItem"/>. A new version stamp is issued each 
        /// time the item is changed.
        ///</value>
        /// 
        ///<remarks>
        /// The version stamp of the current version of a <see cref="HealthRecordItem"/>
        /// is always equal to the <see cref="Id"/> of that item.
        ///</remarks>
        ///
        public Guid VersionStamp
        {
            get { return _versionStamp; }
        }
        private Guid _versionStamp;
        
        /// <summary>
        /// Gets a string representation of the key.
        /// </summary>
        /// 
        /// <returns> 
        /// <see cref="VersionStamp"/>.ToString().
        /// </returns>
        /// 
        public override string ToString()
        {
            if (_versionStamp != Guid.Empty)
            {
                return _thingId.ToString() + "," + _versionStamp.ToString();
            }
            return _thingId.ToString();
        }

        /// <summary>
        /// Compares one <see cref="HealthRecordItemKey"/> to another.
        /// </summary>
        /// 
        /// <param name="obj">
        /// The <see cref="HealthRecordItemKey"/> to compare against this.
        /// </param>
        /// 
        /// <returns>
        /// <b>true</b> if both the health record item keys have
        /// the same ID and version stamp; otherwise, <b>false</b>.
        /// </returns>
        /// 
        public override bool Equals(Object obj)
        {
            bool result = false;
            HealthRecordItemKey rVal = obj as HealthRecordItemKey;
            
            if (rVal != null)
            {
                result =  (_versionStamp == rVal.VersionStamp)
                       && (_thingId == rVal.Id);
            }
            return result;
        }

        /// <summary>
        /// Gets the hashcode value for the object.
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="VersionStamp"/>.GetHashCode().
        /// </returns>
        /// 
        public override int GetHashCode()
        {
            return _versionStamp.GetHashCode();
        }
    }
}

