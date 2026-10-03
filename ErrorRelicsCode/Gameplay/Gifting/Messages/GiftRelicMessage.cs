using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace ErrorRelics.ErrorRelicsCode.Gameplay.Gifting.Messages;

public struct GiftRelicMessage : INetMessage, IPacketSerializable
{
    public ulong TargetPlayerId;
    public int SourceRelicIndex;

    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Debug;
    public bool ShouldBuffer => false;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(TargetPlayerId);
        writer.WriteInt(SourceRelicIndex);
    }

    public void Deserialize(PacketReader reader)
    {
        TargetPlayerId = reader.ReadULong();
        SourceRelicIndex = reader.ReadInt();
    }
}
