using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovaSamples.Inventory
{
    /// <summary>
    /// A description of an available inventory item type, which may be selected when an
    /// <see cref="ItemDatabase"/> goes to randomly populate a list with inventory items.
    /// </summary>
    /// <remarks>
    /// <see cref="ItemDatabase.GetRandomItems(int)"/>.
    /// </remarks>
    [Serializable]
    public class ItemDescription
    {
        public string Name;
        public Texture2D Image;
    }

    /// <summary>
    /// The underlying data type of an element in a game's inventory system.
    /// </summary>
    public class InventoryItem
    {
        public ItemDescription Item;
        public int Count;

        public bool IsEmpty => Item == null;

        public static readonly InventoryItem Empty = new InventoryItem();

        /// <summary>
        /// The max number of items a given inventory slot can hold.
        /// </summary>
        public const int MaxItemsPerSlot = 50;
    }

    /// <summary>
    /// Provides information for all of the different items that are available.
    /// The actual implementation and approach for this will very heavily depending on your scenario.
    /// The implementation here is meant to serve as a simple example/stub.
    /// </summary>
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemDescription> Items = null;

        /// <summary>
        /// Get a list of size <paramref name="count"/> containing randomly generated <see cref="InventoryItem"/>s.
        /// </summary>
        /// <param name="count">The number of items to generate.</param>
        public List<InventoryItem> GetRandomItems(int count)
        {
            List<InventoryItem> list = new List<InventoryItem>(count);

            for (int i = 0; i < count; i++)
            {
                // Make about half of the slots empty
                bool isEmpty = UnityEngine.Random.Range(0, 1f) > 0.5f;

                if (isEmpty)
                {
                    list.Add(InventoryItem.Empty);
                }
                else
                {
                    // Pick a random item with a random count
                    int index = UnityEngine.Random.Range(0, Items.Count);
                    ItemDescription item = Items[index];
                    list.Add(new InventoryItem()
                    {
                        Item = item,
                        Count = UnityEngine.Random.Range(1, InventoryItem.MaxItemsPerSlot),
                    });
                }
            }

            return list;
        }
    }
}
