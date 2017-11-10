using LeanStack.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;
using LeanStack.Filtering;
using System.Collections.Specialized;

namespace LeanStack
{
    /// <summary>
    /// Used only for API result format consistency 
    /// <pre>You can use this class if your want to populate the results on your own but don't want to break your API consistency</pre>
    /// </summary>
    public class APICollection
    {
        public virtual IEnumerable<Object> ResultSet { get; set; }
    }
    /// <summary>
    /// A collection that supports optional paging and filtering on the source data.
    /// <pre>Takes a source IQueryable input , and populates the results based on configuration provided for filtering and paging
    /// See EnablePaging and Enable Filtering methods.
    /// </pre>
    /// </summary>
    /// <typeparam name="TSource">The class type of input and output</typeparam>
    public class APICollection<TSource> : APICollection where TSource : class
    {
        protected PagingHandler<TSource> pageHandler { get; set; }
        public PagingSummary pagedResults { get; set; }
        protected FilteringHandler<TSource> filterHandler { get; set; }
        protected Boolean ReturnInitValues { get; set; }
        /// <summary>
        /// Creates a new instance of an APICollection of a defined class type as source and output.
        /// </summary>
        /// <param name="LoadInitValues">If true returns back to the client all grid initialization data such as drop down population data</param>
        public APICollection(Boolean LoadInitValues = true)
        {
            this.ReturnInitValues = LoadInitValues;
        }
        public virtual IEnumerable<SupportedFilterProperty> SupportedFilterProperties
        {
            get
            {
                return (this.ReturnInitValues && filterHandler != null) ? filterHandler.GetSupportedFilteringProperties() : null;
            }
        }
        
        public virtual IEnumerable<String> SupportedSortProperties
        {
            get
            {
                return (this.ReturnInitValues && pageHandler != null) ? pageHandler.GetSupportedSortProperties() : null;
            }
        }
        /// <summary>
        /// Will enable this collection to read the paging parameters provided on this method and return the results paged accordingly 
        /// </summary>
        /// <param name="PagingParams">Paging configuration parameters</param>
        /// <param name="SortProperties">A list of ordering instructions this list will support</param>
        /// <param name="dynamicPaging">Allows sorting of data based on any source class property</param>
        public void EnablePaging(PagingParams PagingParams, Dictionary<String, Expression<Func<TSource, object>>> SortProperties, bool dynamicPaging = false)
        {
            if (this.pageHandler != null)
                throw new InvalidOperationException("Paging already enabled");
            this.pageHandler = new PagingHandler<TSource>(PagingParams, SortProperties) { DynamicPaging = dynamicPaging };
        }
        /// <summary>
        /// Will enable this collection to read the paging parameters provided on this method and return the results paged accordingly
        /// </summary>
        /// <param name="PagingParams">Paging configuration parameters</param>
        /// <param name="DefaultSorting">A default sort expression is mandatory in order to enable paging</param>
        /// <param name="dynamicPaging">Allows sorting of data based on any source class property</param>
        public void EnablePaging(PagingParams PagingParams, Expression<Func<TSource, object>> DefaultSorting, bool dynamicPaging = false)
        {
            if (DefaultSorting == null)
                throw new ArgumentException("Default sort expression cannot be null");
            var sortProperties = new Dictionary<String, Expression<Func<TSource, object>>> { {PagingHandler<TSource>.DefaultSortKey ,DefaultSorting } };
            this.EnablePaging(PagingParams,sortProperties,dynamicPaging);
        }
        /// <summary>
        /// Adds a supported sort definition for the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expr"></param>
        public void AddSortProperty(string key, Expression<Func<TSource, object>> expr)
        {
            if (this.pageHandler == null)
                throw new InvalidOperationException($"Paging is not enabled. Enable paging by calling {nameof(this.EnablePaging)}");
            this.pageHandler.availiableSortFunctions[key] = expr;
        }
        /// <summary>
        /// Enables this collection to filter results based on query string values and provided filter predicates of each key
        /// </summary>
        /// <param name="queryValues">The HttpRequest to read the query string values</param>
        /// <param name="dynamicFiltering">Enables this collection to filter results based on any source class property</param>
        /// <param name="filterDefinitions">An optional list of predefined filter predicated for each key.Filters can be added also later</param>
        public void EnableFiltering(NameValueCollection queryValues, bool dynamicFiltering=false, Dictionary<String, FilterParamDefinition<TSource>> filterDefinitions=null)
        {
            if (this.filterHandler != null)
                throw new InvalidOperationException("Filtering already enabled");
            this.filterHandler = new FilteringHandler<TSource>(queryValues, filterDefinitions,dynamicFiltering);
        }
        public void EnableFiltering(HttpRequestMessage request, bool dynamicFiltering = false, Dictionary<String, FilterParamDefinition<TSource>> filterDefinitions = null)
        {
            var values = HttpUtility.ParseQueryString(request.RequestUri.Query);
            this.EnableFiltering(values, dynamicFiltering, filterDefinitions);
        }
        /// <summary>
        /// Adds a filter predicate for the specified query string key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="predicate"></param>
        /// <param name="hasSpecificValues">Set true if you want all specific values to be populated for client side initialization</param>
        public void AddFilterProperty<T>(String key, Func<T, Expression<Func<TSource, bool>>> predicate, bool hasSpecificValues = false)
        {
            if (this.filterHandler == null)
                throw new InvalidOperationException($"Filtering is not enabled. Enable filtering by calling {nameof(this.EnableFiltering)}");
            this.filterHandler.definitions[key] = new TypedFilterParamDefinition<TSource, T>(predicate, hasSpecificValues);
        }
        /// <summary>
        /// Populated ResultSet by executing filtering and paging on the provided IQueryable source data.
        /// </summary>
        /// <param name="sourceObjects"></param>
        public virtual void Populate(IQueryable<TSource> sourceObjects)
        {
            if (this.filterHandler != null)
                sourceObjects = filterHandler.Filter(sourceObjects);
            if (this.pageHandler != null)
            {
                int totalResults = sourceObjects.Count();
                this.pagedResults = this.pageHandler.GeneratePagingResultSummary(totalResults);
                sourceObjects = this.pageHandler.OrderAndPage(sourceObjects);
            }
            this.Instantiate(sourceObjects);
        }
        //Finalizes the query
        protected virtual void Instantiate(IQueryable<TSource> sourceObjects)
        {
            this.ResultSet = sourceObjects.ToArray();
        }
    }
    /// <summary>
    /// Extends APICollection by supporting automatic projection of source data into the TResult class type
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class APICollection<TSource, TResult> : APICollection<TSource> where TSource : class
    {
        public new IEnumerable<TResult> ResultSet { get; set; }
        private Expression<Func<TSource, TResult>> bindingExpr { get; set; }
        /// <summary>
        /// Creates and instance of APICollection with a specified binding expression to project source into result. 
        /// </summary>
        /// <param name="BindingExpr">Expression for projecting Source type into Result type</param>
        /// <param name="LoadInitValues">If true returns back to the client all grid initialization data such as drop down population data</param>
        public APICollection(Expression<Func<TSource, TResult>> BindingExpr,Boolean LoadInitValues=true)
            : base (LoadInitValues)
        {
            if (BindingExpr != null)
            this.bindingExpr = BindingExpr ;
        }
        protected override void Instantiate(IQueryable<TSource> sourceObjects)
        {
            this.ResultSet = sourceObjects.Select(bindingExpr).ToArray();
        }
    }

}