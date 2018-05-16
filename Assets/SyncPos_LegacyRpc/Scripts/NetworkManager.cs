/*******************************************************************************************************
* NetworkManager.cs
* script attached to the Manager GameObjectalways available in scene
* establish the connection either as client or as host.
* and receives / sends away all the RPC commands
* Created by Jan Fiess in May 2018
*******************************************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    NetworkView networkView;
    public string ipAddress_server = "127.0.0.1";
    public GameObject startServer_btn, startClient_btn;
    Button testConnection_btn;
    public Text text_console;
    public Text text_ServerOrClient;
    [ReadOnly] public bool isServer;

    public Dictionary<string, GameObject> currentItems = new Dictionary<string, GameObject>();

    ItemSpawner itemSpawner;
    ItemRemover itemRemover;
    bool initSpawn_finished = false;
    public InputField ip_textfield;


    void Awake()
    {
        networkView = GetComponent<NetworkView>();
    }
    void Start()
    {
        // Artnet sender / client
        string ipAddress_server = ip_textfield.text;
        if(ip_textfield.text == "") ipAddress_server = ip_textfield.placeholder.GetComponent<Text>().text;
        else ipAddress_server = "127.0.0.1";

        // StartServer();
        // StartClient();

        itemSpawner = GetComponent<ItemSpawner>();
        itemRemover = GetComponent<ItemRemover>();
    }

    /********************************************************
     Initialize connection (Step N1)
     ******************************************************** */


    // executed when startServer_btn is pressed
    public void Btn_StartServer()
    {
        StartServer();
    }

    // executed when startClient_btn is pressed
    public void Btn_StartClient()
    {
        StartClient();
    }

    void StartServer()
    {
        print("[Network] Start Server!");
        text_ServerOrClient.text = "Server";
        Network.InitializeServer(32, 3000, false);
        isServer = true;
        if (startServer_btn != null) Destroy(startServer_btn);
        if (startClient_btn != null) Destroy(startClient_btn);
    }

    void StartClient()
    {
        print("[Network] Start Client!");
        text_ServerOrClient.text = "Client";
        Network.Connect(ipAddress_server, 3000);
        isServer = false;

        // ask server for existing items
        StartCoroutine(PassExistingItems_toNewClient_Sender());


        if (startServer_btn != null) Destroy(startServer_btn);
        if (startClient_btn != null) Destroy(startClient_btn);

    }

// (Step N1 end)

    /********************************************************
     Pass Existing Items to New Client
     ******************************************************** */
    //  STEP N11
    IEnumerator PassExistingItems_toNewClient_Sender()
    {
        yield return new WaitForSeconds(0.3f);
        networkView.RPC("GetExistingItems_fromServer", RPCMode.Server, null);
    }

    // Called on the server
    [RPC]
    public void GetExistingItems_fromServer()
    {
        if (!isServer) return;
        // get key/value pair from dictionary: http://answers.unity3d.com/questions/957494/iterate-through-stringfloat-dictionary.html
        foreach (KeyValuePair<string, GameObject> itemInDictionary in ItemsManager.items_dict)
        {
            string prefabName_ofThisItem = itemInDictionary.Value.GetComponent<ThisItem>().prefab.name;
            networkView.RPC("PushExistingItems_toNewClient", RPCMode.Others, prefabName_ofThisItem, itemInDictionary.Value.transform.position, itemInDictionary.Value.transform.eulerAngles);
        }

        // when all items have been transmitted, mark it at the client so that it will not spawn items again, when a further client connects.
        networkView.RPC("NewClient_InitSpawnFinished", RPCMode.Others, null);


    }

    // executed for all items of the server dictionary on the new client only 
    [RPC]
    public void PushExistingItems_toNewClient(string prefabName, Vector3 itemPosition, Vector3 itemOrientation)
    {

        if (initSpawn_finished == true) return;
        print("prefabName: " + prefabName);
        GameObject selectedPrefab = ItemsManager.itemPrefabs_dict[prefabName];
        if (selectedPrefab != null)
        {
            itemSpawner.AddExistingItem(selectedPrefab, itemPosition, itemOrientation);
        }
    }
    // STEP N11 end


    [RPC]
    public void NewClient_InitSpawnFinished()
    {
        initSpawn_finished = true;
    }


    /********************************************************
     Test connection (Step N2)
     ******************************************************** */


    // executed when testConnection_btn is pressed
    public void Btn_TestConnection()
    {
        networkView.RPC("TestConnection", RPCMode.All, null);
    }


    // Called on every client and on the host client
    [RPC]
    public void TestConnection()
    {
        print("[Network] TestConnection");
        if (text_console.text == "Works") text_console.text = "Connected";
        else if (text_console.text == "Connected") text_console.text = "Works";
        else text_console.text = "Connected";
    }

