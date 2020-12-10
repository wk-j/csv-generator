using System;
using System.Collections.Generic;
using Sprache;
using System.Linq;

namespace CsvGenerator {
    internal static class CsvParser {
        private static readonly Parser<char> CellSeparator = Parse.Char(',');

        private static readonly Parser<char> QuotedCellDelimiter = Parse.Char('"');

        private static readonly Parser<char> QuoteEscape = Parse.Char('"');

        static Parser<T> Escaped<T>(Parser<T> following) {
            return from escape in QuoteEscape
                   from f in following
                   select f;
        }

        private static readonly Parser<char> QuotedCellContent =
            Parse.AnyChar.Except(QuotedCellDelimiter).Or(Escaped(QuotedCellDelimiter));

        private static readonly Parser<char> LiteralCellContent =
            Parse.AnyChar.Except(CellSeparator).Except(Parse.String(Environment.NewLine));

        private static readonly Parser<string> QuotedCell =
            from open in QuotedCellDelimiter
            from content in QuotedCellContent.Many().Text()
            from end in QuotedCellDelimiter
            select content;

        private static readonly Parser<string> NewLine =
            Parse.String(Environment.NewLine).Text();

        private static readonly Parser<string> RecordTerminator =
            Parse.Return("").End().XOr(
            NewLine.End()).Or(
            NewLine);

        private static readonly Parser<string> Cell =
            QuotedCell.XOr(
            LiteralCellContent.XMany().Text());

        private static readonly Parser<IEnumerable<string>> Record =
            from leading in Cell
            from rest in CellSeparator.Then(_ => Cell).Many()
            from terminator in RecordTerminator
            select Cons(leading, rest);

        static IEnumerable<T> Cons<T>(T head, IEnumerable<T> rest) {
            yield return head;
            foreach (var item in rest)
                yield return item;
        }

        private static string ToTile(string input) {
            var token = input.Split(' ').Select(x => x.Substring(0, 1).ToUpper() + x.Substring(1));
            return string.Concat(token).Trim();
        }

        public static IEnumerable<string> Header(string csv) {
            var head = Record.XMany().End().Parse(csv).First().Select(ToTile);
            return head;
        }

        public static Parser<IEnumerable<IEnumerable<string>>> Csv =>
            Record.XMany().End();
    }
}