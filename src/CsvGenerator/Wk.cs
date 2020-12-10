using System;
using System.Runtime.CompilerServices;
using Sprache;

[assembly: InternalsVisibleToAttribute("Tests")]

namespace CsvGenerator {
    internal static class Wk {
        internal static Parser<string> Identifier =>
                from leadding in Parse.WhiteSpace.Many<char>()
                from first in Parse.Letter.Once().Text()
                from rest in Parse.LetterOrDigit.Many().Text()
                from trailing in Parse.WhiteSpace.Many()
                select first + rest;
    }
}
