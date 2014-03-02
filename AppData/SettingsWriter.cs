using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;

namespace AppData
{
    public abstract class SettingsWriter : IDisposable
    {
        protected StreamWriter writer;

        protected SettingsWriter(Stream stream)
        {
            this.writer = new StreamWriter(stream);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (writer != null)
            {
                if (disposing)
                    writer.Dispose();
                writer = null;

            }
        }

        // Use C# destructor syntax for finalization code. This destructor will run only if the
        // Dispose method does not get called. It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~SettingsWriter()
        {
            Dispose(false);
        }

        public abstract void Start();
        public abstract void End();

        public abstract void StartContainers(ApplicationDataLocality locality);
        public abstract void EndContainers();

        public abstract void StartContainer(ApplicationDataContainer container);
        public abstract void EndContainer(bool isLastAtThisNestingLevel);

        public abstract void StartValues();
        public abstract void EndValues(bool isLastAtThisNestingLevel);

        public abstract void WriteValue(string key, object value, bool isLastAtThisNestingLevel);

        public abstract void StartComposite(string name);
        public abstract void EndComposite(bool isLastAtThisNestingLevel);
    }
}
