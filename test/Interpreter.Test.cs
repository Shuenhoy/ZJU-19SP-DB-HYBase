using System;
using System.IO;
using Xunit;
using HYBase.Interpreter;
using static HYBase.Utils.Utils;
using System.Runtime.InteropServices;

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
    public class InterpreterTest
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
                )
            "

            ;
            var result = parse(InterpreterParser.createTable, creatTableStr);
            if (result.IsFaulted)
            {
                Assert.False(true, result.IsFaulted ? result.Reply.Error.ToString() : "");

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
            // Console.WriteLine(result2.Reply.Error.ToString());
        }
    }
}