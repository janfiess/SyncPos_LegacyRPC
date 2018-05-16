/********************************************************************
Draggable.cs
This script is attached to every interactable items prefab which can be instantiated

STEP N3
Change the position on mouse drag, also for touch
Created by Jan Fiess in May 2018
***********************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class DraggableXZ : MonoBehaviour
{
    ThisItem thisItem;
    GameObject hitGameObject;

    [ReadOnly] public bool isBeingDragged = false;
    NetworkManager networkManager;

    void Start()
    {
        networkManager = ReferenceManager.Instance.networkManager;
        thisItem = GetComponent<ThisItem>();
    }

    void FixedUpdate()
    {
        /**************************************************
        Dragging items - sync over network not in this script
        ***************************************************/
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mPosScreen = Input.mousePosition;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(mPosScreen); //

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    if (thisItem.itemCanBeMoved_byThisClient == false) return;

                    // set thisItem.thisClient_canModify_thisItem = false on every client except the active one 
                    networkManager.RemoveClientAuthorityForItem_sender(this.gameObject.name);
                    GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem = true;

                    networkManager.SetItemMoveAuthority_sender(this.gameObject.name, false);

                    hitGameObject = hit.collider.gameObject;
                    isBeingDragged = true;
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (isBeingDragged == true)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 newPos = new Vector3(hit.point.x, this.transform.position.y, hit.point.z);
                    hitGameObject.transform.position = newPos;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) || !(Input.GetMouseButton(0)))
        {
            if (isBeingDragged == true)
            {
                isBeingDragged = false;

                networkManager.SetItemMoveAuthority_sender(this.gameObject.name, true); // cange bool thisItem.canBeMoved
            }
        }

        // Step N3 End
    }
}