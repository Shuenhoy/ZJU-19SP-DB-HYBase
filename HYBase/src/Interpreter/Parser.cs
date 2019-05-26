using System;
using System.Runtime.CompilerServices;
using LanguageExt;
using LanguageExt.Parsec;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;

[assembly: InternalsVisibleTo("test")]
namespace HYBase.Interpreter
{
    public static class InterpreterParser
    {
        internal static Parser<Unit> discard<T>(Parser<T> p)
            => from _ in p
               select unit;
        internal static Parser<Unit> spaces1 = skipMany1(space);
        internal static Parser<string> identifier =
                from x in letter
                from y in many(choice(letter, digit, ch('_')))
                select x + String.Join("", y);
        internal static Parser<string> charType =
                from _0 in str("char")
                from _1 in spaces
                from _2 in ch('(')
                from _3 in spaces
                from length in asString(many1(digit)).label("length of char")
                from _4 in spaces
                from _5 in ch(')')
                select $"char({length})";
        internal static Parser<string> type =
           choice(str("int"), str("float"), charType);

        internal static Parser<(string colName, string type, bool unique)> createTableColumns =
                from name in identifier.label("column name")
                from _0 in spaces1
                from t in type.label("column type")
                from unique in optional(chain(spaces1, discard(str("unique"))))
                from _2 in spaces
                select (name, t, unique.IsSome);
        internal static Parser<Command> createTable =
                from _0 in str("create")
                from _1 in spaces1
                from _2 in str("table")
                from _3 in spaces1
                from tableName in identifier.label("table name")
                from _4 in spaces
                from _5 in ch('(')
                from _51 in spaces
                from columns in endBy(attempt(createTableColumns), chain(discard(ch(',')), spaces))
                from _6 in spaces
                from _7 in str("primary")
                from _8 in spaces1
                from _9 in str("key")
                from _10 in spaces
                from _11 in ch('(')
                from _12 in spaces
                from primary in identifier.label("primary key name")
                from _13 in spaces
                from _14 in ch(')')
                select new CreateTable(tableName, columns.ToArr(), primary) as Command;
        internal static Parser<Command> dropTable =
                from _0 in str("drop")
                from _1 in spaces1
                from _2 in str("table")
                from _3 in spaces1
                from tableName in identifier.label("table name")
                select new DropTable(tableName) as Command;
        internal static Parser<Command> createIndex =
                from _0 in str("create")
                from _1 in spaces1
                from _2 in str("index")
                from _3 in spaces1
                from indexName in identifier.label("index name")
                from _4 in spaces1
                from _5 in str("on")
                from _6 in spaces1
                from tableName in identifier.label("table name")
                from _7 in spaces
                from _8 in ch('(')
                from columnName in identifier.label("columnName")
                from _9 in spaces
                from _10 in ch(')')
                select new CreateIndex(indexName, tableName, columnName) as Command;
    }
}