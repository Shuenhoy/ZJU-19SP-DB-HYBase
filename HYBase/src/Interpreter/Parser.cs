using System;
using System.Runtime.CompilerServices;
using System.Linq;
using LanguageExt;
using LanguageExt.Parsec;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;

using HYBase.RecordManager;

[assembly: InternalsVisibleTo("test")]
namespace HYBase.Interpreter
{
    public static class InterpreterParser
    {
        internal static Parser<Unit> strD(string s)
            => discard(str(s));
        internal static Parser<Unit> discard<T>(Parser<T> p)
            => from _ in p
               select unit;
        internal static Parser<Unit> keywords(params string[] ps)
            => from _0 in chain(ps.Select(x => new[] { discard(str(x)), spaces1 }).SelectMany(identity).ToArray())
               select unit;
        internal static Parser<Unit> keywords0(params string[] ps)
            => from _0 in chain(ps.Select(x => new[] { discard(str(x)), spaces1 }).SelectMany(identity).SkipLast().ToArray())
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
                from _0 in keywords("create", "table")
                from tableName in identifier.label("table name")
                from _1 in spaces
                from _2 in ch('(')
                from _3 in spaces
                from columns in endBy(attempt(createTableColumns), chain(discard(ch(',')), spaces))
                from _4 in spaces
                from _5 in keywords0("primary", "key")
                from _6 in spaces
                from _7 in ch('(')
                from _8 in spaces
                from primary in identifier.label("primary key name")
                from _9 in spaces
                from _10 in ch(')')
                select new CreateTable(tableName, columns.ToArr(), primary) as Command;
        internal static Parser<Command> dropTable =
                from _0 in keywords("drop", "table")
                from tableName in identifier.label("table name")
                select new DropTable(tableName) as Command;

        internal static Parser<Command> createIndex =

                from _0 in keywords("create", "index")
                from indexName in identifier.label("index name")
                from _1 in spaces1
                from _2 in str("on")
                from _3 in spaces1
                from tableName in identifier.label("table name")
                from _4 in spaces
                from _5 in ch('(')
                from columnName in identifier.label("columnName")
                from _6 in spaces
                from _7 in ch(')')
                select new CreateIndex(indexName, tableName, columnName) as Command;

        internal static Parser<Command> dropIndex =
            from _0 in keywords("drop", "index")
            from indexName in identifier.label("index name")
            select new DropIndex(indexName) as Command;

        internal static Parser<CompOp> op =
            choice(
                from _ in ch('=') select CompOp.EQ,
                from _ in str(">=") select CompOp.GE,
                from _ in ch('>') select CompOp.GT,
                from _ in str("<=") select CompOp.LE,
                from _ in str("<") select CompOp.LT,
                from _ in str("<>") select CompOp.NE);
        internal static Parser<object> intLit =
            from d in asString(many1(digit))
            select Int32.Parse(d) as object;
        internal static Parser<object> floatLit =
            from d in asString(many1(digit))
            from _ in ch('.')
            from c in asString(many1(digit))
            select Single.Parse(d + '.' + c) as object;

        internal static Parser<object> strLit =
            from _0 in ch('\'')
            from x in asString(many(choice(noneOf("\'"), from _1 in ch('\'') select '\'')))
            from _1 in ch('\'')
            select x as object;

        internal static Parser<object> value =
            choice(
                strLit,
                intLit,
                floatLit
            );

        internal static Parser<Condition> cond =
            from column in identifier.label("column name")
            from _0 in spaces
            from op in op
            from _1 in spaces
            from value in value
            select new Condition(column, op, value);

        internal static Parser<Command> select =
            from _0 in keywords("select", "*", "from")
            from tableName in identifier.label("table name")
            from conditions in optional(
                from _1 in spaces1
                from _2 in keywords("where")
                from conds in many(from c in cond from _3 in spaces1 from _4 in keywords("and") select c)
                from condn in cond

                select conds.Add(condn)
            )
            select new Select(tableName, conditions.IsSome ? conditions.First().ToArr() : Arr.empty<Condition>()) as Command;

        internal static Parser<Command> insert =
            from _0 in keywords("insert", "into")
            from tableName in identifier.label("table name")
            from _1 in spaces1
            from _2 in keywords0("values")
            from _3 in ch('(')
            from values in many(from c in value from _3 in spaces1 from _4 in keywords(",") select c)
            from valuen in value

            from _31 in ch(')')

            select new Insert(tableName, values.Add(valuen).ToArr()) as Command;

        internal static Parser<Command> delete =
            from _0 in keywords("delete", "from")
            from tableName in identifier.label("table name")
            from conditions in optional(
                from _1 in spaces1
                from _2 in keywords("where")
                from conds in many(from c in cond from _3 in spaces1 from _4 in keywords("and") select c)
                from condn in cond

                select conds.Add(condn)
            )
            select new Delete(tableName, conditions.IsSome ? conditions.First().ToArr() : Arr.empty<Condition>()) as Command;

        internal static Parser<Command> quit =
            from _0 in str("quit")
            select new Quit() as Command;

        internal static Parser<Command> execfile =
            from _0 in keywords("execfile")
            from file in asString(many(noneOf(";")))
            select new ExecFile(file) as Command;
    }
}