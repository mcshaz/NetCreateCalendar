using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.CreateIcs
{
    public class ContentFolder
    {
        private readonly Stream _stream;
        private readonly Encoding _encoding;
        private byte[] _icsContentFolder;
        private byte[] _icsNewLine;

        private const int _firstLineOctets = 75;
        private const int _subsequentLineOctets = 74;
        private int _octetsRemaining;
        public ContentFolder(Stream stream, string propertyName):this(stream, propertyName, Encoding.UTF8)
        {
        }
        public ContentFolder(Stream stream, string propertyName, Encoding encoding)
        {
            _stream = stream;
            _encoding = encoding;
            _icsContentFolder = _encoding.GetBytes("\r\n ");
            _icsNewLine = _encoding.GetBytes("\\n");
            _octetsRemaining = _firstLineOctets;
            byte[] bytes = _encoding.GetBytes(propertyName);
            _stream.Write(bytes, 0, bytes.Length);
            _octetsRemaining -= bytes.Length;
            bytes = _encoding.GetBytes(new[] { ':' });
            _stream.Write(bytes, 0, bytes.Length);
            _octetsRemaining -= bytes.Length;
        }
        private void Write(byte[] bytes)
        {
            int copyLen = Math.Min(_octetsRemaining, bytes.Length);
            _stream.Write(bytes, 0, copyLen);
            _octetsRemaining -= copyLen;
            int readPos = copyLen;
            var isFolded = false;
            while (readPos < bytes.Length)
            {
                _stream.Write(_icsContentFolder, 0, _icsContentFolder.Length);
                copyLen = Math.Min(_octetsRemaining == 0 ? _subsequentLineOctets : _octetsRemaining, bytes.Length - readPos);
                _stream.Write(bytes, readPos, copyLen);
                readPos += copyLen;
                isFolded = true;
            }
            if (isFolded)
            {
                _octetsRemaining = _subsequentLineOctets - copyLen;
            }
        }
        public void WriteLine(params string[] text)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                Write(_encoding.GetBytes(ReplaceText(text[i])));
            }
            Write(_icsNewLine);
        }

        public static string ReplaceText(string value)
        {
            for (int i = 0; i < _replacements.Length; ++i)
                value = value.Replace(_replacements[i].Item1, _replacements[i].Item2);
            return value;
        }

        private static readonly Tuple<string, string>[] _replacements = new[]
        {
            /* in a "TEXT" property value
             * A BACKSLASH character (US-ASCII decimal 92) MUST be escaped with another BACKSLASH character.
             * A COMMA character MUST be escaped with a BACKSLASH character
             * A SEMICOLON character MUST be escaped with a BACKSLASH character 
             * However, a COLON character SHALL NOT be escaped with a BACKSLASH character
             */
            new Tuple<string, string>(@"\", "\\\\"),
            new Tuple<string, string>(";",  @"\;"),
            new Tuple<string, string>(",",  @"\,"),
            new Tuple<string, string>("\r",  string.Empty),
            new Tuple<string, string>("\n",  @"\n"),
        };
    }
}
