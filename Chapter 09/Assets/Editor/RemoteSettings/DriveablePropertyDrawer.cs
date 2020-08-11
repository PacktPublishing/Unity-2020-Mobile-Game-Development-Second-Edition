using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Analytics;
using UnityObject = UnityEngine.Object;


namespace UnityEngine.Analytics
{
    [CustomPropertyDrawer(typeof(DriveableProperty), true)]
    public class DriveablePropertyDrawer : PropertyDrawer
    {
        public const bool k_AllowReplaceableStructs = true;

        protected class State
        {
            internal ReorderableList m_ReorderableList;
            public int lastSelectedIndex;
        }
        private SerializedProperty m_FieldsArray;
        private ReorderableList m_ReorderableList;
        private int m_LastSelectedIndex;

        private const int k_ExtraSpacing = 9;
        private const int k_HeaderHeight = 16;

        private Dictionary<string, State> m_States = new Dictionary<string, State>();

        private GUIContent m_NoFieldContent = new GUIContent("No Field");
        private GUIContent m_ParametersHeaderContent = new GUIContent("Parameters");
        private GUIContent m_RemoteSettingKeyLabelContent = new GUIContent("Remote Setting Key");

        private const string k_DataStoreName = "RemoteSettingsDataStore";
        private const string k_PathToDataStore = "Assets/Editor/RemoteSettings/Data/{0}.asset";

        private const string k_FieldsString = "m_Fields";
        private const string k_TargetString = "m_Target";
        private const string k_FieldPathString = "m_FieldPath";
        private const string k_TypeString = "m_Type";
        private const string k_RSKeyNameString = "m_RSKeyName";

        static private readonly Dictionary<Type, string> k_TypeNames = new Dictionary<Type, string>() {
            {typeof(bool), "bool"},
            {typeof(float), "float"},
            {typeof(string), "string"},
            {typeof(int), "int"},
            {typeof(Int64), "int"},
            {typeof(Double), "float"}
        };
        private const string k_CSharpBooleanTypeName = "boolean";

        private RemoteSettingsHolder RSDataStore;

        private State GetState(SerializedProperty prop)
        {
            State state;
            string key = prop.propertyPath;
            m_States.TryGetValue(key, out state);
            if (state == null)
            {
                state = new State();
                SerializedProperty fieldsArray = prop.FindPropertyRelative(k_FieldsString);
                state.m_ReorderableList = new ReorderableList(prop.serializedObject, fieldsArray, false, true, true, true);
                state.m_ReorderableList.drawHeaderCallback = DrawHeader;
                state.m_ReorderableList.drawElementCallback = DrawParam;
                state.m_ReorderableList.onSelectCallback = SelectParam;
                state.m_ReorderableList.onReorderCallback = EndDragChild;
                state.m_ReorderableList.onAddCallback = AddParam;
                state.m_ReorderableList.onRemoveCallback = RemoveButton;
                // Two standard lines with standard spacing between and extra spacing below to better separate items visually.
                state.m_ReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + k_ExtraSpacing;
                state.m_ReorderableList.index = 0;

                m_States[key] = state;
            }
            return state;
        }

        private State RestoreState(SerializedProperty prop)
        {
            State state = GetState(prop);
            m_FieldsArray = state.m_ReorderableList.serializedProperty;
            m_ReorderableList = state.m_ReorderableList;
            m_LastSelectedIndex = state.lastSelectedIndex;

            return state;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (RSDataStore == null)
            {
                RSDataStore = AssetDatabase.LoadAssetAtPath(string.Format(k_PathToDataStore, k_DataStoreName), typeof(RemoteSettingsHolder)) as RemoteSettingsHolder;
            }
            State state = RestoreState(property);

            OnGUI(position);

            state.lastSelectedIndex = m_LastSelectedIndex;
        }

        void OnGUI(Rect position)
        {
            if (m_FieldsArray == null || !m_FieldsArray.isArray)
            {
                return;
            }
            if (m_ReorderableList != null)
            {
                var oldIdentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                m_ReorderableList.DoList(position);
                EditorGUI.indentLevel = oldIdentLevel;
            }
        }

