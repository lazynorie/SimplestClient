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
        toggleCreate;

    GameObject networkedClient;

    GameObject findJoinGameSessionButton, placeHolderGameButton;
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
           
        }
           
        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPress);
        toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLoginValueChanged);
        
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