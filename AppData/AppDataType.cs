// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using Windows.Foundation;
using Windows.Storage;

namespace AppData
{
    public class AppDataType
    {
        public enum Type
        {
            Unknown = PropertyType.OtherType,
            Empty = PropertyType.Empty,
            UInt8 = PropertyType.UInt8,
            Int16 = PropertyType.Int16,
            UInt16 = PropertyType.UInt16,
            Int32 = PropertyType.Int32,
            UInt32 = PropertyType.UInt32,
            Int64 = PropertyType.Int64,
            UInt64 = PropertyType.UInt64,
            Single = PropertyType.Single,
            Double = PropertyType.Double,
            Char16 = PropertyType.Char16,
            Boolean = PropertyType.Boolean,
            String = PropertyType.String,
            DateTime = PropertyType.DateTime,
            TimeSpan = PropertyType.TimeSpan,
            Guid = PropertyType.Guid,
            Point = PropertyType.Point,
            Size = PropertyType.Size,
            Rect = PropertyType.Rect,
            ApplicationDataCompositeValue = PropertyType.Inspectable,
            UInt8Array = PropertyType.UInt8Array,
            Int16Array = PropertyType.Int16Array,
            UInt16Array = PropertyType.UInt16Array,
            Int32Array = PropertyType.Int32Array,
            UInt32Array = PropertyType.UInt32Array,
            Int64Array = PropertyType.Int64Array,
            UInt64Array = PropertyType.UInt64Array,
            SingleArray = PropertyType.SingleArray,
            DoubleArray = PropertyType.DoubleArray,
            Char16Array = PropertyType.Char16Array,
            BooleanArray = PropertyType.BooleanArray,
            StringArray = PropertyType.StringArray,
            DateTimeArray = PropertyType.DateTimeArray,
            TimeSpanArray = PropertyType.TimeSpanArray,
            GuidArray = PropertyType.GuidArray,
            PointArray = PropertyType.PointArray,
            SizeArray = PropertyType.SizeArray,
            RectArray = PropertyType.RectArray
        }

        public static string ToString(PropertyType pt)
        {
            return pt.ToString().Replace("Array", "[]").ToUpper();
        }

        public static string ToString(Type t)
        {
            return t.ToString().Replace("Array", "[]").ToUpper();
        }

        public static object Parse(AppDataType.Type type, string s)
        {
            object obj = null;
            switch (type)
            {
                case AppDataType.Type.Empty: obj = PropertyValue.CreateEmpty(); ; break;
                case AppDataType.Type.UInt8: obj = PropertyValue.CreateUInt8(s.IsEmpty() ? (byte)0 : Byte.Parse(s)); break;
                case AppDataType.Type.Int16: obj = PropertyValue.CreateInt16(s.IsEmpty() ? (short)0 : Int16.Parse(s)); break;
                case AppDataType.Type.UInt16: obj = PropertyValue.CreateUInt16(s.IsEmpty() ? (ushort)0 : UInt16.Parse(s)); break;
                case AppDataType.Type.Int32: obj = PropertyValue.CreateInt32(s.IsEmpty() ? 0 : Int32.Parse(s)); break;
                case AppDataType.Type.UInt32: obj = PropertyValue.CreateUInt32(s.IsEmpty() ? 0 : UInt32.Parse(s)); break;
                case AppDataType.Type.Int64: obj = PropertyValue.CreateInt64(s.IsEmpty() ? 0 : Int64.Parse(s)); break;
                case AppDataType.Type.UInt64: obj = PropertyValue.CreateUInt64(s.IsEmpty() ? 0 : UInt64.Parse(s)); break;
                case AppDataType.Type.Single: obj = PropertyValue.CreateSingle(s.IsEmpty() ? 0 : Single.Parse(s)); break;
                case AppDataType.Type.Double: obj = PropertyValue.CreateDouble(s.IsEmpty() ? 0 : Double.Parse(s)); break;
                case AppDataType.Type.Char16: obj = PropertyValue.CreateChar16(Char.Parse(s)); break;
                case AppDataType.Type.Boolean: obj = PropertyValue.CreateBoolean(s.IsEmpty() ? false : Boolean.Parse(s)); break;
                case AppDataType.Type.String: obj = s; break;
                default: break;
            }
            return obj;
        }

