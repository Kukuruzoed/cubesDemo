using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Chat.Demo;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    ChatClient chatClient;
    [SerializeField]
    private GameObject chatContent;
    [SerializeField]
    public GameObject nicknamePanel;
    [SerializeField]
    private Text message;
    public string userID = "";

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log(state);
    }

    public void OnConnected()
    {
        Debug.Log("Connected!");
        chatClient.Subscribe(new string[] { "publicChannel" });
    }

    public void OnDisconnected()
    {
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName.Equals("publicChannel"))
        {
            string text = chatContent.GetComponent<Text>().text;
            foreach (object message in messages) 
            {
                chatContent.GetComponent<Text>().text = text + "\n" + userID + ": " + message;
            }
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed to a new channel!");
    }

    public void OnUnsubscribed(string[] channels)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
    }

    public void SetUsedID(string _userID) 
    {
        userID = _userID;
        chatClient = new ChatClient(this);
        chatClient.ChatRegion = "ru";
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, "1.0", new AuthenticationValues(_userID));
        this.chatClient.AuthValues = new AuthenticationValues(userID);
        this.chatClient.ConnectUsingSettings(PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings());
    }

    // Update is called once per frame
    void Update()
    {
        if(chatClient != null)
        {
            chatClient.Service();
        }
    }

    public void SendMessageFromInput() {
        if (userID.Equals(""))
        {
            nicknamePanel.SetActive(true);
            return;
        }
        chatClient.PublishMessage("publicChannel", message.GetComponent<Text>().text);
        message.GetComponent<Text>().text = "";
    }
}
