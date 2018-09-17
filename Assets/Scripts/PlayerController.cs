using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Concurrent;
using System;
using System.Threading.Tasks;

public class PlayerController : NetworkBehaviour
{

    [SerializeField]
    private Player player;

    [SerializeField]
    private WorldManager worldMgr;

    [SerializeField]
    public TileBase field;

    [SerializeField]
    public TileBase sea;

    [SerializeField]
    public TileBase waku;

    public class DispInfo
    {
        public float x;
        public float y;
        public Vector2 screenSize;
        public int[] generationList;
    }

    DispInfo dispInfo = null;

    [ClientCallback]
    public override void OnStartClient()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        player.setController(this);
    }

    [ServerCallback]
    void Start()
    {
        worldMgr = GameObject.Find("WorldMap").GetComponent<WorldManager>();

        if (dispInfo == null)
        {
            dispInfo = new DispInfo();
        }

        if (!worldMgr.isReady)
        {

            Target_DispMapGeneratorPanel(connectionToClient, worldMgr.loadExistMapList().ToArray());
        }
        else
        {
            BroadcastNewMap(worldMgr.worldData.width, worldMgr.worldData.height);
            player.TopMenu.SetActive(true);
        }

        
    }

    [ClientCallback]
    void Update()
    {
        UpdateMap();
    }
    [ServerCallback]
    public void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("");
    }
    [ServerCallback]
    public void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("");
    }
    [ServerCallback]
    public override void OnStartAuthority()
    {
        Debug.Log("");
    }
    [ServerCallback]
    public override void OnStopAuthority()
    {
        Debug.Log("");
    }
    [ClientCallback]
    public override void OnNetworkDestroy()
    {

    }

    [ServerCallback]
    private void OnDestroy()
    {
   //     worldMgr.removePlayerController(this);
    }




    //↓ここからマップ更新関連
    [Server]
    public void BroadcastNewMap(int width, int height)
    {
        dispInfo.generationList = Enumerable.Repeat(-1, (width / PlayerController.TransferSize) * (height / PlayerController.TransferSize)).ToArray();
        TargetBroadcastNewMap(connectionToClient, width, height);
    }

    [TargetRpc]
    public void TargetBroadcastNewMap(NetworkConnection conn, int width, int height)
    {
        clearMap(width, height);
        if (isServer)
        {
            enableGeneratePanel(true);
        }
    }
    [Client]
    void clearMap(int width, int height)
    {
        if (player.groundTilemap != null)
        {
            player.groundTilemap.ClearAllTiles();
            player.groundTilemap.size = new Vector3Int(width, height, 0);
            int minX = 0;
            int minY = 0;
            int maxX = width - 1;
            int maxY = height - 1;
            if (width > 200)
            {
                minX = width / 2 - 100;
                maxX = width / 2 + 99;
            }
            if (height > 200)
            {
                minY = height / 2 - 100;
                maxY = height / 2 + 99;
            }
            player.groundTilemap.SetTile(new Vector3Int(maxX, maxY, 0), sea);
            player.groundTilemap.BoxFill(Vector3Int.zero, sea, minX, minY, maxX, maxY);
        }
        player.SetPlayerPosition(new Vector2Int(width / 2, height / 2));
        updateList4Disp = new ConcurrentQueue<UpdateMapInfoContainer>();
        sendCameraPosition();
    }

    [Client]
    public void sendCameraPosition()
    {
        float cameraScreenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        float cameraScreenHeight = Camera.main.orthographicSize * 2;
        Cmd_StoreClientPosition(player.transform.localPosition.x, player.transform.localPosition.y, cameraScreenWidth, cameraScreenHeight);
    }

    [Command]
    void Cmd_StoreClientPosition(float _x, float _y, float _width, float _height)
    {
        dispInfo.x = _x;
        dispInfo.y = _y;
        dispInfo.screenSize = new Vector2(_width, _height);

        transferMapData2Client();
    }

    public static int TransferSize = 50;

    [Server]
    public void transferMapData2Client()
    {

        int minX = (int)(dispInfo.x - dispInfo.screenSize.x / 2);
        int minY = (int)(dispInfo.y - dispInfo.screenSize.y / 2);
        if (minX < 0)
        {
            minX = minX + worldMgr.worldData.width;
        }
        if (minY < 0)
        {
            minY = minY + worldMgr.worldData.height;
        }

        int maxX = (int)(minX + dispInfo.screenSize.x);
        int maxY = (int)(minY + dispInfo.screenSize.y);

        if ((maxX >= worldMgr.worldData.width) && (maxY >= worldMgr.worldData.height))
        {
            TransferData_Split(minX, minY, worldMgr.worldData.width - 1, worldMgr.worldData.height - 1);
            TransferData_Split(minX, 0, worldMgr.worldData.width - 1, maxY - worldMgr.worldData.height);
            TransferData_Split(0, minY, maxX - worldMgr.worldData.width, worldMgr.worldData.height - 1);
            TransferData_Split(0, 0, maxX - worldMgr.worldData.width, maxY - worldMgr.worldData.height);
        }
        else if (maxX >= worldMgr.worldData.width)
        {
            TransferData_Split(minX, minY, worldMgr.worldData.width - 1, maxY);
            TransferData_Split(0, minY, maxX - worldMgr.worldData.width, maxY);
        }
        else if (maxY >= worldMgr.worldData.height)
        {
            TransferData_Split(minX, minY, maxX, worldMgr.worldData.height - 1);
            TransferData_Split(minX, 0, maxX, maxY - worldMgr.worldData.height);
        }
        else
        {
            TransferData_Split(minX, minY, maxX, maxY);
        }
    }
    [Server]
    public void TransferData_Split(int minX, int minY, int maxX, int maxY)
    {
        int maxChunkX = maxX / TransferSize;
        int maxChunkY = maxY / TransferSize;
        int minChunkX = minX / TransferSize;
        int minChunkY = minY / TransferSize;

        for (int chunkY = minChunkY; chunkY <= maxChunkY; chunkY++)
        {
            for (int chunkX = minChunkX; chunkX <= maxChunkX; chunkX++)
            {
                if (dispInfo.generationList == null)
                {
                    dispInfo.generationList = Enumerable.Repeat(-1, (worldMgr.worldData.width / TransferSize) * (worldMgr.worldData.height / TransferSize)).ToArray();
                }
                int currentChunkNum_Session = 0;
                try
                {
                    currentChunkNum_Session = dispInfo.generationList[chunkY * (worldMgr.worldData.width / TransferSize) + chunkX];
                }
                catch (System.Exception ex)
                {
                    Debug.Log(chunkX + " " + TransferSize + " " + minY + " " + worldMgr.worldData.width + " " + chunkY + " " + TransferSize + " " + minX);
                }

                int currentChunkNum_System = 0;
                if (worldMgr.generationList.Length < chunkY * (worldMgr.worldData.width / TransferSize) + chunkX)
                    currentChunkNum_System = worldMgr.generationList[chunkY * (worldMgr.worldData.width / TransferSize) + chunkX];
                if (currentChunkNum_Session != currentChunkNum_System)
                {
                    EachTerrainTileData[] result = new EachTerrainTileData[TransferSize * TransferSize];
                    for (int y = 0; y < TransferSize; y++)
                    {
                        for (int x = 0; x < TransferSize; x++)
                        {
                            try
                            {
                                WorldDataConst.EachTerrainData sourceItem = worldMgr.worldData.terrainList[((chunkY * TransferSize) + y) * worldMgr.worldData.width + chunkX * TransferSize + x];
                                EachTerrainTileData item = new EachTerrainTileData();
                                item.type = sourceItem.type;
                                item.x = sourceItem.x;
                                item.y = sourceItem.y;
                                result[y * TransferSize + x] = item;
                            }
                            catch (System.Exception ex)
                            {
                                Debug.Log(ex.StackTrace);
                                Debug.Log(chunkX + " " + TransferSize + " " + minY + " " + worldMgr.worldData.width + " " + chunkY + " " + TransferSize + " " + minX);
                            }

                        }
                    }
                    TargetTransfer(connectionToClient, chunkY * TransferSize + minY, chunkX * TransferSize + minX, TransferSize, TransferSize, result);
                    dispInfo.generationList[chunkY * (worldMgr.worldData.width / TransferSize) + chunkX] = currentChunkNum_System;
                }
            }
        }
    }
    ConcurrentQueue<UpdateMapInfoContainer> updateList4Disp;
    public class UpdateMapInfoContainer
    {
        public EachTerrainTileData[] tiles;
        public int nextReadIndex = 0;
    }
    public class EachTerrainTileData
    {
        public WorldDataConst.TileType type;
        public int x;
        public int y;
    }
    [TargetRpc]
    public void TargetTransfer(NetworkConnection conn, int _x, int _y, int _width, int _height, EachTerrainTileData[] _tiles)
    {
        UpdateMapInfoContainer container = new UpdateMapInfoContainer()
        {
            tiles = _tiles
        };
        updateList4Disp.Enqueue(container);
    }
    [Client]
    public void UpdateMap()
    {
        UpdateMapInfoContainer dispContainer;

        DateTime start = DateTime.Now;

        if (updateList4Disp != null)
            while (updateList4Disp.TryDequeue(out dispContainer))
            {
                while (dispContainer.nextReadIndex < dispContainer.tiles.Length)
                {
                    EachTerrainTileData eachtile = dispContainer.tiles[dispContainer.nextReadIndex];
                    if (eachtile != null)
                    {
                        WorldDataConst.TileType currentTileType = eachtile.type;

                        Vector3Int currentPos = new Vector3Int(eachtile.x, eachtile.y, 0);
                        if (
                            (player.groundTilemap.GetTile(currentPos) == null) ||
                            (!player.groundTilemap.GetTile(currentPos).Equals(currentTileType))
                            )
                        {
                            if ((currentTileType == WorldDataConst.TileType.FIELD))
                            {
                                player.groundTilemap.SetTile(currentPos, field);
                            }
                            else
                            {
                                player.groundTilemap.SetTile(currentPos, sea);
                            }
                        }
                    }
                    dispContainer.nextReadIndex++;
                    DateTime current = DateTime.Now;

                    if ((current - start).TotalMilliseconds > 100)
                    {
                        current = start;
                        break;
                    }
                }
                if (dispContainer.nextReadIndex < dispContainer.tiles.Length)
                {
                    updateList4Disp.Enqueue(dispContainer);
                    break;
                }
            }
    }
    //↑ここまでマップ更新関連



    //↓ここからMap作成パネル関連
    [TargetRpc]
    public void Target_DispMapGeneratorPanel(NetworkConnection conn, MapElement[] list)
    {
        if (player.isServer)
            {

            player.generatorPanel.SetActive(true);
            player.TopMenu.SetActive(false);

            player.widthInput.ClearOptions();
            player.heightInput.ClearOptions();
            player.widthInput.AddOptions(createNumDropBoxCandidate(WorldDataConst.MapMinWidth, WorldDataConst.MapMaxWidth));
            player.heightInput.AddOptions(createNumDropBoxCandidate(WorldDataConst.MapMinHeight, WorldDataConst.MapMaxHeight));
            player.widthInput.RefreshShownValue();
            player.heightInput.RefreshShownValue();

            ToggleGroup group = player.mapCreateToggle.group;
            player.mapCreateToggle.isOn = true;
            player.mapCreateToggle.onValueChanged.RemoveAllListeners();
            player.mapCreateToggle.onValueChanged.AddListener((value) => OnSelectExistMap(value, null));
            foreach (Transform n in player.mapSelectionMenuList.transform)
            {

                GameObject.Destroy(n.gameObject);
            }
            if (list.Length > 0)
            {
                for (int i = 0; i < list.Length; i++)

                {
                    MapElement item = list[i];
                    //ボタン生成

                    Toggle menu = (Toggle)Instantiate(player.mapSelectItem);

                    //ボタンをContentの子に設定

                    menu.transform.SetParent(player.mapSelectionMenuList.GetComponent<RectTransform>(), false);

                    menu.group = group;

                    //ボタンのテキスト変更
                    menu.transform.GetComponentInChildren<Text>().text =
                        item.name + "\n" +
                        "Seeds: " + item.seeds + "\n" +
                        "Size: " + item.width + "x" + item.height + " UUID: " + item.uuid + "\n" +
                        "LastUpdate: " + item.lastUpdate;
                    //ボタンのクリックイベント登録
                    menu.onValueChanged.AddListener((value) => OnSelectExistMap(value, item));
                }
            }
        }
        else
        {
            player.informationText.text = "Waiting... This syastem is not available.";
            player.informationPanel.SetActive(true);
            player.TopMenu.SetActive(false);
        }
    }

    public List<Dropdown.OptionData> createNumDropBoxCandidate(int min,int max)
    {
        List<Dropdown.OptionData> result = new List<Dropdown.OptionData>();
        for (int i = min; i <= max; i++)
        {
            result.Add(new Dropdown.OptionData { text = i.ToString() });
        }
        return result;
    }
    int preWidth = 0;
    int preHeight = 0;
    string preSeeds = "";

    [Client]
    public void startGenerate()
    {

        if (player.seedsInput.text.Length == 0)
        {
            player.createRandomSeeds();
        }
        if (player.mapNameInput.text.Length == 0)
        {
            player.mapNameInput.text = "New Map";
        }

        string name = player.mapNameInput.text;
        string seeds = player.seedsInput.text;
        int width = System.Int32.Parse(player.widthInput.options[player.widthInput.value].text);
        int height = System.Int32.Parse(player.heightInput.options[player.heightInput.value].text);
        if ((width < WorldDataConst.MapMinWidth) || (WorldDataConst.MapMaxWidth < width))
        {
            player.informationPanel.SetActive(true);
            player.informationText.text = "Please input " + WorldDataConst.MapMinWidth + " - " + WorldDataConst.MapMaxWidth + " into width field.";
        }
        else if ((height < WorldDataConst.MapMinHeight) || (WorldDataConst.MapMaxHeight < height))
        {
            player.informationPanel.SetActive(true);
            player.informationText.text = "Please input " + WorldDataConst.MapMinHeight + " - " + WorldDataConst.MapMaxHeight + " into height field.";
        }
        else if (
            (!seeds.Equals(preSeeds)) ||
            (width != preWidth) ||
            (height != preHeight)
            )
        {
            enableGeneratePanel(false);
            player.informationText.text = "Now generating...";
            player.mapCreateToggle.isOn = true;
            worldMgr.Cmd_Generate_Async(player.mapNameInput.text, player.seedsInput.text, width * 100, height * 100);
            preSeeds = seeds;
            preWidth = width;
            preHeight = height;
        }
    }
    [Client]
    void enableGeneratePanel(bool flag)
    {
        player.mapNameInput.interactable = flag;
        player.seedsInput.interactable = flag;
        player.widthInput.interactable = flag;
        player.heightInput.interactable = flag;
        player.generateButton.interactable = flag;
        player.ApplyButton.interactable = flag;
        player.RandomButton.interactable = flag;
        player.informationPanel.SetActive(!flag);
    }
    [Client]
    public void selectNewMap()
    {
        string name = player.mapNameInput.text;
        string seeds = player.seedsInput.text;
        int width = System.Int32.Parse(player.widthInput.options[player.widthInput.value].text);
        int height = System.Int32.Parse(player.heightInput.options[player.heightInput.value].text);
        if (
            (!seeds.Equals(preSeeds)) ||
            (width != preWidth) ||
            (height != preHeight)
            )
        {
            dispInformationPanel_Timer("Please execute generating map...");
        }
        else
        {
            Cmd_SelectNewMap(name);
        }
    }

    [Client]
    public void selectExistMap()
    {
        Cmd_SelectExistMap(existMap);
    }

    [Command]
    public void Cmd_SelectNewMap(String name)
    {
        worldMgr.storeNemMap(name);
        worldMgr.setReadyToStart();
    }
    [Command]
    public void Cmd_SelectExistMap(MapElement element)
    {
        worldMgr.setReadyToStart();
    }

    public class MapElement
    {
        public String name;
        public String seeds;
        public String uuid;
        public String lastUpdate;
        public long lastUpdate_long;
        public int width;
        public int height;
        public String pathName;
    }

    public MapElement existMap;
    [Client]
    public void OnSelectExistMap(bool flag, MapElement item)
    {
        if (flag)
        {
            if (item == null)
            {
                Cmd_loadNewMap();
            }
            else
            {
                existMap = item;
                Cmd_loadExistMap(item);
            }
            player.informationPanel.SetActive(false);
        }
    }
    [Command]
    public void Cmd_loadNewMap()
    {
        worldMgr.loadNewMapDatas();
    }
    [Command]
    public void Cmd_loadExistMap(MapElement item)
    {
        worldMgr.loadExistMapDatas(item);
    }
    //↑ここまでMap作成関連


    //↓ここからトップメニュー関連
    [TargetRpc]
    public void TargetDispTopMenuPanel(NetworkConnection conn)
    {
        player.generatorPanel.SetActive(false);
        player.TopMenu.SetActive(true);
    }

    [Command]
    public void Cmd_Login(string userid,string password)
    {
        try
        {
            user=UserCountryUtility.autehnticateUser(worldMgr.current_uuid, userid, password);
            loginSuccess();
        }
        catch (Exception ex)
        {
            this.Target_DispInformationPanel_Timer(connectionToClient, ex.Message);
        }
    }
    [Command]
    public void Cmd_Logout()
    {
        user = null;
        
    }
    public bool loginstatus_clientside = false; 
    public void loginSuccess()
    {
        loginstatus_clientside = true;
        player.LoginLbl.text = "Logout";
        player.commandSelectionPanel.gameObject.SetActive(true);
    }
    public void logoutSuccess()
    {
        loginstatus_clientside = false;
        player.LoginLbl.text = "Login";
        player.commandSelectionPanel.gameObject.SetActive(false);
    }
    [Command]
    public void Cmd_CreateuUser(string userid, string password)
    {
        try
        {
            UserCountryUtility.createUser(worldMgr.current_uuid, userid, password);
            this.Target_DispInformationPanel_Timer(connectionToClient, "User creation is successful.\nPlease login using user id and password.");
        }
        catch(Exception ex)
        {
            this.Target_DispInformationPanel_Timer(connectionToClient, ex.Message);
        }
    }
    UserCountryUtility.User user = null;
    [Server]
    public UserCountryUtility.User getUserInfo()
    {
        user=UserCountryUtility.getUserFromUUID(worldMgr.current_uuid,user.UUID);
        return user;
    }

    //↑ここまでトップメニュー関連


    //↓ここからオペレーションコマンド
    public class CommandElement
    {
        public Player.ControlMode targetMode;
        public String title;
        public String description;

    }
    [Client]
    public void lookUpCommandList(int x, int y)
    {
        Cmd_RequestCreateCommandList(x, y);
    }

    [Command]
    public void Cmd_RequestCreateCommandList(int x, int y)
    {
        List<CommandElement> list = new List<CommandElement>();
        CommandElement command = new CommandElement();
        command.title = "Test";
        command.description = "Test";
        list.Add(command);
        command = new CommandElement();
        command.title = "Test2";
        command.description = "Test2";
        list.Add(command);
        TargetCommandDisp(connectionToClient, list.ToArray());
    }

    [TargetRpc]
    public void TargetCommandDisp(NetworkConnection conn, CommandElement[] list)
    {
        player.selectionMenuDescription.text = "";
        player.itemApplyButton.onClick.RemoveAllListeners();
        foreach (Transform n in player.selectionMenuList.transform)
        {
            GameObject.Destroy(n.gameObject);
        }
        if (list.Length > 0)
        {
            for (int i = 0; i < list.Length; i++)

            {
                CommandElement item = list[i];
                //ボタン生成

                Button btn = (Button)Instantiate(player.menuItem);

                //ボタンをContentの子に設定

                btn.transform.SetParent(player.selectionMenuList.GetComponent<RectTransform>(), false);

                //ボタンのテキスト変更
                btn.transform.GetComponentInChildren<Text>().text = item.title;
                //ボタンのクリックイベント登録
                btn.onClick.AddListener(() => OnSelectItem(item));

            }
            player.selectionMenu.SetActive(true);
            player.desctiptionMenu.SetActive(true);
            OnSelectItem(list[0]);
        }
        else
        {
            player.selectionMenu.SetActive(false);
            player.desctiptionMenu.SetActive(false);
        }
    }

    [Client]
    public void OnSelectItem(CommandElement item)
    {
        player.itemApplyButton.onClick.RemoveAllListeners();
        player.selectionMenuDescription.text = item.description;
        player.itemApplyButton.onClick.AddListener(() => OnApplyItem(item));
    }

    [Client]
    public void OnApplyItem(CommandElement item)
    {
        Debug.Log("コマンド実行");
    }
    //↑ここまでオペレーションコマンド


    //↓ここから表示系
    [TargetRpc]
    public void Target_DispInformationPanel_Timer(NetworkConnection conn, string str)
    {
        dispInformationPanel_Timer(str);
    }
    [Client]
    public void dispInformationPanel_Timer(string str)
    {
        player.informationText.text = str;
        player.informationPanel.SetActive(true);
        Invoke("closeInformationPanel", 3);
    }
    [Client]
    public void closeInformationPanel()
    {
        player.informationPanel.SetActive(false);
    }
    [TargetRpc]
    public void Target_DispInformationPanel(NetworkConnection conn,string str)
    {
        player.informationText.text = str;
        player.informationPanel.SetActive(true);
    }
    [TargetRpc]
    public void Target_CloseInformationPanel(NetworkConnection conn)
    {
        player.informationText.text = "";
        player.informationPanel.SetActive(false);
    }
    //↑ここまで表示系
}


