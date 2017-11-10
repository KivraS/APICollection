using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using LeanStack.Paging;
using System.Linq.Expressions;
using LeanStackTests.MockData;
using System.Linq;

namespace LeanStackTests
{
    public class SortTests
    {
        public PagingHandler<MockSourceEntity> InitializeSortHandler(String orderBy,bool desc,bool dynamicSortProperties)
        {
            var sortDefinitions = new Dictionary<String, Expression<Func<MockSourceEntity, Object>>>()
            {
                {PagingHandler<MockSourceEntity>.DefaultSortKey, o=>o.Id },
                {"Name",o=>o.Name},
                {"Time",o=>o.Time },
                {"Complex",o=>Math.Pow(o.SingValue,2)}
            };
            var pageParams = new PagingParams
            {
                Desc = desc,
                PageSize = 10,
                PageNumber = 1,
                SortBy = orderBy
            };
            if(dynamicSortProperties==false)
                return new PagingHandler<MockSourceEntity>(pageParams, sortDefinitions);
            else
                return new PagingHandler<MockSourceEntity>(pageParams, null) { DynamicPaging = true };
        }
        [Theory]
        [InlineData("Default")]
        [InlineData(null)]
        [InlineData("")]
        public void OrderByDefaultDescending(String defSort)
        {
            //Arrange
            var pagingHandler = this.InitializeSortHandler(orderBy:defSort, desc:true, dynamicSortProperties: false);
            //Act
            var resultset = pagingHandler.OrderAndPage(MockCollections.SourceEntities.AsQueryable());
            //Assert
            Assert.True(resultset.SequenceEqual(MockCollections.SourceEntities.OrderByDescending(t => t.Id)));
        }
        [Fact]
        public void OrderByTime()
        {
            //Arrange
            var pagingHandler = this.InitializeSortHandler(orderBy: "Time", desc:false, dynamicSortProperties: false);
            //Act
            var resultset = pagingHandler.OrderAndPage(MockCollections.SourceEntities.AsQueryable());
            //Assert
            Assert.True(resultset.SequenceEqual(MockCollections.SourceEntities.OrderBy(t => t.Time)));
        }
        [Fact]
        public void OrderByComplex()
        {
            //Arrange
            var pagingHandler = this.InitializeSortHandler(orderBy: "Complex", desc:true, dynamicSortProperties: false);
            //Act
            var resultset = pagingHandler.OrderAndPage(MockCollections.SourceEntities.AsQueryable());
            //Assert
            Assert.True(resultset.SequenceEqual(MockCollections.SourceEntities.OrderByDescending(t => t.SingValue*t.SingValue)));
        }
        [Fact]
        public void DynamicOrderProperty()
        {
            //Arrange
            var pagingHandler = this.InitializeSortHandler(orderBy: "SingValue", desc:true, dynamicSortProperties: true);
            //Act
            var resultset = pagingHandler.OrderAndPage(MockCollections.SourceEntities.AsQueryable());
            //Assert
            Assert.True(resultset.SequenceEqual(MockCollections.SourceEntities.OrderByDescending(t => t.SingValue)));
        }
        [Theory]
        [InlineData(true,"aPropertyThatDoesNotExist")]
        [InlineData(false, "SingValue")]
        [InlineData(true, "default")]
        public void SortByNotFoundProperty(bool dynamicSort,String property)
        {
            //Arrange
            var pagingHandler = this.InitializeSortHandler(orderBy: property, desc: true, dynamicSortProperties: dynamicSort);
            //Act
            Action resultset = ()=>pagingHandler.OrderAndPage(MockCollections.SourceEntities.AsQueryable());
            //Assert
            Assert.Throws<ArgumentException>(resultset);
        }


    }
}
