/********************************************************************
ReferenceManager.cs
This script is attached to Network_Manager.cs (always available in scene)

STEP N4
This script establishes references for instantiated items from prefabs 
to GameObjects which are always available in scene
Created by Jan Fiess in May 2018
***********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    private static ReferenceManager refManagerScript;

    [HideInInspector] public NetworkManager networkManager;
    [HideInInspector] public ItemsManager itemsManager;
    void Awake()
    {
        refManagerScript = this;
        networkManager = GetComponent<NetworkManager>();
        itemsManager = GetComponent<ItemsManager>();
    }

    public static ReferenceManager Instance
    {
        get
        {
            return refManagerScript;
        }
    }
}