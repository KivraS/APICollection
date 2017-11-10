using LeanStack.Paging;
using System;
using System.Collections.Generic;
using System.Text;
using LeanStackTests.MockData;
using Xunit;
using System.Linq;

namespace LeanStackTests
{
    public class PagingTests
    {
        private const int testDataCount = 500;
        private List<MockSourceEntity> PopulateList()
        {
            List<MockSourceEntity> list = new List<MockSourceEntity>();
            for (int i = 1; i <= testDataCount; i++)
            {
                list.Add(new MockSourceEntity
                {
                    Id = i,
                    Time = DateTime.Now.AddMinutes(-i),
                    Name = i.ToString()
                });
            }
            return list;
        }
        public enum IntegrityChecks
        {
            ActualResult,
            ResultSummary,
            ActualAndResultSummary
        }
        private PagingParams _pagingParams;
        private MockSourceEntity[] _results;
        private List<MockSourceEntity> _dataset;
        PagingHandler<MockSourceEntity> _handler;
        private void commonAct()
        {
            _handler = new PagingHandler<MockSourceEntity>(_pagingParams, null) { DynamicPaging = true };
            //Act
            _dataset = this.PopulateList();
            _results = _handler.OrderAndPage(_dataset.AsQueryable()).ToArray();
        }

        [Theory]
        [InlineData(1, 10, 1, 10, 10, IntegrityChecks.ActualResult)]
        [InlineData(2, 20, 21, 40, 20, IntegrityChecks.ActualResult)]
        [InlineData(4, 50, 151, 200, 50, IntegrityChecks.ActualResult)]
        [InlineData(1, 1000, 1, 500, 500, IntegrityChecks.ActualResult)]
        [InlineData(1, 10, 1, 10, 10, IntegrityChecks.ResultSummary)]
        [InlineData(2, 20, 21, 40, 20, IntegrityChecks.ResultSummary)]
        [InlineData(4, 50, 151, 200, 50, IntegrityChecks.ResultSummary)]
        [InlineData(1, 1000, 1, 500, 500, IntegrityChecks.ResultSummary)]
        [InlineData(1, 10, 1, 10, 10, IntegrityChecks.ActualAndResultSummary)]
        [InlineData(2, 20, 21, 40, 20, IntegrityChecks.ActualAndResultSummary)]
        [InlineData(4, 50, 151, 200, 50, IntegrityChecks.ActualAndResultSummary)]
        [InlineData(1, 1000, 1, 500, 500, IntegrityChecks.ActualAndResultSummary)]
        public void CheckResultPagingConsistency(int pageNum, int pageSize, int startItem, int endItem, int expectedLength, IntegrityChecks checkType)
        {
            //Arrange
            _pagingParams = new PagingParams
            {
                PageNumber = pageNum,
                PageSize = pageSize,
                SortBy = "Id"
            };
            commonAct();
            Dictionary<IntegrityChecks, Action> CheckTypes = new Dictionary<IntegrityChecks, Action>();
            ////Checks the first and last item of the returned list if they match page start - end
            CheckTypes.Add(IntegrityChecks.ActualResult, () =>
            {
                Assert.True(_results.Length == expectedLength);
                Assert.True(_results[0].Id == startItem);
                Assert.True(_results[expectedLength - 1].Id == endItem);
            });
            ////Checks if stated result summary results corespond to requested values
            CheckTypes.Add(IntegrityChecks.ResultSummary, () =>
            {
                PagingSummary pagingSummary = _handler.GeneratePagingResultSummary(_dataset.Count);
                Assert.True(pagingSummary.fromRecord == startItem);
                Assert.True(pagingSummary.toRecord == endItem);
            });
            ////Checks if both actual results and stated result summary values corespond to requested data.
            CheckTypes.Add(IntegrityChecks.ActualAndResultSummary, () =>
            {
                PagingSummary pagingSummary = _handler.GeneratePagingResultSummary(_dataset.Count);
                Assert.True(_results[0].Id == startItem && pagingSummary.fromRecord == startItem);
                Assert.True(_results[expectedLength - 1].Id == endItem && pagingSummary.toRecord == endItem);
            });
            CheckTypes[checkType].Invoke();
        }

        [Theory]
        [InlineData(1, 10, 50)]
        [InlineData(1, 10000, 1)]
        [InlineData(10, 1, 500)]
        public void CheckPageSpliting(int pageNum, int pageSize, int expectedPagecount)
        {
            //Arrange
            _pagingParams = new PagingParams
            {
                PageNumber = pageNum,
                PageSize = pageSize,
                SortBy = "Id"
            };
            //Act
            commonAct();
            PagingSummary pagingSummary = _handler.GeneratePagingResultSummary(_dataset.Count);
            //Assert
            Assert.True(pagingSummary.PagesCount == expectedPagecount);
        }
    }
}
