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

        public SettingsWriterJSON(Stream stream) :
            base(stream)
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
            writer.Write(this.indentString);
        }

        public override void Start()
        {
            WriteIndent();
            writer.WriteLine("{");
            Indent();
        }
        public override void End()
        {
            Unindent();
            WriteIndent();
            writer.WriteLine("}");
        }

        public override void StartContainers(ApplicationDataLocality locality)
        {
        }
        public override void EndContainers()
        {
        }

        public override void StartContainer(ApplicationDataContainer container)
        {
            string name = container.Name;
            WriteIndent();
            writer.WriteLine("\"{0}\": {{", name.JSONEscape());
            Indent();
        }
        public override void EndContainer(bool isLastAtThisNestingLevel)
        {
            Unindent();
            WriteIndent();
            writer.WriteLine(String.Format("}}{0}", Delimiter(isLastAtThisNestingLevel)));
        }

        public override void StartValues()
        {
            WriteIndent();
            writer.WriteLine("\"__values__\": {");
            Indent();
        }
        public override void EndValues(bool isLastAtThisNestingLevel)
        {
            Unindent();
            WriteIndent();
            writer.WriteLine(String.Format("}}{0}", Delimiter(isLastAtThisNestingLevel)));
        }

        public override void WriteValue(string key, object value, bool isLastAtThisNestingLevel)
        {
            var type = value.GetAppDataType();
            System.Diagnostics.Debug.Assert(type != AppDataType.Type.ApplicationDataCompositeValue);

            string jkey = key.JSONEscape();
            string jtype = type.ToString().JSONEscape();
            string jvalue = AppDataExtensions.ValueToJSON(value);
            string suffix = Delimiter(isLastAtThisNestingLevel);
            WriteIndent();
            writer.WriteLine(String.Format("\"{0}\": [ \"{1}\", {2} ]{3}", jkey, jtype, jvalue, suffix));
        }

        public override void StartComposite(string name)
        {
            WriteIndent();
            writer.WriteLine("\"{0}\": [ \"Composite\", {{", name.JSONEscape());
            Indent();
        }
        public override void EndComposite(bool isLastAtThisNestingLevel)
        {
            Unindent();
            WriteIndent();
            writer.WriteLine(String.Format("}} ]{0}", Delimiter(isLastAtThisNestingLevel)));
        }

        private string Delimiter(bool isLastAtThisNestingLevel)
        {
            return isLastAtThisNestingLevel ? "" : ",";
        }
    }
}
