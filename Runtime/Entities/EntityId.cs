using System;
using System.Runtime.CompilerServices;

namespace OpenUGD.ECS.Entities
{
    [Serializable]
    public struct EntityId : IEquatable<EntityId>, IComparable<EntityId>, ICloneable
    {
        public static readonly EntityId Empty = new EntityId(0, 0, 0);

        public static bool operator ==(EntityId left, EntityId right) => left.Id == right.Id;

        public static bool operator !=(EntityId left, EntityId right) => !(left == right);

        public static explicit operator long(EntityId value)
        {
            return ((long)value.SubWorldId << (32 + 16)) | ((long)value.Index << 32) | (long)value.Id;
        }

        public static implicit operator short(EntityId value) => value.Index;

        public static explicit operator EntityId(long value)
        {
            int id = (int)((0xFFFFFFFF) & value);
            short index = (short)((0xFFFF) & (value >> 32));
            short subWorldId = (short)((0xFFFF) & (value >> (32 + 16)));
            return new EntityId(id, index, subWorldId);
        }

        public readonly int Id;

        public readonly short Index;

        public readonly short SubWorldId;

        public EntityId(int id, short index, short subWorldId)
        {
            Id = id;
            Index = index;
            SubWorldId = subWorldId;
        }

        public bool Equals(EntityId other) => Id == other.Id;

        public int CompareTo(EntityId other) => Id.CompareTo(other.Id);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is EntityId && Equals((EntityId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                //return ((Id * 397) ^ Index * 397) ^ SubWorldId;
                return Id;
            }
        }

        // ReSharper disable once HeapView.BoxingAllocation
        object ICloneable.Clone() => new EntityId(Id, Index, SubWorldId);
        public EntityId Clone() => this;

        public override string ToString()
        {
            return $"[EntityId({nameof(Id)}:{Id}, {nameof(Index)}:{Index}, {nameof(SubWorldId)}:{SubWorldId})]";
        }

        public bool FullEquals(EntityId entityId)
        {
            return
                Id == entityId.Id
                && Index == entityId.Index
                && SubWorldId == entityId.SubWorldId;
        }
    }
}