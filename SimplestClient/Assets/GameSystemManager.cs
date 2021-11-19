using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemManager : MonoBehaviour
{
    GameObject  inputFielddUserName,
        inputFieldPassword,
        buttonSubmit,
        toggleLogin,
        toggleCreate,
        chatInputEnterButton,
        chatInput,
        chatwindow;

    GameObject networkedClient;

    GameObject findJoinGameSessionButton, placeHolderGameButton;

    public Text chatText;

    public string userName;
    
    //TicTacToe
    GameObject ticTacToe;
    public int whoseTurn;//0 = player1 and 1= player2
    public int turnCount;//
    public GameObject[] turnIcons; //displays whos turn it is
    public Sprite[] playerIcons;// 0 = player1 icon(x) and 1 =player 2 icon(o)
    public Button[] tictactoeSpace; //playable space for our game
    public int[] markedSpaces; //ID's which space was marked by which player
    public Text systemMessage;//Hold system msg
    public bool myTurn, opponentTurn;
    
    // Start is called before the first frame update
    void Start()
    {
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
            else if (go.name == "PlaceHolderGameButton")
                placeHolderGameButton = go;
            else if (go.name == "ChatInputEnterButton")
                chatInputEnterButton = go;
            else if (go.name == "ChatInput")
                chatInput = go;
            else if (go.name == "Chatwindow")
                chatwindow = go;
            else if (go.name == "ChatText")
                chatText = go.GetComponent<Text>();
            
            else if (go.name == "TicTacToe")
                ticTacToe = go;
           
            
        }
           
        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPress);
        toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLoginValueChanged);
        
        findJoinGameSessionButton.GetComponent<Button>().onClick.AddListener(findJoinGameSessionButtonPressed);
        placeHolderGameButton.GetComponent<Button>().onClick.AddListener(placeHolderGameButtonPressed);
        chatInputEnterButton.GetComponent<Button>().onClick.AddListener(ChatInputEnterButtonPressed);

        ChangeGameState(GameStates.Login);
    }

    void GameSetup()
    {
        whoseTurn = 0;
        turnCount = 0;
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);
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

    // Update is called once per frame
    void Update()
    {
      
    }

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
    
    

    private void findJoinGameSessionButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.AddToGameSessionQueue + "");
        ChangeGameState(GameStates.WaitingForMatch);
    }
    
    private void placeHolderGameButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TicTacToePlay + "");
    }
    private void ToggleCreateValueChanged(bool newValue)
    {
        //Debug.Log("We Create!");
        toggleLogin.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }
    private void ToggleLoginValueChanged(bool newValue)
    {
        //Debug.Log("We Login!");
        toggleCreate.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    public void AddOppositeMessageToChat(string msg)
    {
        chatText.text += msg;
    }

    public void TicTacToeButton(int WhichNumber)
    {
        tictactoeSpace[WhichNumber].image.sprite = playerIcons[whoseTurn];
        tictactoeSpace[WhichNumber].interactable = false;

        markedSpaces[WhichNumber] = whoseTurn+1;
        turnCount++;
        if (turnCount>4)
        {
            WinnerCheck();
        }
        

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
    }

    void WinnerCheck()
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
            if (solutions[i] == 3*(whoseTurn+1))
            {
                WinnerDisplay(i);
                return;
            }
        }
    }

    void WinnerDisplay(int indexIn)
    {
        systemMessage.gameObject.SetActive(true);
        if (whoseTurn == 0)
        {
            systemMessage.text = "Player X wins!";
        }
        else if (whoseTurn ==1)
        {
            systemMessage.text = "Player O wins!";
        }

        for (int i = 0; i < tictactoeSpace.Length; i++)
        {
            tictactoeSpace[i].interactable = false;
        }
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
        placeHolderGameButton.SetActive(false);
        //chatInputEnterButton.SetActive(false);
        chatInput.SetActive(false);
        chatwindow.SetActive(false);
        ticTacToe.SetActive(false);

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
            
        }
        else if (newState ==GameStates.WaitingForMatch)
        {
           
        }
        else if (newState ==GameStates.PlayingTicTacToe)
        {
            placeHolderGameButton.SetActive(true);
            //chatInputEnterButton.SetActive(true);
            chatInput.SetActive(true);
            chatwindow.SetActive(true);
            ticTacToe.SetActive(true);

        }

    }
}

public static class GameStates
{
    public const int Login = 1;
    
    public const int MainMenu = 2;
    
    public const int WaitingForMatch = 3;
    
    public const int PlayingTicTacToe = 4;
    //public const int Login = 1;

}