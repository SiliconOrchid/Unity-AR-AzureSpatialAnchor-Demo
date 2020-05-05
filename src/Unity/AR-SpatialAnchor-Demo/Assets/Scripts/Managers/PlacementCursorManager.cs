using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


/// <summary>
/// Responsible for raycasting to an AR-detected plane and displaying
/// a visual "placement cursor" at that intersection.
/// </summary>
public class PlacementCursorManager : MonoBehaviour
{
    private AppStateManager appStateManager;
    private GeneralConfiguration generalConfiguration;
    private ARRaycastManager arRaycastManager;

    private GameObject placementCursor;


    void Awake()
    {
        appStateManager = FindObjectOfType<AppStateManager>();
        generalConfiguration = FindObjectOfType<GeneralConfiguration>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        placementCursor = Instantiate(generalConfiguration.placementCursorPrefab) as GameObject; 
    }

    void Update()
    {
        UpdateCursorPose();
        UpdateCursorIndicator();
    }

    private void UpdateCursorPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var arRaycastHits = new List<ARRaycastHit>();
        arRaycastManager.Raycast(screenCenter, arRaycastHits, TrackableType.Planes);
        appStateManager.placementCursorIsSurface = arRaycastHits.Count > 0;
        if (appStateManager.placementCursorIsSurface)
        {
            appStateManager.placementCursorPose = arRaycastHits[0].pose;
        }
    }

    private void UpdateCursorIndicator()
    {
        if (appStateManager.placementCursorIsSurface)
        {
            placementCursor.SetActive(true);
            placementCursor.transform.SetPositionAndRotation(appStateManager.placementCursorPose.position, appStateManager.placementCursorPose.rotation);
        }
        else
        {
            placementCursor.SetActive(false);
        }
    }
}
