/********************************************************************
Draggable.cs
This script is attached to every interactable items prefab which can be instantiated

STEP N5
- check if the object´s position changed and transmit it to the server
- manage authorities: 
  - clientIsInteractionInitiator_onThisItem (earlier: canBeModified_byThisClient): 
    When multiple clients are connected, only one has the authority to transmit the item's pos / rot over network.
    This client is the client which initiated the movement. If any other client tries to move the item after the first one
    and before the first client's mouse was released, it will not work
  - itemCanBeMoved_byThisClient (earlier: canBeMoved):
    if an item is not dragged by any client, it can ba dragged by any client -> true for every client / host
    While the item is dragged by a first client, itemCanBeMoved_byThisClient is false for every client including the interaction initiator
    until the first client releases the mouse again.
- Handle collisions with other items
Created by Jan Fiess in May 2018
***********************************************************************/



// attached to every interactable object
// check if the object´s position changed and transmit it to the server

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThisItem : MonoBehaviour
{
    [ReadOnly] public bool clientIsInteractionInitiator_forThisItem = false;
    [ReadOnly] public bool itemCanBeMoved_byThisClient = true;
    NetworkManager networkManager;
    ItemsManager itemsManager;
    Vector3 current_position, previous_position;
    Vector3 current_orientation, previous_orientation;
    [ReadOnly] public GameObject prefab; // check which prefab is this item's origin
    GameObject thatItem = null; // if this item hits another item, thatItem is that other item
    Transform itemAnalysingGrpoup;
    [HideInInspector] public int current_rpcMove_number = 0, current_rpcRotate_number = 0;
    int prev_rpcMove_number = -1, prev_rpcRotate_number = -1;

    void OnEnable()
    {
        networkManager = ReferenceManager.Instance.networkManager;
        itemsManager = ReferenceManager.Instance.itemsManager;

        /*******************************************************
        Find out the prefab of this instance
        ******************************************************* */

        string thisItemName = this.gameObject.name;
        string prefabName;
        if (thisItemName.Contains("("))
        { // an instance of a prefab "Item_Green" gets the name "Item_Green(Clone)" 
            prefabName = thisItemName.Substring(0, thisItemName.IndexOf("(")); // get the prefab's name by removing the "(Instance)"
        }
        else prefabName = thisItemName; // if the item is not instantiated from a prefab but just dragged into the scene

        prefab = ItemsManager.itemPrefabs_dict[prefabName];

        /*******************************************************
        Apply an unique name to this spawned instance: Prefab name + incrementing number
        ******************************************************* */

        gameObject.name = prefabName + itemsManager.itemNumber;
        itemsManager.itemNumber++;


        /*******************************************************
        Apply this instance to the dictionary of items which are currently in scene
        ******************************************************* */

        ItemsManager.items_dict.Add(gameObject.name, gameObject);
        // print("Added to items_dict: " + gameObject.name);

        /*******************************************************
        Create a move counter in the dictionary itemMoveNumbers_dict for making sure
        that no old packages arrive (number increments when item is moved)
        ******************************************************* */

        ItemsManager.itemMoveNumbers_dict.Add(gameObject.name, -1);

        /*******************************************************
        Create a rotstion counter in the dictionary itemRotateNumbers_dict for making sure
        that no old packages arrive (number increments when item is moved)
        ******************************************************* */

        ItemsManager.itemRotationNumbers_dict.Add(gameObject.name, -1);

        /*******************************************************
        apply move FPS - reduce update cycles to reduce network traffic
        ******************************************************* */
    }

    void Start()
    {
        /*******************************************************
		Right after creating the item: Set the authority to the host
		******************************************************* */

        networkManager.SetAuthorityToHostOnly_sender(this.gameObject.name); // 20180515
    }

    void Update()
    {

        /********************************************************
		Detect position change
		******************************************************** */



        current_position = transform.position;
        if (current_position != previous_position)
        {
            if (current_rpcMove_number == prev_rpcMove_number) return;
            prev_rpcMove_number = current_rpcMove_number;
  
            networkManager.NetworkMove_sender(gameObject.name, current_position, current_rpcMove_number);
            previous_position = current_position;
        }





        /********************************************************
		Detect orientation change
		******************************************************** */

        current_orientation = transform.eulerAngles;
        if (current_orientation != previous_orientation)
        {
            if (current_rpcRotate_number == prev_rpcRotate_number) return;
            prev_rpcRotate_number = current_rpcRotate_number;

            networkManager.NetworkTurn_sender(gameObject.name, current_orientation, current_rpcRotate_number);    // you cannot transfer Quaternions via RPC
            previous_orientation = current_orientation;
        }
    }

    /********************************************************
    Prevent stuttering when this item moves another interactable item
    ******************************************************** */

    // STEP N10
    void OnCollisionEnter(Collision col)
    {
        // prevent stuttering on network
        if (col.gameObject.tag == "Item" && this.GetComponent<DraggableXZ>().isBeingDragged == true)
        {
            if (col.gameObject.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem != null)
            {
                //              print ("Success: Collision of " + this.gameObject.name + " with " + col.gameObject.name + ". Component Item + attribute canmodify exists");
                thatItem = col.gameObject;
                networkManager.RemoveClientAuthorityForItem_sender(thatItem.name);
                thatItem.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem = true;

                networkManager.SetItemMoveAuthority_sender(thatItem.name, false);
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject == thatItem && itemCanBeMoved_byThisClient == true)
        {
            networkManager.SetItemMoveAuthority_sender(thatItem.name, true); // cange bool thisItem.canBeMoved
        }
    }
}