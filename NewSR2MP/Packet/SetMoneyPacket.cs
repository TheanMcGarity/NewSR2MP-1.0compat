
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Packet
{
    public class SetMoneyPacket : IPacket
    {
        public PacketReliability Reliability => PacketReliability.ReliableOrdered;

        public PacketType Type => SetCurrency;

        public int newMoney;
        public byte type;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(newMoney);
            msg.Write(type);
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            newMoney = msg.ReadInt32();
            type = msg.ReadByte();
        }
    }
}
