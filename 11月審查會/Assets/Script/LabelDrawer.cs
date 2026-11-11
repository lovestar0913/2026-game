using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LabelAttribute attr = (LabelAttribute)attribute;
        label.text = attr.label;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            if (attr.hasMin && attr.hasMax)
            {
                EditorGUI.Slider(position, property, attr.min, attr.max, label);
                return;
            }

            EditorGUI.PropertyField(position, property, label, true);

            if (attr.hasMin && property.floatValue < attr.min)
                property.floatValue = attr.min;

            return;
        }

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            if (attr.hasMin && attr.hasMax)
            {
                EditorGUI.IntSlider(position, property, (int)attr.min, (int)attr.max, label);
                return;
            }

            EditorGUI.PropertyField(position, property, label, true);

            if (attr.hasMin && property.intValue < (int)attr.min)
                property.intValue = (int)attr.min;

            return;
        }

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}