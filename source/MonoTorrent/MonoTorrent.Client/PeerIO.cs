//
// PeerIO.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2010 Alan McGovern
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
using System.Net;
using MonoTorrent.Client.Connections;
using MonoTorrent.Client.Encryption;
using MonoTorrent.Client.Messages;
using MonoTorrent.Client.Messages.Standard;
using MonoTorrent.Common;

namespace MonoTorrent.Client
{
	internal static partial class PeerIO
	{
		private const int MaxMessageLength = Piece.BlockSize*4;

		private static readonly ICache<ReceiveMessageState> receiveCache = new Cache<ReceiveMessageState>(true).Synchronize();
		private static readonly ICache<SendMessageState> sendCache = new Cache<SendMessageState>(true).Synchronize();

		private static readonly AsyncIOCallback MessageLengthReceivedCallback = MessageLengthReceived;
		private static readonly AsyncIOCallback EndSendCallback = EndSend;
		private static readonly AsyncIOCallback MessageBodyReceivedCallback = MessageBodyReceived;
		private static readonly AsyncIOCallback HandshakeReceivedCallback = HandshakeReceived;

		public static void EnqueueSendMessage(IConnection connection, IEncryption encryptor, PeerMessage message,
			IRateLimiter rateLimiter, ConnectionMonitor peerMonitor, ConnectionMonitor managerMonitor, AsyncIOCallback callback,
			object state)
		{
			int count = message.ByteLength;
			byte[] buffer = ClientEngine.BufferManager.GetBuffer(count);
			message.Encode(buffer, 0);
			encryptor.Encrypt(buffer, 0, count);

			SendMessageState data = sendCache.Dequeue().Initialise(buffer, callback, state);
			NetworkIO.EnqueueSend(connection, buffer, 0, count, rateLimiter, peerMonitor, managerMonitor, EndSendCallback, data);
		}

		private static void EndSend(bool successful, int count, object state)
		{
			var data = (SendMessageState) state;
			ClientEngine.BufferManager.FreeBuffer(data.Buffer);
			data.Callback(successful, count, data.State);
			sendCache.Enqueue(data);
		}

		public static void EnqueueReceiveHandshake(IConnection connection, IEncryption decryptor,
			AsyncMessageReceivedCallback callback, object state)
		{
			byte[] buffer = ClientEngine.BufferManager.GetBuffer(HandshakeMessage.HandshakeLength);
			ReceiveMessageState data = receiveCache.Dequeue()
				.Initialise(connection, decryptor, null, null, null, buffer, callback, state);
			NetworkIO.EnqueueReceive(connection, buffer, 0, HandshakeMessage.HandshakeLength, null, null, null,
				HandshakeReceivedCallback, data);
		}

		private static void HandshakeReceived(bool successful, int transferred, object state)
		{
			var data = (ReceiveMessageState) state;
			PeerMessage message = null;

			if (successful)
			{
				data.Decryptor.Decrypt(data.Buffer, 0, transferred);
				message = new HandshakeMessage();
				message.Decode(data.Buffer, 0, transferred);
			}

			data.Callback(successful, message, data.State);
			ClientEngine.BufferManager.FreeBuffer(data.Buffer);
			receiveCache.Enqueue(data);
		}

		public static void EnqueueReceiveMessage(IConnection connection, IEncryption decryptor, IRateLimiter rateLimiter,
			ConnectionMonitor monitor, TorrentManager manager, AsyncMessageReceivedCallback callback, object state)
		{
			// FIXME: Hardcoded number
			int count = 4;
			byte[] buffer = ClientEngine.BufferManager.GetBuffer(count);
			ReceiveMessageState data = receiveCache.Dequeue()
				.Initialise(connection, decryptor, rateLimiter, monitor, manager, buffer, callback, state);
			NetworkIO.EnqueueReceive(connection, buffer, 0, count, rateLimiter, monitor, data.ManagerMonitor,
				MessageLengthReceivedCallback, data);
		}

		private static void MessageLengthReceived(bool successful, int transferred, object state)
		{
			var data = (ReceiveMessageState) state;
			int messageLength = -1;

			if (successful)
			{
				data.Decryptor.Decrypt(data.Buffer, 0, transferred);
				messageLength = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data.Buffer, 0));
			}

			if (!successful || messageLength < 0 || messageLength > MaxMessageLength)
			{
				ClientEngine.BufferManager.FreeBuffer(data.Buffer);
				data.Callback(false, null, data.State);
				receiveCache.Enqueue(data);
				return;
			}

			if (messageLength == 0)
			{
				ClientEngine.BufferManager.FreeBuffer(data.Buffer);
				data.Callback(true, new KeepAliveMessage(), data.State);
				receiveCache.Enqueue(data);
				return;
			}

			byte[] buffer = ClientEngine.BufferManager.GetBuffer(messageLength + transferred);
			Buffer.BlockCopy(data.Buffer, 0, buffer, 0, transferred);
			ClientEngine.BufferManager.FreeBuffer(data.Buffer);
			data.Buffer = buffer;

			NetworkIO.EnqueueReceive(data.Connection, buffer, transferred, messageLength, data.RateLimiter, data.PeerMonitor,
				data.ManagerMonitor, MessageBodyReceivedCallback, data);
		}

		private static void MessageBodyReceived(bool successful, int transferred, object state)
		{
			var data = (ReceiveMessageState) state;
			if (!successful)
			{
				ClientEngine.BufferManager.FreeBuffer(data.Buffer);
				data.Callback(false, null, data.State);
				receiveCache.Enqueue(data);
				return;
			}

			data.Decryptor.Decrypt(data.Buffer, 4, transferred);
			PeerMessage message = PeerMessage.DecodeMessage(data.Buffer, 0, transferred + 4, data.Manager);
			ClientEngine.BufferManager.FreeBuffer(data.Buffer);
			data.Callback(true, message, data.State);
			receiveCache.Enqueue(data);
		}
	}
}