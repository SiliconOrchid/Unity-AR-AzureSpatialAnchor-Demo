using System;
using System.Linq;

using UnityEngine;

using Assets.Scripts.Utility.Enum;
using Assets.Scripts.Utility;


/// <summary>
/// Responsible for creating "scenery" objects, selecting them (via screen touches) and repositioning them in 3D space.
/// </summary>
public class SceneryPlacementManager : MonoBehaviour
{
    private AppStateManager appStateManager;
    private GeneralConfiguration generalConfiguration;
    private SceneryUtil sceneryUtil;

    private Camera arCamera;  // used for raycasting/touch input
 
    private GameObject currentlyMovingObject = null; //when a piece of scenery is being moved, it is referenced to this variable - otherwise, it is normally left null

    void Awake()
    {
        arCamera = Camera.main;
        appStateManager = FindObjectOfType<AppStateManager>();
        generalConfiguration = FindObjectOfType<GeneralConfiguration>();
        sceneryUtil = FindObjectOfType<SceneryUtil>();
    }


    void Update()
    {
        if (appStateManager.currentCloudAnchorState == CloudAnchorStateEnum.NothingHappening)
        {
            TouchSceneryItem();
            UpdateShowScenerySelectionColour();
            UpdateRepositionMovingScenery();
        }
    }

    public void PlaceScenery()
    {
        if (appStateManager.currentCloudAnchorState == CloudAnchorStateEnum.NothingHappening)
        {
            // randomly select one of the scenery enums we have available
            SceneryTypeEnum randomSceneryTypeEnum =(SceneryTypeEnum) UnityEngine.Random.Range(0, Enum.GetValues(typeof(SceneryTypeEnum)).Length);
            AddSceneryToContainer(randomSceneryTypeEnum);
        }
    }

    public void MoveScenery()
    {
        if (appStateManager.currentCloudAnchorState == CloudAnchorStateEnum.NothingHappening)
        {
            ToggleMoveScenery();
        }
    }


    private void AddSceneryToContainer(SceneryTypeEnum sceneryTypeEnum)
    {
        GameObject sceneryObject = sceneryUtil.SpawnNewScenery( sceneryTypeEnum);
        sceneryObject.transform.position = appStateManager.placementCursorPose.position;
        sceneryObject.transform.rotation = appStateManager.placementCursorPose.rotation;

        appStateManager.currentOutputMessage = $"Added Scenery.   Move scenery by tapping directly on it, to select it.   Alternatively, press 'Save' to store the current scenery, or 'Restore' to load a previous save..";
    }

