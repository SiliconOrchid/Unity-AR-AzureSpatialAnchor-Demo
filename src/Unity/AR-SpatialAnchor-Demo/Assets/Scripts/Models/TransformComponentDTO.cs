using System;

using UnityEngine;

namespace Assets.Scripts.Models

{
    [Serializable]
    public class TransformComponentDTO
    {
        public float[] localPosition = new float[3];
        public float[] localRotation = new float[4];
        public float[] localScale = new float[3];

        public TransformComponentDTO(Transform transform)
        {
            localPosition[0] = transform.localPosition.x;
            localPosition[1] = transform.localPosition.y;
            localPosition[2] = transform.localPosition.z;

            localRotation[0] = transform.localRotation.x;
            localRotation[1] = transform.localRotation.y;
            localRotation[2] = transform.localRotation.z;
            localRotation[3] = transform.localRotation.w;

            localScale[0] = transform.localScale.x;
            localScale[1] = transform.localScale.y;
            localScale[2] = transform.localScale.z;
        }
    }   
}
