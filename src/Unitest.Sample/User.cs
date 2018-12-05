using System.Collections.Generic;

namespace HockyTest.Sample
{
    public class User
    {
        public string Name { get; set; }
        public IEnumerable<string> AccountNumbers { get; set; }
    }
}