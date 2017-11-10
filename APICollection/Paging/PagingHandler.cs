using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LeanStack.Paging
{
    /// <summary>
    /// Used mainly by the APICollections class for paging and sorting.
    /// <pre>
    /// Gets Initialized with the paging params object and a list of expressions for available sort properties.
    /// Reads requested page number and size , calculates total results and sorts by the expression found for requested key on the dictionary.
    /// </pre>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class PagingHandler
    {
        public PagingParams pagingParams { get; set; }
        /// <summary>
        /// Reserved key for the default sort function.
        /// </summary>
        public static String DefaultSortKey = "Default";
        /// <summary>
        /// If dynamic paging is true , this handler can sort by ANY property of the TEntity class using the Property name as a key.
        /// </summary>
        public Boolean DynamicPaging = false;
    }
    public class PagingHandler<TEntity> : PagingHandler
    {
        public Dictionary<String, Expression<Func<TEntity, object>>> availiableSortFunctions { get; set; }
        /// <summary>
        /// Initializes the handler with the requested page number, size etc.
        /// <pre>Optional SortProperties can be passed pre created as dictionary mainly for performance or other reasons.
        /// Alternatively you can add line by line sort expression to the dictionary.
        /// </pre>
        /// </summary>
        /// <param name="PagingParams"></param>
        /// <param name="SortFunctions"></param>
        public PagingHandler(PagingParams PagingParams, Dictionary<String, Expression<Func<TEntity, object>>> SortFunctions=null) 
        {
            this.availiableSortFunctions = SortFunctions?? new Dictionary<string, Expression<Func<TEntity, object>>>();
            this.pagingParams = PagingParams;
        }
        public IQueryable<TEntity> OrderAndPage(IQueryable<TEntity> entities)
        {
            String sortByProperty = this.pagingParams.SortBy;
            IQueryable<TEntity> tempEntity;
            //Fallback to default sort function if none defined.
            if (String.IsNullOrWhiteSpace(sortByProperty))
                sortByProperty = DefaultSortKey;
            //The handler will search for any available sort function with the defined key.
            if (this.availiableSortFunctions.TryGetValue(sortByProperty, out Expression<Func<TEntity, object>> orderExpr))
                tempEntity = entities.UnboxOrderByExpression(orderExpr,this.pagingParams.Desc);          
            else if(DynamicPaging==false)
                throw new ArgumentException("A sort definition for the given sort by property was not found");
            //Dynamic paging is enabled. the handler will search using reflection to sort by a property with the same name as the sort property.
            else
                tempEntity = entities.OrderBy<TEntity>(sortByProperty, this.pagingParams.Desc);

            return Page(tempEntity,this.pagingParams);            
        }
        /// <summary>
        /// Generates a summary of currently viewing items, based on the total results, the requested current page number and size . 
        /// </summary>
        /// <param name="resultsCount"></param>
        /// <returns></returns>
        public PagingSummary GeneratePagingResultSummary(Int32 resultsCount)
        {
            return new PagingSummary(resultsCount, this.pagingParams);
        }
        /// <summary>
        /// Returns all available keys/properties that this handler can sort by.
        /// <pre>
        /// Returns all keys from the dictionary of sort expressions, plus if dynamic paging property is enabled also returns all properties of the source class.
        /// </pre>
        /// </summary>
        /// <returns></returns>
        public List<String> GetSupportedSortProperties()
        {
            List<String> sortProps = null;
            if (this.DynamicPaging)
            {
                sortProps=typeof(TEntity).GetProperties().Where(p => p.GetGetMethod().IsVirtual == false).Select(p => p.Name).ToList();
            }
            if (this.availiableSortFunctions != null)
            {
                if (sortProps != null)
                    sortProps.AddRange(this.availiableSortFunctions.Select(c => c.Key));
                else
                    sortProps = this.availiableSortFunctions.Select(c => c.Key).ToList();
            }
            return sortProps;
        }
        private IQueryable<TEntity> Page(IQueryable<TEntity> source, PagingParams paging)
        {
            int index = paging.PageNumber - 1;
            if (paging.PageSize != 0)
                return source.Skip(index * paging.PageSize).Take(paging.PageSize);
            else
                return source;
        }
    }

}