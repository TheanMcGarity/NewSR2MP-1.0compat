namespace NewSR2MP.Packet;

public class GardenOwnershipPacket : IPacket
{
    public PacketType Type => PlanterOwnership;
    public PacketReliability Reliability => PacketReliability.ReliableOrdered;
    
    public string id;

    public void Serialize(OutgoingMessage om)
    {
        om.Write(id);
    }

    public void Deserialize(IncomingMessage im)
    {
        id = im.ReadString();
    }
}
public class GardenUpdatePacket : IPacket
{
    public PacketType Type => PlanterUpdate;
    public PacketReliability Reliability => PacketReliability.ReliableOrdered;

    public double time;
    public string id;
    
    public void Serialize(OutgoingMessage om)
    {
        om.Write(id);
        om.Write(time);
    }

    public void Deserialize(IncomingMessage im)
    {
        id = im.ReadString();
        time = im.ReadDouble();
    }
}