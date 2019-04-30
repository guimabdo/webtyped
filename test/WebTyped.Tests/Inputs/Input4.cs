using System;
using System.Collections.Generic;
using System.Text;

namespace Bla
{
    [WebTyped.Annotations.ClientType(module: "primeng/components/common/lazyloadevent")]
    public class LazyLoadEvent
    {
        public enum SortOrderKind
        {
            Asceding = 1,
            Descending = -1
        }

        public int? First { get; set; }
        public int? Rows { get; set; }
        public string SortField { get; set; }
        public SortOrderKind? SortOrder { get; set; }
        public string GlobalFilter { get; set; }
        public Dictionary<string, FilterMetadata> Filters { get; set; }
        //multiSortMeta?: SortMeta[];
    }

    public class FilterMetadata
    {
        public string Value { get; set; }
    }
}