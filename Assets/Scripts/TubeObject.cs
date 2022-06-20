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
    
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked pos: " + gameObject.name);
    }
}
