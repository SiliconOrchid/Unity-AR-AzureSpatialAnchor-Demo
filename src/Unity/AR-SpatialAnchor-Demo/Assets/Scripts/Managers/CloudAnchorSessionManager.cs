using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;

using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Utility.Enum;


/// <summary>
/// Responsible for managing the setup of AzureSpatialAnchor code, creating sessions, saving and restoring anchors to the cloud service.
/// </summary>
public class CloudAnchorSessionManager : MonoBehaviour
{
    private AppStateManager appStateManager;
    private GeneralConfiguration generalConfiguration;
    private SpatialAnchorManager spatialAnchorManager;

    private CloudSpatialAnchor currentCloudSpatialAnchor;
    private CloudSpatialAnchorWatcher currentWatcher;
    private AnchorLocateCriteria anchorLocateCriteria;
    private List<string> anchorIdsToLocate = new List<string>();

    private GameObject spawnedAnchorObject;

    private string currentAnchorId = "";


    void Awake()
    {
        appStateManager = FindObjectOfType<AppStateManager>();
        generalConfiguration = FindObjectOfType<GeneralConfiguration>();
        spatialAnchorManager = FindObjectOfType<SpatialAnchorManager>();

        SetupCloudSessionAsync();
    }


    void Update()
    {
        UpdateUIState();
    }

    void UpdateUIState()
    {
        // show the intended combination of UI buttons, depending on whether we're creating anchors 
        // and whether the cursor is currently hitting a surface

        switch (appStateManager.currentCloudAnchorState)
        {
            case CloudAnchorStateEnum.ReadyToCreateLocalAnchor:
                if (appStateManager.placementCursorIsSurface)
                {
                    appStateManager.currentUIState = UIStateEnum.AnchorButtonsOnly_CreateAndRestore;
                }
                else
                {
                    appStateManager.currentUIState = UIStateEnum.AnchorButtonsOnly_RestoreOnly;
                }
                break;

            case CloudAnchorStateEnum.ReadyToSaveAnchorToCloud:
                appStateManager.currentUIState = UIStateEnum.AnchorButtonsOnly_ReadyToSaveAnchor;
                break;

            case CloudAnchorStateEnum.Busy:
                appStateManager.currentUIState = UIStateEnum.AllButtonsDisabled;
                break;
        }
    }




    #region related to starting up the cloud anchor
    private async void SetupCloudSessionAsync()
    {
        spatialAnchorManager.AnchorLocated += CloudManagerAnchorLocated;

        anchorLocateCriteria = new AnchorLocateCriteria();

        if (spatialAnchorManager.Session == null)
        {
            await spatialAnchorManager.CreateSessionAsync();
        }

        currentAnchorId = "";
        currentCloudSpatialAnchor = null;
        await spatialAnchorManager.StartSessionAsync();

        appStateManager.currentOutputMessage = $"Startup OK.  Move camera around to find surfaces, onto which you can place scenery.  Otherwise press 'Restore' to look for previously saved anchor.";
        appStateManager.currentCloudAnchorState = CloudAnchorStateEnum.ReadyToCreateLocalAnchor;
    }
    #endregion




    #region related to saving cloud anchor

    public void SaveAnchor()
    {
        if (appStateManager.currentCloudAnchorState == CloudAnchorStateEnum.ReadyToCreateLocalAnchor)
        {
            if (spawnedAnchorObject == null)
            {
                spawnedAnchorObject = SpawnNewAnchoredObject(appStateManager.placementCursorPose.position, appStateManager.placementCursorPose.rotation);
                appStateManager.currentCloudAnchorState = CloudAnchorStateEnum.ReadyToSaveAnchorToCloud;
            }
            return;
        }

        if (appStateManager.currentCloudAnchorState == CloudAnchorStateEnum.ReadyToSaveAnchorToCloud)
        {
            appStateManager.currentCloudAnchorState = CloudAnchorStateEnum.Busy;
            SaveAnchorToCloudAsync();
            return;
        }
    }


    private GameObject SpawnNewAnchoredObject(Vector3 position, Quaternion rotation)
    {
        GameObject newGameObject = GameObject.Instantiate(generalConfiguration.spatialAnchorGroupPrefab, position, rotation);

        newGameObject.AddComponent<CloudNativeAnchor>();

        if (currentCloudSpatialAnchor != null)
        {
            CloudNativeAnchor cloudNativeAnchor = newGameObject.GetComponent<CloudNativeAnchor>();
            cloudNativeAnchor.CloudToNative(currentCloudSpatialAnchor);
        }

        return newGameObject;
    }

    private async void SaveAnchorToCloudAsync()
    {
        if (spawnedAnchorObject != null)
        {
            await SaveCurrentObjectAnchorToCloudAsync();
        }
    }

