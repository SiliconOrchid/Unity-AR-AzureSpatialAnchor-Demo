using System;
using Assets.Scripts.Utility.Enum;

namespace Assets.Scripts.Models
{
    [Serializable]
    public class SceneryItemDTO
    {
        public SceneryTypeEnum sceneryTypeEnum;
        public TransformComponentDTO sceneryTransformComponent;
    }
}
