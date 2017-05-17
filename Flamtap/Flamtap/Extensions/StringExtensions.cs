﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flamtap.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Evaluates whether or not a string consists of only ASCII characters. 
        /// </summary>
        /// <param name="value">The string to evaluate</param>
        /// <returns>True if the string contains only ASCII characters.</returns>
        public static bool IsAscii(this string value)
        {
            /* It could be argued that IsAscii() should return true when given an empty string, because an empty string
             * isn't *not* ASCII. However, if one is checking a string's contents, they probably meant for the string
             * to hold some kind of value in the first place, so we may as well break. */
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            const int maxAsciiCode = 127;

            return value.All(c => c <= maxAsciiCode);
        }

        /// <summary>
        /// Remove all characters from a string that are not a letter or a number.
        /// </summary>
        public static string RemoveNonAlphanumeric(this string value)
        {
            return new Regex("[^a-zA-Z0-9 ]").Replace(value, string.Empty);
        }

        /// <summary>
        /// Splits up a string of unix-style arguments into an array of arguments.
        /// </summary>
        /// <param name="s">The arguments string to parse.</param>
        /// <returns></returns>
        public static string[] SplitUnixArgs(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return new string[0];

            List<string> args = new List<string>();

            int start = 0, length = 0;

            if (!s.StartsWith("-")) //the first token is a verb
            {
                start = s.IndexOf(" ", StringComparison.Ordinal);

                if (start == -1)
                    return s.Yield().ToArray();

                args.Add(s.Substring(0, start));
            }

            while (length != -1)
            {
                length = s.IndexOf(" -", start + 1, StringComparison.Ordinal);

                string toAdd = length == -1
                    ? s.Substring(start, s.Length - start)
                    : s.Substring(start, length - start);

                start += toAdd.Length;

                if (!string.IsNullOrWhiteSpace(toAdd))
                    args.Add(toAdd.Trim());
            }

            return args.ToArray();
        }

        /// <summary>
        /// Replace the diacritic characters in in a string with their ASCII equivalents (if possible).
        /// e.g. "Éric Søndergard".StripDiacritics() == "Eric Sondergard"
        /// </summary>
        /// <param name="value">The value with diacratics.</param>
        /// <returns>The value without diacritics.</returns>
        /// <see cref="http://stackoverflow.com/a/249126/1672990"/>
        public static string StripDiacritics(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            string normalizedString = value.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Splits a camel-case string into individual words.
        /// (e.g. "HomerSimpson".ToDisplayName() == "Homer Simpson")
        /// </summary>
        /// <param name="value">The value.</param>
        public static string ToDisplayText(this string value)
        {
            return Regex.Replace(value, @"([a-z](?=[A-Z0-9])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        /// <summary>
        /// Converts the string to a valid file name by replacing invalid chars with underscores or a given value.
        /// (e.g. "08/03/2017".ToValidFileName() == "Backup on 08_03_2017")
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="replacement">The string to replace invalid characters with.</param>
        /// <returns>A valid filename.</returns>
        public static string ToValidFileName(this string value, string replacement = "_")
        {
            if (string.IsNullOrEmpty(value))
                return value;

            char[] invalidChars = Path.GetInvalidFileNameChars();

            if (replacement.ToCharArray().Intersect(invalidChars).Any())
                throw new ArgumentException($"{nameof(replacement)} cannot contain invalid file name characters.",
                    nameof(replacement));

            StringBuilder result = new StringBuilder();

            foreach (char c in value.StripDiacritics())
            {
                if (' ' <= c && c <= '~' && Array.IndexOf(invalidChars, c) < 0)
                    result.Append(c);
                else
                    result.Append(replacement);
            }

            return result.ToString();
        }
    }
}
