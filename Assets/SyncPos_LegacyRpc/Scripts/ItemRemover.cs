/**************************************************************
 * ItemRemover.cs
 * attached to the Manager gameobject which is always available in scene
 * STEP N12
 * Created by Jan Fiess in May 2018
 * 
    Remove item pipeline:
    1. (If the item is removed using right mouse click:) In the update function of ItemRemover.cs RemoveItem_trigger() in ItemSpawner.cs is called
    2. RemoveItem_trigger() in ItemRemover.cs calls RemoveItem_overNetwork_sender() in NetworkManager.cs
    3. RemoveItem_overNetwork_sender() in NetworkManager.cs calls RemoveItem_overNetwork() in NetworkManager.cs for every client using an RPC call
    4. RemoveItem_overNetwork() in NetworkManager.cs of each client calls RemoveItem() in ItemSpawner.cs
 **************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemRemover : MonoBehaviour
{
    NetworkManager networkManager;

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // print("mouse right");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitGameObject = hit.collider.gameObject;
                if (hitGameObject.tag == "Item")
                {
                    // print("Remove " + hitGameObject.name);
                    RemoveItem_trigger(hit.collider.gameObject);
                }
            }
        }
    }

    public void RemoveItem_trigger(GameObject hitGameObject)
    {
        networkManager.RemoveItem_overNetwork_sender(hitGameObject);
    }

    // called from Network_Manager.RemoveItem_overNetwork()
    public void RemoveItem(GameObject itemToRemove)
    {
        // remove item from items dictionary
        ItemsManager.items_dict.Remove(itemToRemove.name);
        // remove item from scene
        Destroy(itemToRemove);

		// remove item analyse section Canvas_Debug / AnalyseList / ?
		Transform itemAnalysingGroup = ItemsManager.itemAnalysing_dict[itemToRemove.name].transform;
		foreach(Transform itemName_placeholder in itemAnalysingGroup){
			if(itemName_placeholder.name =="text_ItemName") {
				itemName_placeholder.GetComponent<Text>().text = itemToRemove.name;
				Destroy(itemAnalysingGroup.gameObject);
			}
		}
    }
}