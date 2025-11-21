using System.ComponentModel;
using System.Reflection;

namespace OpenFeature.E2ETests.Utils;

public static class EnumHelpers
{
    public static TEnum ParseFromDescription<TEnum>(string description) where TEnum : struct, Enum
    {
        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null && attr.Description == description)
            {
                return (TEnum)field.GetValue(null)!;
            }
        }
        throw new ArgumentException($"No {typeof(TEnum).Name} with description '{description}' found.");
    }
}
