using UnityEngine;

using Assets.Scripts.Utility.Enum;


namespace Assets.Scripts.Utility
{

    /// <summary>
    /// Responsible for spawning new "scenery" GameObjects.
    /// </summary>
    public class SceneryUtil : MonoBehaviour
    {
        private AppStateManager appStateManager;
        private GeneralConfiguration generalConfiguration;

        void Start()
        {
            appStateManager = FindObjectOfType<AppStateManager>();
            generalConfiguration = FindObjectOfType<GeneralConfiguration>();
        }

        public GameObject SpawnNewScenery(SceneryTypeEnum sceneryTypeEnum)
        {
            appStateManager.SetAnchorAndContainerIfNull();
            GameObject sceneryObject = null;

            switch (sceneryTypeEnum)
            {
                case SceneryTypeEnum.Tree_TypeOne:
                    sceneryObject = Instantiate(generalConfiguration.sceneryItemTreeTypeOnePrefab) as GameObject;
                    break;
                case SceneryTypeEnum.Tree_TypeTwo:
                    sceneryObject = Instantiate(generalConfiguration.sceneryItemTreeTypeTwoPrefab) as GameObject;
                    break;
                default:
                    sceneryObject = Instantiate(generalConfiguration.sceneryItemTreeTypeOnePrefab) as GameObject;
                    break;
            }

            sceneryObject.AddComponent<Scenery>().sceneryTypeEnum = sceneryTypeEnum;
            sceneryObject.transform.parent = appStateManager.currentSceneryContainer.transform;

            return sceneryObject;
        }
    }
}
