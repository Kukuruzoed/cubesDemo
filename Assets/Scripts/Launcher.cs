using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Chat;

namespace Com.Alex.MyGame
{
    public class Launcher : MonoBehaviourPunCallbacks, IOnEventCallback {

        public const byte PAINT_ALL_IN_RED = 1;
        public const byte CALL_RAISE_EVENT = 2;
        
        string gameVersion = "1";
        public GameObject cubePrefab;
        [SerializeField]
        public GameObject nicknamePanel; 
        [SerializeField]
        public GameObject raiseEventPanel; 
        [SerializeField]
        private GameObject chatManager;
        [SerializeField]
        private GameObject chatPanel;
        [SerializeField]
        private Text nickname;
        [SerializeField]
        private Button connectButton;
        [SerializeField]
        private Text connectionUIInfo;
        [SerializeField]
        private GameObject[] cubes;

        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        void Awake() {
            PhotonNetwork.AutomaticallySyncScene = true;
            StartCoroutine(PaintAllInRedEventCoroutine());
        }

        private void OnEnable() {
            PhotonNetwork.AddCallbackTarget(this);
            SetConnectButtonState(false);
        }

        private void OnDisable() {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private IEnumerator Delay(int seconds) {
            yield return new WaitForSeconds(seconds);
            raiseEventPanel.SetActive(false);
        }
        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            switch (eventCode)
            {
                case PAINT_ALL_IN_RED:
                    if (cubes.Length == 0)
                    {
                        cubes = GameObject.FindGameObjectsWithTag("Cube");
                    }
                    foreach (GameObject cube in cubes)
                    {
                        cube.GetComponent<Renderer>().material.color = Color.red;
                    }
                    break;
                case CALL_RAISE_EVENT:
                    if((int)photonEvent.CustomData != PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        raiseEventPanel.SetActive(true);
                        StartCoroutine(Delay(5));
                    }
                    break;
            }
        }

        private void FixedUpdate() {
            connectionUIInfo.GetComponent<Text>().text = PhotonNetwork.IsConnected ? "Подключено\nПинг - " + PhotonNetwork.GetPing() + "\n" + 
                (PhotonNetwork.InRoom ? "В комнате " + PhotonNetwork.CurrentRoom.Name + "\n" + (PhotonNetwork.IsMasterClient ? "Хозяин" : "Гость") : "Не в комнате\n") : "Отключено"; 
        }

        public void SetConnectButtonState(bool isConnected) {
            connectButton.onClick.RemoveAllListeners();
            if (isConnected) {
                connectButton.GetComponentInChildren<Text>().text = "Отключиться";
                connectButton.onClick.AddListener(Disconnect);
            } 
            else {
                connectButton.GetComponentInChildren<Text>().text = "Подключиться";
                connectButton.onClick.AddListener(Connect);
            }
        }

        public void Connect() {
            if (PhotonNetwork.IsConnected) {
                PhotonNetwork.JoinRandomRoom();
            }
            else {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        public void Disconnect() {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
            }
        }

        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            foreach (GameObject cube in cubes) {
                Color c = cube.GetComponent<Renderer>().material.color;
                Vector3 color = new Vector3(c.r * 255f, c.g * 255f, c.b * 255f);
                cube.GetComponent<PhotonView>().RPC("setColor", RpcTarget.All, color);
            }
        }

        public override void OnConnectedToMaster() {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }


        public override void OnDisconnected(DisconnectCause cause) {
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
            SetConnectButtonState(false);
        }

        public override void OnJoinRandomFailed(short returnCode, string message) {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom() {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            SetConnectButtonState(true);
            chatPanel.SetActive(true);
            if (this.cubePrefab != null && PhotonNetwork.IsMasterClient)
            {
                cubes = new GameObject[3];
                cubes[0] = PhotonNetwork.Instantiate(this.cubePrefab.name, new Vector3(-5f, 2f, 0f), Quaternion.identity, 0);
                cubes[1] = PhotonNetwork.Instantiate(this.cubePrefab.name, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
                cubes[2] = PhotonNetwork.Instantiate(this.cubePrefab.name, new Vector3(5f, 2f, 0f), Quaternion.identity, 0);
                cubes[1].transform.localScale = new Vector3(2, 2, 2);
                cubes[2].transform.localScale = new Vector3(3, 3, 3);
            }
        }

        #endregion

        private IEnumerator PaintAllInRedEventCoroutine() {
            while (true) {
                yield return new WaitForSeconds(10);
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
                PhotonNetwork.RaiseEvent(PAINT_ALL_IN_RED, null, raiseEventOptions, SendOptions.SendReliable);
            }
        }

        public void SetNickname() {
            chatManager.GetComponent<ChatManager>().SetUsedID(nickname.GetComponent<Text>().text);
            nicknamePanel.SetActive(false);
        }

        public void CallRaiseEvent() {
            PhotonNetwork.RaiseEvent(CALL_RAISE_EVENT, PhotonNetwork.LocalPlayer.ActorNumber, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }
    }
}
