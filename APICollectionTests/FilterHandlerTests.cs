using LeanStack.Filtering;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using LeanStackTests.MockData;
using static LeanStackTests.MockData.MockSourceEntity;
using System.Linq;
using Xunit;
using System.Web;

namespace LeanStackTests
{
    public class FilterHandlerTests
    {
        private Dictionary<String, FilterParamDefinition<MockSourceEntity>> FilterProperties = new Dictionary<String, FilterParamDefinition<MockSourceEntity>>
        {
           {"name_contains",new TypedFilterParamDefinition<MockSourceEntity,String>(v=>{return t=>t.Name.Contains(v);} )},
           {"name_equals",new TypedFilterParamDefinition<MockSourceEntity,String>(v=>{return t=>t.Name==v;})},
           {"earlier_than_time",new TypedFilterParamDefinition<MockSourceEntity,DateTime>(v=>{return t=>t.Time<v;} )},
           {"after_equal_time",new TypedFilterParamDefinition<MockSourceEntity,DateTime>(v=>{return t=>t.Time>=v;} )},
           {"enum_value",new TypedFilterParamDefinition<MockSourceEntity,EnumProperty>(v=>{return t=>t.EnumProp==v;} )},
           {"id", new TypedFilterParamDefinition<MockSourceEntity,Int32>(v=>{return t=>t.Id==v;})},
           {"less_than_half_dec", new TypedFilterParamDefinition<MockSourceEntity,Int32>(v=>{return t=>v<t.DecValue/2;})},
           {"customEnumCheck", new TypedFilterParamDefinition<MockSourceEntity,EnumProperty>(v=>{
               switch(v)
               {
                   case EnumProperty.fifth:
                   case EnumProperty.fourth:
                       return t => t.EnumProp >= EnumProperty.fourth;
                   case EnumProperty.first:
                   case EnumProperty.second:
                   default:
                       return t=>t.EnumProp <= EnumProperty.third;
               }
           })}
        };
        [Fact]
        public void TestCorrectFiltering()
        {
            //Arrange
            var values = HttpUtility.ParseQueryString("id=2&customEnumCheck=first");
            FilteringHandler<MockSourceEntity> filterHandler = new FilteringHandler<MockSourceEntity>(values, FilterProperties, false);
            //Act
            var filtered=filterHandler.Filter(MockData.MockCollections.SourceEntities.AsQueryable()).ToArray();
            //Assert
            Assert.DoesNotContain(filtered, t => t.Id != 2);
            Assert.Contains(filtered, t => t.Id == 2);
        }
        [Theory]
        [InlineData("id=2&customEnumCheck=first", 1)]
        [InlineData("Id=2", 7)]// Filter definitions are case sensitive, filter will be ignored
        [InlineData("id=5&customEnumCheck=first", 0)]
        [InlineData("customEnumCheck=fourth", 2)]
        [InlineData("name_contains=e", 4)]
        [InlineData("earlier_than_time=2017/11/07", 2)]
        [InlineData("after_equal_time=2017/11/07", 5)]
        [InlineData("notDefinedPropertyIgnored=aasd", 7)]
        [InlineData("id=1&id=2&id=1", 2)]
        [InlineData("id=1&name_equals=One&name_equals=Two", 1)]
        [InlineData("name_equals=One&name_equals=Two&name_equals=Six", 3)]
        public void TestFilteringResultsNumber(String filter,int expectedToPass)
        {
            //Arrange
            var values = HttpUtility.ParseQueryString(filter);
            FilteringHandler<MockSourceEntity> filterHandler = new FilteringHandler<MockSourceEntity>(values, FilterProperties, false);
            //Act
            var filtered = filterHandler.Filter(MockData.MockCollections.SourceEntities.AsQueryable()).ToArray();
            //Assert
            Assert.True(filtered.Count() == expectedToPass,$"Passed: {filtered.Count()}");
        }

        [Theory]
        [InlineData("Id=2", 1)]
        [InlineData("EnumProp=first", 2)]
        [InlineData("EnumProp=first&EnumProp=third", 3)]
        [InlineData("Name=F", 2)]
        [InlineData("DecValue=1.1", 1)]
        [InlineData("DecValue=-1.1", 1)]
        [InlineData("DecValue=-1.1&DecValue=1.1", 2)]
        [InlineData("SingValue=-3.34", 1)]
        [InlineData("Time=2017-11-07", 1)]
        public void TestDynamicFilteringResultsNumber(String filter, int expectedToPass)
        {
            //Arrange
            var values = HttpUtility.ParseQueryString(filter);
            FilteringHandler<MockSourceEntity> filterHandler = new FilteringHandler<MockSourceEntity>(values, null, true);
            //Act
            var filtered = filterHandler.Filter(MockCollections.SourceEntities.AsQueryable()).ToArray();
            //Assert
            Assert.True(filtered.Count() == expectedToPass,$"Passed: {filtered.Count()}");
        }
        //String comparison testing
        //Filter Initial values population should be tested also.


    }
}
