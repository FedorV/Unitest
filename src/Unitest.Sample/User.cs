using System.Collections.Generic;

namespace Unitest.Sample
{
    public class User
    {
        public string Name { get; set; }
        public IEnumerable<string> AccountNumbers { get; set; }
    }
}