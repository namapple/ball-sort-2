using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TubeObject : MonoBehaviour, IPointerDownHandler
{
    public List<GameObject> posList = new List<GameObject>();
    public GameObject posTop;
    public List<BallObject> ballObjects = new List<BallObject>();
    public Action<TubeObject> onClickTube;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown: " + gameObject.name);
        onClickTube?.Invoke(this);
    }


    public bool IsTubeEmpty()
    {
        if (ballObjects.Count != 0)
        {
            return false;
        }

        return true;
    }

    public bool IsTubeDone()
    {
        if (ballObjects.Count != 4)
        {
            return false;
        }

        if (ballObjects[0].type != ballObjects[1].type || ballObjects[0].type != ballObjects[2].type
            || ballObjects[0].type != ballObjects[3].type)
        {
            return false;
        }

        return true;
    }
    
}
