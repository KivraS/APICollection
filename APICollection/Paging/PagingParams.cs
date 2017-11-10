using System;

namespace LeanStack.Paging
{
    public class PagingParams
    {
        public Int32 PageSize { get; set; }
        public Int32 PageNumber { get; set; }
        public String SortBy { get; set; }
        public bool Desc { get; set; }
        public Int32? allCount { get; set; }
    }
}