        protected virtual void DrawHeader(Rect headerRect)
        {
            headerRect.height = k_HeaderHeight;
            GUI.Label(headerRect, m_ParametersHeaderContent);
        }

        Rect[] GetRowRects(Rect rect)
        {
            Rect[] rects = new Rect[3];

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2;

            Rect targetRect = rect;
            targetRect.width *= 0.5f;

            Rect keyRect = rect;
            keyRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Rect propRect = rect;
            propRect.xMin = targetRect.xMax + EditorGUIUtility.standardVerticalSpacing;

            rects[0] = targetRect;
            rects[1] = keyRect;
            rects[2] = propRect;
            return rects;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RestoreState(property);

            float height = 0f;
            if (m_ReorderableList != null)
            {
                height = m_ReorderableList.GetHeight();
            }
            return height;
        }

        void DrawParam(Rect rect, int index, bool isactive, bool isfocused)
        {
            var field = m_FieldsArray.GetArrayElementAtIndex(index);

            rect.y++;

            Rect[] subRects = GetRowRects(rect);
            Rect targetRect = subRects[0];
            Rect keyRect = subRects[1];
            Rect propRect = subRects[2];

            var fieldTarget = field.FindPropertyRelative(k_TargetString);
            var fieldName = field.FindPropertyRelative(k_FieldPathString);
            var keyName = field.FindPropertyRelative(k_RSKeyNameString);

            EditorGUI.BeginChangeCheck();
            {
                GUI.Box(targetRect, GUIContent.none);

                EditorGUI.ObjectField(targetRect, fieldTarget, null, GUIContent.none);

                if (EditorGUI.EndChangeCheck())
                {
                    fieldName.stringValue = null;
                }
            }

            EditorGUI.BeginDisabledGroup(fieldTarget.objectReferenceValue == null);

            EditorGUI.BeginProperty(propRect, GUIContent.none, fieldName);
            {
                GUIContent buttonContent;
                var buttonLabel = new StringBuilder();
                if (string.IsNullOrEmpty(fieldName.stringValue) || fieldTarget.objectReferenceValue == null)
                {
                    buttonLabel.Append("No field");
                }
                else
                {
                    buttonLabel.Append(fieldTarget.objectReferenceValue.GetType().Name);
                    if (!string.IsNullOrEmpty(fieldName.stringValue))
                    {
                        buttonLabel.Append(".");
                        buttonLabel.Append(fieldName.stringValue);
                    }
                }
                buttonContent = new GUIContent(buttonLabel.ToString());

                // TODO: Might be nice to color text red if field path doesn't exist
                if (GUI.Button(propRect, buttonContent, EditorStyles.popup))
                {
                    BuildPopupMenuForField(fieldTarget.objectReferenceValue, field).DropDown(propRect);
                }
            }

            EditorGUI.EndProperty();
            EditorGUI.EndDisabledGroup();

            var label = new GUIContent(m_RemoteSettingKeyLabelContent);

            EditorGUI.BeginProperty(keyRect, label, keyName);
            {
                var newKeyRect = EditorGUI.PrefixLabel(keyRect, label);
                GUIContent buttonContent;
                var buttonLabel = new StringBuilder();
                if (string.IsNullOrEmpty(keyName.stringValue))
                {
                    buttonLabel.Append("No Field");
                }
                else if (RSDataStore.rsKeys == null || !RSDataStore.rsKeys.ContainsKey(keyName.stringValue))
                {
                    buttonLabel.Append("Key no longer exists");
                }
                else
                {
                    buttonLabel.Append(keyName.stringValue);
                }
                buttonContent = new GUIContent(buttonLabel.ToString());
                if (GUI.Button(newKeyRect, buttonContent, EditorStyles.popup))
                {
                    BuildPopupListForRSKeys(field).DropDown(newKeyRect);
                }

                EditorGUI.EndProperty();
            }
        }

