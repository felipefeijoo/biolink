using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Utilities {

    public class PushbackReader : TextReader {

        private const int DEFAULT_BUFFER_SIZE = 1;
        private TextReader _reader;
        private int _pos;
        private char[] _buf;

        public PushbackReader(TextReader reader) : this(reader, DEFAULT_BUFFER_SIZE) {
        }

        public PushbackReader(TextReader reader, int size) {

            if (reader == null) {
                throw new ArgumentNullException();
            }

            if (size <= 0) {
                throw new Exception("Size must be positive");
            }

            this._reader = reader;
            this._buf = new char[size];
            _pos = size;
        }

        public virtual TextReader Reader {
            get { return this._reader; }
        }

        public override void Close() {

            lock (this) {
                this._buf = null;
            }

            this._reader.Close();
            base.Close();
        }

        public override int Peek() {
            return this._reader.Peek();
        }

        public override int Read() {
            if (this._buf == null) {
                throw new Exception("Stream has already been disposed");
            }
            if (_pos == this._buf.Length) {
                return this.Reader.Read();
            }
            _pos++;
            return this._buf[_pos - 1]; 
        }

        public override int Read(char[] buffer, int index, int count) {
            if (_buf == null) {
                throw new Exception("Stream as already been disposed");
            }

            if (index < 0 || count < 0 || index + count > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            int numBytes = Math.Min(_buf.Length - _pos, count);

            if (numBytes > 0) {
                Array.Copy(_buf, _pos, buffer, index, numBytes);
                _pos += numBytes;
            }

            int num = count - numBytes;
            if (num > 0) {
                num = this.Reader.Read(buffer, numBytes, num);
                numBytes += num;
            }

            return numBytes;
        }

        public int Read(char[] buffer) {
            return this.Read(buffer, 0, buffer.Length);
        }

        public override string ReadLine() {
            int ch;

            StringBuilder sb = new StringBuilder();
            ch = this.Read();

            while (true) {
                if (ch == '\r') {
                    if (Peek() == '\n') { 
                        this.Reader.Read();
                    }
                    break;
                }
                sb.Append(ch);
                ch = this.Read();
            }

            return sb.ToString();
        }

        public override int ReadBlock(char[] buffer, int index, int count) {
            if (_buf == null) {
                throw new Exception("Stream has already been disposed");
            }

            if (index < 0 || count < 0 || index + count > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            int numBytes = Math.Min(_buf.Length - _pos, count);

            if (numBytes > 0) {
                Array.Copy(_buf, _pos, buffer, index, numBytes);
                _pos += numBytes;
            }

            int num = count - numBytes;
            if (num > 0) {
                num = this.Reader.ReadBlock(buffer, numBytes, num);
                numBytes += num;
            }

            return numBytes;
        }

        public override string ReadToEnd() {

            StringBuilder sb = new StringBuilder();
            char[] ch1 = new char[256];

            while (true) {
                int num = Reader.Read(ch1, 0, 256);
                if (num == -1) {
                    break;
                }
                sb.Append(_buf, 0, num);
            }

            return sb.ToString();
        }

        public void Unread(int c) {

            if (_buf == null) {
                throw new Exception("Stream has already been disposed!");
            }

            if (_pos == 0) {
                throw new IOException("Pushback buffer is full");
            }

            _pos--;
            _buf[_pos] = (char)c; 
        }


        public void Unread(char[] buffer) {
            Unread(buffer, 0, buffer.Length);
        }

        public void Unread(char[] buffer, int offset, int length) {
            if (_buf == null) {
                throw new Exception("Stream has already been disposed");
            }

            if (_pos == 0) {
                throw new IOException("Pushback buffer is full");
            }

            if (length > _buf.Length) {
                throw new System.ArgumentOutOfRangeException("length");
            }
            Array.Copy(buffer, offset, _buf, _pos - length, length);
            _pos -= length;
        }

    }
}