    private void TouchSceneryItem()
    {
        if (appStateManager.currentCloudAnchorState == CloudAnchorStateEnum.NothingHappening)
        {
            if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Touch touch = Input.GetTouch(0);

                if (!touch.position.IsTouchUIObject()) //only accept screen-touches that are in the 'camera' area, not the UI panel 
                {
                    currentlyMovingObject = null; // stop moving any scenery, regardless

                    RaycastHit raycastHit;

                    Ray ray = arCamera.ScreenPointToRay(touch.position);

                    if (Physics.Raycast(ray, out raycastHit))  //we've touched a piece of scenery
                    {
                        GameObject tappedObject = raycastHit.collider.gameObject;

                        if (tappedObject != null)
                        {
                            SelectTouchedScenery(tappedObject);
                        }
                    }
                    else
                    {
                        //otherwise, deselect any previously selected objects
                        DeselectAllScenery();
                    }
                }
            }
        }
    }


    private void SelectTouchedScenery(GameObject tappedSceneryObject)
    {
        appStateManager.SetAnchorAndContainerIfNull();

        foreach (Transform item in appStateManager.currentSceneryContainer.transform)
        {
            GameObject currentSceneryItemObject = item.gameObject;
            Scenery currentSceneryItem = currentSceneryItemObject.GetComponent<Scenery>();

            currentSceneryItem.isSelected = false; // set defaults
            currentSceneryItem.isMoving = false;

            if (tappedSceneryObject.GetInstanceID() == currentSceneryItemObject.GetInstanceID())
            {
                currentSceneryItem.isSelected = true;
                currentSceneryItem.isMoving = false;

                appStateManager.currentUIState = UIStateEnum.SceneryButtonsOnly_ScenerySelected;
                appStateManager.currentOutputMessage = $"Selected Scenery. Next, use 'Move Scenery' button, or, tap anywhere else in the view to deselect scenery.";
            }
        }
    }

    private void DeselectAllScenery()
    {
        appStateManager.SetAnchorAndContainerIfNull();

        foreach (Transform item in appStateManager.currentSceneryContainer.transform)
        {
            GameObject currentSceneryItemObject = item.gameObject;
            Scenery currentSceneryItem = currentSceneryItemObject.GetComponent<Scenery>();

            if (currentSceneryItem.isSelected)
            {
                appStateManager.currentOutputMessage = $"Deselected Scenery. Move scenery by tapping directly on it, to select it.   Alternatively, press 'Save' to store the current scenery, or 'Restore' to load a previous save.";
            }

            currentSceneryItem.isSelected = false; // turn off switches as default
            currentSceneryItem.isMoving = false;
        }

        appStateManager.currentUIState = UIStateEnum.SceneryButtonsOnly_NothingHappening;
    }


    private void UpdateShowScenerySelectionColour()
    {
        appStateManager.SetAnchorAndContainerIfNull();

        foreach (Transform item in appStateManager.currentSceneryContainer.transform)
        {
            //iterate over the collection of different trees, etc.
            GameObject currentSceneryItemObject = item.gameObject;
            Scenery currentSceneryItem = currentSceneryItemObject.GetComponent<Scenery>();

            //gotcha: be specific with "cakeslice" namespace - there is also an "Outline" class in the Unity.UI namespace
            cakeslice.Outline outline = currentSceneryItemObject.GetComponent<cakeslice.Outline>(); // the outline component that makes the 3D model highlighted

            if ((currentSceneryItem.isSelected || currentSceneryItem.isMoving))
            {
                if (outline == null)
                {
                    outline = currentSceneryItemObject.AddComponent(typeof(cakeslice.Outline)) as cakeslice.Outline;
                }

                if (currentSceneryItem.isSelected)
                {
                    outline.color = 1; //green  (defined on "Outline Effect" script attached to Camera)
                }

                if (currentSceneryItem.isMoving)
                {
                    outline.color = 0; //red 
                }

                continue;
            }

            if (!(currentSceneryItem.isSelected || currentSceneryItem.isMoving) && (outline != null))
            {
                // no options selected - but an outline-component is present - so remove the component
                Destroy(outline);
                continue;
            }
        }
    }

    private void ToggleMoveScenery()
    {
        appStateManager.SetAnchorAndContainerIfNull();

        foreach (Transform item in appStateManager.currentSceneryContainer.transform)
        {
            GameObject currentSceneryItemObject = item.gameObject;
            Scenery currentSceneryItem = currentSceneryItemObject.GetComponent<Scenery>();

            if (currentSceneryItem.isSelected)
            {
                if (currentSceneryItem.isMoving)
                {
                    currentSceneryItem.isMoving = false;
                    currentlyMovingObject =null;

                    DeselectAllScenery();
                }
                else
                {
                    currentSceneryItem.isMoving = true;
                    currentlyMovingObject = currentSceneryItemObject;
                    appStateManager.currentUIState = UIStateEnum.SceneryButtonsOnly_SceneryMoving;
                    appStateManager.currentOutputMessage = $"Moving Selected Scenery. Point where you want it to go.  Click 'Stop Moving' when finished.";
                }
            }
        }
    }


    private void UpdateRepositionMovingScenery()
    {
        if (currentlyMovingObject != null)
        {
            currentlyMovingObject.transform.rotation = appStateManager.placementCursorPose.rotation;
            currentlyMovingObject.transform.position = Vector3.MoveTowards(currentlyMovingObject.transform.position, appStateManager.placementCursorPose.position, generalConfiguration.sceneryMoveSpeed * Time.deltaTime);
        }
    }
}
