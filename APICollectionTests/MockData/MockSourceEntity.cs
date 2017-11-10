using System;
using System.Collections.Generic;
using System.Text;

namespace LeanStackTests.MockData
{
    public class MockSourceEntity
    {
        public enum EnumProperty
        {
            first,
            second,
            third,
            fourth,
            fifth
        }
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public DateTime Time { get; set; }
        public Decimal DecValue { get; set; }
        public Single SingValue { get; set; }
        public EnumProperty EnumProp { get; set; }
        
    }
}
