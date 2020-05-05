using UnityEngine;

using Assets.Scripts.Utility.Enum;

public class Scenery : MonoBehaviour
{
    public SceneryTypeEnum sceneryTypeEnum; // used to flag what type of scenery this is used (used when serialising, saving to cloud and restoring back again)
    public bool isSelected { get; set; }  // used to flag whether the item is selected (e.g. to be moved or deleted) 
    public bool isMoving { get; set; }  // used to flag whether the item currently being moved
}
