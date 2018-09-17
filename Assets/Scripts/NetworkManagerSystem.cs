using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerSystem : MonoBehaviour {


    [SerializeField]
    private InputField serverPortInput;
    [SerializeField]
    private InputField ClientIPInput;
    [SerializeField]
    private InputField clientPortInput;

    [SerializeField]
    private InputField communicationLogField;

    [SerializeField]
    private NetworkManagerHUD hud;
    
    // Use this for initialization
    void Start()
    {

    }
    //// ボタン等の主なUIを含むGameObject
    //GameObject m_MainUIs;

    //// 接続中であることを示すUIのGameObject
    //GameObject m_ConnectingText;

    //// 接続状態種別
    enum ConnectionState
    {
        // 純粋なサーバーとして起動中
        Server,
        // ホスト（サーバー兼クライアント）として起動中
        Host,
        // リモートクライアントとして接続確立済み
        RemoteClientConnected,
        // リモートクライアントとして接続試行中
        RemoteClientConnecting,
        // 接続なし
        Nothing,
    }

    enum ConnectionMode
    {
        None,
        Server,
        Client
    }
        


    // ネットワーク接続の状態を取得する
    ConnectionState GetConnectionState()
    {
        // サーバーが起動しているか？
        if (NetworkServer.active)
        {
            // クライアントとして接続しているか？
            if (NetworkManager.singleton.IsClientConnected())
            {
                // ホストとして起動中
                return ConnectionState.Host;
            }
            else
            {
                // サーバーとして起動中
                return ConnectionState.Server;
            }
        }
        // クライアントとして接続しているか？
        else if (NetworkManager.singleton.IsClientConnected())
        {
            // リモートクライアントとして接続確立済み
            return ConnectionState.RemoteClientConnected;
        }
        else
        {
            NetworkClient client = NetworkManager.singleton.client;

            // Connectionが存在するか？
            if (client != null && client.connection != null && client.connection.connectionId != -1)
            {
                // 接続試行中
                return ConnectionState.RemoteClientConnecting;
            }
            else
            {
                // 接続なし（何もしていない）
                return ConnectionState.Nothing;
            }
        }
    }

    //void Start()
    //{
    //    m_MainUIs = GameObject.Find("MainUIs");
    //    m_ConnectingText = GameObject.Find("ConnectingText");
    //}

    void Update()
    {
        ConnectionState state = GetConnectionState();

        // 接続試行中
        if (state == ConnectionState.RemoteClientConnecting)
        {

            setEnableUI(false);
            
            // Escapeキーで接続中止
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                NetworkManager.singleton.StopHost();
            }
        }
        else if((connectMode == ConnectionMode.Client)&&(state == ConnectionState.RemoteClientConnected))
        {
            communicationLogField.text = "Connection is Succeeded...";
            connectMode = ConnectionMode.None;


            ClientScene.Ready(NetworkManager.singleton.client.connection);
            if (ClientScene.localPlayers.Count == 0)
            {
                ClientScene.AddPlayer(0);
            }
        }
        else if ((connectMode == ConnectionMode.Server) && (state == ConnectionState.Host))
        {
            communicationLogField.text = "Server is wakeuped...";
            connectMode = ConnectionMode.None;


            ClientScene.Ready(NetworkManager.singleton.client.connection);
          //if (ClientScene.localPlayers.Count == 0)
            //{
                ClientScene.AddPlayer(0);
            //}
        }
        else
        {
            if(connectMode == ConnectionMode.Client)
            {
                communicationLogField.text = "Connection Time out...";
                connectMode = ConnectionMode.None;

               
            }
            else if (connectMode == ConnectionMode.Server)
            {
                communicationLogField.text = "Port number is already used...";
                connectMode = ConnectionMode.None;
            }
            else
            {
                setEnableUI(true);
            }
        }
    }
    void setEnableUI(bool flag)
    {
        serverPortInput.interactable = flag;
        clientPortInput.interactable = flag;
        ClientIPInput.interactable = flag;

    }

    ConnectionMode connectMode = ConnectionMode.None;
    // 「サーバーとして起動」ボタンが押された時の処理
    public void WakeUpServer()
    {
        NetworkManager.singleton.networkPort = System.Int32.Parse(serverPortInput.text);
        NetworkManager.singleton.StartServer();
        communicationLogField.text = "Wakeup server...";
        connectMode = ConnectionMode.Server;
    }

    //// 「ホストとして起動」ボタンが押された時の処理
    //public void OnHostButtonClicked()
    //{
    //    NetworkManager.singleton.StartHost();
    //}

    // 「サーバーへ接続(クライアント)」ボタンが押された時の処理
    public void ConnectToServer()
    {
        //       InputField input = GameObject.Find("ServerAddressInputField").GetComponent<InputField>();
        NetworkManager.singleton.networkPort = System.Int32.Parse(clientPortInput.text);
        NetworkManager.singleton.networkAddress = ClientIPInput.text;
        NetworkManager.singleton.StartClient();
        communicationLogField.text = "Connect to server...";
        connectMode = ConnectionMode.Client;
    }
}

