using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TouchScript.Gestures.TransformGestures;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using TouchScript.Gestures;
using System.Linq;

public class Player : MonoBehaviour
{



    [SerializeField]
    private Camera primaryCamera;

    [SerializeField]
    private Camera secondaryCameraX;

    [SerializeField]
    private Camera secondaryCameraY;

    [SerializeField]
    private Camera secondaryCameraXY;

    [SerializeField]
    public Tilemap groundTilemap;
    [SerializeField]
    public Tilemap buildingTilemap;
    [SerializeField]
    public Tilemap movingTilemap;
    [SerializeField]
    public Tilemap selectTilemap;

    [SerializeField]
    public InputField mapNameInput;

    [SerializeField]
    public InputField seedsInput;

    [SerializeField]
    public Dropdown widthInput;

    [SerializeField]
    public Dropdown heightInput;

    [SerializeField]
    public Text informationText;

    [SerializeField]
    public GameObject informationPanel;

    [SerializeField]
    public Button RandomButton;

    [SerializeField]
    public Button generateButton;

    [SerializeField]
    public Button ApplyButton;

    [SerializeField]
    public GameObject generatorPanel;

    [SerializeField]
    private PlayerController ctr;


    [SerializeField]
    private CustomNetworkManagerHUD hud;

    //   [SerializeField]
    //   private Vector2Int mapPosition;

    [SerializeField]
    public WorldGenerator generator;

    [SerializeField]
    public Canvas commandSelectionPanel;
    [SerializeField]
    public GameObject menuPanel;
    [SerializeField]
    public Button viewButton;
    [SerializeField]
    public Button constructionButton;
    [SerializeField]
    public Button configureButton;
    [SerializeField]
    public Button militaryButton;
    [SerializeField]
    public GameObject selectionMenu;
    [SerializeField]
    public GameObject selectionMenuList;

    [SerializeField]
    public GameObject desctiptionMenu;
    [SerializeField]
    public InputField selectionMenuDescription;
    [SerializeField]
    public Button itemApplyButton;
    [SerializeField]
    public Button menuItem;

    [SerializeField]
    public Toggle mapCreateToggle;
    [SerializeField]
    public Toggle mapSelectItem;
    [SerializeField]
    public GameObject mapSelectionMenuList;

    [SerializeField]
    public GameObject TopMenu;
    [SerializeField]
    public GameObject TopMenuPanel;
    [SerializeField]
    public Text LoginLbl;

    [SerializeField]
    public GameObject LoginMenu;
    [SerializeField]
    public GameObject LoginMenuPanel;

    [SerializeField]
    public Toggle LoginTggle;
    [SerializeField]
    public Toggle CreateUserToggle;
    [SerializeField]
    public InputField UserIDInput;
    [SerializeField]
    public InputField PasswordInput;
    [SerializeField]
    public InputField RePasswordInput;
    [SerializeField]
    public Button LoginButton;
    //    float moveTime = 0.0f;

    //    int x_dir = 0;
    //   int y_dir = 0;

    //   float offsetX;
    //   float offsetY;

    public bool isServer = false;

    public void setController(PlayerController _ctr)
    {
        ctr = _ctr;
    }

    public void startGeneratingMap()
    {
        ctr.startGenerate();
    }

    public void createRandomSeeds()
    {
        seedsInput.text = System.Guid.NewGuid().ToString().Replace("-", "");
    }

    public void selectMap()
    {
        ToggleGroup group = mapCreateToggle.group;
        if (group.ActiveToggles().FirstOrDefault() == mapCreateToggle)
        {
            ctr.selectNewMap();
        }
        else
        {
            ctr.selectExistMap();
        }
    }

    public void SetPlayerPosition(Vector2Int pos)
    {
        //       mapPosition.x = pos.x;
        //       mapPosition.y = pos.y;
        //
        transform.localPosition = new Vector3(pos.x + 0.5f, pos.y + 0.5f, -10);
        //
    }

    //   public void SetTilemap( Tilemap target ) {
    //       targetTilemap = target;
    //   }


    public float LoopValue(float value, float adjust, int min, int max)
    {

        value += adjust;

        if (value >= max)
        {
            value = min;
        }
        if (value < min)
        {
            value = max - 1;
        }

        return value;
    }

