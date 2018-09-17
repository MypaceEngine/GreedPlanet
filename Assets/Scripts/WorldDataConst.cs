using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldDataConst{

    static public int MapMinWidth=1;
    static public int MapMinHeight = 1;
    static public int MapMaxWidth = 10;
    static public int MapMaxHeight = 10;

    //地形種別
    public enum TileType
    {
        NONE,
        SEA,
        FIELD
    };

    public enum BuildingType
    {
        NONE,
        SEA,
        FIELD
    };
    public enum MaterialType
    {
        NONE,
        SEA,
        FIELD
    };

    //1セル分の地形データ
    [Serializable]
    public class EachTerrainData
    {
        public TileType type;
        public int level;
        public int x;
        public int y;
    }
    [Serializable]
    public class EachRegionData
    {
        public BuildingType type;
        public int level;
        public int x;
        public int y;
        IList<StorageInfo> storageList;
    }
    [Serializable]
    public class StorageInfo
    {
        public MaterialType type;
        public long value;
    }
    [Serializable]
    public class EachVehicleInfo
    {
        public MaterialType type;
        public int level;
        List<StorageInfo> storageList;
        public int x;
        public int y;
    }
    [Serializable]
    public class EachWildAnimal
    {
        public MaterialType type;
        public long value;
        public int x;
        public int y;

    }
    [Serializable]
    public class WorldData
    {
        public string uuid;
        public string name;
        public string seeds;
        public int width;
        public int height;
        public long lastUpdate;
        [NonSerialized]
        public List<EachTerrainData> terrainList;
        [NonSerialized]
        public List<EachRegionData> buildingList;
        [NonSerialized]
        public List<EachVehicleInfo> vehicleList;
        [NonSerialized]
        public List<EachWildAnimal> wildAnimalList;
        [NonSerialized]
        public Texture2D mapPicture;
    }

    //情報分析用定数
    static public Vector2[] DIRECTIONS = new Vector2[]
    {
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(-1, 0),
        new Vector2(0, -1),
        new Vector2(1, 1),
        new Vector2(1, -1),
        new Vector2(-1, 1),
        new Vector2(-1, -1)
    };
}
