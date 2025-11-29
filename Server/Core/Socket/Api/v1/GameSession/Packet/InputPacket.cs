using Altruist;

using MessagePack;

using Server.Gameplay;

namespace Server;

[MessagePackObject]
public struct InputPacket : IPacketBase
{
    [Key(0)]
    public int Sequence { get; set; }

    [Key(1)]
    public sbyte MoveX { get; set; }
    [Key(2)]
    public sbyte MoveY { get; set; }

    [Key(3)]
    public short LookDeltaX { get; set; }
    [Key(4)]
    public short LookDeltaY { get; set; }

    [Key(5)]
    public InputSlots Slots { get; set; }

    public InputPacket(
        int sequence,
        sbyte moveX,
        sbyte moveY,
        short lookDeltaX,
        short lookDeltaY,
        InputSlots slots)
    {
        Sequence = sequence;
        MoveX = moveX;
        MoveY = moveY;
        LookDeltaX = lookDeltaX;
        LookDeltaY = lookDeltaY;
        Slots = slots;
    }
}

