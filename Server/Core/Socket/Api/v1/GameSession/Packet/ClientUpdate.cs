using System.Text.Json.Serialization;

using Altruist;

using MessagePack;

namespace Server.Packet
{

    // TODO: move this to central place e.g. Combat
    [Flags]
    public enum CharacterStateFlags : ushort
    {
        None = 0,

        OnGround = 1 << 0,
        Jumping = 1 << 1,
        Falling = 1 << 2,
        Dashing = 1 << 3,
        Boosting = 1 << 4,
        Stunned = 1 << 5,
        Rooted = 1 << 6,
    }

    [MessagePackObject]
    public struct UpdateClientPositionAndOrientation : IPacketBase
    {
        [Key(0)]
        [JsonPropertyName("x")]
        public float X { get; set; }

        [Key(1)]
        [JsonPropertyName("y")]
        public float Y { get; set; }

        [Key(2)]
        [JsonPropertyName("z")]
        public float Z { get; set; }

        [Key(3)]
        [JsonPropertyName("yaw")]
        public float Yaw { get; set; }

        [Key(4)]
        [JsonPropertyName("pitch")]
        public float Pitch { get; set; }

        [Key(5)]
        [JsonPropertyName("roll")]
        public float Roll { get; set; }

        /// <summary>
        /// Bitwise character state flags (on ground, jumping, dashing, etc.).
        /// </summary>
        [Key(6)]
        [JsonPropertyName("state")]
        public CharacterStateFlags State { get; set; }

        public UpdateClientPositionAndOrientation(
            float x,
            float y,
            float z,
            float yaw,
            float pitch,
            float roll,
            CharacterStateFlags state)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
            State = state;
        }
    }
}
