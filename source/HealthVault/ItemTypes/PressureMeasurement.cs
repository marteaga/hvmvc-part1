﻿// Copyright(c) Microsoft Corporation.
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
    /// Represents a pressure measurement and a display value
    /// associated with the measurement.
    /// </summary>
    /// 
    /// <remarks>
    /// In HealthVault, pressure measurements have values and display values. 
    /// All values are stored in a standard SI unit of pascal (Pa). 
    /// An application can take a pressure value using any scale the application 
    /// chooses and can store the user-entered value as the display value, 
    /// but the pressure value must be converted to pascals to be stored in HealthVault.
    /// </remarks>
    /// 
    public class PressureMeasurement : Measurement<double>
    {
         /// <summary>
        /// Creates a new instance of the <see cref="PressureMeasurement"/> class 
        /// with empty values.
        /// </summary>
        /// 
        public PressureMeasurement()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PressureMeasurement"/> class 
        /// with the specified value in pascal.
        /// </summary>
        /// 
        /// <param name="pascals">
        /// The pressure value in pascal.
        /// </param>
        /// 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "pascals is a valid element name.")]
        public PressureMeasurement(double pascals)
            : base(pascals)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PressureMeasurement"/> class with 
        /// the specified value in pascals and an optional display value.
        /// </summary>
        /// 
        /// <param name="pascals">
        /// The pressure in pascal.
        /// </param>
        /// 
        /// <param name="displayValue">
        /// The display value of the pressure. This should contain the
        /// exact pressure as entered by the user even if it uses some
        /// other unit of measure besides pascal. The display value
        /// <see cref="Microsoft.Health.ItemTypes.DisplayValue.Units"/> and 
        /// <see cref="Microsoft.Health.ItemTypes.DisplayValue.UnitsCode"/> 
        /// represents the unit of measure for the user-entered value.
        /// </param>
        /// 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "pascals is a valid element name.")]
        public PressureMeasurement(double pascals, DisplayValue displayValue)
            : base(pascals, displayValue)
        {
        }

        /// <summary>
        /// Verifies the value is a legal pressure value in Pa.
        /// </summary>
        /// 
        /// <param name="value">
        /// The pressure measurement.
        /// </param>
        /// 
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="value"/> parameter is less than zero.
        /// </exception>
        /// 
        protected override void AssertMeasurementValue(double value)
        {
            Validator.ThrowArgumentOutOfRangeIf(
                value < 0.0,
                "value",
                "PressureNotPositive");
        }

        /// <summary> 
        /// Populates the data for the pressure from the XML.
        /// </summary>
        /// 
        /// <param name="navigator"> 
        /// The XML node representing the pressure.
        /// </param>
        /// 
        protected override void ParseValueXml(XPathNavigator navigator)
        {
            Value = navigator.SelectSingleNode("pascals").ValueAsDouble;
        }

        /// <summary> 
        /// Writes the pressure to the specified XML writer.
        /// </summary>
        /// 
        /// <param name="writer"> 
        /// The XmlWriter to write the pressure to.
        /// </param>
        /// 
        protected override void WriteValueXml(XmlWriter writer)
        {
            writer.WriteElementString(
                "pascals",
                XmlConvert.ToString(Value));
        }

        /// <summary>
        /// Gets a string representation of the pressure in the base units.
        /// </summary>
        /// 
        /// <returns>
        /// The pressure as a string in the base units.
        /// </returns>
        /// 
        protected override string GetValueString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
