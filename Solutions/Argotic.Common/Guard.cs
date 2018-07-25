namespace Argotic.Common
{
    using System;

    /// <summary>
    /// Provides common validation methods shared across the framework entities. This class cannot be inherited.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not a null reference.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is a <b>null</b> reference, an <see cref="ArgumentNullException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not a null reference or an empty string.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is a <b>null</b> reference or an empty string, an <see cref="ArgumentNullException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotNullOrEmptyString(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not greater than the specified maximum.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="maximum">The maximum acceptable value.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>greater than</b> the specified <paramref name="maximum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotGreaterThan(int value, string name, int maximum)
        {
            if (value > maximum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not greater than the specified maximum.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="maximum">The maximum acceptable value.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>greater than</b> the specified <paramref name="maximum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotGreaterThan(long value, string name, long maximum)
        {
            if (value > maximum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not greater than the specified maximum.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="maximum">The maximum acceptable value.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>greater than</b> the specified <paramref name="maximum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotGreaterThan(decimal value, string name, decimal maximum)
        {
            if (value > maximum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not less than the specified minimum.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="minimum">The minimum acceptable value.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>less than</b> the specified <paramref name="minimum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotLessThan(int value, string name, int minimum)
        {
            if (value < minimum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not less than the specified minimum.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="minimum">The minimum acceptable value.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>less than</b> the specified <paramref name="minimum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotLessThan(long value, string name, long minimum)
        {
            if (value < minimum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is not less than the specified minimum.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="minimum">The minimum acceptable value.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>less than</b> the specified <paramref name="minimum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotLessThan(decimal value, string name, decimal minimum)
        {
            if (value < minimum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is within the specified range.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="minimum">The minimum acceptable value of the range.</param>
        /// <param name="maximum">The maximum acceptable value of the range.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>less than</b> the specified <paramref name="minimum"/> <u>or</u> <b>greater than</b> the specified <paramref name="maximum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotOutOfRange(int value, string name, int minimum, int maximum)
        {
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is within the specified range.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="minimum">The minimum acceptable value of the range.</param>
        /// <param name="maximum">The maximum acceptable value of the range.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>less than</b> the specified <paramref name="minimum"/> <u>or</u> <b>greater than</b> the specified <paramref name="maximum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotOutOfRange(long value, string name, int minimum, long maximum)
        {
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        /// <summary>
        /// Validates that the supplied <paramref name="value"/> is within the specified range.
        /// </summary>
        /// <param name="value">The value of the method argument to validate.</param>
        /// <param name="name">The name of the method argument.</param>
        /// <param name="minimum">The minimum acceptable value of the range.</param>
        /// <param name="maximum">The maximum acceptable value of the range.</param>
        /// <remarks>
        ///     If the <paramref name="value"/> is <b>less than</b> the specified <paramref name="minimum"/> <u>or</u> <b>greater than</b> the specified <paramref name="maximum"/>,
        ///     an <see cref="ArgumentOutOfRangeException"/> is raised using the supplied <paramref name="name"/>.
        /// </remarks>
        public static void ArgumentNotOutOfRange(decimal value, string name, int minimum, decimal maximum)
        {
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }
}