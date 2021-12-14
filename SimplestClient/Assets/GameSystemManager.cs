using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemManager : MonoBehaviour
{
    //timer
    private float ftime;
    public bool start;

    public const int IStartFirst = 0;
    public const int TheyStartFirst = 1;
    
    GameObject  inputFielddUserName,
        inputFieldPassword,
        buttonSubmit,
        toggleLogin,
        toggleCreate,
        chatInputEnterButton,
        resetButton,
        chatInput,
        chatwindow,
        observeGameRoomInputField,
        saveReplayButton,
        selectReplayDropDown,
        playReplayBUtton,
        inputFieldReplayNumber;

    GameObject networkedClient;

    GameObject findJoinGameSessionButton, backToMainMenuButton,enterObserverButton;

    public Text chatText;

    public string userName;

    public int gameRoomID;
    
    //TicTacToe
    GameObject ticTacToe;
    public int whoseTurn;//0 = player1 and 1= player2
    public int turnCount;//
    public GameObject[] turnIcons; //displays whos turn it is
    public Sprite[] playerIcons;// 0 = player1 icon(x) and 1 =player 2 icon(o)
    private int playerIconsdecider;
    public Button[] tictactoeSpace; //playable space for our game
    public int[] markedSpaces; //ID's which space was marked by which player
    public Text systemMessage;//Hold system msg
    public bool myTurn, opponentTurn;
    public int playerID;
    
    //REPLAY
    public static LinkedList<Replay> replays;
    public string tempReplay;
    private static uint lastUsedIndex;
    public const string ReplayMetaFile = "ReplayIndicesAndName.txt";

    // Start is called before the first frame update
    void Start()
    {
        tempReplay = "";
        GameSetup();
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
        foreach (GameObject go in allObjects)
        {
            if (go.name == "InputFieldUserName")
                inputFielddUserName = go;
            else if (go.name == "InputFieldPassword")
                inputFieldPassword = go;
            else if (go.name == "ButtonSubmit")
                buttonSubmit = go;
            else if (go.name == "ToggleLogin")
                toggleLogin = go;
            else if (go.name == "ToggleCreate")
                toggleCreate = go;
            else if (go.name == "NetworkedClient")
                networkedClient = go;
            else if (go.name == "FindJoinGameSessionButton")
                findJoinGameSessionButton = go;
            else if (go.name == "BackToMainMenuButton")
                backToMainMenuButton = go;
            else if (go.name == "ChatInputEnterButton")
                chatInputEnterButton = go;
            else if (go.name == "ResetButton")
                resetButton = go;
            else if (go.name == "EnterObserverButton")
                enterObserverButton = go;
            else if (go.name == "ChatInput")
                chatInput = go;
            else if (go.name == "Chatwindow")
                chatwindow = go;
            else if (go.name == "ChatText")
                chatText = go.GetComponent<Text>();
            else if (go.name == "TicTacToe")
                ticTacToe = go;
            else if (go.name == "ObserveGameRoomInputField")
                observeGameRoomInputField = go;
            else if (go.name == "SaveReplayButton")
                saveReplayButton = go;
            else if (go.name == "SelectReplayDropDown")
                selectReplayDropDown = (GameObject)go;
            else if (go.name == "PlayReplayBUtton")
                playReplayBUtton = go;
            if (go.name == "InputFieldReplayNumber")
                inputFieldReplayNumber = go;
            
            
            
        }
           
        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPress);
        toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLoginValueChanged);
        
        findJoinGameSessionButton.GetComponent<Button>().onClick.AddListener(findJoinGameSessionButtonPressed);
        backToMainMenuButton.GetComponent<Button>().onClick.AddListener(BacktoMainMenuButtonPressed);
        chatInputEnterButton.GetComponent<Button>().onClick.AddListener(ChatInputEnterButtonPressed);
        enterObserverButton.GetComponent<Button>().onClick.AddListener(enterObserverButtonButtonPressed);
        resetButton.GetComponent<Button>().onClick.AddListener(ClearBoard);
        saveReplayButton.GetComponent<Button>().onClick.AddListener(saveReplayButtonPressed); 
        selectReplayDropDown.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { DropDownChanged(); });
        playReplayBUtton.GetComponent<Button>().onClick.AddListener(PlayReplayButtonPressed); 
        ChangeGameState(GameStates.Login);
    }

   


    void GameSetup()
    {
        playerID = -1;
        
        turnCount = 0;
        if (whoseTurn == 0)
        {
            
            turnIcons[0].SetActive(true);
            turnIcons[1].SetActive(false);
        }
        else if (whoseTurn == 1)
        {
            turnIcons[0].SetActive(false);
            turnIcons[1].SetActive(true);
        }
        myTurn = false;
        for (int i = 0; i < tictactoeSpace.Length; i++)
        {
            tictactoeSpace[i].interactable = true;
            tictactoeSpace[i].GetComponent<Image>().sprite = null;
        }

        for (int i = 0; i <  markedSpaces.Length; i++)
        {
            markedSpaces[i] = -100;
        }
    }

    
    void Update()
    {
        WinnerCheck();
        DrawCheck();
    }
    //Replays
    private void DropDownChanged()
    {
        int menuIndex = selectReplayDropDown.GetComponent<Dropdown>().value;
        List<Dropdown.OptionData> menuOptions = selectReplayDropDown.GetComponent<Dropdown>().options;
        string value = menuOptions[menuIndex].text;
        ReplayDropDownChanged(value);
    }

    public void ReplayDropDownChanged(string value)
    {
        throw new NotImplementedException();
    }
    
    private void saveReplayButtonPressed()
    {
        if (tempReplay!="")
        {
            lastUsedIndex++;
            StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + lastUsedIndex + ".txt");
            sw.Write(tempReplay);
            sw.Close();
            tempReplay = "";
        }
        else
            Debug.Log("no play yet");
    }

    IEnumerator delayBetweenPlay(LinkedList<int> movePlayed)
    {
        int turn = 0;
        foreach (var play in movePlayed)
        {
            turn++;
            yield return new WaitForSeconds(0.5f);
            DrawButtonInReplay(play,turn%2);
        }
        
    }
    private void PlayReplayButtonPressed()
    {
        
        string input = inputFieldReplayNumber.GetComponent<InputField>().text;
        systemMessage.text = "game replay";
        string path = Application.dataPath + Path.DirectorySeparatorChar + input + ".txt";
        LinkedList<int> movePlayed = new LinkedList<int>();
        if (File.Exists(path))
        {
            string line = "";
            int turn = 0;
            StreamReader sr = new StreamReader(path);
            while ((line = sr.ReadLine()) != null)
            {

                string[] csv = line.Split(',');
                for (int i = 0; i < csv.Length - 1; i++)
                {
                    movePlayed.AddLast(int.Parse(csv[i]));
                }

            }

            ChangeGameState(GameStates.PlayingTicTacToe);
            myTurn = true;
            EnableGamePlay();
            float nextPlayTime = 0.0f;
            float period = 1.0f;

            StartCoroutine(delayBetweenPlay(movePlayed));

        }
        else
            systemMessage.text = "Replay you looking isn't valid.";
    }
    
    static public void SavePartyMetaData()
    {
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + ReplayMetaFile);


        sw.WriteLine("1," + lastUsedIndex);


        foreach (Replay pData in replays)
        {
            sw.WriteLine("2," + pData.index + "," + pData.name);
        }

        sw.Close();

    }

    //Chat room
    private void SubmitButtonPress()
    {
        string n = inputFielddUserName.GetComponent<InputField>().text;
        string p = inputFieldPassword.GetComponent<InputField>().text;

        if (toggleLogin.GetComponent<Toggle>().isOn)
        {
            //Debug.Log(ClientToServerSignifiers.Login + "," + n + "," + p);
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.Login + "," + n + "," + p);
        }
        else
        {               
            //Debug.Log(ClientToServerSignifiers.CreateAccount + "," + n + "," + p);
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.CreateAccount + "," + n + "," + p);
        }

    }

    private void ChatInputEnterButtonPressed()
    {
        string msg = chatInput.GetComponent<InputField>().text;
        if (msg != "")
        {
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",",ClientToServerSignifiers.PlayerMessage, userName, msg));
            chatText.text+= "\n" + userName + ": " + msg;
            chatInput.GetComponent<InputField>().text = "";
        }
    }

    //OB
    private void enterObserverButtonButtonPressed()
    {
        string input = observeGameRoomInputField.GetComponent<InputField>().text;
        if (input != "")
        {
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(string.Join(",", ClientToServerSignifiers.OBrequestSent,input));
            observeGameRoomInputField.GetComponent<InputField>().text = "";
        }
    }
    
    

    private void findJoinGameSessionButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.AddToGameSessionQueue + "");
        ChangeGameState(GameStates.WaitingForMatch);
    }
    
  
    
    private void ToggleCreateValueChanged(bool newValue)
    {
        toggleLogin.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }
    private void ToggleLoginValueChanged(bool newValue)
    {
        toggleCreate.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    public void AddOppositeMessageToChat(string msg)
    {
        chatText.text += msg;
    }

    private void BacktoMainMenuButtonPressed()
    {
        ClearBoard();
        ChangeGameState(GameStates.MainMenu);
        systemMessage.text = "";
    }
    
    //GameBoard
    public void TicTacToeButton(int WhichNumber)
    {
        if (myTurn)
        {
            DrawButton(WhichNumber, playerID);

            
            markedSpaces[WhichNumber] = playerID+1;
          

            myTurn = false;
            opponentTurn = true;

            if (whoseTurn == 0)
            {
                whoseTurn = 1;
                turnIcons[0].SetActive(false);
                turnIcons[1].SetActive(true);
            }
            else
            {
                whoseTurn = 0;
                turnIcons[0].SetActive(true);
                turnIcons[1].SetActive(false);
            }
            //send info to every play is made
            //csv[0]siginifier  csv[1]which button   csv[2]change my turn  csv[3]tell server it's opponent's turn
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TicTacToePlayMade + "," + WhichNumber + "," +  playerID + "," + opponentTurn);
        }
    }
    //this method set which player goes 1st
    public void SetWhichPlayerStart(bool myTurn)
    {
        if (myTurn == true)
        {
            this.myTurn = true;
            systemMessage.text = "You start 1st";
            EnableGamePlay();
        }
        else
        {
            this.myTurn = false;
            systemMessage.text = "Enemy starts 1st";
        }
    }

    public void WinnerCheck()
    {
        //3 horizontal lines
        int s1 = markedSpaces[0] + markedSpaces[1] + markedSpaces[2];
        int s2 = markedSpaces[3] + markedSpaces[4] + markedSpaces[5];
        int s3 = markedSpaces[6] + markedSpaces[7] + markedSpaces[8];
        //3 vertical lines
        int s4 = markedSpaces[0] + markedSpaces[3] + markedSpaces[6];
        int s5 = markedSpaces[1] + markedSpaces[4] + markedSpaces[7];
        int s6 = markedSpaces[2] + markedSpaces[5] + markedSpaces[8];
        //2 diagonal lines
        int s7 = markedSpaces[0] + markedSpaces[4] + markedSpaces[8];
        int s8 = markedSpaces[2] + markedSpaces[4] + markedSpaces[6];
        
        var solutions = new int[] { s1, s2, s3, s4, s5, s6, s7, s8 };
        for (int i = 0; i < solutions.Length; i++)
        {
            if (solutions[i] == 3*(playerID+1))
            { 
                WinnerDisplay();
               return;
            }
        }
    }

    void WinnerDisplay()
    {
        systemMessage.gameObject.SetActive(true);
        systemMessage.text = "You Win!";
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.WinMsg.ToString());
        DisableGamePlay();
    }

    public void DisableGamePlay()
    {
        for (int i = 0; i < tictactoeSpace.Length; i++)
        {
            tictactoeSpace[i].interactable = false;
        }
    }

    public void EnableGamePlay()
    {
        for (int i = 0; i < tictactoeSpace.Length; i++)
        {
            tictactoeSpace[i].interactable = true;
        }
    }

    public void DisplayMessage(string msg)
    {
        systemMessage.gameObject.SetActive(true);
        systemMessage.text = msg;
    }

    public void DrawButton(int buttonNumber, int buttonShape)
    {
        WinnerCheck();
        tictactoeSpace[buttonNumber].image.sprite = playerIcons[buttonShape];
        tictactoeSpace[buttonNumber].interactable = false;
        tempReplay += buttonNumber.ToString() + ",";
        turnCount++;
        
    }

    public void DrawButtonInReplay(int buttonNumber, int buttonShape)
    {
        tictactoeSpace[buttonNumber].image.sprite = playerIcons[buttonShape];
        tictactoeSpace[buttonNumber].interactable = false;
    }

    public void DrawCheck()
    {
        if (turnCount == 9)
        {
            systemMessage.gameObject.SetActive(true);
            systemMessage.text = "You Draw!";
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.GameDraw.ToString());
            DisableGamePlay();
        }
       
    }
    
    public void ClearBoard()
    {
        for (int i = 0; i < 9; i++)
        {
            
            tictactoeSpace[i].image.sprite = null;
            tictactoeSpace[i].interactable = true;
            markedSpaces[i] = -100;
        }

        turnCount = 0;
        //EnableGamePlay();
        tempReplay = "";
        systemMessage.text = " ClearBoard";
        SetWhichPlayerStart(myTurn);
    }
    
    public void ChangeGameState(int newState)
    {
        // very tranditional way to do gamestates 
        
        inputFielddUserName.SetActive(false);
        inputFieldPassword.SetActive(false);
        buttonSubmit.SetActive(false);
        toggleLogin.SetActive(false);
        toggleCreate.SetActive(false);
        findJoinGameSessionButton.SetActive(false);
        backToMainMenuButton.SetActive(false);
        //chatInputEnterButton.SetActive(false);
        chatInput.SetActive(false);
        chatwindow.SetActive(false);
        ticTacToe.SetActive(false);
        observeGameRoomInputField.SetActive(false);
        resetButton.SetActive(false);
        saveReplayButton.SetActive(false);
        playReplayBUtton.SetActive(false);
        selectReplayDropDown.SetActive(false);
        inputFieldReplayNumber.SetActive(false);

        if (newState == GameStates.Login)
        {
            inputFielddUserName.SetActive(true);
            inputFieldPassword.SetActive(true);
            buttonSubmit.SetActive(true);
            toggleLogin.SetActive(true);
            toggleCreate.SetActive(true);
        }
        else if (newState ==GameStates.MainMenu)
        {
            findJoinGameSessionButton.SetActive(true);
            observeGameRoomInputField.SetActive(true);
            playReplayBUtton.SetActive(true);
            inputFieldReplayNumber.SetActive(true);
            
        }
        else if (newState ==GameStates.WaitingForMatch)
        {
           
        }
        else if (newState ==GameStates.PlayingTicTacToe)
        {
            backToMainMenuButton.SetActive(true);
            //chatInputEnterButton.SetActive(true);
            chatInput.SetActive(true);
            chatwindow.SetActive(true);
            ticTacToe.SetActive(true);
            resetButton.SetActive(false);
            saveReplayButton.SetActive(true);


        }

    }
}

public static class GameStates
{
    public const int Login = 1;
    
    public const int MainMenu = 2;
    
    public const int WaitingForMatch = 3;
    
    public const int PlayingTicTacToe = 4;

}

public class Replay
{
    
    
    public uint index;
    public string name;

    public Replay(uint index)
    {
        this.index = index;
        name = index.ToString();
    }
    
    public void SaveReplay(string replay)
    {
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + index + ".txt");
        sw.Write(replay);
        sw.Close();
    }

    public void LoadReplay()
    {
        string path = Application.dataPath + Path.DirectorySeparatorChar + index + ".txt";

        if (File.Exists(path))
        {
            string line = "";
            StreamReader sr = new StreamReader(path);

            while ((line = sr.ReadLine()) != null)
            {
                string[] csv = line.Split(',');
                
            }
        }
        
    }
}





