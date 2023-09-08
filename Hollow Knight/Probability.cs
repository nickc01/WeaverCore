using System;
using System.Collections.Generic;
using UnityEngine;

public class Probability
{
    [Serializable]
    public class ProbabilityGameObject
    {
        public GameObject prefab;

        [Tooltip("If probability = 0, it will be considered 1.")]
        public float probability;

        public ProbabilityGameObject()
        {
            probability = 1f;
        }
    }

    public static GameObject GetRandomGameObjectByProbability(ProbabilityGameObject[] array)
    {
        if (array.Length > 1)
        {
            List<ProbabilityGameObject> list = new List<ProbabilityGameObject>(array);
            ProbabilityGameObject probabilityGameObject = null;
            list.Sort((ProbabilityGameObject x, ProbabilityGameObject y) => x.probability.CompareTo(y.probability));
            float num = 0f;
            foreach (ProbabilityGameObject item in list)
            {
                num += ((item.probability != 0f) ? item.probability : 1f);
            }
            float num2 = UnityEngine.Random.Range(0f, num);
            float num3 = 0f;
            foreach (ProbabilityGameObject item2 in list)
            {
                if (num2 >= num3)
                {
                    probabilityGameObject = item2;
                }
                num3 += item2.probability;
            }
            return probabilityGameObject.prefab;
        }
        if (array.Length == 1)
        {
            return array[0].prefab;
        }
        return null;
    }
}
