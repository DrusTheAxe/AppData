// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.IO;
using Windows.Storage;

namespace AppData
{
    public class SettingsWriterXML : SettingsWriter
    {
        int indent = 0;
        string indentString = "";

        public SettingsWriterXML(Stream stream) :
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
            writer.WriteLine("<Settings>");
            Indent();
        }
        public override void End()
        {
            Unindent();
            WriteIndent();
            writer.WriteLine("</Settings>");
        }

        public override void StartContainers(ApplicationDataLocality locality)
        {
            WriteIndent();
            writer.WriteLine("<Containers locality='{0}'>", locality.ToString());
            Indent();
        }
        public override void EndContainers()
        {
            Unindent();
            WriteIndent();
            writer.WriteLine("</Containers>");
        }

        public override void StartContainer(ApplicationDataContainer container)
        {
            WriteIndent();
            writer.WriteLine("<Container Name='{0}'>", container.Name.XMLEscape());
            Indent();
        }
        public override void EndContainer(bool isLastAtThisNestingLevel)
        {
            Unindent();
            WriteIndent();
            writer.WriteLine("</Container>");
        }

        public override void StartValues()
        {
            WriteIndent();
            writer.WriteLine("<Values>");
            Indent();
        }
        public override void EndValues(bool isLastAtThisNestingLevel)
        {
            Unindent();
            WriteIndent();
            writer.WriteLine("</Values>");
        }

        public override void WriteValue(string key, object value, bool isLastAtThisNestingLevel)
        {
            var type = value.GetAppDataType();
            System.Diagnostics.Debug.Assert(type != AppDataType.Type.ApplicationDataCompositeValue);

            WriteIndent();
            writer.WriteLine(String.Format("<Value Name='{0}' Type='{1}'>{2}</Value>",
                key.XMLEscape(), type.ToString(), value.ToString().XMLEscape()));
        }

        public override void StartComposite(string name)
        {
            WriteIndent();
            writer.WriteLine(String.Format("<Composite Name='{0}'>", name.XMLEscape()));
            Indent();
        }
        public override void EndComposite(bool isLastAtThisNestingLevel)
        {
            Unindent();
            WriteIndent();
            writer.WriteLine("</Composite>");
        }
    }
}
