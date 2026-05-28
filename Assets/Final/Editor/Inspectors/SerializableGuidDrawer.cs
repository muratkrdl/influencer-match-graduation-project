using System;
using Final.InfluencerMatch.Common;
using UnityEditor;
using UnityEngine;

namespace Final.Editor.Inspectors
{
    /// <summary>
    /// Renders a <see cref="SerializableGuid"/> as a read-only hyphenated GUID string.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty part1 = property.FindPropertyRelative("m_Part1");
            SerializedProperty part2 = property.FindPropertyRelative("m_Part2");
            SerializedProperty part3 = property.FindPropertyRelative("m_Part3");
            SerializedProperty part4 = property.FindPropertyRelative("m_Part4");

            string display = ComposeGuidString(part1, part2, part3, part4);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(position, label, display);
            EditorGUI.EndDisabledGroup();
        }

        private static string ComposeGuidString(SerializedProperty p1, SerializedProperty p2, SerializedProperty p3, SerializedProperty p4)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes((uint)p1.longValue).CopyTo(bytes, 0);
            BitConverter.GetBytes((uint)p2.longValue).CopyTo(bytes, 4);
            BitConverter.GetBytes((uint)p3.longValue).CopyTo(bytes, 8);
            BitConverter.GetBytes((uint)p4.longValue).CopyTo(bytes, 12);
            return new Guid(bytes).ToString();
        }
    }
}
