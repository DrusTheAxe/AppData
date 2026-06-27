// Copyright (c) Howard Kapustein and Contributors.
// Licensed under the MIT License.

using System;
using System.IO;
using Windows.Storage;

namespace AppData
{
    public class SettingsWriterJSON : SettingsWriter
    {
        int indent = 0;
        string indentString = "";

        public SettingsWriterJSON(Stream stream, bool leaveOpen = false) :
            base(stream, leaveOpen)
        {
        }

        void Indent()
        {
            System.Diagnostics.Debug.Assert(this.indent >= 0);
            this.indentString = new String(' ', ++this.indent * 3);
        }

        void Unindent()
        {
            System.Diagnostics.Debug.Assert(this.indent > 0);
            this.indentString = new String(' ', --this.indent * 3);
        }

        void WriteIndent()
        {
            writer?.Write(this.indentString);
        }

        public override void Start()
        {
            if (writer != null)
            {
                WriteIndent();
                writer.WriteLine("{");
                Indent();
            }
        }
        public override void End()
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine("}");
            }
        }

        public override void StartContainers(ApplicationDataLocality locality)
        {
        }
        public override void EndContainers()
        {
        }

        public override void StartContainer(ApplicationDataContainer container)
        {
            if (writer != null)
            {
                string name = container.Name;
                WriteIndent();
                writer.WriteLine($"\"{name.JSONEscape()}\": {{");
                Indent();
            }
        }
        public override void EndContainer(bool isLastAtThisNestingLevel)
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine($"}}{Delimiter(isLastAtThisNestingLevel)}");
            }
        }

        public override void StartValues()
        {
            if (writer != null)
            {
                WriteIndent();
                writer.WriteLine("\"__values__\": {");
                Indent();
            }
        }
        public override void EndValues(bool isLastAtThisNestingLevel)
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine($"}}{Delimiter(isLastAtThisNestingLevel)}");
            }
        }

        public override void WriteValue(string key, object value, bool isLastAtThisNestingLevel)
        {
            if (writer == null)
            {
                return;
            }

            var type = value.GetAppDataType();
            System.Diagnostics.Debug.Assert(type != AppDataType.Type.ApplicationDataCompositeValue);

            string jkey = key.JSONEscape();
            string jtype = type.ToString().JSONEscape();
            string jvalue = AppDataExtensions.ValueToJSON(value);
            string suffix = Delimiter(isLastAtThisNestingLevel);
            WriteIndent();
            writer.WriteLine($"\"{jkey}\": [ \"{jtype}\", {jvalue} ]{suffix}");
        }

        public override void StartComposite(string name)
        {
            if (writer != null)
            {
                WriteIndent();
                writer.WriteLine($"\"{name.JSONEscape()}\": [ \"Composite\", {{");
                Indent();
            }
        }
        public override void EndComposite(bool isLastAtThisNestingLevel)
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine($"}} ]{Delimiter(isLastAtThisNestingLevel)}");
            }
        }

        private string Delimiter(bool isLastAtThisNestingLevel)
        {
            return isLastAtThisNestingLevel ? "" : ",";
        }
    }
}
