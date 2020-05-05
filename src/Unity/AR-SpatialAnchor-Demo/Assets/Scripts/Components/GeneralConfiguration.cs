using UnityEngine;


/// <summary>
/// Responsible for being the primary place where configuration can be set and objects assigned.
/// </summary>
public class GeneralConfiguration : MonoBehaviour
{
    #region Visual Prefabs
    [Header("Visual Items")]

    [SerializeField]
    [Tooltip("Prefab : Used to visualise where an object will be placed on planes - instanced by 'PlacementCursorManager'")]
    public GameObject placementCursorPrefab;

    [SerializeField]
    [Tooltip("Prefab : 'AnchorGroup' represents both the spatial anchor visually as a sphere - and also provides a sub-container for scenery.")]
    public GameObject spatialAnchorGroupPrefab;

    [SerializeField]
    [Tooltip("Prefab : Specific Scenery Item - Tree Type One.")]
    public GameObject sceneryItemTreeTypeOnePrefab;

    [SerializeField]
    [Tooltip("Prefab : Specific Scenery Item - Tree Type Two.")]
    public GameObject sceneryItemTreeTypeTwoPrefab;
    #endregion


    #region API Config strings
    [Header("API Configuration Items")]

    [SerializeField]
    [Tooltip("Current CloudAnchorId API : Url to API that GETs/POSTs the current 'Azure Spatial Cloud' ID, to cloud storage ")]
    public string apiUrl_CloudAnchorId = "";

    [SerializeField]
    [Tooltip("Current SceneData API : Url to API that GETs/POSTs  scene data serialized as JSON, to cloud storage")]
    public string apiUrl_SceneData = "";
    #endregion


    #region Misc Settings
    [Header("Misc Settings")]

    [SerializeField]
    [Tooltip("SceneryMoveSpeed : When repositioning scenery items, this is the speed at which the object moves toward the placement cursor.")]
    public float sceneryMoveSpeed = 1.0f; 
    #endregion
}
