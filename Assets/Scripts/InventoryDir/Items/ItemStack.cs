using System;

namespace InventoryDir.Items
{
    [Serializable]
    public struct ItemStack : IEquatable<ItemStack>
    {
        public string itemId;
        public int count;

        public ItemStack(string id, int c)
        {
            itemId = id;
            count = c;
            IsNull = false;
        }

        public bool IsEmpty => string.IsNullOrEmpty(itemId) || count <= 0;
        public bool IsNull { get; }

        public bool Equals(ItemStack other)
        {
            return itemId == other.itemId && count == other.count;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(itemId, count, IsNull);
        }
    }
}