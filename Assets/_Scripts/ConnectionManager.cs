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
    public GameObject mainCamera;
    public GameObject mainCanvas;
    public GameObject connectionCanvas;
    public InputField photonRoomToJoinText;

    private float timer = 0; // The total game time in seconds

    void Awake()
    {
        // Connect to the main photon server
        if (!PhotonNetwork.connectedAndReady) PhotonNetwork.ConnectUsingSettings("v1.0.0");

        // create and seta random  player name
        PhotonNetwork.playerName = "Player" + Random.Range(1000, 9999);
        playerNameText.text = "Player Name: " + PhotonNetwork.playerName;
        photonCurrentRoomText.text = "Room: (No Room)";
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
        
        // Store the start time in the server so that players will not have to rely on the master client for a working timer
        // See: https://answers.unity.com/questions/1147387/photon-multiplayer-countdown-timer.html?childToView=1399777#comment-1399777
        int startTime = PhotonNetwork.ServerTimestamp;
        PhotonNetwork.room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "startTime", startTime } });
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
        connectionCanvas.SetActive(false);
        mainCanvas.SetActive(true);
        Destroy(mainCamera);
        int startTime = (int)PhotonNetwork.room.CustomProperties["startTime"];
        GameManager.Instance.SetStartTime(startTime);
        GameManager.Instance.InstantiatePlayer(teamDropdown.value == 0 ? "Blue" : "Red");
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

    // This will only be called by the master client and will set game times for other clients
    void IncrementTimer()
    {
        timer++;
        GameManager.Instance.photonView.RPC("FormatTime", PhotonTargets.All, new object[] { timer });
    }
}