// (Step N2 end)

    /********************************************************
     Remove authority to transform an object over network for all connected clients (including host)
     STEP N6
     ******************************************************** */

    // called when mouse down: First the operating client removes the authority over the object for all
    // clients, then (not here) only the operating client gets the authority.
    public void RemoveClientAuthorityForItem_sender(string itemName)
    {
        networkView.RPC("RemoveClientAuthorityForItem", RPCMode.Others, itemName);
    }

    [RPC]
    public void RemoveClientAuthorityForItem(string movingItem)
    {
        GameObject selectedItem = ItemsManager.items_dict[movingItem];
        selectedItem.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem = false;
    }

// STEP N6 end
// STEP N5B

    /********************************************************
     Sync position over network
     ******************************************************** */

    public void NetworkMove_sender(string movingItem, Vector3 current_position, int rpcMove_number_toSend)
    {
        networkView.RPC("NetworkMove", RPCMode.OthersBuffered, movingItem, current_position, rpcMove_number_toSend);
    }

    [RPC]
    public void NetworkMove(string movingItem, Vector3 current_position, int received_rpcMove_number)
    {
        if (ItemsManager.items_dict.ContainsKey(movingItem) == false) return;
        GameObject selectedItem = ItemsManager.items_dict[movingItem];


        int current_rpcMove_number = selectedItem.GetComponent<ThisItem>().current_rpcMove_number;
        if (received_rpcMove_number != current_rpcMove_number) return;
        if (current_rpcMove_number <= ItemsManager.itemMoveNumbers_dict[selectedItem.name] && current_rpcMove_number != 0) return;

        ItemsManager.itemMoveNumbers_dict[selectedItem.name] = current_rpcMove_number;

        networkView.RPC("Sync_rpcMoveCounter", RPCMode.All, selectedItem.name, current_rpcMove_number);


        if (selectedItem != null)
        {
            // you needn´t update the position of the local player if thisClient_canModify_thisItem == true, because that´s the initiator of the movement
            if (selectedItem.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem == true)
            {
                return;
            }
            selectedItem.transform.position = current_position;
        }
    }

    [RPC]
    public void Sync_rpcMoveCounter(string selectedItem_name, int current_rpcMove_number)
    {
        current_rpcMove_number++;
        GameObject selectedItem = ItemsManager.items_dict[selectedItem_name];
        selectedItem.GetComponent<ThisItem>().current_rpcMove_number = current_rpcMove_number;
    }

    /********************************************************
     Sync rotation over network
     ******************************************************** */

    public void NetworkTurn_sender(string movingItem, Vector3 current_orientation, int rpcRotate_number_toSend)
    {
        networkView.RPC("NetworkTurn", RPCMode.OthersBuffered, movingItem, current_orientation, rpcRotate_number_toSend);
    }

    [RPC]
    public void NetworkTurn(string movingItem, Vector3 current_orientation, int received_rpcRotate_number)
    {
        if (ItemsManager.items_dict.ContainsKey(movingItem) == false) return;
        GameObject selectedItem = ItemsManager.items_dict[movingItem];

        int current_rpcRotate_number = selectedItem.GetComponent<ThisItem>().current_rpcRotate_number;
        if (received_rpcRotate_number != current_rpcRotate_number) return;
        if (current_rpcRotate_number <= ItemsManager.itemMoveNumbers_dict[selectedItem.name] && current_rpcRotate_number != 0) return;


        ItemsManager.itemMoveNumbers_dict[selectedItem.name] = current_rpcRotate_number;
        // print("received_rpcMove_number: " + received_rpcMove_number + " |   current_rpcMove_number: " + current_rpcMove_number);

        networkView.RPC("Sync_rpcRotateCounter", RPCMode.All, selectedItem.name, current_rpcRotate_number);


        if (selectedItem != null)
        {
            // you needn´t update the position of the local player if thisClient_canModify_thisItem == true, because that´s the initiator of the movement
            if (selectedItem.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem == true)
            {
                return;
            }
            selectedItem.transform.eulerAngles = current_orientation;
        }


    }
    [RPC]
    public void Sync_rpcRotateCounter(string selectedItem_name, int current_rpcRotate_number)
    {
        current_rpcRotate_number++;
        GameObject selectedItem = ItemsManager.items_dict[selectedItem_name];
        selectedItem.GetComponent<ThisItem>().current_rpcRotate_number = current_rpcRotate_number;
    }

    // STEP N5B end


    /********************************************************
     Set authority over an item to be moved by any client to true or false
     ******************************************************** */

    public void SetItemMoveAuthority_sender(string itemName, bool bool_authority)
    {
        networkView.RPC("SetItemMoveAuthority", RPCMode.Others, itemName, bool_authority);
    }

    [RPC]
    public void SetItemMoveAuthority(string movingItem, bool bool_authority)
    {
        GameObject selectedItem = ItemsManager.items_dict[movingItem];

        // Only set the move authority to true on collision exit if it is not the object you interact with right now.
        if (selectedItem.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem == true) return;

        if (selectedItem != null)
        {
            selectedItem.GetComponent<ThisItem>().itemCanBeMoved_byThisClient = bool_authority;
        }
    }



   /********************************************************
   Set authority to host STEP N7
   ******************************************************** */

    // called when release mouse: Give the authority back to the host
    public void SetAuthorityToHostOnly_sender(string itemName)
    {
        networkView.RPC("SetAuthorityToHostOnly", RPCMode.All, itemName);
    }

    [RPC]
    public void SetAuthorityToHostOnly(string itemName)
    {
        GameObject selectedItem = ItemsManager.items_dict[itemName];
        if (isServer == false)
        {
            selectedItem.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem = false;
        }
        if (isServer == true)
        {
            selectedItem.GetComponent<ThisItem>().clientIsInteractionInitiator_forThisItem = true;
        }
    }

// STEP N7 end

    /********************************************************
    Spawn items over network
    ******************************************************** */
    public void SpawnItem_overNetwork_sender(GameObject itemToSpawn_prefab, Vector3 position)
    {
        networkView.RPC("SpawnItem_overNetwork", RPCMode.All, itemToSpawn_prefab.name, position);

    }

    [RPC]
    public void SpawnItem_overNetwork(string itemToSpawn_prefab_name, Vector3 position)
    {
        GameObject itemToSpawn_prefab = ItemsManager.itemPrefabs_dict[itemToSpawn_prefab_name];
        itemSpawner.SpawnItem(itemToSpawn_prefab, position);
    }


    /********************************************************
    Remove items over network
    ******************************************************** */
    public void RemoveItem_overNetwork_sender(GameObject itemToRemove)
    {
        networkView.RPC("RemoveItem_overNetwork", RPCMode.All, itemToRemove.name);
    }

    [RPC]
    public void RemoveItem_overNetwork(string itemToRemove_name)
    {
        GameObject itemToRemove = ItemsManager.items_dict[itemToRemove_name];
        itemRemover.RemoveItem(itemToRemove);
    }
}