    protected async Task SaveCurrentObjectAnchorToCloudAsync()
    {
        CloudNativeAnchor cloudNativeAnchor = spawnedAnchorObject.GetComponent<CloudNativeAnchor>();

        if (cloudNativeAnchor.CloudAnchor == null) { cloudNativeAnchor.NativeToCloud(); }

        CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;

        cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(2);

        while (!spatialAnchorManager.IsReadyForCreate)
        {
            await Task.Delay(200);
            float createProgress = spatialAnchorManager.SessionStatus.RecommendedForCreateProgress;
            appStateManager.currentOutputMessage = $"Move your device to capture more data points in the environment : {createProgress:0%}";
        }

        appStateManager.currentOutputMessage = $"Saving anchor to cloud ...";

        await spatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);
        currentCloudSpatialAnchor = cloudSpatialAnchor;

        if (currentCloudSpatialAnchor != null)
        {
            OnSaveCloudAnchorSuccessful();
        }
        else
        {
            appStateManager.currentOutputMessage = $"Failed saving anchor to cloud.";
        }

    }


    private void OnSaveCloudAnchorSuccessful()
    {
        appStateManager.currentOutputMessage = $"Saving anchor ID '{currentCloudSpatialAnchor.Identifier}' to cloud ...";
        StartCoroutine(SendAnchorIdToCloud( currentCloudSpatialAnchor.Identifier));
    }

    IEnumerator SendAnchorIdToCloud(string anchorId)
    {
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(generalConfiguration.apiUrl_CloudAnchorId, anchorId))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");
            yield return unityWebRequest.SendWebRequest();
        }
        OnAnchorIdSavedSuccess();
    }

    private void OnAnchorIdSavedSuccess()
    {
        appStateManager.currentOutputMessage = $"Saved anchor ID '{currentCloudSpatialAnchor.Identifier}' to cloud OK.";
        appStateManager.currentCloudAnchorState = CloudAnchorStateEnum.NothingHappening;
        appStateManager.currentUIState = UIStateEnum.SceneryButtonsOnly_NothingHappening;
    }

    #endregion


    #region related to restoring an existing cloud anchor

    public void RestoreAnchor()
    {
        if (appStateManager.currentCloudAnchorState != CloudAnchorStateEnum.ReadyToLookForCloudAnchor)
        {
            appStateManager.currentOutputMessage = $"Retrieving last used anchor ID from cloud...";
            appStateManager.currentCloudAnchorState = CloudAnchorStateEnum.ReadyToLookForCloudAnchor;

            StartCoroutine(GetAnchorIdFromCloud());
        }
    }


    IEnumerator GetAnchorIdFromCloud()
    {
        UnityWebRequest uwr = UnityWebRequest.Get(generalConfiguration.apiUrl_CloudAnchorId);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            appStateManager.currentOutputMessage = $"Failed retrieving last used anchor ID from cloud.";
        }
        else
        {
            appStateManager.currentOutputMessage = $"Retrieved last used anchor ID from cloud, OK.  Looking for anchor - move the device about...";
            currentAnchorId = uwr.downloadHandler.text.Trim();
            SetAnchorIdsToLocate();
            currentWatcher = CreateWatcher();
        }
    }

    private void SetAnchorIdsToLocate()
    {
        anchorIdsToLocate.Clear();
        anchorIdsToLocate.Add(currentAnchorId);
        anchorLocateCriteria.Identifiers = anchorIdsToLocate.ToArray();
    }

    private CloudSpatialAnchorWatcher CreateWatcher()
    {
        if ((spatialAnchorManager != null) && (spatialAnchorManager.Session != null))
        {
            return spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);
        }
        return null;
    }

    private void CloudManagerAnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        if (args.Status == LocateAnchorStatus.Located && appStateManager.currentCloudAnchorState == CloudAnchorStateEnum.ReadyToLookForCloudAnchor)
        {
            if (spawnedAnchorObject == null)
            {
                currentCloudSpatialAnchor = args.Anchor;
                Pose anchorPose = currentCloudSpatialAnchor.GetPose();
                spawnedAnchorObject = SpawnNewAnchoredObject(anchorPose.position, anchorPose.rotation);
            }

            appStateManager.currentCloudAnchorState = CloudAnchorStateEnum.NothingHappening;
            appStateManager.currentUIState = UIStateEnum.SceneryButtonsOnly_NothingHappening;
            appStateManager.currentOutputMessage = $"Found cloud anchor OK.  Press 'Place' to add new scenery, or press 'Restore' to get previously saved scenery..";
        }
    }
    #endregion
}

