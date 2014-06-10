using System;
using MonoTorrent.Client.Connections;

namespace MonoTorrent.Client
{
	public class NewConnectionEventArgs : TorrentEventArgs
	{
		private readonly IConnection connection;
		private readonly Peer peer;

		public NewConnectionEventArgs(Peer peer, IConnection connection, TorrentManager manager)
			: base(manager)
		{
			if (!connection.IsIncoming && manager == null)
				throw new InvalidOperationException("An outgoing connection must specify the torrent manager it belongs to");

			this.connection = connection;
			this.peer = peer;
		}

		public IConnection Connection
		{
			get { return connection; }
		}

		public Peer Peer
		{
			get { return peer; }
		}
	}
}