namespace LeanStack.Paging
{
    public class PagingSummary
    {
        public int fromRecord { get; set; }
        public int toRecord { get; set; }
        public int ResultsCount { get; set; }
        public int PagesCount { get; set; }
        public int CurrentPage { get; set; }
        /// <summary>
        /// By convention requesting 0 page size will bring all results.
        /// </summary>
        public int PageSize { get; set; }
        public PagingSummary(int resultsCount, PagingParams prms)
        {
            this.ResultsCount = resultsCount;
            this.PageSize = prms.PageSize;
            if (prms.PageSize != 0)
            {
                this.CurrentPage = prms.PageNumber;
                int pageIndex = prms.PageNumber - 1;
                this.PagesCount = (this.ResultsCount / prms.PageSize);
                int modulus = this.ResultsCount % prms.PageSize;
                if (modulus != 0 || PagesCount == 0)
                    this.PagesCount++;
                if (resultsCount > 0)
                {
                    this.fromRecord = (pageIndex * prms.PageSize) + 1;
                    this.toRecord = (fromRecord + prms.PageSize) - 1;
                    if (this.toRecord > ResultsCount)
                        this.toRecord = ResultsCount;
                }
            }
            else
            {
                this.fromRecord = 1;
                this.toRecord = resultsCount;
                this.CurrentPage = 1;
                this.PagesCount = 1;
            }
        }
    }
}
