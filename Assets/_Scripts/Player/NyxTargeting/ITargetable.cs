using UnityEngine;

/// <summary>
/// Interface for objects that can be targeted by Nyx's targeting system
/// </summary>
public interface ITargetable
{
    /// <summary>
    /// The main transform of this targetable object
    /// </summary>
    Transform Transform { get; }
    
    /// <summary>
    /// The specific point to target (e.g., center mass, weak point)
    /// </summary>
    Transform TargetPoint { get; }
    
    /// <summary>
    /// Priority level for targeting (higher = more likely to be selected)
    /// </summary>
    int TargetPriority { get; }
    
    /// <summary>
    /// Whether this object can currently be targeted
    /// </summary>
    bool CanBeTargeted { get; }
    
    /// <summary>
    /// The type of target this is (Enemy, Grappleable, Interactive, etc.)
    /// </summary>
    TargetType TargetType { get; }
    
    /// <summary>
    /// Called when this target is selected/locked onto
    /// </summary>
    void OnTargetSelected();
    
    /// <summary>
    /// Called when this target is deselected/unlocked
    /// </summary>
    void OnTargetDeselected();
    
    /// <summary>
    /// Get the bounds of this target for UI/visual feedback
    /// </summary>
    Bounds GetTargetBounds();
}

/// <summary>
/// Types of targets that can be detected
/// </summary>
public enum TargetType
{
    Enemy,
    Grappleable,
    Interactive,
    Destructible,
    Boss
} 