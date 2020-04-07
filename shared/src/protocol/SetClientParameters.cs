namespace shared
{
    public class SetClientParameters : ISerializable
    {
        public int id = 0, skin = 0;
        

        public void SetParameters(int _id, int _skin)
        {
            id = _id;
            skin = _skin;
        }

        public void Serialize(Packet pPacket)
        {
            pPacket.Write(id);
            pPacket.Write(skin);
        }

        public void Deserialize(Packet pPacket)
        {
            id = pPacket.ReadInt();
            skin = pPacket.ReadInt();
        }
    }
}
