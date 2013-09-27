using System;
using System.IO;

namespace MobileDevice
{
	public class iPhoneFile : Stream
	{
		private long _handle;
		private readonly OpenMode _mode;
		private readonly iPhone _phone;

		private iPhoneFile(iPhone phone, long handle, OpenMode mode)
		{
			_phone = phone;
			_mode = mode;
			_handle = handle;
		}

		protected override unsafe void Dispose(bool disposing)
		{
			if (disposing && (_handle != 0L))
			{
				MobileDevice.AFCFileRefClose(_phone.AFCHandle, _handle);
				_handle = 0L;
			}
			base.Dispose(disposing);
		}

		public override unsafe void Flush()
		{
			MobileDevice.AFCFlushData(_phone.AFCHandle, _handle);
		}

		public static unsafe iPhoneFile Open(iPhone phone, string path, FileAccess openmode)
		{
			long num;
			var none = OpenMode.None;
			switch (openmode)
			{
				case FileAccess.Read:
					none = OpenMode.Read;
					break;

				case FileAccess.Write:
					none = OpenMode.Write;
					break;

				case FileAccess.ReadWrite:
					throw new NotImplementedException("Read+Write not (yet) implemented");
			}
			string str = phone.FullPath(phone.GetCurrentDirectory(), path);
			int num2 = MobileDevice.AFCFileRefOpen(phone.AFCHandle, str, (int)none, 0, out num);
			if (num2 != 0)
			{
				throw new IOException("AFCFileRefOpen failed with error " + num2);
			}
			return new iPhoneFile(phone, num, none);
		}

		public static iPhoneFile OpenRead(iPhone phone, string path)
		{
			return Open(phone, path, FileAccess.Read);
		}

		public static iPhoneFile OpenWrite(iPhone phone, string path)
		{
			return Open(phone, path, FileAccess.Write);
		}

		public override unsafe int Read(byte[] buffer, int offset, int count)
		{
			byte[] buffer2;
			if (_mode != OpenMode.Read)
			{
				throw new NotImplementedException("Stream open for writing only");
			}
			if (offset == 0)
			{
				buffer2 = buffer;
			}
			else
			{
				buffer2 = new byte[count];
			}
			uint len = (uint)count;
			int num2 = MobileDevice.AFCFileRefRead(_phone.AFCHandle, _handle, buffer2, ref len);
			if (num2 != 0)
			{
				throw new IOException("AFCFileRefRead error = " + num2);
			}
			if (buffer2 != buffer)
			{
				Buffer.BlockCopy(buffer2, 0, buffer, offset, (int)len);
			}
			return (int)len;
		}

		public override unsafe long Seek(long offset, SeekOrigin origin)
		{
			int num = MobileDevice.AFCFileRefSeek(_phone.AFCHandle, _handle, (uint)offset, 0);
			Console.WriteLine("ret = {0}", num);
			return offset;
		}

		public override unsafe void SetLength(long value)
		{
			MobileDevice.AFCFileRefSetFileSize(this._phone.AFCHandle, this._handle, (uint)value);
		}

		public override unsafe void Write(byte[] buffer, int offset, int count)
		{
			byte[] buffer2;
			if (_mode != OpenMode.Write)
			{
				throw new NotImplementedException("Stream open for reading only");
			}
			if (offset == 0)
			{
				buffer2 = buffer;
			}
			else
			{
				buffer2 = new byte[count];
				Buffer.BlockCopy(buffer, offset, buffer2, 0, count);
			}
			uint len = (uint)count;
			MobileDevice.AFCFileRefWrite(this._phone.AFCHandle, this._handle, buffer2, len);
		}

		public override bool CanRead
		{
			get
			{
				return (_mode == OpenMode.Read);
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanTimeout
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return (_mode == OpenMode.Write);
			}
		}

		public override long Length
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public unsafe override long Position
		{
			get
			{
				uint position = 0;
				MobileDevice.AFCFileRefTell(_phone.AFCHandle, _handle, ref position);
				return position;
			}
			set
			{
				Seek(value, SeekOrigin.Begin);
			}
		}

		private enum OpenMode
		{
			None = 0,
			Read = 2,
			Write = 3
		}
	}
}