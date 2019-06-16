using System;
using System.IO;
using Xunit;
using HYBase.Interpreter;
using HYBase.RecordManager;
using static HYBase.Utils.Utils;
using System.Runtime.InteropServices;
using System.Text;
//
using LanguageExt;
using LanguageExt.Parsec;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;

namespace HYBase.UnitTests
{
    public class InterpreterParserTest
    {
        [Fact]
        void IdentifierTest()
        {
            Assert.Equal("test", parse(InterpreterParser.identifier, "test").Reply.Result);
            Assert.True(parse(InterpreterParser.identifier, "1test").IsFaulted);
            Assert.Equal("tes_t1", parse(InterpreterParser.identifier, "tes_t1").Reply.Result);
            Assert.Equal("test", parse(InterpreterParser.identifier, "test 234").Reply.Result);
        }

        [Fact]
        void CreateTableTest()
        {
            var creatTableStr = @"create table student(
                    sno char(8),
                    sname char(16) unique ,
                    sage int, 
                    sgendar char (1),
                    primary  key  ( sno  )
                );
            "

            ;
            var result = parse(InterpreterParser.createTable, creatTableStr);
            if (result.IsFaulted)
            {
                Assert.False(true, result.Reply.Error.ToString());

            }
            var real = new CreateTable("student", Seq(
                ("sno", "char(8)", false),
                ("sname", "char(16)", true),
                ("sage", "int", false),
                ("sgendar", "char(1)", false)
            ).ToArr(), "sno");
            Assert.Equal(real, result.Reply.Result);
            var result2 = parse(InterpreterParser.createTable, "create table 1table1_Name");
            Assert.True(result2.IsFaulted, result2.Reply.Error.ToString());
        }
        void Faulted<T>(ParserResult<T> p)
        {
            if (p.IsFaulted)
            {
                Assert.False(true, p.Reply.Error.ToString());
            }
        }

        [Fact]
        void DropTableTest()
        {
            var dropTableStr = @"drop table student;";
            var result = parse(InterpreterParser.dropTable, dropTableStr);
            Faulted(result);
            var real = new DropTable("student");
            Assert.Equal(real, result.Reply.Result);
        }

        [Fact]
        void CreateIndexTest()
        {
            var str = @"create index   stunameidx on student ( sname);";
            var result = parse(InterpreterParser.createIndex, str);
            Faulted(result);
            var real = new CreateIndex("stunameidx", "student", "sname");
            Assert.Equal(real, result.Reply.Result);
        }
        [Fact]
        void DropIndexTest()
        {
            var dropTableStr = @"drop index stunameidx;";
            var result = parse(InterpreterParser.dropIndex, dropTableStr);
            Faulted(result);
            var real = new DropIndex("stunameidx");
            Assert.Equal(real, result.Reply.Result);
        }
        [Fact]
        void SelectTest()
        {
            var str1 = @"select * from student  ;";
            var result1 = parse(InterpreterParser.selects, str1);
            Faulted(result1);
            var real1 = new Select("student", Arr<Condition>.Empty);
            Assert.Equal(real1, result1.Reply.Result);

            var str2 = @"select * from student where sno = '88\'11'  ;";
            var result2 = parse(InterpreterParser.selects, str2);
            Faulted(result2);
            var real2 = new Select("student", new[] { new Condition("sno", CompOp.EQ, (Encoding.UTF8.GetBytes("88'11"), AttrType.String)) });
            Assert.Equal(real2, result2.Reply.Result);

            var str3 = @"select * from student where sage > 20  and sgender = 'F';";
            var result3 = parse(InterpreterParser.selects, str3);
            Faulted(result3);
            var real3 = new Select("student", new[] { new Condition("sage", CompOp.GT,
                (BitConverter.GetBytes(20), AttrType.Int)),
                new Condition("sgender", CompOp.EQ, (Encoding.UTF8.GetBytes("F"),AttrType.String) ) });
            Assert.Equal(real3, result3.Reply.Result);

        }

        [Fact]
        void InsertTest()
        {
            var str1 = @"insert  into  student values('12345678','wy',22,'M');";
            var result1 = parse(InterpreterParser.insert, str1);
            Faulted(result1);
            var real1 = new Insert("student", new (byte[], AttrType)[] {
                 (Encoding.UTF8.GetBytes("12345678"),AttrType.String),
                 (Encoding.UTF8.GetBytes("wy"),AttrType.String),
                 (BitConverter.GetBytes(22),AttrType.Int),
                 (Encoding.UTF8.GetBytes("M"),AttrType.String) });
            Assert.Equal(real1, result1.Reply.Result);
        }

        [Fact]
        void DeleteTest()
        {
            var str1 = @"delete from student;";
            var result1 = parse(InterpreterParser.delete, str1);
            Faulted(result1);
            var real1 = new Delete("student", Arr<Condition>.Empty);
            Assert.Equal(real1, result1.Reply.Result);

            var str2 = @"delete  from student where sno = '88\'11';";
            var result2 = parse(InterpreterParser.delete, str2);
            Faulted(result2);
            var real2 = new Delete("student", new[] { new Condition("sno", CompOp.EQ, (Encoding.UTF8.GetBytes("88'11"), AttrType.String)) });
            Assert.Equal(real2, result2.Reply.Result);

            var str3 = @"delete from student where sage > 20  and sgender = 'F' ;";
            var result3 = parse(InterpreterParser.delete, str3);
            Faulted(result3);
            var real3 = new Delete("student", new[] { new Condition("sage", CompOp.GT, (BitConverter.GetBytes(20), AttrType.Int)),
            new Condition("sgender", CompOp.EQ, (Encoding.UTF8.GetBytes("F"),AttrType.String)) });
            Assert.Equal(real3, result3.Reply.Result);

        }

        [Fact]
        void QuitTest()
        {
            var str1 = @"quit;";
            var result1 = parse(InterpreterParser.quit, str1);
            Faulted(result1);
            var real1 = new Quit();
            Assert.Equal(real1, result1.Reply.Result);
        }

        [Fact]
        void ExecTest()
        {
            var str1 = @"execfile a/a.sql;";
            var result1 = parse(InterpreterParser.execfile, str1);
            Faulted(result1);
            var real1 = new ExecFile("a/a.sql");
            Assert.Equal(real1, result1.Reply.Result);
        }

    }
}