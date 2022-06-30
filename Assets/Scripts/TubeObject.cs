using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TubeObject : MonoBehaviour, IPointerDownHandler
{
    public int id = 0;
    public List<GameObject> posList = new List<GameObject>();
    public GameObject posTop = null;
    public List<BallObject> ballObjects = new List<BallObject>();
    public Action<TubeObject> onClickTube = null;
    public int MAX_BALL = 4;

    public void OnPointerDown(PointerEventData eventData)
    {
        onClickTube?.Invoke(this);
    }


    public bool IsTubeFull()
    {
        if (ballObjects.Count < MAX_BALL)
        {
            return false;
        }

        return true;
    }
    public bool IsTubeEmpty()
    {
        if (ballObjects.Count != 0)
        {
            return false;
        }

        return true;
    }

    public bool IsTubeResolved()
    {
        if (ballObjects.Count != MAX_BALL)
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

    public int GetTopBallType()
    {
        return ballObjects[ballObjects.Count - 1].type;
    }

    public int GetIndexFree()
    {
        return ballObjects.Count;
    }

    // public bool IsTubeResolved()
    // {
    //     if (ballObjects.Count < MAX_BALL)
    //     {
    //         return false;
    //     }
    //
    //     for (int i = 1; i < ballObjects.Count; i++)
    //     {
    //         if (ballObjects[i].type != ballObjects[0].type)
    //         {
    //             return false;
    //         }
    //     }
    //     
    //     return true;
    // }
}
