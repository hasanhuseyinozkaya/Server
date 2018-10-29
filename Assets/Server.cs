using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnityEngine.Networking;
using Game.Model;
using System;
public class ClientList
{
    public int connectionId
    {
        get;
        set;
    }
    public string userName
    {
        get;
        set;
    }


}
public class Server : MonoBehaviour
{

    public const int MAX_CONNECTION = 100;
    private int port = 5701;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;
    private float lastMovementUpdate;
    private float movementUpdateRate = 0.09f;
    List<Players> clientList = new List<Players>();
    // Use this for initialization
    void Start()
    {
        print("Server Starting");
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();


        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo,port,null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);
        isStarted = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStarted)
            return;

            //print("startted");
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;

            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData)
            {
             
                case NetworkEventType.ConnectEvent:
                    print("Somebody connected me");
                OnConnection(connectionId);
                break;


                case NetworkEventType.DataEvent:
                byte[] receivedData = new byte[dataSize];
                Array.Copy(recBuffer, receivedData, dataSize);


                Players ply = SerializationHelper.ProtoDeserialize<Players>(receivedData);
                switch (ply.gameEventType)
                {
                    case EventTypes.Login:
                        GameData(ply);
                        break;
                    case EventTypes.Move:
                        PlayerMove(ply);
                        //print(ply.PlayerName + " still in connection with " + ply.ConnectionId);
                        break;
                    default:
                        break;
                }
               
                print("data received");
                break;

                case NetworkEventType.DisconnectEvent:
                Players disconnectedPlayer = clientList.Find(t => t.ConnectionId == connectionId);
                print(disconnectedPlayer.PlayerName + " disconnected");
                clientList.Remove(disconnectedPlayer);
                break;


                //case NetworkEventType.BroadcastEvent:
           
                 //   break;
            
        }

        if(Time.time-lastMovementUpdate>movementUpdateRate)
        {

            foreach (var cl in clientList)
            {
                cl.gameEventType = EventTypes.Position;
                byte[] payerData = SerializationHelper.ProtoSerialize(cl);
                NetworkTransport.Send(hostId, connectionId, unreliableChannel, payerData, payerData.Length, out error);

            }

          
        }
    }

    private void OnConnection(int cnnId)
    {
        //rndm
        System.Random rnd = new System.Random();
        string message = "tempname" + rnd.Next(0, 50);



        Players payer = new Players();
        payer.ConnectionId = cnnId;
        payer.PlayerName = message;
        payer.gameEventType = EventTypes.OnConnectionResponse;
        clientList.Add(payer);
    

        byte[] payerData = SerializationHelper.ProtoSerialize(payer);
        NetworkTransport.Send(hostId, cnnId, reliableChannel, payerData, payerData.Length, out error);

        //Players client = new Players();
        //client.ConnectionId = connectionId;
        //client.PlayerName = message;
        //clientList.Add(client);
    }

    private void GameData(Players player)
    {
        print(player.ConnectionId);
        Players payer = new Players();
        clientList.Find(t => t.ConnectionId == player.ConnectionId).PlayerName = player.PlayerName;
        payer.ConnectionId = player.ConnectionId;
        payer.PlayerName = player.PlayerName;
        payer.gameEventType = EventTypes.LoginResponse;
        byte[] payerData = SerializationHelper.ProtoSerialize(payer);
        NetworkTransport.Send(hostId, payer.ConnectionId, reliableChannel, payerData, payerData.Length, out error);

        payer.gameEventType = EventTypes.PlayerSpawn;
        byte[] payerDataForSpawn = SerializationHelper.ProtoSerialize(payer);
        List<Players> reducedList = clientList.FindAll(t => t.ConnectionId != player.ConnectionId);
        for (int i = 0; i < reducedList.Count; i++)
        {

                reducedList[i].gameEventType = EventTypes.PlayerSpawn;
                byte[] spawnData = SerializationHelper.ProtoSerialize(reducedList[i]);
                NetworkTransport.Send(hostId, reducedList[i].ConnectionId, reliableChannel, payerDataForSpawn, payerDataForSpawn.Length, out error);

            NetworkTransport.Send(hostId, payer.ConnectionId, reliableChannel, spawnData, spawnData.Length, out error);

        }

       
    }

    public void PlayerMove(Players player)
    {

        List<Players> reducedList = clientList;//.FindAll(t => t.ConnectionId != player.ConnectionId);
        if (reducedList.Count == 0)
        {
            player.gameEventType = EventTypes.MoveResponse;
            byte[] payerDataForSpawn = SerializationHelper.ProtoSerialize(player);
            NetworkTransport.Send(hostId, player.ConnectionId, reliableChannel, payerDataForSpawn, payerDataForSpawn.Length, out error);
        }
        else
        {
            player.gameEventType = EventTypes.MoveResponse;
            byte[] payerDataForSpawn = SerializationHelper.ProtoSerialize(player);

            for (int i = 0; i < reducedList.Count; i++)
            {

                reducedList[i].gameEventType = EventTypes.MoveResponse;
                byte[] spawnData = SerializationHelper.ProtoSerialize(reducedList[i]);
                NetworkTransport.Send(hostId, reducedList[i].ConnectionId, reliableChannel, payerDataForSpawn, payerDataForSpawn.Length, out error);

            //    NetworkTransport.Send(hostId, player.ConnectionId, reliableChannel, spawnData, spawnData.Length, out error);



            }
        }

    }


}
