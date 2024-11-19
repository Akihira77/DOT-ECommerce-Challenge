namespace ECommerce.Util;

public static class Extensions
{
    public static T? ToEnumOrDefault<T>(this string value) where T : struct, Enum
    {
        if (Enum.TryParse(value.ToUpper(), out T result))
        {
            return result;
        }

        return null;
    }

    public static T ToEnumOrThrow<T>(this string value) where T : struct, Enum
    {
        if (Enum.TryParse(value.ToUpper(), out T result))
        {
            return result;
        }

        throw new ArgumentException($"'{value}' is not a valid value for enum type {typeof(T).Name}");
    }
}
