using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.Utility.Enum;


/// <summary>
/// Responsible for updating UI elements (buttons) - e.g. updating text and showing/hiding controls as appopriate to the current app state.
/// </summary>
public class UIManager : MonoBehaviour
{

    private AppStateManager appStateManager;
    private CloudAnchorSessionManager appAnchorManager;
    private SceneryPlacementManager sceneryPlacementManager;
    private SceneryPersistenceManager sceneryPersistenceManager;

    private GameObject uiContentObject; // Entire UI Container.  

    private GameObject uiButtonSetAnchorObject;
    private GameObject uiButtonRestoreAnchorObject;
    private GameObject uiButtonPlaceSceneryObject;
    private GameObject uiButtonMoveSceneryObject;
    private GameObject uiButtonSaveSceneryObject;
    private GameObject uiButtonRestoreSceneryObject;

    private GameObject uiLogOutputObject;
    private Text uiLogOutputText; //UI Textbox used to display debugging info directly on the device screen.

    void Awake()
    {
        appStateManager = FindObjectOfType<AppStateManager>();
        appAnchorManager = FindObjectOfType<CloudAnchorSessionManager>();
        sceneryPlacementManager = FindObjectOfType<SceneryPlacementManager>();
        sceneryPersistenceManager = FindObjectOfType<SceneryPersistenceManager>();

        uiContentObject = GameObject.Find("/UICanvas/Panel/UIContent");

        uiLogOutputObject = uiContentObject.transform.Find("UILogOutput").gameObject;
        uiLogOutputText = uiLogOutputObject.GetComponent<Text>();

        uiButtonSetAnchorObject = uiContentObject.transform.Find("UIButton_SetAnchor").gameObject;
        uiButtonRestoreAnchorObject = uiContentObject.transform.Find("UIButton_RestoreAnchor").gameObject;
        uiButtonPlaceSceneryObject = uiContentObject.transform.Find("UIButton_PlaceScenery").gameObject;
        uiButtonMoveSceneryObject = uiContentObject.transform.Find("UIButton_MoveScenery").gameObject;
        uiButtonSaveSceneryObject = uiContentObject.transform.Find("UIButton_SaveScenery").gameObject;
        uiButtonRestoreSceneryObject = uiContentObject.transform.Find("UIButton_RestoreScenery").gameObject;

        uiButtonSetAnchorObject.GetComponent<Button>().onClick.AddListener(delegate { UIButton_SetAnchor_Click(); });
        uiButtonRestoreAnchorObject.GetComponent<Button>().onClick.AddListener(delegate { UIButton_RestoreAnchor_Click(); });
        uiButtonPlaceSceneryObject.GetComponent<Button>().onClick.AddListener(delegate { UIButton_PlaceScenery_Click(); });
        uiButtonMoveSceneryObject.GetComponent<Button>().onClick.AddListener(delegate { UIButton_MoveScenery_Click(); });
        uiButtonSaveSceneryObject.GetComponent<Button>().onClick.AddListener(delegate { UIButton_SaveScenery_Click(); });
        uiButtonRestoreSceneryObject.GetComponent<Button>().onClick.AddListener(delegate { UIButton_RestoreScenery_Click(); });
    }

    void Update()
    {
        UpdateOutputMessage();
        UpdateUIButtonsVisibilityAccordingToState();
    }


    #region Click Handlers
    private void UIButton_SetAnchor_Click()
    {
        appAnchorManager.SaveAnchor();
    }

    private void UIButton_RestoreAnchor_Click()
    {
        appAnchorManager.RestoreAnchor();
    }

    private void UIButton_PlaceScenery_Click()
    {
        sceneryPlacementManager.PlaceScenery();
    }

    private void UIButton_MoveScenery_Click()
    {
        sceneryPlacementManager.MoveScenery();
    }

    private void UIButton_SaveScenery_Click()
    {
        sceneryPersistenceManager.SaveScenery();
    }

    private void UIButton_RestoreScenery_Click()
    {
        sceneryPersistenceManager.RestoreScenery();
    }

    #endregion


    private void UpdateUIButtonsVisibilityAccordingToState()
    {
        switch (appStateManager.currentUIState)
        {
            case UIStateEnum.AllButtonsDisabled:
                UpdateUIButtonVisibility(false, false, false, false, false, false);
                break;

            case UIStateEnum.AnchorButtonsOnly_CreateAndRestore:
                UpdateUIButtonVisibility(true, true, false, false,false,false);
                uiButtonSetAnchorObject.GetComponentInChildren<Text>().text = "Create Anchor";
                uiButtonRestoreAnchorObject.GetComponentInChildren<Text>().text = "Restore Anchor";
                break;

            case UIStateEnum.AnchorButtonsOnly_RestoreOnly:
                UpdateUIButtonVisibility(false, true, false, false, false, false);
                uiButtonSetAnchorObject.GetComponentInChildren<Text>().text = "Create Anchor";
                uiButtonRestoreAnchorObject.GetComponentInChildren<Text>().text = "Restore Anchor";
                break;

            case UIStateEnum.AnchorButtonsOnly_ReadyToSaveAnchor:
                UpdateUIButtonVisibility(true, false, false, false, false, false);
                uiButtonSetAnchorObject.GetComponentInChildren<Text>().text = "Save Anchor To Cloud";
                break;

            case UIStateEnum.SceneryButtonsOnly_NothingHappening:
                if (appStateManager.currentSceneryContainer != null && appStateManager.currentSceneryContainer.transform.childCount > 0)   // only show "save scenery" button if there is at least one piece of scenery
                {
                    UpdateUIButtonVisibility(false, false, true, false, true, true);
                }
                else
                {
                    UpdateUIButtonVisibility(false, false, true, false, false, true);
                }

                uiButtonPlaceSceneryObject.GetComponentInChildren<Text>().text = "Place Scenery";
                uiButtonRestoreSceneryObject.GetComponentInChildren<Text>().text = "Restore Scenery";
                uiButtonSaveSceneryObject.GetComponentInChildren<Text>().text = "Save Scenery";
                break;

            case UIStateEnum.SceneryButtonsOnly_ScenerySelected:
                UpdateUIButtonVisibility(false, false, false, true, false, false);
                uiButtonMoveSceneryObject.GetComponentInChildren<Text>().text = "Move Scenery";
                break;

            case UIStateEnum.SceneryButtonsOnly_SceneryMoving:
                UpdateUIButtonVisibility(false, false, false, true, false, false);
                uiButtonMoveSceneryObject.GetComponentInChildren<Text>().text = "Stop Moving Scenery";
                break;

        }
    }

    private void UpdateUIButtonVisibility(
        bool SetAnchor_IsVisible, 
        bool RestoreAnchor_IsVisible,
        bool PlaceScenery_IsVisible,
        bool MoveScenery_IsVisible,
        bool SaveScenery_IsVisible,
        bool RestoreScenery_IsVisible
        )
    {
        uiButtonSetAnchorObject.SetActive(SetAnchor_IsVisible);
        uiButtonRestoreAnchorObject.SetActive(RestoreAnchor_IsVisible);
        uiButtonPlaceSceneryObject.SetActive(PlaceScenery_IsVisible);
        uiButtonMoveSceneryObject.SetActive(MoveScenery_IsVisible);
        uiButtonSaveSceneryObject.SetActive(SaveScenery_IsVisible);
        uiButtonRestoreSceneryObject.SetActive(RestoreScenery_IsVisible);
    }


    private void UpdateOutputMessage()
    {
        uiLogOutputText.text = appStateManager.currentOutputMessage;
    }
}