    // Use this for initialization
    float def_orthographicSize = 0;
    void Start()
    {
        //       if (targetTilemap != null) { 
        var tip_size = Screen.height / primaryCamera.orthographicSize;
        //       offsetX = targetTilemap.size.x; //Screen.width / tip_size * 4.0f;
        //       offsetY = targetTilemap.size.y; //Screen.height / tip_size * 4.0f;
        def_orthographicSize = primaryCamera.orthographicSize;
        //       Camera.setmain = secondaryCameraX;
        secondaryCameraX.transform.localPosition = new Vector3(groundTilemap.size.x, 0.0f, 0.0f);
        secondaryCameraY.transform.localPosition = new Vector3(0.0f, groundTilemap.size.y, 0.0f);
        secondaryCameraXY.transform.localPosition = new Vector3(groundTilemap.size.x, groundTilemap.size.y, 0.0f);
        //   }

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            //           NetworkManager.singleton.networkPort = 7777;
            //           NetworkManager.singleton.networkAddress = "localhost";
            hud.enabled = false;
            NetworkManager.singleton.StartClient();
        }
        else
        {
            hud.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        //       if ( ( x_dir == 0 ) && ( y_dir == 0 ) ) {
        //          if ( Input.GetAxis( "Horizontal" ) > 0.0f ) {
        //                  x_dir = 1;
        //                   moveTime = 0.0f;
        //           }
        //           if ( Input.GetAxis( "Horizontal" ) < 0.0f ) {
        //                   x_dir = -1;
        //                    moveTime = 0.0f;
        //           }
        //       }
        //       if ( ( x_dir == 0 ) && ( y_dir == 0 ) ) {

        //           if ( Input.GetAxis( "Vertical" ) > 0.0f ) {
        //                   y_dir = 1;
        //                   moveTime = 0.0f;
        //           }
        //           if ( Input.GetAxis( "Vertical" ) < 0.0f ) {
        //                   y_dir = -1;
        //                   moveTime = 0.0f;
        //           }
        //       }

        zoomMouse();


        //       if ( ( x_dir != 0 ) || ( y_dir != 0 ) ) {

        //           moveTime += Time.deltaTime;

        //           if ( moveTime > 0.4f ) {


        //               moveTime = 0.0f;

        //           }
        //transform.localPosition = new Vector3( mapPosition.x + Mathf.Clamp01( moveTime / 0.4f ) * x_dir + 0.5f, mapPosition.y + Mathf.Clamp01( moveTime / 0.4f ) * y_dir + 0.5f, -10 );
        //           transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -10+ fWheel);


        //       }

    }
    private void zoomMouse()
    {
        float fWheel = Input.GetAxis("Mouse ScrollWheel");
        float orthographicSize = primaryCamera.orthographicSize + fWheel * 10;
        if (groundTilemap != null)
            zoomExec(orthographicSize);
    }
    private void zoomGesture(float times)
    {
        float orthographicSize = primaryCamera.orthographicSize * times;
        if (groundTilemap != null)
            zoomExec(orthographicSize);
    }

    float previousZoom = -1;

    private void zoomExec(float orthographicSize)
    {
        //       GameObject cube = GameObject.Find("Ground");
        //        targetTilemap = null;
        //       if(cube!=null)
        //           targetTilemap=cube.GetComponent<Tilemap>();
        //       if (targetTilemap != null)
        //       {

        float cameraScreenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        float cameraScreenHeight = Camera.main.orthographicSize * 2;
        float limtHeight = groundTilemap.size.y / 2.0f;
        float limtwidth = groundTilemap.size.x / 2.0f;
        if (Screen.width < Screen.height)
        {
            limtHeight = (limtHeight * cameraScreenWidth) / cameraScreenHeight;
        }
        if (Screen.width > Screen.height)
        {
            limtwidth = (limtwidth * cameraScreenHeight / cameraScreenWidth);
        }
        if (orthographicSize > limtHeight)
            orthographicSize = limtHeight;
        if (orthographicSize > limtwidth)
            orthographicSize = limtwidth;


        if (orthographicSize < 6)
        {
            orthographicSize = 6;
        }
        primaryCamera.orthographicSize = orthographicSize;
        secondaryCameraX.orthographicSize = orthographicSize;
        secondaryCameraY.orthographicSize = orthographicSize;
        secondaryCameraXY.orthographicSize = orthographicSize;
        //       }
        if (previousZoom != orthographicSize)
        {
            if (previousZoom != -1)
            {
                ctr.sendCameraPosition();
            }

            previousZoom = orthographicSize;
        }


    }


