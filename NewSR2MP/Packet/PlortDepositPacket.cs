namespace NewSR2MP.Packet;

public class PlortDepositPacket : IPacket
{
    public PacketReliability Reliability => PacketReliability.ReliableOrdered;

    public PacketType Type => PacketType.PlortDepositor;

    public string id;
    public ushort count;
    public ushort max; // For loading
        
    public void Serialize(OutgoingMessage msg)
    {
        msg.Write(id);
        msg.Write(count);
        msg.Write(max);
    }

    public void Deserialize(IncomingMessage msg)
    {
        id = msg.ReadString();
        count = msg.ReadUInt16();
        max = msg.ReadUInt16();
    }
}