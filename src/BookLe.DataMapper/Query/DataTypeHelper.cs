using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BookLe.DataMapper.Query
{
    internal static class DataTypeHelper
    {
        public static DbType MapDataType(DataTypeEnum dataType)
        {
            switch (dataType)
            {
                case DataTypeEnum.AnsiString: return DbType.AnsiString;
                case DataTypeEnum.AnsiStringFixedLength: return DbType.AnsiStringFixedLength;
                case DataTypeEnum.Binary: return DbType.Binary;
                case DataTypeEnum.Boolean: return DbType.Boolean;
                case DataTypeEnum.Byte: return DbType.Byte;
                case DataTypeEnum.Currency: return DbType.Currency;
                case DataTypeEnum.Date: return DbType.Date;
                case DataTypeEnum.DateTime: return DbType.DateTime;
                case DataTypeEnum.DateTime2: return DbType.DateTime2;
                case DataTypeEnum.DateTimeOffset: return DbType.DateTimeOffset;
                case DataTypeEnum.Decimal: return DbType.Decimal;
                case DataTypeEnum.Double: return DbType.Double;
                case DataTypeEnum.Guid: return DbType.Guid;
                case DataTypeEnum.Int16: return DbType.Int16;
                case DataTypeEnum.Int32: return DbType.Int32;
                case DataTypeEnum.Int64: return DbType.Int64;
                case DataTypeEnum.Object: return DbType.Object;
                case DataTypeEnum.SByte: return DbType.SByte;
                case DataTypeEnum.Single: return DbType.Single;
                case DataTypeEnum.String: return DbType.String;
                case DataTypeEnum.StringFixedLength: return DbType.StringFixedLength;
                case DataTypeEnum.Time: return DbType.Time;
                case DataTypeEnum.UInt16: return DbType.UInt16;
                case DataTypeEnum.UInt32: return DbType.UInt32;
                case DataTypeEnum.UInt64: return DbType.UInt64;
                case DataTypeEnum.VarNumeric: return DbType.VarNumeric;
                case DataTypeEnum.Xml: return DbType.Xml;
                case DataTypeEnum.VarChar: return DbType.AnsiString;
                case DataTypeEnum.Char: return DbType.AnsiStringFixedLength;
            }
            return DbType.Object;
        }

        public static ParameterDirection MapDirection(QueryParameterDirectionEnum direction)
        {
            switch (direction)
            {
                case QueryParameterDirectionEnum.Input: return ParameterDirection.Input;
                case QueryParameterDirectionEnum.Output: return ParameterDirection.Output;
                case QueryParameterDirectionEnum.InputOutput: return ParameterDirection.InputOutput;
                case QueryParameterDirectionEnum.ReturnValue: return ParameterDirection.ReturnValue;
            }
            return ParameterDirection.Input;
        }
    }

}
