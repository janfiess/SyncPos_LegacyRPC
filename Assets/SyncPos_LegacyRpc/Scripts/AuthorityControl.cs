/**************************************************************
 * AuthorityControl.cs
 * attached to the Manager GameObject of every interactable object
 * Control UI for checking item authorities canBeMoved, canBeModified_byThisClient
 * Created by Jan Fiess in May 2018
 * STEP N13
 *************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthorityControl : MonoBehaviour
{
    Transform itemAnalysingGroup;
    ItemsManager itemsManager;
    ThisItem thisItem;
    NetworkManager networkManager;

    void Start()
    {
        thisItem = GetComponent<ThisItem>();
        networkManager = ReferenceManager.Instance.networkManager;

        /*******************************************************
		Create an analysing section for each item in a list (for debugging)
		******************************************************* */

        itemsManager = ReferenceManager.Instance.itemsManager;
        GameObject itemAnalyser = Instantiate(itemsManager.itemsAnalyser_prefab);
        ItemsManager.itemAnalysing_dict.Add(gameObject.name, itemAnalyser);
        //		print("Items_Manager.itemAnalysing_dict.Count: " + Items_Manager.itemAnalysing_dict.Count);
        itemAnalyser.transform.parent = itemsManager.itemsAnalyser_placeholder.transform;
        itemAnalysingGroup = ItemsManager.itemAnalysing_dict[gameObject.name].transform;

        // set the item name in this section
        foreach (Transform itemName_placeholder in itemAnalysingGroup)
        {
            if (itemName_placeholder.name == "text_ItemName") itemName_placeholder.GetComponent<Text>().text = gameObject.name;
        }
    }

    void Update()
    {
        /*************************************************************************
         update authority UI panel
         *************************************************************************/

        if (thisItem.itemCanBeMoved_byThisClient == true)
        {
            // consoleText.text = "canBeMoved == true Auth";
            foreach (Transform checkImage in itemAnalysingGroup)
            {
                if (checkImage.name == "Image_CanBeMoved") checkImage.GetComponent<Image>().color = Color.green;
            }
        }

        if (thisItem.itemCanBeMoved_byThisClient == false)
        {
            // consoleText.text = "canBeMoved == false Auth";
            foreach (Transform checkImage in itemAnalysingGroup)
            {
                if (checkImage.name == "Image_CanBeMoved") checkImage.GetComponent<Image>().color = Color.red;
            }
        }

        if (thisItem.clientIsInteractionInitiator_forThisItem == true)
        {
            foreach (Transform checkImage in itemAnalysingGroup)
            {
                if (checkImage.name == "Image_ThisClientCanModifyThisItem") checkImage.GetComponent<Image>().color = Color.green;
            }
        }

        if (thisItem.clientIsInteractionInitiator_forThisItem == false)
        {
            foreach (Transform checkImage in itemAnalysingGroup)
            {
                if (checkImage.name == "Image_ThisClientCanModifyThisItem") checkImage.GetComponent<Image>().color = Color.red;
            }
        }
    }
}
