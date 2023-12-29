using CreateCalendar.CreateIcs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace TestRosterToCal
{
    [TestClass]
    public class UnitTestICalFolding : IDisposable
    {
        private MemoryStream ms;
        private bool disposedValue;

        public UnitTestICalFolding() { 
            ms = new MemoryStream(256);
        }

        private string CurrentValue()
        {
            ms.Flush();
            return Encoding.UTF8.GetString(ms.ToArray());
                // .Split(new[] { "\r\n " }, StringSplitOptions.None);
        }
        [TestMethod]
        public void TestFoldingSingle()
        {
            var s = new ContentFolder(ms, "TEST");
            s.WriteLine(new string('z', 150));
            var encoded = CurrentValue();
            Assert.AreEqual("TEST:zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz\r\n zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz\r\n zzzzzz\\n", encoded);
        }
        [TestMethod]
        public void TestFoldingSequential()
        {
            var s = new ContentFolder(ms, "TEST");
            var encoded = CurrentValue();
            Assert.AreEqual("TEST:", encoded);
            s.WriteLine("abcdef");
            encoded = CurrentValue();
            Assert.AreEqual("TEST:abcdef\\n", encoded);
            s.WriteLine("abcdef");
            encoded = CurrentValue();
            Assert.AreEqual("TEST:abcdef\\nabcdef\\n", encoded);
            // 5 + 8 + 8 = 21 chars long already + 2 for extra newline - make to 75
            s.WriteLine(new string('z', 52));
            encoded = CurrentValue();
            Assert.AreEqual("TEST:abcdef\\nabcdef\\nzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz\\n", encoded);
            s.WriteLine("g");
            encoded = CurrentValue();
            Assert.AreEqual("TEST:abcdef\\nabcdef\\nzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz\\n\r\n g\\n", encoded);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ms.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~UnitTest1()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
