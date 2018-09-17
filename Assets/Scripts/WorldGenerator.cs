using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class WorldGenerator
{

    //面積計算のためのエリア区分データ
    public class AreaInvestigationInfo
    {
        public int size;
        public int parentArea;
    }
    //円形のSeedデータ
    public class TerrainSeed
    {
        public Vector2Int position;
        public int radius;
    }
    //四角いSeedデータ
    public class BandSeed
    {
        public int position;
        public int bandWidth;
    }

    //サーバでの世界データ作成
    static public WorldDataConst.WorldData GenerateMap(string name, string seeds, int width, int height)
    {
        Vector2Int size = new Vector2Int(width, height);
        Debug.Log("Start Generating! Seeds:" + seeds + " Width:" + size.x + " Height:" + size.y);

        //Seedsごとに同じ地図データを生成するため、Seeds固定の乱数生成器を作成
        System.Random rnd = new System.Random(seeds.GetHashCode());

        int seaSplitNum = 10;
        int seaSplitSizeX = size.x / seaSplitNum;
        int seaSplitSizeY = size.y / seaSplitNum;

        Debug.Log("SplitWidth:" + seaSplitSizeX + " SplitHeight:" + seaSplitSizeY);

        //地図上に描く縦横の海バンド生成
        List<BandSeed> verticalBands = generateBandSeeds(rnd, 0, 2, seaSplitNum, seaSplitSizeX);
        List<BandSeed> horizontalBands = generateBandSeeds(rnd, 0, 2, seaSplitNum, seaSplitSizeY);

        //地図上に描く海のSeeds
        List<TerrainSeed> sea_seeds = generateSeeds(rnd, size, 0, 5, seaSplitSizeX, seaSplitSizeY, 2);
        //地図上に描く陸のSeeds
        List<TerrainSeed> field_seeds = generateSeeds(rnd, size, 5, 12, seaSplitSizeX, seaSplitSizeY, 1);

        //Seedsを元に初期マップ生成
        WorldDataConst.EachTerrainData[] tileMap = generateFirstTileMap(rnd, size, verticalBands, horizontalBands, sea_seeds, field_seeds);

        //マップをランダマイズ
        int tryIndex = rnd.Next(10, 30);
        for (int l = 0; l < tryIndex; l++)
        {

            tileMap = deformationTileMap(rnd, size, tileMap);
        }
        //マップに存在する1Cellの陸や海を除去
        tileMap = fillGap(size, tileMap);


        //すべての陸と海のサイズを計算
        int[] areaNoList = Enumerable.Repeat<int>(-1, tileMap.Length).ToArray();
        List<AreaInvestigationInfo> sizeList = investigationSizeList(size, tileMap, areaNoList);
        sizeList = aggregateSizeList(sizeList);

        //小さな島は削除
        tileMap = removeSmallIsland(size, tileMap, areaNoList, sizeList);

        WorldDataConst.WorldData worldData = new WorldDataConst.WorldData();

        worldData.name = name;
        worldData.uuid = Guid.NewGuid().ToString().Replace("-", "");
        worldData.seeds = seeds;
        worldData.width = width;
        worldData.height = height;
        //現在のUTC時刻取得
        DateTime dt = DateTime.Now.ToUniversalTime();
        DateTime dtUnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        worldData.lastUpdate = (long)dt.Subtract(dtUnixEpoch).TotalSeconds;
        worldData.terrainList = new List<WorldDataConst.EachTerrainData>(tileMap);
        worldData.buildingList = new List<WorldDataConst.EachRegionData>();
        worldData.vehicleList = new List<WorldDataConst.EachVehicleInfo>();
        worldData.wildAnimalList = new List<WorldDataConst.EachWildAnimal>();

        return worldData;

    }

    //幅のあるバンド領域を生成
    static private List<BandSeed> generateBandSeeds(System.Random rnd, int cntMin, int cntMax, int seaSplitNum, int seaSplitSize)
    {
        List<BandSeed> bands = new List<BandSeed>();
        int cnt = rnd.Next(cntMin, cntMax);
        for (int i = 0; i < cnt; i++)
        {
            bands.Add(new BandSeed() { position = rnd.Next(2, seaSplitNum - 2), bandWidth = seaSplitSize });
        }
        return bands;
    }

    //地形のSeedsを生成
    static private List<TerrainSeed> generateSeeds(System.Random rnd, Vector2Int size, int min, int max, int seaSplitSizeX, int seaSplitSizeY, int magnification)
    {
        List<TerrainSeed> seeds = new List<TerrainSeed>();
        int seaSeedCount = rnd.Next(min, max);

        int seedPosX = 0;
        int seedPosY = 0;
        // 種を作る。
        for (int i = 0; i < seaSeedCount; ++i)
        {

            seedPosX = (seedPosX + rnd.Next(size.x / 5, size.x / 2)) % size.x;
            seedPosY = (seedPosY + rnd.Next(size.y / 5, size.y / 2)) % size.y;

            TerrainSeed new_seed = new TerrainSeed();
            new_seed.position.x = seedPosX;
            new_seed.position.y = seedPosY;
            new_seed.radius = rnd.Next((seaSplitSizeX + seaSplitSizeY) * magnification / 8, seaSplitSizeX * magnification);

            seeds.Add(new_seed);
        }
        return seeds;
    }

    //初期マップを生成
    static private WorldDataConst.EachTerrainData[] generateFirstTileMap(
        System.Random rnd,
        Vector2Int size,
        List<BandSeed> verticalBands,
        List<BandSeed> horizontalBands,
        List<TerrainSeed> sea_seeds,
        List<TerrainSeed> field_seeds
        )
    {
        WorldDataConst.EachTerrainData[] tileMap = new WorldDataConst.EachTerrainData[size.x * size.y];
        for (int y = 0; y < size.y; y++)
        {
            bool verticalSplitFlag = false;
            for (int i = 0; i < verticalBands.Count; i++)
            {
                if ((verticalBands[i].position * verticalBands[i].bandWidth <= y) && ((verticalBands[i].position + 1) * verticalBands[i].bandWidth > y))
                {
                    verticalSplitFlag = true;
                    break;
                }
            }
            for (int x = 0; x < size.x; x++)
            {

                WorldDataConst.EachTerrainData tile = new WorldDataConst.EachTerrainData();
                tile.x = x;
                tile.y = y;
                WorldDataConst.TileType type = WorldDataConst.TileType.NONE;

                for (int i = 0; i < sea_seeds.Count; i++)
                {
                    if ((Mathf.Pow((sea_seeds[i].position.x - x), 2) + Mathf.Pow((sea_seeds[i].position.y - y), 2)) < Mathf.Pow(sea_seeds[i].radius, 2))
                    {
                        type = WorldDataConst.TileType.SEA;
                        break;
                    }
                }

                if (type == WorldDataConst.TileType.NONE)
                {
                    for (int i = 0; i < field_seeds.Count; i++)
                    {
                        if ((Mathf.Pow((field_seeds[i].position.x - x), 2) + Mathf.Pow((field_seeds[i].position.y - y), 2)) < Mathf.Pow(field_seeds[i].radius, 2))
                        {
                            type = WorldDataConst.TileType.FIELD;
                            break;
                        }
                    }
                }
                if ((type == WorldDataConst.TileType.NONE) && (verticalSplitFlag))
                {
                    type = WorldDataConst.TileType.SEA;
                }

                if (type == WorldDataConst.TileType.NONE)
                {
                    for (int i = 0; i < horizontalBands.Count; i++)
                    {
                        if ((horizontalBands[i].position * horizontalBands[i].bandWidth <= x) && ((horizontalBands[i].position + 1) * horizontalBands[i].bandWidth > x))
                        {
                            type = WorldDataConst.TileType.SEA;
                            break;
                        }
                    }
                }
                if (type == WorldDataConst.TileType.NONE)
                {
                    type = WorldDataConst.TileType.SEA;
                    if (rnd.Next(0, 2) == 0)
                    {
                        type = WorldDataConst.TileType.FIELD;
                    }
                }

                tile.type = type;
                tileMap[y * size.x + x] = tile;
            }
        }
        return tileMap;
    }

    //地形をランダマイズ
    static private WorldDataConst.EachTerrainData[] deformationTileMap(System.Random rnd, Vector2Int size, WorldDataConst.EachTerrainData[] tileMap)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {

                // 隣接タイルが同じだったら浸食
                int treatCount = WorldDataConst.DIRECTIONS.Length;

                CountInfo countInfo = countSameCell(treatCount, size, tileMap, x, y);

                if ((y == 0) || (y == size.y - 1))
                {
                    countInfo.currentTile.type = WorldDataConst.TileType.SEA;
                }
                else if (countInfo.count == treatCount)
                {
                    countInfo.currentTile.type = countInfo.diffType;
                }

                else if ((countInfo.count > 0) && (rnd.Next(0, treatCount) < countInfo.count))
                {
                    countInfo.currentTile.type = countInfo.diffType;
                }
            }
        }
        return tileMap;
    }

    //1Cellだけの陸と海を除去
    static private WorldDataConst.EachTerrainData[] fillGap(Vector2Int size, WorldDataConst.EachTerrainData[] tileMap)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                // 隣接タイルが同じだったら浸食
                int treatCount = 4;
                CountInfo countInfo = countSameCell(treatCount, size, tileMap, x, y);
                if ((y == 0) || (y == size.y - 1))
                {
                    countInfo.currentTile.type = WorldDataConst.TileType.SEA;
                }
                else if (countInfo.count == treatCount)
                {
                    countInfo.currentTile.type = countInfo.diffType;
                }
                else if ((countInfo.currentTile.type == WorldDataConst.TileType.FIELD) && (countInfo.count == 2))
                {
                    countInfo.currentTile.type = WorldDataConst.TileType.SEA;
                }
            }
        }
        return tileMap;
    }

    //周囲の地形をカウント
    public class CountInfo
    {
        internal WorldDataConst.EachTerrainData currentTile;
        internal WorldDataConst.TileType diffType;
        internal int count;
    }

    //周囲の同じ種別の地形をカウント
    static private CountInfo countSameCell(int treatCount, Vector2Int size, WorldDataConst.EachTerrainData[] tileMap, int x, int y)
    {
        int cellCount = 0;
        WorldDataConst.EachTerrainData currentTile = tileMap[y * size.x + x];
        WorldDataConst.TileType currentTileType = currentTile.type;
        WorldDataConst.TileType diffTileType = WorldDataConst.TileType.NONE;

        for (int i = 0; i < treatCount; i++)
        {

            Vector2Int targetPos = treatPosition(size, x + (int)WorldDataConst.DIRECTIONS[i].x, y + (int)WorldDataConst.DIRECTIONS[i].y);
            //           Debug.Log(targetPos.x + " " + size.x + " " +  targetPos.y + " "+size.y);
            WorldDataConst.TileType roundTileType = tileMap[targetPos.y * size.x + targetPos.x].type;
            if (currentTileType != roundTileType)
            {
                cellCount++;
                diffTileType = roundTileType;
            }
        }
        return new CountInfo()
        {
            currentTile = currentTile,
            diffType = diffTileType,
            count = cellCount
        };
    }

    //地形のサイズを計るため、エリア情報を作成し、サイズをカウント
    static private List<AreaInvestigationInfo> investigationSizeList(Vector2Int size, WorldDataConst.EachTerrainData[] tileMap, int[] areaNolist)
    {
        List<AreaInvestigationInfo> sizeList = new List<AreaInvestigationInfo>();


        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                WorldDataConst.EachTerrainData tile = tileMap[y * size.x + x];

                if (areaNolist[y * size.x + x] == -1)
                {
                    areaNolist[y * size.x + x] = sizeList.Count;
                    AreaInvestigationInfo newfield = new AreaInvestigationInfo();
                    newfield.parentArea = sizeList.Count;
                    newfield.size = 1;
                    sizeList.Add(newfield);
                }

                Vector2[] dirirections_2 = new Vector2[]
                {
                           new Vector2(1, 0),
                           new Vector2(0, 1)
                };

                int currentAreaNo = areaNolist[y * size.x + x];
                for (int i = 0; i < dirirections_2.Length; i++)
                {
                    Vector2Int targetPos = treatPosition(size, x + (int)WorldDataConst.DIRECTIONS[i].x, y + (int)WorldDataConst.DIRECTIONS[i].y);
                    WorldDataConst.EachTerrainData roundTile = tileMap[targetPos.y * size.x + targetPos.x];
                    if (roundTile.type == tile.type)
                    {
                        int targetAreaNo = areaNolist[targetPos.y * size.x + targetPos.x];
                        if (targetAreaNo == -1)
                        {
                            areaNolist[targetPos.y * size.x + targetPos.x] = currentAreaNo;
                            sizeList[currentAreaNo].size++;
                        }
                        else if (
                            (targetAreaNo != currentAreaNo) &&
                            (sizeList[targetAreaNo].parentArea != currentAreaNo) &&
                            (targetAreaNo != sizeList[currentAreaNo].parentArea)
                            )
                        {
                            int serachPoleA = getRootAreaNo(targetAreaNo, sizeList);
                            int serachPoleB = getRootAreaNo(currentAreaNo, sizeList);
                            if (serachPoleA != serachPoleB)
                            {
                                sizeList[serachPoleA].parentArea = serachPoleB;
                            }

                        }
                    }

                }
            }
        }
        return sizeList;
    }

    //エリアのルートノードを取得
    static private int getRootAreaNo(int serachPole, List<AreaInvestigationInfo> sizeList)
    {
        while (sizeList[serachPole].parentArea != serachPole)
        {
            serachPole = sizeList[serachPole].parentArea;
        }
        return serachPole;
    }

    //エリアを連結して、島のサイズを計算
    static private List<AreaInvestigationInfo> aggregateSizeList(List<AreaInvestigationInfo> sizeList)
    {
        for (int i = 0; i < sizeList.Count; i++)
        {
            int serachPole = i;
            if (sizeList[serachPole].parentArea != serachPole)
            {
                while (sizeList[serachPole].parentArea != serachPole)
                {
                    serachPole = sizeList[serachPole].parentArea;
                }
                sizeList[i].parentArea = serachPole;
                sizeList[serachPole].size = sizeList[serachPole].size + sizeList[i].size;
                sizeList[i].size = 0;
            }
        }
        return sizeList;
    }

    //小さな島を削除
    static private WorldDataConst.EachTerrainData[] removeSmallIsland(Vector2Int size, WorldDataConst.EachTerrainData[] tileMap, int[] areaNolist, List<AreaInvestigationInfo> sizeList)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                WorldDataConst.EachTerrainData tile = tileMap[y * size.x + x];
                int areaNo = areaNolist[y * size.x + x];
                int islandNum = sizeList[areaNo].parentArea;
                int islandSize = sizeList[islandNum].size;

                if ((tile.type == WorldDataConst.TileType.FIELD) && (islandSize < 10))
                {
                    tile.type = WorldDataConst.TileType.SEA;
                }
            }
        }
        return tileMap;
    }

    //マップ外の座標をマップ座標に計算
    static private Vector2Int treatPosition(Vector2Int size, int x, int y)
    {
        if (x < 0)
        {
            x = size.x + x;
        }
        else if (size.x <= x)
        {
            x = size.x - x;
        }
        if (y < 0)
        {
            y = size.y + y;
        }
        else if (size.y <= y)
        {
            y = size.y - y;
        }
        return new Vector2Int(x, y);
    }
    static public Texture2D createMapPicture(WorldDataConst.WorldData worldData)
    {
        // テクスチャ生成
        var tex = new Texture2D(worldData.width, worldData.height);
        for (int y = 0; y < worldData.height; y++)
        {
            for (int x = 0; x < worldData.width; x++)
            {
                WorldDataConst.EachTerrainData terrainData = worldData.terrainList[y * worldData.width + x];
                Color color = Color.cyan;
                string colorString = "#4080C0"; // 赤色の16進数文字列
                ColorUtility.TryParseHtmlString(colorString, out color);
                if (terrainData.type == WorldDataConst.TileType.FIELD)
                {
                    colorString = "#A7C131"; // 赤色の16進数文字列
                    ColorUtility.TryParseHtmlString(colorString, out color);
                }
                tex.SetPixel(x, y, color);
            }
        }
        return tex;
    }
}
