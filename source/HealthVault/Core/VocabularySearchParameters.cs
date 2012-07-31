// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Globalization;

namespace Microsoft.Health
{
    /// <summary>
    /// The set of search parameters are used with the Vocabulary Search feature to specify the 
    /// vocabulary etc.
    /// </summary>
    public class VocabularySearchParameters 
    {
        /// <summary>
        /// Creates a vocabulary search parameter set with the <see cref="VocabularyKey"/> that is
        /// used to identify the vocabulary to search.
        /// </summary>
        /// <param name="vocabulary">
        /// A key to identify the vocabulary that is searched.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="vocabulary"/> is <b>null</b>.
        /// </exception>
        public VocabularySearchParameters(VocabularyKey vocabulary) 
        {
            Validator.ThrowIfArgumentNull(vocabulary, "vocabulary", "VocabularyKeyNullOrEmpty");
            _vocabulary = vocabulary;
        }

        /// <summary>
        /// Gets the vocabulary key used to identify the vocabulary to be searched.
        /// </summary>
        public VocabularyKey Vocabulary
        {
            get { return _vocabulary; }
        }
        private VocabularyKey _vocabulary;

        /// <summary>
        /// Gets or sets the culture in which the vocabulary will be searched. 
        /// </summary>
        /// <remarks>
        /// If the culture is not set, the current UI culture will be used by default.
        /// </remarks>
        public CultureInfo Culture
        {
            get 
            { 
                return _culture ?? CultureInfo.CurrentUICulture; 
            }

            set { _culture = value; }
        }
        private CultureInfo _culture;

        /// <summary>
        /// Gets or sets the maximum number of results to be returned from the search.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is a negative number.
        /// </exception>
        public int? MaxResults
        {
            get { return _maxResults; }
            set 
            {
                Validator.ThrowArgumentOutOfRangeIf(
                    value < 0,
                    "MaxResults",
                    "VocabularySearchMaxResultsInvalid");
                _maxResults = value; 
            }
        }
        private int? _maxResults;
    }
}