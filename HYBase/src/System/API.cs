using System;
using HYBase.Interpreter;
using HYBase.RecordManager;
using HYBase.IndexManager;
namespace HYBase.System
{
    public class API
    {
        DataBaseSystem system;
        RecordManager.FileScan fileScan;
        IndexManager.IndexScan indexScan;
        public void CreateTable(Interpreter.CreateTable command)
        {

            // 1 使用 system.catalogManager.TableExist 是否存在，存在则throw 异常
            // 2 删掉 table/<tableName> 文件
            // 3 使用 system.catalogManager.CreateTable 将表的信息写到catalog中
            // 4 使用 system.recoardManager.CreateFile 初始化记录文件

            throw new NotImplementedException();
        }
        public void CreateIndex(Interpreter.CreateIndex command)
        {

            // 1 使用 system.catalogManager.GetIndex 检查是否已经存在索引，存在则throw 异常
            // 2 删掉 index/<indexName> 文件
            // 3 使用 system.catalogManager.CreateIndex 创建新索引，返回值为indexNo
            // 4 使用 system.indexManager.CreateIndex 初始化索引文件

            throw new NotImplementedException();
        }
        public void DropIndex(Interpreter.DropIndex command)
        {
            // 1 使用 system.catalogManager.DropIndex
            // 2 删掉 index/<indexName> 文件
            throw new NotImplementedException();
        }
        public void DropTable(Interpreter.DropTable command)
        {
            // 1 使用 system.catalogManager.DropTable
            // 2 删掉 table/<tableName> 文件
            throw new NotImplementedException();
        }

        public void ExecFile(Interpreter.ExecFile command)
        {

            // 1 读取 command.FileName 的内容
            // 2 调用 system.interpreter.Exec
            throw new NotImplementedException();
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