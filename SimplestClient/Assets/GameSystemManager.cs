using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{
    
    
    GameObject  inputFielddUserName,
        inputFieldPassword,
        buttonSubmit,
        toggleLogin,
        toggleCreate,
        gameStatusText;

    GameObject networkedClient;

    GameObject findJoinGameSessionButton, placeHolderGameButton;

    public int gameSessionID;

    GameObject gameBoard;
    public Button[] gamecellArray;
    string playersTicTacToeSymbol,opponentsTicTacToeSymbol, currentReplaySymbol;
    public TextMeshProUGUI chatRoomText;
    
    public string userName;
    
    int numberOfTotalMovesMade = 0;
    float playerTurnCounter = 0.0f, opponentTurnCounter = 0.0f, replayRecordingCounter = 0.0f, errorMessageTimer = 2.5f, errorMessageCounter = 0.0f;
    public bool myTurnToMove = false, isWatchingReplay, recordingIsPaused = false, displayErrorScreenMessage = false, opponentsTurn = false, gameStarted = false;
    // Start is called before the first frame update
    void Start()
    {
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
            
            else if (go.name == "GameBoard")
                gameBoard = go;
            
            else if (go.name == "ChatRoomText")
                chatRoomText = go.GetComponent<TextMeshProUGUI>();
           
        }
           
        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPress);
        toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLoginValueChanged);
        gamecellArray = gameBoard.GetComponentsInChildren<Button>();
        
        
        findJoinGameSessionButton.GetComponent<Button>().onClick.AddListener(findJoinGameSessionButtonPressed);
        placeHolderGameButton.GetComponent<Button>().onClick.AddListener(placeHolderGameButtonPressed);

        ChangeGameState(GameStates.Login);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeGameState(GameStates.Login);
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeGameState(GameStates.MainMenu);
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            ChangeGameState(GameStates.WaitingForMatch);
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            ChangeGameState(GameStates.PlayingTicTacToe);
        }*/
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
    
    private void AddListenersToButtonCellArray()
    {
        foreach (Button button in gamecellArray)
        {
            button.onClick.AddListener(ButtonCellPressed);
        }
    }
    private void ButtonCellPressed()
    {
        Button button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        for (int i = 0; i < gamecellArray.Length; i++)
        {
            if (button == gamecellArray[i] && buttonText.text == "" && myTurnToMove == true)
            {
                opponentsTurn = true;
                numberOfTotalMovesMade++;
                Debug.Log("Number of moves made: " + numberOfTotalMovesMade);
                myTurnToMove = false;
                UpdatePlayersCurrentTurnText(myTurnToMove);
                buttonText.text = playersTicTacToeSymbol;
                networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TicTacToeMoveMade + "," + i + "," + playersTicTacToeSymbol);
                if (CheckIfGameOver())
                {
                    Debug.Log("Printing Symbols");
                    for(int j = 0; j < 7; j +=3)
                    {
                        Debug.Log(gamecellArray[j].GetComponentInChildren<TextMeshProUGUI>().text + "," + gamecellArray[j+1].GetComponentInChildren<TextMeshProUGUI>().text + "," + gamecellArray[j + 2].GetComponentInChildren<TextMeshProUGUI>().text);
                        
                    }
                }
                return;
            }
        }   
    }
    
    public bool CheckIfGameOver()
    {
        //Earliest a game can be over is 5 moves so only start checking after the 5th move
        if (numberOfTotalMovesMade >= 5)
        {
            if (CheckIfGameWon())
            {
                gameStatusText.GetComponent<TextMeshProUGUI>().text = userName + " Won!";
                networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.GameOver.ToString() + "," + userName);
                GameOver();
                return true;
            }
            else if (numberOfTotalMovesMade == 9)
            {
                gameStatusText.GetComponent<TextMeshProUGUI>().text = "Game Drawn";
                networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.GameDrawn.ToString());
                GameOver();
                return true;
            }
        }
        return false;
    }

    private void GameOver()
    {
        throw new System.NotImplementedException();
    }

    bool CheckIfGameWon()
    {
        //Checks for rows having same symbol
        for (int i = 0; i < 7; i+=3)
        {
            string leftCell = gamecellArray[i].GetComponentInChildren<TextMeshProUGUI>().text;
            string middleCell = gamecellArray[i + 1].GetComponentInChildren<TextMeshProUGUI>().text;
            string rightCell = gamecellArray[i + 2].GetComponentInChildren<TextMeshProUGUI>().text;

            if (leftCell != "" && leftCell == middleCell && leftCell == rightCell)
                return true;
        }
        //Checks for columns having same symbol
        for (int i = 0; i < 3; i++)
        {
            string topCell = gamecellArray[i].GetComponentInChildren<TextMeshProUGUI>().text;
            string middleCell = gamecellArray[i + 3].GetComponentInChildren<TextMeshProUGUI>().text;
            string bottomCell = gamecellArray[i + 6].GetComponentInChildren<TextMeshProUGUI>().text;

            if (topCell != "" && topCell == middleCell && topCell == bottomCell)
                return true;
        }
        //Checks for diagonals
        string topLeftCorner = gamecellArray[0].GetComponentInChildren<TextMeshProUGUI>().text;
        string middleGridCell = gamecellArray[4].GetComponentInChildren<TextMeshProUGUI>().text;
        string topRightCorner = gamecellArray[2].GetComponentInChildren<TextMeshProUGUI>().text;

        if (topLeftCorner != "" && topLeftCorner == middleGridCell & topLeftCorner == gamecellArray[8].GetComponentInChildren<TextMeshProUGUI>().text)
            return true;
        if (topRightCorner != "" && topRightCorner == middleGridCell && topRightCorner == gamecellArray[6].GetComponentInChildren<TextMeshProUGUI>().text)
            return true;

        return false;
    }
    
    
    public void InitGameSymbolsSetCurrentTurn(string playerSymbol, string opponentSymbol, bool myTurn)
    {
        playersTicTacToeSymbol = playerSymbol;
        opponentsTicTacToeSymbol = opponentSymbol;
        myTurnToMove = myTurn;
        UpdatePlayersCurrentTurnText(myTurnToMove);
        gameStarted = true;
        if (myTurn == true)
        {
            opponentsTurn = false;
        }
        else
        {
            opponentsTurn = true;
        }

    }
    
    
    
    public void UpdatePlayersCurrentTurnText(bool myTurn)
    {
        gameStatusText.GetComponent<TextMeshProUGUI>().text = (myTurn == true) ? "Your Move" : "Opponents Move";
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
        gameBoard.SetActive(false);
        //gameStatusText.SetActive(false);
        

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
            numberOfTotalMovesMade = 0;
            gameBoard.SetActive(true);
            //gameStatusText.SetActive(true);

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