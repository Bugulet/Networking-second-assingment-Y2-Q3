using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;

public class LobbyClient
{
    public TcpClient TcpClient;

    public int id { get;}
    //int skin { get; }
    //int x { get; }
    //int y { get; }

        public LobbyClient(TcpClient _tcpClient, int _id)
        {
            TcpClient = _tcpClient;
            id = _id;
        }
    }
