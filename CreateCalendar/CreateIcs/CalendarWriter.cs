using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.CreateIcs
{
    public class CalendarWriter
    {
        private readonly Stream _stream;
        public readonly Encoding Encoding;
        private readonly byte[] _icsNewLine;
        public CalendarWriter(Stream stream)
        {
            _stream = stream;
            Encoding = Encoding.UTF8;
            _icsNewLine = Encoding.GetBytes("\r\n");
        }

        public async Task WriteRaw(params string[] text)
        {
            for (int i=0; i < text.Length; ++i) 
            {
                var textBytes = Encoding.GetBytes(text[i]);
                await _stream.WriteAsync(textBytes, 0, textBytes.Length);
            }
        }
        public async Task WriteLineRaw(params string[] text)
        {
            await WriteRaw(text);
            await WriteLine();
        }
        public async Task WriteLine()
        {
            await _stream.WriteAsync(_icsNewLine, 0, _icsNewLine.Length);
        }
        public async Task WriteProperty(string propertyName, DateOnly date)
        {
            await WriteLineRaw(propertyName, ";VALUE=DATE:", date.ToString("yyyymmdd"));
        }
        public async Task WriteProperty(string propertyName, DateTime date)
        {
            await WriteRaw(propertyName, ":", date.ToString("yyyymmddThhnnss"));
            if (date.Kind == DateTimeKind.Utc)
            {
                await WriteLineRaw("Z");
            }
            else
            {
                await WriteLine();
            }
        }
        public async Task WriteProperty(string propertyName, int value)
        {
            await WriteLineRaw(propertyName, value.ToString());
        }
        public async Task WriteProperty(string propertyName, string propertyValue)
        {
            await WriteLineRaw(propertyName, ":", propertyValue);
        }

        public async Task WriteProperty(string propertyName, Action<ContentFolder> textValue)
        {
            var cf = new ContentFolder(_stream, propertyName, Encoding);
            textValue(cf);
            await WriteLine();
        }

        public void Flush()
        {
            _stream.Flush();
        }
    }
}
