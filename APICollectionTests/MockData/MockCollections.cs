using System;
using System.Collections.Generic;
using System.Text;
using static LeanStackTests.MockData.MockSourceEntity;

namespace LeanStackTests.MockData
{
    public static class MockCollections
    {
        public static DateTime RefTime { get; set; }
        public static List<MockSourceEntity> SourceEntities { get; set; }
        static MockCollections()
        {
            RefTime = DateTime.Parse("2017-11-07T13:00:00.000Z");
            SourceEntities =new List<MockSourceEntity>
            {
                new MockSourceEntity{Id =1,Name="One",Time= RefTime.AddMinutes(-1),DecValue= 1.1M,SingValue=3.34f,EnumProp= EnumProperty.first},
                new MockSourceEntity{Id =2,Name="Two",Time= RefTime.AddHours(-40),DecValue= -1.1M,SingValue=-3.34f,EnumProp= EnumProperty.second},
                new MockSourceEntity{Id =3,Name="Three",Time= RefTime.AddMinutes(200),DecValue= 200M,SingValue=3f,EnumProp=EnumProperty.third},
                new MockSourceEntity{Id =4,Name="Four",Time= RefTime.AddYears(2),DecValue= -000.1M,SingValue=1500f,EnumProp=EnumProperty.fourth},
                new MockSourceEntity{Id =5,Name="Five",Time= RefTime.AddYears(-1666),DecValue=1M,SingValue=-1500f,EnumProp=EnumProperty.fifth},
                new MockSourceEntity{Id =6,Name="Six",Time= RefTime.AddMilliseconds(1),DecValue=1M,SingValue=0.0001f,EnumProp=EnumProperty.first},
                new MockSourceEntity{Id =7,Name="Seven",Time= RefTime.Date,DecValue=0.4M,SingValue=0.0001f,EnumProp=EnumProperty.second},
            };
        }
    }
}