        public static AppDataType.Type ToAppDataType(string s)
        {
            if ("Empty".Equals(s, StringComparison.InvariantCultureIgnoreCase))
                return AppDataType.Type.Empty;

            foreach (var tx in AppDataType.typeXrefTable)
            {
                if (tx.appdataTypeString.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                    return tx.appdataType;
            }
            throw new NotSupportedException(String.Format("Unknown type ({0})", s));
        }

        public static AppDataType.Type ToAppDataType(string s, AppDataType.Type[] allowedTypes)
        {
            AppDataType.Type appdataType = ToAppDataType(s);
            if (Array.Exists(allowedTypes, t => t == appdataType))
                return appdataType;
            throw new NotSupportedException(String.Format("Unknown type ({0})", s));
        }

        internal class TypeXref
        {
            public System.Type objectType { get; private set; }
            public AppDataType.Type appdataType { get; private set; }
            public string appdataTypeString { get; private set; }

            public TypeXref(System.Type objectType, AppDataType.Type appdataType)
            {
                this.objectType = objectType;
                this.appdataType = appdataType;
                this.appdataTypeString = AppDataType.ToString(appdataType);
            }
        };
        internal static TypeXref[] typeXrefTable = new TypeXref[]{
            new TypeXref(typeof(byte), AppDataType.Type.UInt8),
            new TypeXref(typeof(Int16), AppDataType.Type.Int16),
            new TypeXref(typeof(UInt16), AppDataType.Type.UInt16),
            new TypeXref(typeof(Int32), AppDataType.Type.Int32),
            new TypeXref(typeof(UInt32), AppDataType.Type.UInt32),
            new TypeXref(typeof(Int64), AppDataType.Type.Int64),
            new TypeXref(typeof(UInt64), AppDataType.Type.UInt64),
            new TypeXref(typeof(float), AppDataType.Type.Single),
            new TypeXref(typeof(double), AppDataType.Type.Double),
            new TypeXref(typeof(char), AppDataType.Type.Char16),
            new TypeXref(typeof(bool), AppDataType.Type.Boolean),
            new TypeXref(typeof(string), AppDataType.Type.String),
            new TypeXref(typeof(DateTimeOffset), AppDataType.Type.DateTime),
            new TypeXref(typeof(TimeSpan), AppDataType.Type.TimeSpan),
            new TypeXref(typeof(Guid), AppDataType.Type.Guid),
            new TypeXref(typeof(Point), AppDataType.Type.Point),
            new TypeXref(typeof(Size), AppDataType.Type.Size),
            new TypeXref(typeof(Rect), AppDataType.Type.Rect),
            new TypeXref(typeof(ApplicationDataCompositeValue), AppDataType.Type.ApplicationDataCompositeValue),
            new TypeXref(typeof(byte[]), AppDataType.Type.UInt8Array),
            new TypeXref(typeof(Int16[]), AppDataType.Type.Int16Array),
            new TypeXref(typeof(UInt16[]), AppDataType.Type.UInt16Array),
            new TypeXref(typeof(Int32[]), AppDataType.Type.Int32Array),
            new TypeXref(typeof(UInt32[]), AppDataType.Type.UInt32Array),
            new TypeXref(typeof(Int64[]), AppDataType.Type.Int64Array),
            new TypeXref(typeof(UInt64[]), AppDataType.Type.UInt64Array),
            new TypeXref(typeof(float[]), AppDataType.Type.SingleArray),
            new TypeXref(typeof(double[]), AppDataType.Type.DoubleArray),
            new TypeXref(typeof(char[]), AppDataType.Type.Char16Array),
            new TypeXref(typeof(bool[]), AppDataType.Type.BooleanArray),
            new TypeXref(typeof(string[]), AppDataType.Type.StringArray),
            new TypeXref(typeof(DateTimeOffset[]), AppDataType.Type.DateTimeArray),
            new TypeXref(typeof(TimeSpan[]), AppDataType.Type.TimeSpanArray),
            new TypeXref(typeof(Guid[]), AppDataType.Type.GuidArray),
            new TypeXref(typeof(Point[]), AppDataType.Type.PointArray),
            new TypeXref(typeof(Size[]), AppDataType.Type.SizeArray),
            new TypeXref(typeof(Rect[]), AppDataType.Type.RectArray),
        };

    }

    public static class ObjectExtensions
    {
        public static AppDataType.Type GetAppDataType(this object obj)
        {
            if (obj == null)
                return AppDataType.Type.Empty;
            var type = obj.GetType();
            foreach (var tx in AppDataType.typeXrefTable)
            {
                if (type == tx.objectType)
                    return tx.appdataType;
            }
            throw new NotSupportedException(String.Format("Unknown type ({0})", type.ToString()));
        }
    }
}
