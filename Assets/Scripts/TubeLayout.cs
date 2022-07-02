using System.Collections.Generic;
using UnityEngine;

public class TubeLayout : MonoBehaviour
{
    public List<TubeObject> tubeList = new List<TubeObject>();
    public GameObject rootTubes;

    public Vector3 GetTubeScale()
    {
        if (rootTubes == null)
        {
            return Vector3.one;
        }

        return rootTubes.transform.localScale;
    }

}
