//
// TorrentFile.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2006 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Text;

namespace MonoTorrent.Common
{
	/// <summary>
	///     This is the base class for the files available to download from within a .torrent.
	///     This should be inherited by both Client and Tracker "TorrentFile" classes
	/// </summary>
	public class TorrentFile : IEquatable<TorrentFile>
	{
		#region Private Fields

		private readonly BitField bitfield;
		private readonly byte[] ed2k;
		private readonly int endPiece;
		private readonly long length;
		private readonly string path;
		private readonly byte[] sha1;
		private readonly int startPiece;
		private BitField selector;

		#endregion Private Fields

		#region Member Variables

		/// <summary>
		///     The number of pieces which have been successfully downloaded which are from this file
		/// </summary>
		public BitField BitField
		{
			get { return bitfield; }
		}

		public long BytesDownloaded
		{
			get { return (long) (BitField.PercentComplete*Length/100.0); }
		}

		/// <summary>
		///     The ED2K hash of the file
		/// </summary>
		public byte[] ED2K
		{
			get { return ed2k; }
		}

		/// <summary>
		///     The index of the last piece of this file
		/// </summary>
		public int EndPieceIndex
		{
			get { return endPiece; }
		}

		public string FullPath { get; internal set; }

		/// <summary>
		///     The length of the file in bytes
		/// </summary>
		public long Length
		{
			get { return length; }
		}

		/// <summary>
		///     The MD5 hash of the file
		/// </summary>
		public byte[] MD5 { get; internal set; }

		/// <summary>
		///     In the case of a single torrent file, this is the name of the file.
		///     In the case of a multi-file torrent this is the relative path of the file
		///     (including the filename) from the base directory
		/// </summary>
		public string Path
		{
			get { return path; }
		}

		/// <summary>
		///     The priority of this torrent file
		/// </summary>
		public Priority Priority { get; set; }

		/// <summary>
		///     The SHA1 hash of the file
		/// </summary>
		public byte[] SHA1
		{
			get { return sha1; }
		}

		/// <summary>
		///     The index of the first piece of this file
		/// </summary>
		public int StartPieceIndex
		{
			get { return startPiece; }
		}

		#endregion

		#region Constructors

		public TorrentFile(string path, long length)
			: this(path, length, path)
		{
		}

		public TorrentFile(string path, long length, string fullPath)
			: this(path, length, fullPath, 0, 0)
		{
		}

		public TorrentFile(string path, long length, int startIndex, int endIndex)
			: this(path, length, path, startIndex, endIndex)
		{
		}

		public TorrentFile(string path, long length, string fullPath, int startIndex, int endIndex)
			: this(path, length, fullPath, startIndex, endIndex, null, null, null)
		{
		}

		public TorrentFile(string path, long length, string fullPath, int startIndex, int endIndex, byte[] md5, byte[] ed2k,
			byte[] sha1)
		{
			bitfield = new BitField(endIndex - startIndex + 1);
			this.ed2k = ed2k;
			endPiece = endIndex;
			this.FullPath = fullPath;
			this.length = length;
			this.MD5 = md5;
			this.path = path;
			Priority = Priority.Normal;
			this.sha1 = sha1;
			startPiece = startIndex;
		}

		#endregion

		#region Methods

		public bool Equals(TorrentFile other)
		{
			return other == null ? false : path == other.path && length == other.length;
			;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TorrentFile);
		}

		public override int GetHashCode()
		{
			return path.GetHashCode();
		}

		internal BitField GetSelector(int totalPieces)
		{
			if (selector != null)
				return selector;

			selector = new BitField(totalPieces);
			for (int i = StartPieceIndex; i <= EndPieceIndex; i++)
				selector[i] = true;
			return selector;
		}

		public override string ToString()
		{
			var sb = new StringBuilder(32);
			sb.Append("File: ");
			sb.Append(path);
			sb.Append(" StartIndex: ");
			sb.Append(StartPieceIndex);
			sb.Append(" EndIndex: ");
			sb.Append(EndPieceIndex);
			return sb.ToString();
		}

		#endregion Methods
	}
}