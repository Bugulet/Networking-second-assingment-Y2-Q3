using shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ChatLobbyClient : MonoBehaviour
{
    private AvatarAreaManager _avatarAreaManager;
    private PanelWrapper _panelWrapper;
    

    [SerializeField] private string _server = "localhost";
    [SerializeField] private int _port = 55555;

    int avatarID = (int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private TcpClient _client;

    private void Start()
    {
        connectToServer();

        _avatarAreaManager = FindObjectOfType<AvatarAreaManager>();
        _avatarAreaManager.OnAvatarAreaClicked += onAvatarAreaClicked;

        _panelWrapper = FindObjectOfType<PanelWrapper>();
        _panelWrapper.OnChatTextEntered += onChatTextEntered;

        

    }

    private void connectToServer()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(_server, _port);

            AddClient addRequest = new AddClient();

            

            addRequest.SetParameters(avatarID, 1);

            sendObject(addRequest);

            Debug.Log("Connected to server.");

            
        }
        catch (Exception e)
        {
            Debug.Log("Could not connect to server:");
            Debug.Log(e.Message);
        }
    }

    private void onAvatarAreaClicked(Vector3 pClickPosition)
    {
        Debug.Log("ChatLobbyClient: you clicked on " + pClickPosition);
    }

    private void onChatTextEntered(string pText)
    {
        _panelWrapper.ClearInput();
        

        //send the command
        SimpleMessage textRequest = new SimpleMessage();
        textRequest.text=pText;
        sendObject(textRequest);

        
    }

    private void sendObject(ISerializable pOutObject)
    {
        try
        {
            Debug.Log("Sending:" + pOutObject);

            Packet outPacket = new Packet();
            outPacket.Write(pOutObject);

            StreamUtil.Write(_client.GetStream(), outPacket.GetBytes());
        }

        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();

            for (int i = 0; i < _avatarAreaManager.GetAllAvatarIds().Count; i++)
            {
                _avatarAreaManager.RemoveAvatarView(i);
            }
            
            connectToServer();
        }
    }

    // RECEIVING CODE

    private void Update()
    {
        try
        {
            if (_client.Available > 0)
            {
                byte[] inBytes = StreamUtil.Read(_client.GetStream());
                Packet inPacket = new Packet(inBytes);
                ISerializable inObject = inPacket.ReadObject();

                if (inObject is SetClientParameters) { setLocalClient(inObject as SetClientParameters); }
                else if (inObject is SimpleMessage) { showMessage(inObject as SimpleMessage); }
            }
        }
        catch (Exception e)
        {
            //for quicker testing, we reconnect if something goes wrong.
            Debug.Log(e.Message);
            _client.Close();
            connectToServer();
        }
    }


    private void setLocalClient(SetClientParameters clientParameters)
    {
        //adds an avatar when you start
        {
            print(clientParameters.skin);
            AvatarView avatar = _avatarAreaManager.AddAvatarView(clientParameters.id);
            avatar.transform.localPosition = getRandomPosition();
            avatar.SetSkin(clientParameters.skin);
            print("added avatar");
        }
    }


    private void showMessage(SimpleMessage messageObject)
    {
        List<int> allAvatarIds = _avatarAreaManager.GetAllAvatarIds();

        print(messageObject.id+"id");
        if (allAvatarIds.Count == 0)
        {
            Debug.Log("No avatars available to show text through:" + messageObject.text);
            return;
        }
        
        AvatarView avatarView = _avatarAreaManager.GetAvatarView(messageObject.id);
        avatarView.Say(messageObject.text);
    }


    private Vector3 getRandomPosition()
    {
        //set a random position
        float randomAngle =UnityEngine.Random.Range(0, 180) * Mathf.Deg2Rad;
        float randomDistance =UnityEngine. Random.Range(0, 18);
        return new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)) * randomDistance;
    }
}