    private void OnEnable()
    {
        GetComponent<TransformGesture>().TransformStarted += OnTransformStarted;
        GetComponent<TransformGesture>().Transformed += OnTransformed;
        GetComponent<TransformGesture>().TransformCompleted += OnTransformCompleted;
        GetComponent<TapGesture>().Tapped += OnTapped;
    }

    private void OnDisable()
    {
        GetComponent<TransformGesture>().TransformStarted -= OnTransformStarted;
        GetComponent<TransformGesture>().Transformed -= OnTransformed;
        GetComponent<TransformGesture>().TransformCompleted -= OnTransformCompleted;
        GetComponent<TapGesture>().Tapped -= OnTapped;
    }

    private void OnTapped(object sender, EventArgs e)
    {

        if ((groundTilemap.size.x > 0) && (groundTilemap.size.y > 0))
        {

            var g = GetComponent<TapGesture>();
            Vector2 vec = g.ScreenPosition;
            if (isNotTouch(vec))
            {
                LoginMenu.SetActive(false);

                float orthographicSize = primaryCamera.orthographicSize;
                //float scale = (Screen.dpi) / orthographicSize;
                //float scale = orthographicSize;
                float scale = (Screen.dpi*3.25f) / orthographicSize;
                float xpos = (transform.localPosition.x + (vec.x - Screen.width / 2) / scale);
                float ypos = (transform.localPosition.y + (vec.y - Screen.height / 2) / scale);

             

                if (xpos > groundTilemap.size.x)
                {
                    xpos = xpos - groundTilemap.size.x;
                }
                if (ypos > groundTilemap.size.y)
                {
                    ypos = ypos - groundTilemap.size.y;
                }
                if (xpos < 0)
                {
                    xpos = groundTilemap.size.x + xpos;
                }
                if (ypos < 0)
                {
                    ypos = groundTilemap.size.y + ypos;
                }
                selectTilemap.ClearAllTiles();
                selectTilemap.SetTile(new Vector3Int((int)xpos, (int)ypos, 0), ctr.waku);

                //groundTilemap.SetColor(new Vector3Int((int)xpos, (int)ypos, 0), Color.red);
                Debug.Log("タップされた" + Screen.dpi + " " + groundTilemap.size.x + " " + orthographicSize + " " + vec.x + " " + vec.y + " " + xpos + " " + ypos + " " + transform.localPosition.x + " " + transform.localPosition.y);
                //                if ((ctrMode != ControlMode.VIEW) && (vec.y < heightDisable))
                //                {
                //                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - 200 / scale, transform.localPosition.z);
                //                }
                ctr.lookUpCommandList((int)xpos, (int)ypos);

            }

        }
    }

    public bool isNotTouch(Vector2 vec)
    {
        float widthDisable = 0;
        float heightDisable = 0;
        if (commandSelectionPanel.gameObject.activeSelf)
        {
            heightDisable = menuPanel.GetComponent<RectTransform>().sizeDelta.y;
            widthDisable = menuPanel.GetComponent<RectTransform>().sizeDelta.x;
            if (selectionMenu.activeSelf)
            {
                widthDisable = Screen.width;
            }
            if ((vec.x < widthDisable) && (vec.y < heightDisable))
            {
                return false;
            }
        }
        if (TopMenu.gameObject.activeSelf)
        {
            heightDisable = TopMenuPanel.GetComponent<RectTransform>().sizeDelta.y;
            widthDisable = TopMenuPanel.GetComponent<RectTransform>().sizeDelta.x;
            if (vec.y > Screen.height - heightDisable)
            {
                return false;
            }
        }
        if (LoginMenu.gameObject.activeSelf)
        {
            heightDisable = LoginMenuPanel.GetComponent<RectTransform>().sizeDelta.y;
            widthDisable = LoginMenuPanel.GetComponent<RectTransform>().sizeDelta.x;
            if ((vec.x > Screen.width - widthDisable) && (vec.y > Screen.height - heightDisable))
            {
                return false;
            }
        }
        return true;
    }


