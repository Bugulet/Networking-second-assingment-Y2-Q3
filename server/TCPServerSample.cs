using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;

/**
 * This class implements a simple tcp highscore server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServerSample
{

    int currentClientId = 0;



    public static void Main(string[] args)
    {
        TCPServerSample server = new TCPServerSample();
        server.run();
    }

    private TcpListener _listener;
    private List<LobbyClient> _clients = new List<LobbyClient>();

    private void run()
    {
        Console.WriteLine("Server started on port 55555");

        _listener = new TcpListener(IPAddress.Any, 55555);
        _listener.Start();

        while (true)
        {
            processNewClients();
            removeFaultyClients();
            processExistingClients();

            //Although technically not required, now that we are no longer blocking, 
            //it is good to cut your CPU some slack
            Thread.Sleep(100);
        }
    }

    private void processNewClients()
    {
        while (_listener.Pending())
        {
            TcpClient clientToAdd = _listener.AcceptTcpClient();

            byte[] inBytes = StreamUtil.Read(clientToAdd.GetStream());
            Packet inPacket = new Packet(inBytes);
            ISerializable inObject = inPacket.ReadObject();
            
            if (inObject is AddClient)
            {
                LobbyClient newClient=new LobbyClient(clientToAdd, currentClientId++);
               
                SetClientParameters newClientParameters = new SetClientParameters();
                newClientParameters.id = newClient.id;

                newClientParameters.skin = newClient.id;
                //newClientParameters.skin = new System.Random().Next(1000)+_clients.Count;

                Console.WriteLine("joined client with id: " + newClientParameters.id +" and skin "+newClientParameters.skin);

                foreach (LobbyClient client in _clients)
                    sendObject(client.TcpClient, newClientParameters);

                _clients.Add(newClient);

                foreach (LobbyClient client in _clients)
                {
                    SetClientParameters parameters = new SetClientParameters();
                    parameters.id = client.id;
                    parameters.skin = client.id;
                    sendObject(newClient.TcpClient, parameters);
                }
            }
            Console.WriteLine("Accepted new client.");
        }
    }

    private void removeFaultyClients()
    {
        for (int i = 0; i < _clients.Count; i++)
        {
            LobbyClient client = _clients[i];

            //remove faulty clients
            if (!client.TcpClient.Connected)
            {
                Console.WriteLine("CLIENT " + client.id + " DISCONNECTED");
                
                RemoveClient clientToRemove = new RemoveClient();
                clientToRemove.SetParameters(client.id);

                _clients.RemoveAt(i);

                disconnectAvatarFromClients(clientToRemove);

                continue;
            }
        }
    }

    private void processExistingClients()
    {
        for (int i = 0; i < _clients.Count; i++)
        {
            LobbyClient client = _clients[i];

            if (client.TcpClient.Available == 0) continue;

            byte[] inBytes = StreamUtil.Read(client.TcpClient.GetStream());
            Packet inPacket = new Packet(inBytes);
            ISerializable inObject = inPacket.ReadObject();
            Console.WriteLine("Received:" + inObject);

            if (inObject is SimpleMessage)
            {
                SimpleMessage message = inObject as SimpleMessage;
                handleSimpleMessage(client, message);
            }
        }
    }

    private void disconnectAvatarFromClients(RemoveClient clientToRemove)
    {
        for (int i = 0; i < _clients.Count; i++)
        {
            LobbyClient client = _clients[i];

            sendObject(client.TcpClient, clientToRemove);
        }
    }

    private void handleSimpleMessage(LobbyClient pClient, SimpleMessage _simpleMessage)
    {
        _simpleMessage.id = pClient.id;

        Console.WriteLine("client "+pClient.id+" said "+ _simpleMessage.text);
        foreach (LobbyClient client in _clients)
        {
            sendObject(client.TcpClient, _simpleMessage);
        }
    }


    private void sendObject(TcpClient pClient, ISerializable pOutObject)
    {
        Console.WriteLine("Sending:" + pOutObject);
        Packet outPacket = new Packet();
        outPacket.Write(pOutObject);


        //try to write to client
        try
        {
            StreamUtil.Write(pClient.GetStream(), outPacket.GetBytes());
        }
        catch (Exception)
        {
            Console.WriteLine("Client is faulty and will be removed");
            pClient.Close();
        }
        
    }

}

