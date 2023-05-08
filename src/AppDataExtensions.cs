// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using System.Text;
using Windows.Storage;

namespace AppData
{
    class AppDataExtensions
    {
        public static string BuildPath(ApplicationDataContainer container, string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(container.Locality.ToString().ToUpper());
            if (!path.IsEmpty())
            {
                if (path[0] != '\\')
                    sb.Append('\\');
                sb.Append(path);
            }
            return sb.ToString();
        }

        public static string ValueToJSON(object value)
        {
            if (value == null)
                return "null";

            switch (value.GetAppDataType())
            {
                case AppDataType.Type.Boolean:
                    return (bool)value ? "true" : "false";
                case AppDataType.Type.UInt8:
                case AppDataType.Type.Int16:
                case AppDataType.Type.UInt16:
                case AppDataType.Type.Int32:
                case AppDataType.Type.UInt32:
                case AppDataType.Type.Int64:
                case AppDataType.Type.UInt64:
                case AppDataType.Type.Single:
                case AppDataType.Type.Double:
                    return value.ToString();
                default:
                    return String.Format("\"{0}\"", value.ToString().JSONEscape());
            }
        }
    }
}
