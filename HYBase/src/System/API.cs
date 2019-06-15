using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HYBase.Interpreter;
using HYBase.RecordManager;
using HYBase.CatalogManager;
using LanguageExt;
using static LanguageExt.Prelude;
using HYBase.IndexManager;
namespace HYBase.System
{
    public class API
    {
        DataBaseSystem system;
        RecordManager.FileScan fileScan;
        IndexManager.IndexScan indexScan;

        Dictionary<string, RecordFile> tables;
        Dictionary<string, Index> indexs;

        static (AttrType, int) GetType(string type)
        {
            switch (type)
            {
                case "int": return (AttrType.Int, 4);
                case "float": return (AttrType.Float, 4);
                default:
                    if (type.StartsWith("char(") && type.EndsWith(")"))
                    {
                        return (AttrType.String, Int32.Parse(type.Substring(5, type.Length - 6)));
                    }
                    else
                    {
                        throw new Exception($"no such type <{type}>!");
                    }
            }
        }
        public void CreateTable(Interpreter.CreateTable command)
        {
            if (system.catalogManager.TableExist(command.TableName))
            {
                throw new Exception("table already exists!");
            }

            File.Delete($"table/{command.TableName}");
            var attributes = command.Columns.Select((x) =>
                          {
                              var (name, type, _) = x;
                              var (t, l) = GetType(type);
                              return new AttributeInfo(t, name, l);
                          }).ToArray();
            system.catalogManager.CreateTable(command.TableName, attributes);

            tables[command.TableName] = system.recoardManager.CreateFile(
                new FileStream($"table/{command.TableName}", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                    attributes.Aggregate(0, (x, y) => x + y.AttributeLength)
                );

            // 1 使用 system.catalogManager.TableExist 是否存在，存在则throw 异常
            // 2 删掉 table/<tableName> 文件
            // 3 使用 system.catalogManager.CreateTable 将表的信息写到catalog中
            // 4 使用 system.recoardManager.CreateFile 初始化记录文件

        }
        public void CreateIndex(Interpreter.CreateIndex command)
        {
            var attr = system.catalogManager.GetColumn(command.TableName, command.ColumnName);

            if (attr == null)
            {
                throw new Exception("no such column!");
            }
            if (attr?.indexNo != -1)
            {
                throw new Exception("the column already has index!");

            }
            if (system.catalogManager.IndexExist(command.IndexName))
            {
                throw new Exception("the index already exists!");
            }
            File.Delete($"index/{command.IndexName}");
            system.catalogManager.CreateIndex(command.TableName, command.ColumnName, command.IndexName);

            indexs[command.IndexName] = system.indexManager.CreateIndex(
                new FileStream($"index/{command.IndexName}", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                attr.Value.attributeType,
                attr.Value.attributeLength
            );

            // 1 使用 system.catalogManager.GetIndex 检查是否已经存在索引，存在则throw 异常
            // 2 删掉 index/<indexName> 文件
            // 3 使用 system.catalogManager.CreateIndex 创建新索引，返回值为indexNo
            // 4 使用 system.indexManager.CreateIndex 初始化索引文件
        }
        public void DropIndex(Interpreter.DropIndex command)
        {
            if (!system.catalogManager.IndexExist(command.IndexName))
            {
                throw new Exception("no such index!");
            }
            system.catalogManager.DropIndex(command.IndexName);
            var index = indexs[command.IndexName];
            if (index != null)
            {
                index.Close();
            }
            File.Delete($"index/{command.IndexName}");
            // 1 使用 system.catalogManager.DropIndex
            // 2 删掉 index/<indexName> 文件
        }
        public void DropTable(Interpreter.DropTable command)
        {
            if (!system.catalogManager.TableExist(command.TableName))
            {
                throw new Exception("no such table!");
            }
            system.catalogManager.DropTable(command.TableName);
            var table = tables[command.TableName];
            if (table != null)
            {
                table.Close();
            }
            File.Delete($"table/{command.TableName}");
            // 1 使用 system.catalogManager.DropTable
            // 2 删掉 table/<tableName> 文件
        }

        public void ExecFile(Interpreter.ExecFile command)
        {
            var text = File.ReadAllText(command.FileName);
            system.interpreter.Exec(text);
            // 1 读取 command.FileName 的内容
            // 2 调用 system.interpreter.Exec
        }
        public void Select(Interpreter.Select command)
        {

            // 1 检查表 command.TableName 是否存在，不存在抛异常

            // 2 system.catalogManager.GetAttributes 获取列信息
            // 3 检查条件中每一列 command.Conditions[i].ColumnName 是否存在 不存在抛异常
            // 4 检查条件中每一列，是否有存在索引的，如果有则使用indexScan来以这一列为主要条件查询，否则用fileScan
            // 4.5 其他条件，则在fileScan或者indexScan的结果上暴力查询
            throw new NotImplementedException();
        }
        public void Quit(Interpreter.Quit command)
        {
            Environment.Exit(0);
        }
        public void Delete(Interpreter.Delete command)
        {
            // 1 检查表 command.TableName 是否存在，不存在抛异常

            // 2 system.catalogManager.GetAttributes 获取列信息
            // 3 检查条件中每一列 command.Conditions[i].ColumnName 是否存在 不存在抛异常
            // 4 检查条件中每一列，是否有存在索引的，如果有则使用indexScan来以这一列为主要条件查询，否则用fileScan
            // 4.5 其他条件，则在fileScan或者indexScan的结果上暴力查询
            // 6 找到后，删除，需要注意，若有多个index，需要对每个index都执行删除操作

            throw new NotImplementedException();
        }


    }
}