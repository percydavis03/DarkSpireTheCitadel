using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom property drawer for CharacterType dropdown
/// </summary>
[CustomPropertyDrawer(typeof(CharacterTypeAttribute))]
public class CharacterTypeDrawer : PropertyDrawer
{
    private readonly string[] options = { "Nyx", "Worker Enemy", "Spear Enemy", "Boss" };
    private readonly int[] values = { 0, 1, 2, 3 };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Get the current value
        int currentValue = property.intValue;
        
        // Find the current index
        int currentIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == currentValue)
            {
                currentIndex = i;
                break;
            }
        }

        // Create the dropdown
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);
        
        // Update the property if changed
        if (newIndex != currentIndex && newIndex >= 0 && newIndex < values.Length)
        {
            property.intValue = values[newIndex];
        }

        EditorGUI.EndProperty();
    }
}