    bool movingEnable = true;
    private void OnTransformStarted(object sender, System.EventArgs e)
    {
        var g = GetComponent<TransformGesture>();
        if (
            (selectionMenu.gameObject.activeSelf) &&
            (g.ScreenPosition.y < selectionMenu.gameObject.GetComponent<RectTransform>().sizeDelta.y)
            )
        {
            movingEnable = false;
        }
        Debug.Log("変形を開始した");
    }

    Vector2 previousPos = new Vector2(0, 0);
    DateTime previousEchoTime = new DateTime(0);
    //    long previousEchoMilliSec = 0;
    private void OnTransformed(object sender, System.EventArgs e)
    {
        if (movingEnable)
        {
            //LoginMenu.SetActive(false);

            var g = GetComponent<TransformGesture>();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("変形中");
            sb.AppendLine("Position : " + g.ScreenPosition);
            sb.AppendLine("DeltaPosition : " + g.DeltaPosition);
            sb.AppendLine("DeltaRotation: " + g.DeltaRotation);
            sb.AppendLine("DeltaScale: " + g.DeltaScale);
            sb.AppendLine("RotationAxis: " + g.RotationAxis);
            Debug.Log(sb);

            //       float times=(def_orthographicSize)/primaryCamera.orthographicSize;
            float times = 1;
            zoomGesture(g.DeltaScale);

            float x = LoopValue(transform.localPosition.x, -g.DeltaPosition.x / times, 0, groundTilemap.size.x);
            float y = LoopValue(transform.localPosition.y, -g.DeltaPosition.y / times, 0, groundTilemap.size.y);

            transform.localPosition = new Vector3(x, y, -10);

            Vector2 v = new Vector3(groundTilemap.size.x, groundTilemap.size.y, 0.0f);
            if (transform.localPosition.x < (groundTilemap.size.x / 2.0f))
            {
                secondaryCameraX.transform.localPosition = new Vector3(groundTilemap.size.x, 0.0f, 0.0f);
                v.x = groundTilemap.size.x;
            }
            if (transform.localPosition.x > (groundTilemap.size.x / 2.0f))
            {
                secondaryCameraX.transform.localPosition = new Vector3(-groundTilemap.size.x, 0.0f, 0.0f);
                v.x = -groundTilemap.size.x;
            }

            if (transform.localPosition.y < (groundTilemap.size.y / 2.0f))
            {
                secondaryCameraY.transform.localPosition = new Vector3(0.0f, groundTilemap.size.y, 0.0f);
                v.y = groundTilemap.size.y;
            }
            if (transform.localPosition.y > (groundTilemap.size.y / 2.0f))
            {
                secondaryCameraY.transform.localPosition = new Vector3(0.0f, -groundTilemap.size.y, 0.0f);
                v.y = -groundTilemap.size.y;
            }
            secondaryCameraXY.transform.localPosition = v;

            if ((previousPos.x != x) || (previousPos.y != y))
            {
                DateTime now = DateTime.Now;

                if ((now - previousEchoTime).TotalMilliseconds > 100)
                {
                    if (ctr != null)
                        ctr.sendCameraPosition();
                    previousPos.x = x;
                    previousPos.y = y;
                    previousEchoTime = now;

                }
            }
        }
    }

    private void OnTransformCompleted(object sender, System.EventArgs e)
    {
        movingEnable = true;
        Debug.Log("変形を完了した");
    }


    //   public void loadMapList()
    //   {
    //       ctr.loadMapList();
    //   }


