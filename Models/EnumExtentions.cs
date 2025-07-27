using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StudentHub.Models
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>(false);
                if (displayAttribute != null)
                {
                    return displayAttribute.GetName();
                }
            }
            return enumValue.ToString();
        }
    }
}