// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Health.ItemTypes.Csv
{
    /// <summary>
    /// Represents a floating-point number entry in the CSV list
    /// </summary>
    internal class OtherItemDataCsvDouble : OtherItemDataCsvItem
    {
        /// <summary>
        /// Create an instance of the <see cref="OtherItemDataCsvDouble"/> type with a double value.
        /// </summary>
        /// <param name="value">The value to store in the instance.</param>
        internal OtherItemDataCsvDouble(double value)
        {
            Value = value;
        }

        private double _value;

        /// <summary>
        /// Gets or sets the value of the double entry.
        /// </summary>
        internal double Value
        {
            get { return _value; }
            set { _value = value; }
        }

    }
}