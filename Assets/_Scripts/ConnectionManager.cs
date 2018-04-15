using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * Based on script from lab 8
 */
public class ConnectionManager : Photon.MonoBehaviour
{
    public Text photonStatusText;
    public Text playerNameText;
    public Text photonCurrentRoomText;
    public Dropdown teamDropdown;
    public GameObject createRoomObject;
    public GameObject joinRoomObject;
    public GameObject startGameObject;

    public InputField photonRoomToJoinText;



    void Awake()
    {
        // Connect to the main photon server
        if (!PhotonNetwork.connectedAndReady) PhotonNetwork.ConnectUsingSettings("v1.0");

        // create and seta random  player name
        PhotonNetwork.playerName = "Player" + Random.Range(1000, 9999);
        playerNameText.text = "Player Name: " + PhotonNetwork.playerName;
        photonCurrentRoomText.text = "Room: (no room)";
    }

    void UpdateRoomInfo()
    {
        if (PhotonNetwork.room == null) // not in a room
        {
            photonCurrentRoomText.text = "Room: (No Room)";
        }
        else // in a room
        {
            photonCurrentRoomText.text = "Room: " + PhotonNetwork.room.Name + " (" + PhotonNetwork.room.PlayerCount + ")";
        }
    }

    // BUTTON HANDLERS

    public void ButtonHandlerCreateRoom()
    {
        if (PhotonNetwork.connectedAndReady && photonRoomToJoinText.text.Length > 0) // check there is a name entered before creating
        {
            PhotonNetwork.CreateRoom(photonRoomToJoinText.text);
        }
    }

    public void ButtonHandlerJoinRoom()
    {
        if (PhotonNetwork.connectedAndReady && photonRoomToJoinText.text.Length > 0) // check there is a name entered before joining
        {
            PhotonNetwork.JoinRoom(photonRoomToJoinText.text);
        }
    }

    public void ButtonHandlerLeaveRoom()
    {
        if (PhotonNetwork.connectedAndReady)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void ButtonHandlerStartGame()
    {
        photonView.RPC("StartGame", PhotonTargets.AllBufferedViaServer);
        // Players can't join in the middle of a game
        PhotonNetwork.room.IsOpen = false;
    }

    [PunRPC]
    void StartGame()
    {
        // Save the players chosen team and then load the game
        if (teamDropdown.value == 0)
        {
            Debug.Log("player prefs blue");
            PlayerPrefs.SetString("team", "Blue");
        }
        else
        {
            Debug.Log("player prefs red");
            PlayerPrefs.SetString("team", "Red");
        }
        SceneManager.LoadSceneAsync(1);
    }

    // EVENT CALLBACKS

    void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster!");
        photonStatusText.text = "Status: Connected";
        UpdateRoomInfo();
    }

    void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");
        photonStatusText.text = "Status: Connection Failed";
        UpdateRoomInfo();
    }

    void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom: " + PhotonNetwork.room.Name);
        UpdateRoomInfo();

    }

    void OnPhotonCreateRoomFailed()
    {
        Debug.Log("OnPhotonCreateRoomFailed");
        UpdateRoomInfo();
    }

    void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom: " + PhotonNetwork.room.Name);
        UpdateRoomInfo();
        createRoomObject.SetActive(false);
        joinRoomObject.SetActive(false);
        startGameObject.SetActive(true);
        // GameObject snowboarder = PhotonNetwork.Instantiate("NetworkedSnowboarder", new Vector3(123.3915f, 0.1103587f, 31.88877f), Quaternion.identity, 0);
    }

    void OnPhotonPlayerConnected()
    {
        Debug.Log("OnPhotonPlayerConnected");
        UpdateRoomInfo();
    }

    void OnPhotonPlayerDisconnected()
    {
        Debug.Log("OnPhotonPlayerDisconnected");
        UpdateRoomInfo();
    }

    void OnPhotonJoinRoomFailed()
    {
        Debug.Log("OnPhotonJoinRoomFailed");
        photonStatusText.text = "Status: Join Room Failed!";
        UpdateRoomInfo();
    }

    void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom");
        photonStatusText.text = "Status: Left Room!";
        UpdateRoomInfo();
    }
}