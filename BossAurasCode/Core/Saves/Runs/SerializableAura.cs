using System.Text.Json.Serialization;
using BossAuras.Core.Models;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossAuras.Core.Saves.Runs;

public class SerializableAura : IPacketSerializable
{
    public ModelId? Id { get; set; }
    public SavedProperties? Props { get; set; }
    
    
    public void Serialize(PacketWriter writer)
    {
        writer.WriteModelEntry(Id!);
        writer.WriteBool(Props != null);
        if (Props != null)
        {
            writer.Write(Props);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        Id = reader.ReadModelIdAssumingType<AuraModel>();
        if (reader.ReadBool())
        {
            Props = reader.Read<SavedProperties>();
        }
    }
    
    // Moved into "AuraModel" because base game has these in their model equivalents too
    //public static SerializableAura FromAura(AuraModel aura) => new() { Id = aura.Id };
    //public AuraModel ToAura() => (AuraModel)ModelDb.GetById<AuraModel>(Id!).MutableClone();
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
    public override string ToString()
    {
        return $"SerializableBossAura {Id}.";
    }
}