    //   public void generate()
    //    {
    //       Cmd_test();
    //   }
    //   public void Cmd_test()
    //   {
    //       Debug.Log("dayo");
    //   }
    public bool get_isServer()
    {
        return true;
    }
    public enum ControlMode
    {
        VIEW,
        CONSTRUCTION,
        CONFIGURATION,
        MILITARY
    };
    ControlMode ctrMode = ControlMode.VIEW;
    public void selectViewMode()
    {
        ctrMode = ControlMode.VIEW;
        swicthMenu();
    }
    public void selectConstructionMode()
    {
        ctrMode = ControlMode.CONSTRUCTION;
        swicthMenu();

    }
    public void selectConfigureMode()
    {
        ctrMode = ControlMode.CONFIGURATION;
        swicthMenu();

    }
    public void selectMilitaryMode()
    {
        ctrMode = ControlMode.MILITARY;
        swicthMenu();

    }
    public void swicthMenu()
    {
        var viewblock = viewButton.colors;
        viewblock.normalColor = new Color(viewblock.normalColor.r, viewblock.normalColor.g, viewblock.normalColor.b, 100f / 255f);
        var constructionblock = constructionButton.colors;
        constructionblock.normalColor = new Color(constructionblock.normalColor.r, constructionblock.normalColor.g, constructionblock.normalColor.b, 100f / 255f);
        var configureblock = configureButton.colors;
        configureblock.normalColor = new Color(configureblock.normalColor.r, configureblock.normalColor.g, configureblock.normalColor.b, 100f / 255f);
        var militaryblock = militaryButton.colors;
        militaryblock.normalColor = new Color(militaryblock.normalColor.r, militaryblock.normalColor.g, militaryblock.normalColor.b, 100f / 255f);
        //       selectionMenu.SetActive(false);
        //        desctiptionMenu.SetActive(false);
        if (ctrMode == ControlMode.MILITARY)
        {
            militaryblock.normalColor = new Color(militaryblock.normalColor.r, militaryblock.normalColor.g, militaryblock.normalColor.b, 255f / 255f);
            //            selectionMenu.SetActive(true);
            //           desctiptionMenu.SetActive(true);
        }
        else if (ctrMode == ControlMode.CONSTRUCTION)
        {
            constructionblock.normalColor = new Color(constructionblock.normalColor.r, constructionblock.normalColor.g, constructionblock.normalColor.b, 255f / 255f);
            //            selectionMenu.SetActive(true);
            //            desctiptionMenu.SetActive(true);
        }
        else if (ctrMode == ControlMode.CONFIGURATION)
        {
            configureblock.normalColor = new Color(configureblock.normalColor.r, configureblock.normalColor.g, configureblock.normalColor.b, 255f / 255f);
            //           selectionMenu.SetActive(true);
            //            desctiptionMenu.SetActive(true);
        }
        else
        {
            viewblock.normalColor = new Color(viewblock.normalColor.r, viewblock.normalColor.g, viewblock.normalColor.b, 255f / 255f);
        }
        viewButton.colors = viewblock;
        constructionButton.colors = constructionblock;
        configureButton.colors = configureblock;
        militaryButton.colors = militaryblock;
    }

    public void logInOutPanelCtr()
    {
        if (ctr.loginstatus_clientside)
        {
            ctr.Cmd_Logout();
            ctr.logoutSuccess();
        }
        else
        {
            LoginMenu.SetActive(true);
        }
        
    }

    public void ajustPositionLoginPanel()
    {
        if (LoginTggle.isOn)
        {
            RePasswordInput.gameObject.SetActive(false);
            LoginButton.gameObject.GetComponentsInChildren<Text>()[0].text = "Login";
        }
        else
        {
            RePasswordInput.gameObject.SetActive(true);
            LoginButton.gameObject.GetComponentsInChildren<Text>()[0].text = "Create";
        }
        Invoke("treatPositionLoginPanel", 0.05f);
    }

    void treatPositionLoginPanel()
    {
        LoginMenuPanel.GetComponent<RectTransform>().position = new Vector3(LoginMenuPanel.GetComponent<RectTransform>().position.x, Screen.height - LoginMenuPanel.GetComponent<RectTransform>().sizeDelta.y + 5, LoginMenuPanel.GetComponent<RectTransform>().position.z);

    }
    public void clearUserPassword()
    {
        UserIDInput.text = "";
        PasswordInput.text = "";
        RePasswordInput.text = "";
    }

    public void login_createUserExec()
    {
        if (LoginTggle.isOn)
        {
            ctr.Cmd_Login(UserIDInput.text, PasswordInput.text);
        }
        else
        {
            if (PasswordInput.text.Equals(RePasswordInput.text))
            {
                ctr.Cmd_CreateuUser(UserIDInput.text, PasswordInput.text);
            }
            else
            {
                ctr.dispInformationPanel_Timer("Password and Re-enter password were different.");
            }
        }
        LoginMenu.SetActive(false);
    }

}
