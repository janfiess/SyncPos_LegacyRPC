/*******************************************************************************************************
* ItemSpawner.cs
* script attached to the manager always available in scene
* Spawn items over network
* STEP N9
* Created by Jan Fiess in May 2018
*******************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    //group all interactable items to one common GameObject
    public GameObject interactableItems_container;
    NetworkManager networkManager;
    void Start()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    /**********************************************************************************************
	Spawn item pipeline: 
	1. (If the item is spawned using a button:) SpawnItem_button() in ItemSpawner.cs calls SpawnItemTrigger in ItemSpawner.cs
	2. SpawnItem_trigger() in ItemSpawner.cs calls SpawnItem_overNetwork_sender() in Network_Manager.cs
	3. SpawnItem_overNetwork_sender() in Network_Manager.cs calls SpawnItem_overNetwork() in Network_Manager.cs for every client using an RPC call
	4. SpawnItem_overNetwork() in Network_Manager.cs of each client calls SpawnItem() in ItemSpawner.cs
	********************************************************************************************** */
    public void SpawnItem_button(GameObject itemToSpawn_prefab)
    {
        SpawnItem_trigger(itemToSpawn_prefab, GetRandomSpawnPosition());
    }

    public void SpawnItem_trigger(GameObject itemToSpawn_prefab, Vector3 spawnPosition)
    {
        networkManager.SpawnItem_overNetwork_sender(itemToSpawn_prefab, spawnPosition);
    }

    public void SpawnItem(GameObject itemToSpawn_prefab, Vector3 spawnPosition)
    {
        GameObject newItem = Instantiate(itemToSpawn_prefab, spawnPosition, Quaternion.identity);
        // parent all items to a central container, later it can be deleted more easily
        newItem.transform.parent = interactableItems_container.transform;
    }

    // for debug purposes: If items are spawned using a key, their position is set randomly
    Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(-1.2f, 1.2f),
            0.5f,
            Random.Range(-1.2f, 1.2f)
        );
    }



    /**********************************************************************************************
	Add existing items for a new client:
    In NetworkManager: When a client is connected, there will be an RPC call to the server only: Network_Manager.GetExistingItems_fromServer
    In this function, the host will iterate through its item dictionary. For each entry find out the prefab name and the item position and orientation.
    Pass this information to all clients except the host client using an other RPC call (PushExistingItems_toNewClient). 
    The content of this function is only executed only on the new client (if initSpawn_finished == false).
    In the RPC function NetworkManager.PushExistingItems_toNewClient() you call the function itemSpawner.AddExistingItem for each item one after another.
    Like this the instances get the same numbers in their names, which is important to be identified.
    When the loop through the host dictionary is done, the local bool for all connected clients initSpawn_finished are set to true, so that the items won't spawn again
    as soon as there is another new client connecting to the network.
    **********************************************************************************************/

    // STEP N11B
    public void AddExistingItem(GameObject itemToSpawn_prefab, Vector3 spawnPosition, Vector3 spawnOrientation)
    {
        GameObject newItem = Instantiate(itemToSpawn_prefab, spawnPosition, Quaternion.identity);
        newItem.transform.eulerAngles = spawnOrientation;
        // parent all items to a central container, later it can be deleted more easily
        newItem.transform.parent = interactableItems_container.transform;
    }
}
