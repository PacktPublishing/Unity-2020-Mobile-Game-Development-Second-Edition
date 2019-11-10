using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEngine.Analytics
{
    [Serializable]
    public class DriveableProperty
    {
        [Serializable]
        public class FieldWithRemoteSettingsKey
        {
            [SerializeField]
            private UnityObject m_Target;
            public UnityObject target
            {
                get { return m_Target; }
                set { m_Target = value; }
            }

            [SerializeField]
            private string m_FieldPath;
            public string fieldPath
            {
                get { return m_FieldPath; }
                set { m_FieldPath = value; }
            }

            [SerializeField]
            private string m_RSKeyName;
            public string rsKeyName
            {
                get { return m_RSKeyName; }
                set { m_RSKeyName = value; }
            }

            [SerializeField]
            private string m_Type;
            public string type
            {
                get { return m_Type; }
                set { m_Type = value; }
            }

            // We can't set a field or property of a ValueType using reflection because calling
            // SetValue requires passing in the target object which is only a copy of the
            // ValueType whose field we want to modify. In that case we need to make a copy
            // with the desired field value and replace the reference to the entire ValueType
            // object with a reference to the new, modified copy.
            //
            // For example, to set `transform.position.x` to 5, we have to do the equivalent of:
            //     var Vector3 pos = transform.position;
            //     pos.x = 5;
            //     transform.position = pos;
            //
            // Because ValueTypes could conceivably be nested inside other ValueTypes, this
            // function travels down the field path recursively until it finds the primitive
            // value to set. If it belongs to a ValueType, it will return the modified copy
            // up the callstack until it finds a reference that can be set. If the value or
            // struct was set successfully, it will return null.
            public object SetValueRecursive(object val, object target, string path)
            {
                // Cases:
                // - path is empty: return primitive value
                // - path has no dot: this is the last field to set
                // - path has multiple dots: keep recursing
                if (path.Length == 0)
                {
                    return val;
                }

                string[] split = path.Split(new char[] {'.'}, 2);
                string next = split[0];
                string rest = split.Length > 1 ? split[1] : "";

#if NETFX_CORE
                TypeInfo targetType = target.GetType().GetTypeInfo();
                var field = targetType.GetDeclaredField(next);
                var prop = targetType.GetDeclaredProperty(next);
#else
                Type targetType = target.GetType();
                var field = targetType.GetField(next);
                var prop = targetType.GetProperty(next);
#endif
                object[] parameters = null;

                object newTarget;
                if (field != null)
                {
                    newTarget = field.GetValue(target);
                }
                else if (prop != null)
                {
                    // If property is a List, parse out the index
                    var indexParams = prop.GetIndexParameters();
                    if (indexParams.GetLength(0) == 1 && indexParams[0].ParameterType == typeof(int))
                    {
                        int index;
                        split = rest.Split(new char[] { '.' }, 2);
                        if (split[0] != null && Int32.TryParse(split[0], out index))
                        {
                            parameters = new object[] { index };
                            rest = split.Length > 1 ? split[1] : "";
                        }
                    }

                    newTarget = prop.GetValue(target, parameters);
                }
                else
                {
                    // This can only happen if you remove a field or property after assigning a RemoteSettings mapping.
                    // TODO: We should check for this condition at build time.
                    throw new InvalidOperationException(String.Format("Member '{0}' on target {1} is neither a field nor property", next, target));
                }

                var newVal = SetValueRecursive(val, newTarget, rest);
                if (newVal != null)
                {
                    if (field != null)
                    {
                        if (field.IsInitOnly)
                        {
                            Debug.LogWarning("You probably shouldn't set a field on a readonly struct even though it works (sometimes)");
                        }
                        field.SetValue(target, newVal);
#if NETFX_CORE
                        bool isValueType = target.GetType().GetTypeInfo().IsValueType;
#else
                        bool isValueType = target.GetType().IsValueType;
#endif
                        if (isValueType)
                        {
                            return target;
                        }
                    }
                    else //(prop != null)
                    {
                        if (prop.CanWrite)
                        {
                            prop.SetValue(target, newVal, parameters);
                        }
                        else
                        {
                            // TODO: We should also protect against this at build time.
                            throw new InvalidOperationException(String.Format("Property '{0}' on target {1} is readonly", next, target));
                        }
                    }
                }
                return null;
            }

            public void SetValue(object val)
            {
                SetValueRecursive(val, m_Target, m_FieldPath);
            }
        }
        [SerializeField]
        private List<FieldWithRemoteSettingsKey> m_Fields;
        public List<FieldWithRemoteSettingsKey> fields
        {
            get { return m_Fields; }
            set { m_Fields = value; }
        }
    }
}
