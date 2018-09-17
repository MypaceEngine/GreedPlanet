using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldDataUtility {

    static string WORLD_FILENAME = "basic.info";
    static string TERRAIN_FILENAME = "terrain.map";
    static string BUILDING_FILENAME = "building.map";
    static string VEHICLE_FILENAME = "vehicle.map";
    static string WILDANIMAL_FILENAME = "animal.map";
    static string MAPIMAGE_FILENAME = "map.png";
    static public void storeNemMap(WorldDataConst.WorldData worldData,string name)
    {
        worldData.name = name;
        DateTime dt = DateTime.Now.ToUniversalTime();
        DateTime dtUnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        worldData.lastUpdate = (long)dt.Subtract(dtUnixEpoch).TotalSeconds;

        DataUtility.save(worldData, worldData.uuid, WORLD_FILENAME);
        DataUtility.save(ListStore<WorldDataConst.EachTerrainData>.getListStore(worldData.terrainList), worldData.uuid, TERRAIN_FILENAME);
        DataUtility.save(ListStore<WorldDataConst.EachRegionData>.getListStore(worldData.buildingList), worldData.uuid, BUILDING_FILENAME);
        DataUtility.save(ListStore<WorldDataConst.EachVehicleInfo>.getListStore(worldData.vehicleList), worldData.uuid, VEHICLE_FILENAME);
        DataUtility.save(ListStore<WorldDataConst.EachWildAnimal>.getListStore(worldData.wildAnimalList), worldData.uuid, WILDANIMAL_FILENAME);
        DataUtility.storePictureData(worldData.mapPicture, worldData.uuid, MAPIMAGE_FILENAME);
    }

    [Serializable]
    class ListStore<T>
    {

        public List<T> list;

        static public ListStore<T> getListStore(List<T> _list)
        {
            ListStore<T> store = new ListStore<T>();
            store.list = _list;
            return store;
        }
        public List<T> getList()
        {
            return list;
        }

    }

    class MapComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            PlayerController.MapElement aMapElement = a as PlayerController.MapElement;
            PlayerController.MapElement bMapElement = b as PlayerController.MapElement;
            return aMapElement.lastUpdate_long.CompareTo(bMapElement.lastUpdate_long);
        }
    }
    static public List<PlayerController.MapElement> loadExistMapList()
    {
        List<PlayerController.MapElement> list = new List<PlayerController.MapElement>();

        String[] directories = DataUtility.getFolderList();
        for (int i = 0; i < directories.Length; i++)
        {
            WorldDataConst.WorldData worldInfo = DataUtility.load<WorldDataConst.WorldData>(directories[i], WORLD_FILENAME);
            if (worldInfo != null)
            {
                PlayerController.MapElement element = new PlayerController.MapElement();
                element.name = worldInfo.name;
                element.uuid = worldInfo.uuid;
                element.seeds = worldInfo.seeds;
                element.width = worldInfo.width;
                element.height = worldInfo.height;
                element.lastUpdate_long = worldInfo.lastUpdate;
                element.lastUpdate = DateTimeOffset.FromUnixTimeSeconds(worldInfo.lastUpdate).LocalDateTime.ToString();
                element.pathName = directories[i];
                list.Add(element);
            }

        }

        MapComparer comparer = new MapComparer();
        list.Sort((a, b) => (int)(a.lastUpdate_long - b.lastUpdate_long));
        list.Reverse();
        return list;
    }
    static public WorldDataConst.WorldData loadExistMapDatas(PlayerController.MapElement element)
    {
        var dbPath = element.pathName;
        WorldDataConst.WorldData worldData = DataUtility.load<WorldDataConst.WorldData>(dbPath, WORLD_FILENAME);
        worldData.terrainList = DataUtility.load<ListStore<WorldDataConst.EachTerrainData>>(dbPath, TERRAIN_FILENAME).getList();
        worldData.buildingList = DataUtility.load<ListStore<WorldDataConst.EachRegionData>>(dbPath, BUILDING_FILENAME).getList();
        worldData.vehicleList = DataUtility.load<ListStore<WorldDataConst.EachVehicleInfo>>(dbPath, VEHICLE_FILENAME).getList();
        worldData.wildAnimalList = DataUtility.load<ListStore<WorldDataConst.EachWildAnimal>>(dbPath, WILDANIMAL_FILENAME).getList();
        worldData.mapPicture = DataUtility.readPictureData(dbPath, MAPIMAGE_FILENAME);

        return worldData;

    }
}
