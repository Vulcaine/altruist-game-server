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
    public struct UpdateClientPositionAndOrientation :
        IPacketBase,
        IEquatable<UpdateClientPositionAndOrientation>
    {
        private const float PositionEpsilon = 0.01f; // 1 cm
        private const float RotationEpsilon = 0.5f;  // 0.5 degrees

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

        // ----------------- equality with thresholds -----------------

        public bool Equals(UpdateClientPositionAndOrientation other)
        {
            // If state flags differ, treat as different regardless of position/rotation.
            if (State != other.State)
                return false;

            return
                MathF.Abs(X - other.X) <= PositionEpsilon &&
                MathF.Abs(Y - other.Y) <= PositionEpsilon &&
                MathF.Abs(Z - other.Z) <= PositionEpsilon &&
                MathF.Abs(Yaw - other.Yaw) <= RotationEpsilon &&
                MathF.Abs(Pitch - other.Pitch) <= RotationEpsilon &&
                MathF.Abs(Roll - other.Roll) <= RotationEpsilon;
        }

        public override bool Equals(object? obj) =>
            obj is UpdateClientPositionAndOrientation other && Equals(other);

        public override int GetHashCode()
        {
            // Hash based on quantized values so it's at least consistent
            var hash = new HashCode();

            hash.Add(MathF.Round(X, 2));
            hash.Add(MathF.Round(Y, 2));
            hash.Add(MathF.Round(Z, 2));
            hash.Add(MathF.Round(Yaw, 1));
            hash.Add(MathF.Round(Pitch, 1));
            hash.Add(MathF.Round(Roll, 1));
            hash.Add(State);

            return hash.GetHashCode();
        }

        public static bool operator ==(
            UpdateClientPositionAndOrientation left,
            UpdateClientPositionAndOrientation right)
            => left.Equals(right);

        public static bool operator !=(
            UpdateClientPositionAndOrientation left,
            UpdateClientPositionAndOrientation right)
            => !left.Equals(right);
    }
}
