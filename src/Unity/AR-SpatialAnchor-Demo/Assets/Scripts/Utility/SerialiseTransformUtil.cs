using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.Utilities
{
    public static class SerialiseTransformUtil
    {
        public static string ReturnJsonFromTransform(Transform transform)
        {
            TransformComponentDTO serialiseTransform = new TransformComponentDTO(transform);
            return JsonUtility.ToJson(serialiseTransform);
        }

        public static void UpdateTransform(ref GameObject objectToUpdate, TransformComponentDTO inputSerialiseTransform)
        {
            objectToUpdate.transform.localPosition = new Vector3(inputSerialiseTransform.localPosition[0], inputSerialiseTransform.localPosition[1], inputSerialiseTransform.localPosition[2]);
            objectToUpdate.transform.localRotation = new Quaternion(inputSerialiseTransform.localRotation[0], inputSerialiseTransform.localRotation[1], inputSerialiseTransform.localRotation[2], inputSerialiseTransform.localRotation[3]);
            objectToUpdate.transform.localScale = new Vector3(inputSerialiseTransform.localScale[0], inputSerialiseTransform.localScale[1], inputSerialiseTransform.localScale[2]);
        }
    }
}