        public GenericMenu BuildPopupListForRSKeys(SerializedProperty field)
        {
            var keyName = field.FindPropertyRelative(k_RSKeyNameString).stringValue;

            var menu = new GenericMenu();

            menu.AddItem(m_NoFieldContent,
                string.IsNullOrEmpty(keyName),
                SetRSKey,
                new RemoteKeySetter(field, null, null));

            menu.AddSeparator("");

            if (RSDataStore != null)
            {
                foreach (RemoteSettingsKeyValueType rsKeyVal in RSDataStore.rsKeyList)
                {
                    var fieldPath = field.FindPropertyRelative(k_FieldPathString).stringValue;
                    var fieldType = field.FindPropertyRelative(k_TypeString).stringValue;
                    // Correct discrepancy between C# boolean type name and Remote Settings
                    if (fieldType == k_CSharpBooleanTypeName)
                    {
                        fieldType = k_TypeNames[typeof(bool)];
                    }
                    // Filter RemoteSettings key list to match drivable property type.
                    if (string.IsNullOrEmpty(fieldType) || string.IsNullOrEmpty(fieldPath) ||
                        fieldType == rsKeyVal.type)
                    {
                        var activated = (keyName == rsKeyVal.key);
                        menu.AddItem(new GUIContent(rsKeyVal.key),
                            activated,
                            SetRSKey,
                            new RemoteKeySetter(field, rsKeyVal.key, rsKeyVal.type));
                    }
                }
            }
            return menu;
        }

        public GenericMenu BuildPopupMenuForField(UnityObject target, SerializedProperty field)
        {
            GameObject targetToUse = null;
            if (target is Component)
                targetToUse = ((Component)target).gameObject;
            else if (target is GameObject)
                targetToUse = (GameObject)target;
            else
            {
                Debug.LogError("Remote Settings target must be GameObject or Component, found: " + target);
            }

            var fieldName = field.FindPropertyRelative(k_FieldPathString).stringValue;

            var menu = new GenericMenu();

            menu.AddItem(m_NoFieldContent,
                string.IsNullOrEmpty(fieldName),
                SetProperty,
                new PropertySetter(field, target, null, null));

            if (targetToUse == null)
                return menu;

            menu.AddSeparator("");

            var menuItems = CollectMenuItems(field, targetToUse);

            var activeTarget = field.FindPropertyRelative(k_TargetString).objectReferenceValue;
            foreach (var item in menuItems)
            {
                PropertySetter ps = item.Value;
                bool activated = (ReferenceEquals(activeTarget, ps.target) && fieldName == ps.fieldPath);

                menu.AddItem(new GUIContent(item.Key),
                    activated,
                    SetProperty,
                    item.Value);
            }

            return menu;
        }

        static internal SortedDictionary<string, PropertySetter> CollectMenuItems(SerializedProperty field, GameObject targetToUse)
        {
            var typeString = field.FindPropertyRelative(k_TypeString).stringValue;
            var rsKeyName = field.FindPropertyRelative(k_RSKeyNameString).stringValue;

            string typeConstraint = "";
            if (!string.IsNullOrEmpty(typeString) && !string.IsNullOrEmpty(rsKeyName))
            {
                typeConstraint = typeString;
            }

            var menuItems = new SortedDictionary<string, PropertySetter>();

            AddMenuItemsForType(menuItems, targetToUse, targetToUse, field, "", typeConstraint);

            Component[] comps = targetToUse.GetComponents<Component>();
            foreach (Component comp in comps)
            {
                // Don't allow Remote Settings Component to be mapped to Remote Settings.
                // That can only end in tears.
                if (comp == null || comp.GetType() == typeof(RemoteSettings))
                    continue;
                AddMenuItemsForType(menuItems, comp, comp, field, "", typeConstraint);
            }

            return menuItems;
        }

        static private void AddMenuItemsForType(SortedDictionary<string, PropertySetter> menuItems,
            object originalTarget,
            object target,
            SerializedProperty serializedProp,
            String prefix,
            string typeConstraint,
            int depth = 0)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public;
            MemberInfo[] fields = target.GetType().GetFields(flags);

            MemberInfo[] properties = target.GetType().GetProperties(flags);
            var fieldsAndProperties = fields.Concat(properties);

