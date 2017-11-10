using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeanStack.Filtering
{
    public class SupportedFilterProperty
    {
        public String Key { get; set; }
        public String Type { get; set; }
        public IEnumerable<KeyValuePair<String, String>> SupportedValues { get; set; }
    }
}
