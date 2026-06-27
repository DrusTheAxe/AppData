// Copyright (c) Howard Kapustein and Contributors.
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

        public SettingsWriterXML(Stream stream, bool leaveOpen = false) :
            base(stream, leaveOpen: leaveOpen)
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
                writer.WriteLine("<Settings>");
                Indent();
            }
        }
        public override void End()
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine("</Settings>");
            }
        }

        public override void StartContainers(ApplicationDataLocality locality)
        {
            if (writer != null)
            {
                WriteIndent();
                writer.WriteLine($"<Containers locality='{locality.ToString()}'>");
                Indent();
            }
        }
        public override void EndContainers()
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine("</Containers>");
            }
        }

        public override void StartContainer(ApplicationDataContainer container)
        {
            if (writer != null)
            {
                WriteIndent();
                writer.WriteLine($"<Container Name='{container.Name.XMLEscape()}'>");
                Indent();
            }
        }
        public override void EndContainer(bool isLastAtThisNestingLevel)
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine("</Container>");
            }
        }

        public override void StartValues()
        {
            if (writer != null)
            {
                WriteIndent();
                writer.WriteLine("<Values>");
                Indent();
            }
        }
        public override void EndValues(bool isLastAtThisNestingLevel)
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine("</Values>");
            }
        }

        public override void WriteValue(string key, object value, bool isLastAtThisNestingLevel)
        {
            if (writer == null)
            {
                return;
            }

            AppDataType.Type type = value.GetAppDataType();
            System.Diagnostics.Debug.Assert(type != AppDataType.Type.ApplicationDataCompositeValue);

            WriteIndent();
            if (type == AppDataType.Type.Empty)
            {
                writer.WriteLine($"<Value Name='{key.XMLEscape()}' Type='{type}'/>");
            }
            else
            {
                writer.WriteLine($"<Value Name='{key.XMLEscape()}' Type='{type}'>{value.ToString().XMLEscape()}</Value>");
            }
        }

        public override void StartComposite(string name)
        {
            if (writer != null)
            {
                WriteIndent();
                writer.WriteLine($"<Composite Name='{name.XMLEscape()}'>");
                Indent();
            }
        }
        public override void EndComposite(bool isLastAtThisNestingLevel)
        {
            if (writer != null)
            {
                Unindent();
                WriteIndent();
                writer.WriteLine("</Composite>");
            }
        }
    }
}
