using System.Net;
using System.Net.Sockets;
using LiteEntitySystem;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared;

namespace Minimal;

internal class GameClient : INetEventListener
{
    private readonly NetPacketProcessor PacketProcessor = new();
    private readonly NetDataWriter Writer = new();
    private readonly NetManager NetManager;

    private ClientEntityManager EntityManager;

    public MinimalPawn MyPawn { get; private set; }

    public GameClient()
    {
        NetManager = new(this)
        {
            AutoRecycle = false,
        };
        NetManager.Start();
    }

    public void Connect()
        => NetManager.Connect(NetworkGeneral.SERVER_HOST, NetworkGeneral.SERVER_PORT, "");

    public void Update()
    {
        NetManager.PollEvents();
        EntityManager?.Update();
    }

    private void SendPacket<T>(T packet, DeliveryMethod deliveryMethod)
    where T : class, new()
    {
        if (NetManager.FirstPeer is null)
            return;

        Writer.Reset();
        Writer.Put((byte)PacketType.Serialized);
        PacketProcessor.Write(Writer, packet);
        NetManager.FirstPeer.Send(Writer, deliveryMethod);
    }

    public void OnPeerConnected(NetPeer peer)
    {
        SendPacket(new JoinPacket
        {
            Username = "Minimal Client",
        }, DeliveryMethod.ReliableOrdered);
        var typesMap = new EntityTypesMap<GameEntities>()
            .Register(GameEntities.MinimalPawn, e => new MinimalPawn(e))
            .Register(GameEntities.MinimalController, e => new MinimalController(e));
        EntityManager = new(typesMap,
            new InputProcessor<PacketInput>(),
            peer, (byte)PacketType.EntitySystem,
            NetworkGeneral.FPS);
        EntityManager.GetEntities<MinimalPawn>().SubscribeToConstructed(pawn =>
        {
            if (pawn.IsLocalControlled)
                MyPawn = pawn;
        }, true);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        => EntityManager = null;

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var packetType = reader.GetByte();
        if (packetType >= NetworkGeneral.PacketTypesCount)
        {
            reader.Recycle();
            return;
        }
        var pt = (PacketType)packetType;
        switch (pt)
        {
            case PacketType.EntitySystem:
                EntityManager!.Deserialize(reader);
                break;

            case PacketType.Serialized:
                PacketProcessor.ReadAllPackets(reader);
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
        => request.Reject();
}
