using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Contracts
{
    public class CollectionsDto
    {
        public int[] Scores { get; set; }
        public int[,] Matrix { get; set; }
        public byte[] AvatarBytes { get; set; }
        public List<string> Tags { get; set; }
        public ICollection<Guid> Ids { get; set; }
        public IEnumerable<DateTime> Dates { get; set; }
        public IReadOnlyList<long> Indexes { get; set; }
        public ReadOnlyCollection<decimal> Totals { get; set; }
        public Dictionary<string, int> Counters { get; set; }
        public IDictionary<int, string> ReverseLookup { get; set; }
        public IReadOnlyDictionary<string, ICollection<int>> NestedMap { get; set; }
        public Array UntypedArray { get; set; }
    }
}