            foreach (var field in fieldsAndProperties)
            {
                var path = prefix == "" ? field.Name : String.Concat(prefix, "/", field.Name);

                Type memberType;
                bool writable;
                bool indexable = false;         // property has index parameters
                bool arrayIndexable = false;    // property has a single Int32 index parameter

                if (field.GetType().Name == "MonoField")
                {
                    FieldInfo info = (FieldInfo)field;
                    writable = !info.IsLiteral && !info.IsInitOnly;
                    memberType = info.FieldType;
                }
                else
                {
                    PropertyInfo info = (PropertyInfo)field;
                    writable = info.CanWrite;
                    memberType = info.PropertyType;
                    var parameters = info.GetIndexParameters();
                    if (parameters.GetLength(0) > 0)
                    {
                        indexable = true;
                        if (parameters.GetLength(0) == 1 &&
                            parameters[0].ParameterType == typeof(int) &&
                            target.GetType().GetProperty("Count") != null &&
                            target.GetType().GetProperty("Count").PropertyType == typeof(int))
                        {
                            arrayIndexable = true;
                        }
                    }
                }

                if ((memberType.IsPrimitive || memberType == typeof(string)))
                {
                    // FIXME: k_AllowReplaceableStructs lets in things like Matrix4x4.inverse, transpose, zero, etc.
                    if (!writable || target.GetType().IsValueType && !k_AllowReplaceableStructs)
                    {
                        continue;
                    }

                    string typeStr = "";
                    if (!k_TypeNames.ContainsKey(memberType))
                    {
                        continue;
                    }
                    else
                    {
                        typeStr = k_TypeNames[memberType];
                    }

                    if (String.IsNullOrEmpty(typeConstraint) || typeConstraint == typeStr)
                    {
                        var fieldPath = path.Replace("/", ".");
                        var propertySetter = new PropertySetter(serializedProp, originalTarget, fieldPath, typeStr);
                        var menuPath = originalTarget.GetType().Name + "/" + path;

                        // Indexable primitives are not yet supported
                        if (indexable)
                        {
                            continue;
                        }

                        // We encounter some duplicate property names because each indexer defined on a class or struct
                        // adds a property named "Item" to it. So structs like Matrix4x4 that have multiple indexers
                        // appear to have two properties named "Item".
                        if (menuItems.ContainsKey(menuPath))
                        {
                            continue;
                        }

                        menuItems.Add(menuPath, propertySetter);
                    }
                }
                else if (depth <= 2)
                {
                    /* it must be a struct, and we can expand it */

                    // Blacklist properties that leak objects into the scene when called from the Editor.
                    // see error: "Instantiating mesh due to calling MeshFilter.mesh during edit mode. This will leak
                    //             meshes. Please use MeshFilter.sharedMesh instead."
                    if ((target is MeshFilter && field.Name == "mesh") ||
                        (target is Renderer && (field.Name == "material" || field.Name == "materials")))
                    {
                        continue;
                    }

                    // Non-array-indexable indexable properties not supported
                    if (indexable && !arrayIndexable)
                    {
                        continue;
                    }

                    if (arrayIndexable)
                    {
                        int count = (int)GetValue(target.GetType().GetProperty("Count"), target);
                        for (var i = 0; i < count; i++)
                        {
                            var indexParams = new object[] { i };
                            object child = GetValue(field, target, indexParams);
                            if (child != null)
                            {
                                AddMenuItemsForType(menuItems,
                                    originalTarget,
                                    child,
                                    serializedProp,
                                    String.Concat(path, "/", i),
                                    typeConstraint,
                                    depth + 1);
                            }
                        }
                    }
                    else
                    {
                        object child = GetValue(field, target);
                        if (child != null)
                        {
                            AddMenuItemsForType(menuItems,
                                originalTarget,
                                child,
                                serializedProp,
                                path,
                                typeConstraint,
                                depth + 1);
                        }
                    }
                }
                /* ignore structs at depth > 2, because we can't expand forever and
                 * we don't want to send structs as strings
                 */
            }
        }

        void SelectParam(ReorderableList list)
        {
            m_LastSelectedIndex = list.index;
        }

        void EndDragChild(ReorderableList list)
        {
            m_LastSelectedIndex = list.index;
        }

