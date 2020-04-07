namespace shared
{
    public class RemoveClient : ISerializable
    {
        public int id = 0;
        

        public void SetParameters(int _id)
        {
            id = _id;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(id);
        }

        public void Deserialize(Packet pPacket)
        {
            id = pPacket.ReadInt();
        }
    }
}
