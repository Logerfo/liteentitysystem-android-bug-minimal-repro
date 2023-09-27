using System.Net;
using System.Net.Sockets;
using LiteEntitySystem;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared;

namespace Server;

internal class GameServer : INetEventListener
{
    private readonly NetPacketProcessor PacketProcessor = new();
    private readonly NetManager NetManager;
    private readonly ServerEntityManager EntityManager;

    public GameServer()
    {
        NetManager = new(this)
        {
            AutoRecycle = false,
        };
        var typeMap = new EntityTypesMap<GameEntities>()
            .Register(GameEntities.MinimalPawn, e => new MinimalPawn(e))
            .Register(GameEntities.MinimalController, e => new MinimalController(e));
        EntityManager = new(typeMap,
            new InputProcessor<PacketInput>(),
            (byte)PacketType.EntitySystem,
            NetworkGeneral.FPS, ServerSendRate.EqualToFPS);
        PacketProcessor.SubscribeReusable<JoinPacket, NetPeer>(OnJoinReceived);
        NetManager.Start(NetworkGeneral.SERVER_PORT);
    }

    public void Update()
    {
        NetManager.PollEvents();
        EntityManager.Update();
    }

    private void OnJoinReceived(JoinPacket joinPacket, NetPeer peer)
    {
        if (EntityManager.AddPlayer(peer, true) is not { } serverPlayer)
            return;

        var pawn = EntityManager.AddEntity<MinimalPawn>(e => e.RandomString.Value = "RANDOM STRING HERE");
        EntityManager.AddController<MinimalController>(serverPlayer, e => e.StartControl(pawn));
        Console.WriteLine($"{joinPacket.Username} joined");
    }

    public void OnPeerConnected(NetPeer peer)
        => Console.WriteLine($"{peer.Id} connected");

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (peer.Tag is NetPlayer player)
            EntityManager.RemovePlayer(player);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var packetType = reader.GetByte();
        switch ((PacketType)packetType)
        {
            case PacketType.EntitySystem:
                EntityManager.Deserialize(peer, reader);
                break;

            case PacketType.Serialized:
                PacketProcessor.ReadAllPackets(reader, peer);
                reader.Recycle();
                break;

            default:
                reader.Recycle();
                break;
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

    public void OnConnectionRequest(ConnectionRequest request)
        => request.Accept();
}
