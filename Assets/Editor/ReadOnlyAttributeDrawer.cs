// Placed into Assets/Editor.
// For creating ReadOnly Attributes in Inspector: Instructions: https://disybell.com/229
// Any public variable in inspector can be tagged with [ReadOnly] to be greyed out.
// in addition to this script there must also be the ReadOnlyAttribute.cs script somewhere in the Assets folder (here: In the script folder)


using UnityEngine;
using UnityEditor;
 
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
 
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var readonlyAttribute = attribute as ReadOnlyAttribute;
        GUI.enabled = !readonlyAttribute.Condition;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}