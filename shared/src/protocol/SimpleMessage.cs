namespace shared
{
    public class SimpleMessage : ISerializable
    {
        public string text { get; set; }
        public int id { get; set; }
        

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(text);
            pPacket.Write(id);
        }

        public void Deserialize(Packet pPacket)
        {
            text = pPacket.ReadString();
            id = pPacket.ReadInt();
        }
    }
}
