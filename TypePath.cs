namespace Paschoalotto_RPA
{
    public enum TypePath
    {
        ID,
        CLASS_NAME,
        NAME,
        CSS_SELECTOR,
        TAG_NAME,
        XPATH
    }
    public static class EnumExtensions
    {
        public static string ToLowerString(this Enum value)
        {
            return value.ToString().Replace("_", "").ToLower();
        }
    }
}
