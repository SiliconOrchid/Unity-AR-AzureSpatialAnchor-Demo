using UnityEngine;

using Microsoft.Azure.SpatialAnchors.Unity;

using Assets.Scripts.Utility.Enum;

/// <summary>
/// Responsible for being the primary object for all application state.
/// </summary>
public class AppStateManager : MonoBehaviour
{
    [System.NonSerialized] // attribute used to prevent item showing in Unity Editor
    public CloudAnchorStateEnum currentCloudAnchorState = CloudAnchorStateEnum.ReadyToCreateSession;

    [System.NonSerialized] 
    public UIStateEnum currentUIState = UIStateEnum.AllButtonsDisabled; // ultimately used to drive which UI elements are currently showing/hidden

    [System.NonSerialized]
    public string currentOutputMessage;  // drives the onscreen message box

    [System.NonSerialized] 
    public Pose placementCursorPose; // used as a way to expose the placement and position of the spot where they raycast touches a surface

    [System.NonSerialized]
    public bool placementCursorIsSurface = false; // used to flag whether the raycast is hitting at least one surface

    [System.NonSerialized]
    public GameObject currentAnchorGroup = null; // when we created the anchor object, we added the 'cloudnativeanchor' component, so use this as a search target

    [System.NonSerialized]
    public GameObject currentSceneryContainer = null; // currentAnchorGroupContainer is used as a container of scenery.  This is a direct child of "currentAnchorGroup"


    public void SetAnchorAndContainerIfNull()
    {
        // n.b. these only exist once an anchor has been added - they are NOT startup items
        if (currentAnchorGroup == null)
        {
            currentAnchorGroup = FindObjectOfType<CloudNativeAnchor>().gameObject;
            currentSceneryContainer = currentAnchorGroup.transform.Find("SceneryContainer").gameObject;
        }
    }

    public void DestroyAllSceneryInCollection()
    {
        SetAnchorAndContainerIfNull();

        foreach (Transform item in currentSceneryContainer.transform)
        {
            Destroy(item.gameObject);
        }
    }
}
