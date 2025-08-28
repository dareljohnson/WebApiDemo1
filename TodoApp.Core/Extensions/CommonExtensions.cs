using System;
using System.Collections.Generic;
using System.Linq;

namespace TodoApp.Core.Extensions
{
    /// <summary>
    /// Extension methods for common operations
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Safely converts string to enum
        /// </summary>
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
                
            return Enum.TryParse<T>(value, true, out T result) ? result : defaultValue;
        }
        
        /// <summary>
        /// Checks if collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }
        
        /// <summary>
        /// Safe substring operation
        /// </summary>
        public static string SafeSubstring(this string value, int length)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
                
            return value.Length <= length ? value : value.Substring(0, length);
        }
        
        /// <summary>
        /// Converts DateTime to display string
        /// </summary>
        public static string ToDisplayString(this DateTime dateTime)
        {
            return dateTime.ToString("MMM dd, yyyy HH:mm");
        }
        
        /// <summary>
        /// Converts nullable DateTime to display string
        /// </summary>
        public static string ToDisplayString(this DateTime? dateTime)
        {
            return dateTime?.ToDisplayString() ?? "Not set";
        }
    }
}
