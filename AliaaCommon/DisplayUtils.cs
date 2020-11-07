using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AliaaCommon
{
    public static class DisplayUtils
    {
        public static string DisplayName(MemberInfo member)
        {
            if (member == null)
                return null;
            Attribute attr = member.GetCustomAttribute<DisplayAttribute>();
            if (attr != null)
                return ((DisplayAttribute)attr).Name;

            attr = member.GetCustomAttribute(typeof(DisplayNameAttribute));
            if (attr != null)
                return ((DisplayNameAttribute)attr).DisplayName;

            return member.Name;
        }

        public static string DisplayName(Type classType, string memberName)
        {
            MemberInfo[] members = classType.GetMember(memberName);
            if (members == null || members.Length == 0)
                return memberName;
            return DisplayName(members[0]);
        }

        public static string DisplayName<T>(Expression<Func<T, object>> p)
        {
            string memberName;
            if (p.Body is MemberExpression)
                memberName = ((MemberExpression)p.Body).Member.Name;
            else if (p.Body is UnaryExpression)
                memberName = ((p.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            else
                throw new NotImplementedException();
            return DisplayName(typeof(T), memberName);
        }

        public static string DisplayName<E>(E value) where E : struct, IConvertible
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            return DisplayName(fieldInfo);
        }

        public static IEnumerable<E> GetEnumValues<E>() where E : struct, IConvertible
        {
            return Enum.GetValues(typeof(E)).Cast<E>();
        }
    }
}
