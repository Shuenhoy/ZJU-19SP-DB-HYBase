using System.Collections.Generic;
using LanguageExt;

namespace HYBase.Utils
{
    class LRUCache<K, T>
    {
        private int capacity;
        private int size;

        private LinkedList<(K key, T value)> buffer;
        private IDictionary<K, LinkedListNode<(K key, T value)>> hashTable;
        public LRUCache(int cap)
        {
            buffer = new LinkedList<(K key, T value)>();
            hashTable = new Dictionary<K, LinkedListNode<(K key, T value)>>();
            capacity = cap;
        }
        public void Set(K key, T value)
        {
            hashTable.TryGetValue<K, LinkedListNode<(K key, T value)>>(key).Match(Some: node =>
            {
                buffer.Remove(node);

                hashTable[key] = buffer.AddFirst((key, value));
            },
            None: () =>
            {
                hashTable[key] = buffer.AddFirst((key, value));
                if (size > capacity)
                {
                    hashTable.Remove(buffer.Last.Value.key);
                    buffer.RemoveLast();
                }
            });
        }
        public bool Exist(K key)
            => hashTable.ContainsKey(key);
        public T Get(K key)
        {
            if(Exist(key)){
                return 
            }
        }
    }
}