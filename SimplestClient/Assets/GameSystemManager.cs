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
        }
           
        buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPress);
        toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
        toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLoginValueChanged);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SubmitButtonPress()
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
    public void ToggleCreateValueChanged(bool newValue)
    {
        //Debug.Log("We Create!");
        toggleLogin.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }
    public void ToggleLoginValueChanged(bool newValue)
    {
        //Debug.Log("We Login!");
        toggleCreate.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }
}


public static class ClientToServerSignifiers
{
    public const int Login = 1;
    public const int CreateAccount = 2;
}

public static class ServerToClientSignifiers    
{
    public const int LoginResponse = 1;
}

public static class LoginResponses
{
    public const int Success = 1;

    public const int FailureNameInUse = 2;
    
    public const int FailureNameInNotFound = 3;

    public const int FailureIncorrectPassword = 4;


}