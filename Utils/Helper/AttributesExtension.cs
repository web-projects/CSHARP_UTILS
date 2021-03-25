using System;
using System.Linq;
using System.Reflection;

namespace Utils.Helper
{
    public static class AttributesExtension
    {
        /// <summary>
        /// Returns the value of a member attribute for any member in a class.
        ///     (a member is a Field, Property, Method, etc...)    
        /// <remarks>
        /// If there is more than one member of the same name in the class, it will return the first one (this applies to overloaded methods)
        /// </remarks>
        /// <example>
        /// Read System.ComponentModel Description Attribute from method 'MyMethodName' in class 'MyClass': 
        ///     var Attribute = typeof(MyClass).GetAttribute("MyMethodName", (DescriptionAttribute d) => d.Description);
        /// </example>
        /// <param name="type">The class that contains the member as a type</param>
        /// <param name="MemberName">Name of the member in the class</param>
        /// <param name="valueSelector">Attribute type and property to get (will return first instance if there are multiple attributes of the same type)</param>
        /// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events</param>
        /// </summary>    
        public static TValue GetAttribute<TAttribute, TValue>(this Type type, string MemberName, Func<TAttribute, TValue> valueSelector, bool inherit = false) where TAttribute : Attribute
        {
            var att = type.GetMember(MemberName).FirstOrDefault().GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }

        //public static void ListAllAttributes()
        //{
        //    System.Reflection.MemberInfo info = typeof(MyClass);
        //    object[] attributes = info.GetCustomAttributes(true);

        //    for (int i = 0; i < attributes.Length; i++)
        //    {
        //        if (attributes[i] is DomainNameAttribute)
        //        {
        //            System.Console.WriteLine(((DomainNameAttribute)attributes[i]).Name);
        //        }
        //    }
        //}
    }
}
