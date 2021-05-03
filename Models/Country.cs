using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallengeAPI.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alpha2Code { get; set; }
        public string Alpha3Code { get; set; }
        public string Capital { get; set; }
        public string Region { get; set; }
        public string NativeName { get; set; }

    }
}
