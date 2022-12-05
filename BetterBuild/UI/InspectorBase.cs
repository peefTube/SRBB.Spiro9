using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterBuild.UI
{
    public class InspectorBase : MonoBehaviour
    {
        public delegate object Getter();
        public delegate void Setter(object value);

        public Getter getter;
        public Setter setter;

        public void BindTo(object parent, MemberInfo member, string variableName = null)
        {
            if (member is FieldInfo)
            {
                FieldInfo field = (FieldInfo)member;
                if (variableName == null)
                    variableName = field.Name;

                BindTo(() => field.GetValue(parent), (value) =>
                {
                    field.SetValue(parent, value);
                });
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo property = (PropertyInfo)member;
                if (variableName == null)
                    variableName = property.Name;

                BindTo(() => property.GetValue(parent, null), (value) =>
                {
                    property.SetValue(parent, value, null);
                });
            }
            else
                throw new ArgumentException("Member can either be a field or a property");
        }

        public void BindTo(Getter getter, Setter setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
    }
}
