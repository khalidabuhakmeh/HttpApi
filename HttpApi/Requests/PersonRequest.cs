using System;
using System.Collections;
using System.Collections.Generic;

namespace HttpApi.Requests
{
    public class PersonRequest
    {
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public IEnumerable<int> Count { get; set; }
    }
}