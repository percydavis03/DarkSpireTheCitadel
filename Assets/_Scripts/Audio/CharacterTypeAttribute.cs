using UnityEngine;

/// <summary>
/// Custom attribute to display character type as a dropdown
/// </summary>
public class CharacterTypeAttribute : PropertyAttribute
{
    public string[] options = { "Nyx", "Worker Enemy", "Spear Enemy", "Boss" };
    public int[] values = { 0, 1, 2, 3 };
}
