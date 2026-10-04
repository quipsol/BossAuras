using Godot;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace BossAuras.Util;

public class SerializableVector2I : IPacketSerializable
{
    public int X { get; set; }
    public int Y { get; set; }
    
    
    public SerializableVector2I(){}
    public SerializableVector2I(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(X);
        writer.WriteInt(Y);
    }

    public void Deserialize(PacketReader reader)
    {
        X = reader.ReadInt(8);
        Y = reader.ReadInt(8);
    }
    
    public static implicit operator Vector2I(SerializableVector2I value) => new Vector2I(value.X, value.Y);
    public static implicit operator SerializableVector2I(Vector2I value) => new SerializableVector2I(value.X, value.Y);
    
}