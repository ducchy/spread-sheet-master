using System.IO;
using System.Linq;

namespace SpreadSheetMaster.Editor
{
    public static class StringUtility
    {
        private static readonly System.Text.RegularExpressions.Regex INVALID_FILENAME_REGEX =
            new System.Text.RegularExpressions.Regex(
                "[\\x00-\\x1f<>:\"/\\\\|?*]" +
                "|^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9]|CLOCK\\$)(\\.|$)" +
                "|[\\. ]$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        public static string Convert(string str, NamingConvention from, NamingConvention to)
        {
            switch (from)
            {
                case NamingConvention.SnakeCase:
                    switch (to)
                    {
                        case NamingConvention.LowerCamelCase: return SnakeToLowerCamel(str);
                        case NamingConvention.UpperCamelCase: return SnakeToUpperCamel(str);
                        case NamingConvention.UpperSnakeCase: return SnakeToUpperSnake(str);
                        case NamingConvention.KebabCase: return SnakeToKebab(str);
                    }

                    break;
                case NamingConvention.UpperSnakeCase:
                    switch (to)
                    {
                        case NamingConvention.SnakeCase: return UpperSnakeToSnake(str);
                        case NamingConvention.LowerCamelCase: return UpperSnakeToLowerCamel(str);
                        case NamingConvention.UpperCamelCase: return UpperSnakeToUpperCamel(str);
                        case NamingConvention.KebabCase: return SnakeToKebab(str);
                    }

                    break;
                
                case NamingConvention.LowerCamelCase:
                    switch (to)
                    {
                        case NamingConvention.SnakeCase: return CamelToSnake(str);
                        case NamingConvention.UpperSnakeCase: return CamelToUpperSnake(str);
                        case NamingConvention.KebabCase: return CamelToKebab(str);
                    }
                    break;
                case NamingConvention.UpperCamelCase:
                    switch (to)
                    {
                        case NamingConvention.SnakeCase: return CamelToSnake(str);
                        case NamingConvention.UpperSnakeCase: return CamelToUpperSnake(str);
                        case NamingConvention.KebabCase: return CamelToKebab(str);
                    }
                    break;
                case NamingConvention.KebabCase:
                    switch (to)
                    {
                        case NamingConvention.SnakeCase: return KebabToSnake(str);
                        case NamingConvention.UpperSnakeCase: return KebabToUpperSnake(str);
                        case NamingConvention.UpperCamelCase: return KebabToUpperCamel(str);
                        case NamingConvention.LowerCamelCase: return KebabToLowerCamel(str);
                    }
                    break;
            }

            return str;
        }

        #region SnakeTo
        
        public static string SnakeToUpperCamel(string snake)
        {
            if (string.IsNullOrEmpty(snake))
                return snake;

            return snake
                .Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }

        public static string SnakeToLowerCamel(string snake)
        {
            if (string.IsNullOrEmpty(snake))
                return snake;

            return SnakeToUpperCamel(snake)
                .Insert(0, char.ToLowerInvariant(snake[0]).ToString()).Remove(1, 1);
        }

        public static string SnakeToUpperSnake(string snake)
        {
            return string.IsNullOrEmpty(snake) ? snake : snake.ToUpper();
        }
        
        #endregion SnakeTo

        #region UpperSnakeTo
        
        public static string UpperSnakeToSnake(string upperSnake)
        {
            return string.IsNullOrEmpty(upperSnake) ? upperSnake : upperSnake.ToLower();
        }

        public static string UpperSnakeToLowerCamel(string upperSnake)
        {
            return SnakeToLowerCamel(UpperSnakeToSnake(upperSnake));
        }

        public static string UpperSnakeToUpperCamel(string upperSnake)
        {
            return SnakeToUpperCamel(UpperSnakeToSnake(upperSnake));
        }
        
        #endregion UpperSnakeTo

        public static string CamelToSnake(string camel)
        {
            var regex = new System.Text.RegularExpressions.Regex("[a-z][A-Z]");
            return regex.Replace(camel, s => $"{s.Groups[0].Value[0]}_{s.Groups[0].Value[1]}").ToLower();
        }

        public static string CamelToUpperSnake(string camel)
        {
            return CamelToSnake(camel).ToUpper();
        }
        
        public static string CamelToKebab(string camel)
        {
            var regex = new System.Text.RegularExpressions.Regex("[a-z][A-Z]");
            return regex.Replace(camel, s => $"{s.Groups[0].Value[0]}-{s.Groups[0].Value[1]}").ToLower();
        }
        
        public static string SnakeToKebab(string snake)
        {
            return snake.Replace("_", "-").ToLower();
        }
        
        public static string KebabToSnake(string kebab)
        {
            return kebab.Replace("_", "-").ToLower();
        }
        
        public static string KebabToUpperSnake(string kebab)
        {
            return kebab.Replace("_", "-").ToUpper();
        }

        public static string KebabToUpperCamel(string kebab)
        {
            return SnakeToUpperCamel(KebabToSnake(kebab));
        }

        public static string KebabToLowerCamel(string kebab)
        {
            return SnakeToLowerCamel(KebabToSnake(kebab));
        }
        
        public static bool IsExistInvalidPathChars(string path)
        {
            return path.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        public static bool IsExistInvalidFileNameChars(string fileName)
        {
            return IsExistInvalidPathChars(fileName) || INVALID_FILENAME_REGEX.IsMatch(fileName);
        }
    }
}