using System;
using HYBase.RecordManager;
using System.Linq;
using System.Collections.Generic;
using static HYBase.Utils.Utils;
using LanguageExt;

namespace HYBase.Interpreter
{
    abstract class Command { }
    class CreateTable : Command
    {
        public readonly string TableName;
        public readonly Arr<(string colName, string type, bool unique)> Columns;
        public readonly string Primary;
        public CreateTable(string tableName, Arr<(string colName, string type, bool unique)> columns, string primary)
            => (TableName, Columns, Primary) = (tableName, columns, primary);
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            CreateTable other = (CreateTable)obj;
            return TableName == other.TableName && Enumerable.SequenceEqual(Columns, other.Columns) && Primary == other.Primary;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableName.GetHashCode(), Columns.GetHashCode(), Primary.GetHashCode());
        }
    }
    class DropTable : Command
    {
        public readonly string TableName;
        public DropTable(string tableName)
            => TableName = tableName;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            DropTable other = (DropTable)obj;
            return TableName == other.TableName;
        }

        public override int GetHashCode()
        {
            return TableName.GetHashCode();
        }
    }
    class CreateIndex : Command
    {
        public readonly string IndexName;
        public readonly string TableName;
        public readonly string ColumnName;
        public CreateIndex(string indexName, string tableName, string columnName)
            => (IndexName, TableName, ColumnName) = (indexName, tableName, columnName);
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            CreateIndex other = (CreateIndex)obj;
            return TableName == other.TableName && IndexName == other.IndexName && ColumnName == other.ColumnName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableName.GetHashCode(), IndexName.GetHashCode(), ColumnName.GetHashCode());
        }
    }
    class DropIndex : Command
    {
        public readonly string IndexName;
        public DropIndex(string indexName)
            => IndexName = indexName;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            DropIndex other = (DropIndex)obj;
            return IndexName == other.IndexName;
        }

        public override int GetHashCode()
        {
            return IndexName.GetHashCode();
        }
    }
    class Condition
    {
        public readonly string ColumnName;
        public readonly CompOp Op;
        public readonly object Value;
        public Condition(string columnName, CompOp op, object value)
            => (ColumnName, Op, Value) = (columnName, op, value);
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Condition other = (Condition)obj;
            return ColumnName == other.ColumnName && Op == other.Op && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ColumnName.GetHashCode(), Op.GetHashCode(), Value.GetHashCode());
        }

    }
    class Select : Command
    {
        public readonly string TableName;
        public readonly Arr<Condition> Conditions;
        public Select(string tableName, Arr<Condition> conditions)
            => (TableName, Conditions) = (tableName, conditions);
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Select other = (Select)obj;
            return TableName == other.TableName && Enumerable.SequenceEqual(Conditions, other.Conditions);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableName.GetHashCode(), Conditions.GetHashCode());
        }
    }

    class Insert : Command
    {
        public readonly string TableName;
        public readonly Arr<object> Values;
        public Insert(string tableName, Arr<object> values)
            => (TableName, Values) = (tableName, values);
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Insert other = (Insert)obj;
            return TableName == other.TableName && Enumerable.SequenceEqual(Values, other.Values);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableName.GetHashCode(), Values.ToArr());
        }
    }

    class Delete : Command
    {
        public readonly string TableName;
        public readonly Arr<Condition> Conditions;
        public Delete(string tableName, Arr<Condition> conditions)
            => (TableName, Conditions) = (tableName, conditions);

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Delete other = (Delete)obj;
            return TableName == other.TableName && Enumerable.SequenceEqual(Conditions, other.Conditions);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TableName.GetHashCode(), Conditions.GetHashCode());
        }
    }


    class Quit : Command
    {
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return 233.GetHashCode();
        }
    }

    class ExecFile : Command
    {
        public readonly string FileName;

        public ExecFile(string fileName)
            => FileName = fileName;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            ExecFile other = (ExecFile)obj;
            return FileName == other.FileName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileName.GetHashCode());
        }
    }
}