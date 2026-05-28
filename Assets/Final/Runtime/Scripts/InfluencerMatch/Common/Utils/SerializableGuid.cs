using System;
using UnityEngine;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Unity-serializable counterpart of <see cref="System.Guid"/>.
    /// </summary>
    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>, IComparable<SerializableGuid>
    {
        [SerializeField, HideInInspector] private uint m_Part1;
        [SerializeField, HideInInspector] private uint m_Part2;
        [SerializeField, HideInInspector] private uint m_Part3;
        [SerializeField, HideInInspector] private uint m_Part4;

        public static SerializableGuid Empty => default;

        public SerializableGuid(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            m_Part1 = BitConverter.ToUInt32(bytes, 0);
            m_Part2 = BitConverter.ToUInt32(bytes, 4);
            m_Part3 = BitConverter.ToUInt32(bytes, 8);
            m_Part4 = BitConverter.ToUInt32(bytes, 12);
        }

        public bool IsEmpty => m_Part1 == 0u && m_Part2 == 0u && m_Part3 == 0u && m_Part4 == 0u;

        public static SerializableGuid NewGuid() => new SerializableGuid(Guid.NewGuid());

        public Guid ToGuid()
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(m_Part1).CopyTo(bytes, 0);
            BitConverter.GetBytes(m_Part2).CopyTo(bytes, 4);
            BitConverter.GetBytes(m_Part3).CopyTo(bytes, 8);
            BitConverter.GetBytes(m_Part4).CopyTo(bytes, 12);
            return new Guid(bytes);
        }

        public override string ToString() => ToGuid().ToString();

        public bool Equals(SerializableGuid other) =>
            m_Part1 == other.m_Part1 && m_Part2 == other.m_Part2 && m_Part3 == other.m_Part3 && m_Part4 == other.m_Part4;

        public int CompareTo(SerializableGuid other)
        {
            int c = m_Part1.CompareTo(other.m_Part1);
            if (c != 0) return c;
            c = m_Part2.CompareTo(other.m_Part2);
            if (c != 0) return c;
            c = m_Part3.CompareTo(other.m_Part3);
            if (c != 0) return c;
            return m_Part4.CompareTo(other.m_Part4);
        }

        public override bool Equals(object obj) => obj is SerializableGuid other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(m_Part1, m_Part2, m_Part3, m_Part4);

        public static bool operator ==(SerializableGuid left, SerializableGuid right) => left.Equals(right);

        public static bool operator !=(SerializableGuid left, SerializableGuid right) => !left.Equals(right);
    }
}
