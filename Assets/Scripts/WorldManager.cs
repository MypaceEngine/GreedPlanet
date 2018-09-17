using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Concurrent;
using System;
using System.Linq;
using LiteDB;

using System.IO;

public class WorldManager : NetworkBehaviour
{
    public WorldDataConst.WorldData worldData;
    public WorldDataConst.WorldData newWorldData_Generate;
    public int[] generationList = null;
    bool isNewMapAppered = false;
    public bool isReady = false;
    public string current_uuid;

    public void Update()
    {
        if ((worldData != null) && (isNewMapAppered))
        {
            if (worldData.mapPicture == null)
            {
                worldData.mapPicture = WorldGenerator.createMapPicture(worldData);
            }
            foreach (KeyValuePair<NetworkInstanceId,NetworkIdentity> dict in ClientScene.objects)
            {
                NetworkIdentity networkIdentity = dict.Value;
                PlayerController playerCtr=networkIdentity.GetComponent<PlayerController>();
                if (playerCtr!=null)
                {
                    playerCtr.BroadcastNewMap(worldData.width, worldData.height);
                }
            }
            isNewMapAppered = false;
        }
    }

    public void storeNemMap(string name)
    {
        newWorldData_Generate.name = name;
        WorldDataUtility.storeNemMap(newWorldData_Generate, name);
        current_uuid = newWorldData_Generate.uuid;
    }

    public List<PlayerController.MapElement> loadExistMapList()
    {
        return WorldDataUtility.loadExistMapList();
    }
    public void loadExistMapDatas(PlayerController.MapElement element)
    {
        worldData = WorldDataUtility.loadExistMapDatas(element);
        preNotifyNewMap();
        current_uuid = element.pathName;
    }

    public void loadNewMapDatas()
    {
        if (newWorldData_Generate != null)
        {
            worldData = newWorldData_Generate;
            preNotifyNewMap();
        }
    }

    //サーバでの世界データ作成
    [Server]
    public async void Cmd_Generate_Async(string name, string seeds, int width, int height)
    {
        var context = SynchronizationContext.Current;
        await Task.Run(() =>
        {
            worldData = WorldGenerator.GenerateMap(name,seeds,width,height);
            newWorldData_Generate = worldData;
            preNotifyNewMap();
            current_uuid = newWorldData_Generate.uuid;
            Debug.Log("Generate End!");
        });
    }

    public void preNotifyNewMap()
    {
        int[] generationList_tmp = Enumerable.Repeat(0, (worldData.width / PlayerController.TransferSize) * (worldData.height / PlayerController.TransferSize)).ToArray();
        generationList = generationList_tmp;

        isNewMapAppered = true;
    }

    public void setReadyToStart()
    {
        isReady = true;
        newWorldData_Generate = null;
        foreach (KeyValuePair<NetworkInstanceId, NetworkIdentity> dict in ClientScene.objects)
        {
            NetworkIdentity networkIdentity = dict.Value;
            PlayerController playerCtr = networkIdentity.GetComponent<PlayerController>();
            if (playerCtr != null)
            {
                playerCtr.TargetDispTopMenuPanel(playerCtr.connectionToClient);
                playerCtr.Target_CloseInformationPanel(playerCtr.connectionToClient);
            }
        }
    }
}
