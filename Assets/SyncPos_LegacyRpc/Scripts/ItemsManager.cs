/*******************************************************************************************************
* ItemsManager.cs
* script attached to the manager always available in scene
* This script has a dictionary which stores the gameObjects, which are currently available in the scene.
* As only the name of the synchronized objects are transmitted over network, you can look up the corresponding gameobject
* STEP N8
* Created by Jan Fiess in May 2018
*******************************************************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsManager : MonoBehaviour {
	// public GameObject cube_orange, cube_green;
	// this dictionary stores all items which are currently in scene- filled in each Item's ThisItem.cs script on start function
	public static Dictionary<string, GameObject> items_dict = new Dictionary<string, GameObject>();

	// this dictionary stores all item prefabs that can be instantiated
	public static Dictionary<string, GameObject> itemPrefabs_dict = new Dictionary<string, GameObject>();
	
	// this dictionary stores all text analysing parts on the canvas
	public static Dictionary<string, GameObject> itemAnalysing_dict = new Dictionary<string, GameObject>();
	// this dictionary stores the arrival numbers of each item arriving all the time when moved
	public static Dictionary<string, int> itemMoveNumbers_dict = new Dictionary<string, int>();

	// this dictionary stores the arrival numbers of each item arriving all the time when rotated
	public static Dictionary<string, int> itemRotationNumbers_dict = new Dictionary<string, int>();
	
	// Prefabs to be added to itemPrefabs_dict
	public GameObject orange_prefab, green_prefab, blue_prefab; 

	// every instance of newly instantiated items gets an incrementing number
	[HideInInspector] public int itemNumber = 0; 
	public GameObject itemsAnalyser_prefab, itemsAnalyser_placeholder;

	public Text text_console;

	void Start () {
		// store the prefabs in the dictionary itemPrefabs_dict
		itemPrefabs_dict.Add(orange_prefab.name,orange_prefab);
		itemPrefabs_dict.Add(green_prefab.name,green_prefab);
		itemPrefabs_dict.Add(blue_prefab.name,blue_prefab);
	}
}