        void RemoveButton(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            m_LastSelectedIndex = list.index;

            list.displayAdd = true;
        }

        private void AddParam(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
            m_LastSelectedIndex = list.index;
            var field = m_FieldsArray.GetArrayElementAtIndex(list.index);

            var target = field.FindPropertyRelative(k_TargetString);
            var fieldPath = field.FindPropertyRelative(k_FieldPathString);
            var rsKeyName = field.FindPropertyRelative(k_RSKeyNameString);

            if (list.index == 0)
            {
                target.objectReferenceValue = null;
            }
            else
            {
                var prev = m_FieldsArray.GetArrayElementAtIndex(list.index - 1);
                target.objectReferenceValue =
                    prev.FindPropertyRelative(k_TargetString).objectReferenceValue;
            }
            fieldPath.stringValue = null;
            rsKeyName.stringValue = null;


            list.displayAdd = true;
        }

        public static object GetValue(MemberInfo m, object v, object[] indexParams = null)
        {
            object ret = null;
            try
            {
                ret = ((m is FieldInfo) ?
                       ((FieldInfo)m).GetValue(v) :
                       ((PropertyInfo)m).GetValue(v, indexParams));
            }
            /* some properties are not supported, and we should just not list them */
            catch (TargetInvocationException) {}
            /* we don't support indexed properties, either, which trigger this exception */
            catch (TargetParameterCountException) {}
            return ret;
        }

        static void SetProperty(object source)
        {
            ((PropertySetter)source).Assign();
        }

        static void ClearProperty(object source)
        {
            ((PropertySetter)source).Clear();
        }

        static void SetRSKey(object source)
        {
            ((RemoteKeySetter)source).Assign();
        }

        static void ClearRSKey(object source)
        {
            ((RemoteKeySetter)source).Clear();
        }

        internal struct PropertySetter
        {
            readonly SerializedProperty m_Prop;
            readonly object m_Target;
            readonly String m_FieldPath;
            readonly String m_Type;

            public object target { get { return m_Target; } }
            public String fieldPath { get { return m_FieldPath; } }

            public PropertySetter(SerializedProperty p,
                                  object target,
                                  String fp,
                                  String t)
            {
                m_Prop = p;
                m_Target = target;
                m_FieldPath = fp;
                m_Type = t;
            }

            public void Assign()
            {
                m_Prop.FindPropertyRelative(k_TargetString).objectReferenceValue = (UnityObject)m_Target;
                m_Prop.FindPropertyRelative(k_FieldPathString).stringValue = m_FieldPath;
                if (string.IsNullOrEmpty(m_Prop.FindPropertyRelative(k_RSKeyNameString).stringValue))
                {
                    m_Prop.FindPropertyRelative(k_TypeString).stringValue = m_Type;
                }
                m_Prop.serializedObject.ApplyModifiedProperties();
            }

            public void Clear()
            {
                m_Prop.FindPropertyRelative(k_TargetString).objectReferenceValue = null;
                m_Prop.FindPropertyRelative(k_FieldPathString).stringValue = null;
                m_Prop.FindPropertyRelative(k_TypeString).stringValue = null;
                m_Prop.serializedObject.ApplyModifiedProperties();
            }
        }

        struct RemoteKeySetter
        {
            readonly SerializedProperty m_Prop;
            readonly String m_RsKey;
            readonly String m_Type;

            public RemoteKeySetter(SerializedProperty p, String rsk, String t)
            {
                m_Prop = p;
                m_RsKey = rsk;
                m_Type = t;
            }

            public void Assign()
            {
                m_Prop.FindPropertyRelative(k_RSKeyNameString).stringValue = m_RsKey;
                if (string.IsNullOrEmpty(m_Prop.FindPropertyRelative(k_FieldPathString).stringValue))
                {
                    m_Prop.FindPropertyRelative(k_TypeString).stringValue = m_Type;
                }
                m_Prop.serializedObject.ApplyModifiedProperties();
            }

            public void Clear()
            {
                m_Prop.FindPropertyRelative(k_RSKeyNameString).stringValue = null;
                m_Prop.FindPropertyRelative(k_TypeString).stringValue = null;

                m_Prop.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
