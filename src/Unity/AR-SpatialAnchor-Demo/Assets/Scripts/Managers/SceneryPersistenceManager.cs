using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

using Assets.Scripts.Models;
using Assets.Scripts.Utility;
using Assets.Scripts.Utilities;

/// <summary>
/// Responsible for saving and restoring a collection of "scenery" objects to cloud storage.
/// (De)Serialises and transfers using HTTP calls.
/// </summary>
public class SceneryPersistenceManager : MonoBehaviour
{
    private AppStateManager appStateManager;
    private GeneralConfiguration generalConfiguration;
    private SceneryUtil sceneryUtil;


    void Awake()
    {
        appStateManager = FindObjectOfType<AppStateManager>();
        generalConfiguration = FindObjectOfType<GeneralConfiguration>();
        sceneryUtil = FindObjectOfType<SceneryUtil>();
    }

    public void SaveScenery()
    {
        appStateManager.currentOutputMessage = $"Saving Scenery data to cloud...";
        SceneryItemContainerDTO sceneryItemContainer = MapSceneryGameobjectsToDTO();

        string sceneryItemContainerJson = JsonUtility.ToJson(sceneryItemContainer);

        StartCoroutine(PostSceneryDataJsonToApi(sceneryItemContainerJson));
    }

    public void RestoreScenery()
    {
        appStateManager.currentOutputMessage = $"Restoring Scenery data to cloud...";
        StartCoroutine(GetSceneryDataFromApi(RePopulateGameItemsFromData));
    }


    #region Save Scenery to cloud storage
    private SceneryItemContainerDTO MapSceneryGameobjectsToDTO()
    {
        SceneryItemContainerDTO sceneryItemContainer = new SceneryItemContainerDTO();

        appStateManager.SetAnchorAndContainerIfNull();

        foreach (Transform item in appStateManager.currentSceneryContainer.transform)
        {
            GameObject currentSceneryItemObject = item.gameObject;
            Scenery currentSceneryItem = currentSceneryItemObject.GetComponent<Scenery>();

            SceneryItemDTO sceneryItemDTO = new SceneryItemDTO(); 

            sceneryItemDTO.sceneryTypeEnum = currentSceneryItem.sceneryTypeEnum;
            sceneryItemDTO.sceneryTransformComponent = new TransformComponentDTO(currentSceneryItemObject.transform);

            sceneryItemContainer.sceneryItems.Add(sceneryItemDTO);
        }
        return sceneryItemContainer;
    }

    IEnumerator PostSceneryDataJsonToApi(string jsonData)
    {

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(generalConfiguration.apiUrl_SceneData, jsonData))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");
            yield return unityWebRequest.SendWebRequest();
        }
        appStateManager.currentOutputMessage = $"Saved Scenery data to cloud OK.";
    }
    #endregion


    #region Restore Scenery from cloud storage
    IEnumerator GetSceneryDataFromApi(Action<SceneryItemContainerDTO> onSuccessSceneryDataFromAPI)
    {
        appStateManager.currentOutputMessage = $"Getting Scenery data from cloud...";

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(generalConfiguration.apiUrl_SceneData))
        {
            yield return unityWebRequest.SendWebRequest();
            while (!unityWebRequest.isDone)
            {
                yield return null;
            }

            byte[] sceneryItemContainerResult = unityWebRequest.downloadHandler.data;
            string sceneryItemContainerJSON = System.Text.Encoding.Default.GetString(sceneryItemContainerResult);
            SceneryItemContainerDTO sceneryItemContainer = JsonUtility.FromJson<SceneryItemContainerDTO>(sceneryItemContainerJSON);

            onSuccessSceneryDataFromAPI(sceneryItemContainer);
        }
    }

    private void RePopulateGameItemsFromData(SceneryItemContainerDTO sceneryItemContainerDTO)
    {
        appStateManager.DestroyAllSceneryInCollection(); // clear out any existing objects

        foreach (SceneryItemDTO sceneryItemData in sceneryItemContainerDTO.sceneryItems)
        {
            GameObject sceneryObject = sceneryUtil.SpawnNewScenery(sceneryItemData.sceneryTypeEnum);
            SerialiseTransformUtil.UpdateTransform(ref sceneryObject, sceneryItemData.sceneryTransformComponent); // apply the positioning and rotation info, that was saved to the cloud.  note this is the "local" version only, meaning they are relative to the parent (the spatial anchor prefab)
        }
        appStateManager.currentOutputMessage = $"Restored Scenery data from cloud OK.";
    }
    #endregion
}
