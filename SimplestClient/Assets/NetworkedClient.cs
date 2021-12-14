using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{

    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    byte error;
    bool isConnected = false;
    int ourClientID;

    private GameObject gameSystemManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
        foreach (GameObject go in allObjects)
        {
            if (go.name == "GameManager")
                gameSystemManager = go;
        }
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.S))
            SendMessageToHost("Hello from client");
            */

        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }
    
    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "192.168.0.11", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }
    
    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }
    
    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);
        if (signifier == ServerToClientSignifiers.LoginResponse)
        {
            int loginResultSignifier = int.Parse(csv[1]);
            
            if (loginResultSignifier == LoginResponses.Success)
            {
                gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameStates.MainMenu);
                gameSystemManager.GetComponent<GameSystemManager>().userName = csv[2];
            }
            // on success load
            // ChangeGameState(GameStates.MainMenu);
        }
        
        else if (signifier == ServerToClientSignifiers.GameSessionStarted)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameStates.PlayingTicTacToe);
            
            //decide who goes first
            bool myTurn = (int.Parse(csv[1]) == 1) ? true : false;
            gameSystemManager.GetComponent<GameSystemManager>().SetWhichPlayerStart(myTurn);
            if (myTurn == true)
            {
                gameSystemManager.GetComponent<GameSystemManager>().playerID = 0;
            }
            else if (myTurn == false)
            {
                gameSystemManager.GetComponent<GameSystemManager>().playerID = 1;
            }
            gameSystemManager.GetComponent<GameSystemManager>().gameRoomID = int.Parse(csv[2]);
        }
        else if (signifier == ServerToClientSignifiers.OpponentTicTacToePlay)
        {
            Debug.Log("Your foe does!!!!!!");
            int button = int.Parse(csv[1]);
            int shape =  int.Parse(csv[2]);
            
            //update on opponent play on client
            gameSystemManager.GetComponent<GameSystemManager>().DrawButton(button,shape);
            gameSystemManager.GetComponent<GameSystemManager>().myTurn = bool.Parse(csv[3]);
            gameSystemManager.GetComponent<GameSystemManager>().WinnerCheck();
            
        }
        else if (signifier == ServerToClientSignifiers.UpDateOB)
        {
            int button = int.Parse(csv[1]);
            int shape =  int.Parse(csv[2]);
            
            //update on opponent play on client
            gameSystemManager.GetComponent<GameSystemManager>().DrawButton(button,shape);
            //gameSystemManager.GetComponent<GameSystemManager>().myTurn = bool.Parse(csv[3]);
            gameSystemManager.GetComponent<GameSystemManager>().WinnerCheck();
        }
        else if (signifier == ServerToClientSignifiers.SendChatToOpponent)
        {
            string _msg = "\n" + csv[1] + ": " + csv[2];
            gameSystemManager.GetComponent<GameSystemManager>().AddOppositeMessageToChat(_msg);
        }
        else if (signifier == ServerToClientSignifiers.GGMsg)
        {
            gameSystemManager.GetComponent<GameSystemManager>().systemMessage.text = "You Lose!";
            gameSystemManager.GetComponent<GameSystemManager>().DisableGamePlay();
        } 
        else if (signifier == ServerToClientSignifiers.DrawMsg)
        {
            gameSystemManager.GetComponent<GameSystemManager>().systemMessage.text = "You Draw!";
            gameSystemManager.GetComponent<GameSystemManager>().DisableGamePlay();
        }
        
        else if (signifier ==ServerToClientSignifiers.OBrequestRecieved)
        {
            //string OBI = csv[1];
            string currentboardresult;
            gameSystemManager.GetComponent<GameSystemManager>().ChangeGameState(GameStates.PlayingTicTacToe);
        }
        else if (signifier == ServerToClientSignifiers.UpdateCurrentBoardToOB)
        {
            string obID = csv[1];
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }
}


public static class ClientToServerSignifiers
{
    public const int Login = 1;
    public const int CreateAccount = 2;
    public const int AddToGameSessionQueue = 3;
    public const int TicTacToePlay = 4;
    public const int PlayerMessage = 5;
    public const int TicTacToePlayMade = 6;
    public const int WinMsg = 7;
    public const int OBrequestSent = 8;
    public const int GameDraw = 9;



}

public static class ServerToClientSignifiers    
{
    public const int LoginResponse = 1;
    public const int GameSessionStarted = 2;
    public const int OpponentTicTacToePlay = 3;
    public const int SendChatToOpponent = 4;
    public const int PlayerDC = 5;
    public const int GGMsg = 6;
    public const int OBrequestRecieved = 8;
    public const int UpdateCurrentBoardToOB = 9;
    public const int UpDateOB = 10;
    public const int DrawMsg = 11;

}

public static class LoginResponses
{
    public const int Success = 1;

    public const int FailureNameInUse = 2;
    
    public const int FailureNameInNotFound = 3;

    public const int FailureIncorrectPassword = 4;
    
}

