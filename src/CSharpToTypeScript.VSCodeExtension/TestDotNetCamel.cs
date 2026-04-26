using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(ToCamelCase("IPAddress"));
        Console.WriteLine(ToCamelCase("HTTPResponse"));
        Console.WriteLine(ToCamelCase("MyProperty"));
        Console.WriteLine(ToCamelCase("JSON Value"));
        Console.WriteLine(ToCamelCase("ABCdefg"));
        Console.WriteLine(ToCamelCase("ID"));
    }

    public static string ToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text) || !char.IsUpper(text[0]))
        {
            return text;
        }

        char[] chars = text.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (i == 1 && !char.IsUpper(chars[i]))
            {
                break;
            }

            bool hasNext = (i + 1 < chars.Length);
            
            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                if (chars[i + 1] == ' ')
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);
                }

                break;
            }

            chars[i] = char.ToLowerInvariant(chars[i]);
        }

        return new string(chars);
    }
}
