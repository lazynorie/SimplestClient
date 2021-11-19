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
    public int whoTurn;//0 = player1 and 1= player2
    public int turnCount;//
    public GameObject[] turnIcons; //displays whos turn it is
    public Sprite[] playerIcons;// 0 = player1 icon and 1 =player 2 icon
    public Button[] tictactoeSpace; //playable space for our game
    
    
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
        whoTurn = 0;
        turnCount = 0;
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);
        for (int i = 0; i < tictactoeSpace.Length; i++)
        {
            tictactoeSpace[i].interactable = true;
            tictactoeSpace[i].GetComponent<Image>().sprite = null;
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