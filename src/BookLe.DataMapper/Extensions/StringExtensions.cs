using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLe.DataMapper.Extensions
{
    public static class StringExtensions
    {

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        [DebuggerStepThrough]
        public static bool IsNotEmpty(this string value)
        {
            return !value.IsNullOrEmpty();
        }

        [DebuggerStepThrough]
        public static string Take(this string value, int count)
        {
            if (value.IsNullOrEmpty()) return value;
            if (value.Length <= count) return value;
            return value.Substring(0, count);
        }

        public static string TakeLast(this string value, int count)
        {
            if (value.IsNullOrEmpty()) return value;
            if (value.Length <= count) return value;
            return value.Substring(value.Length - count);
        }

        public static T? ToNullable<T>(this string value, Func<string, T> map) where T : struct
        {
            if (value.IsNullOrEmpty()) return null;
            return map(value);
        }


        private static T? _toNumber<T>(string s, Func<string, T> parse) where T : struct
        {
            if (!IsNumber(s)) return null;
            return parse(s);
        }

        [DebuggerStepThrough]
        public static decimal? ToDecimal(this string value)
        {
            return _toNumber(value, decimal.Parse);
        }

        [DebuggerStepThrough]
        public static double? ToDouble(this string value)
        {
            return _toNumber(value, double.Parse);
        }

        [DebuggerStepThrough]
        public static float? ToFloat(this string value)
        {
            return _toNumber(value, float.Parse);
        }

        [DebuggerStepThrough]
        public static int? ToInt(this string value)
        {
            return _toNumber(value, int.Parse);
        }

        [DebuggerStepThrough]
        public static long? ToLong(this string value)
        {
            return _toNumber(value, long.Parse);
        }

        [DebuggerStepThrough]
        public static bool IsNumber(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text[0] == '-')
            {
                text = text.Substring(1);
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
            }

            if (text.Where(c => c == '.').Count() > 1) return false;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsNumber(text, i) || text[i] == '.') continue;
                return false;
            }

            return true;
        }

        public static DateTime? ToDate(this string text)
        {
            if (DateTime.TryParse(text, out var date))
            {
                return date;
            }

            return null;
        }

        public static Guid? ToGuid(this string source)
        {
            if (Guid.TryParse(source, out var guid))
            {
                return guid;
            }

            return null;
        }

        public static string Format(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static bool HasUpperCase(this string source)
        {
            return source.Any(char.IsUpper);
        }

        public static bool HasLowerCase(this string source)
        {
            return source.Any(char.IsLower);
        }

        public static bool HasDigit(this string source)
        {
            return source.Any(char.IsDigit);
        }

        public static bool HasLetter(this string source)
        {
            return source.Any(char.IsLetter);
        }

        /// <summary>
        /// Special Characters: ! " # $ % & ' ( ) * + , - . / : ; < = > ? @ [ \ ] ^ _ ` { | } ~
        /// </summary>
        public static bool HasSpecialCharacter(this string source)
        {
            return source.Any(c => char.IsPunctuation(c) || char.IsSymbol(c));
        }

        public static bool HasOneOfThese(this string source, char[] characters)
        {
            return source.Any(characters.Contains);
        }

        [DebuggerStepThrough]
        public static string TrimValue(this string value)
        {
            if (value == null) return value;
            return value.Trim();
        }

        public static string Filter(this string source, Func<char, bool> predicate)
        {
            return new string(source.Where(predicate).ToArray());

        }

        
    }
}
