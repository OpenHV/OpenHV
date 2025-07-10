#region Copyright & License Information
/*
 * Copyright 2023-2024 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Meebey.SmartIrc4net;
using OpenRA.Primitives;

namespace OpenRA.Mods.HV
{
	public enum ChatConnectionStatus { Disconnected, Connecting, Connected, Disconnecting, Joined, Error }
	public enum ChatMessageType { Message, PrivateMessage, Notification }

	public sealed class ChatUser
	{
		public readonly string Name;
		public bool IsOp;
		public bool IsVoiced;

		public ChatUser(string name, bool isOp, bool isVoice)
		{
			Name = name;
			IsOp = isOp;
			IsVoiced = isVoice;
		}
	}

	public sealed class ChatMessage
	{
		static long nextUID;

		public readonly DateTime Time;
		public readonly string Format;
		public readonly ChatMessageType Type;
		public readonly string Nick;
		public readonly string Message;
		public readonly string UID;

		public ChatMessage(DateTime time, string format, ChatMessageType type, string nick, string message)
		{
			Time = time;
			Format = format;
			Type = type;
			Nick = nick;
			Message = message;

			UID = Interlocked.Increment(ref nextUID).ToString(NumberFormatInfo.CurrentInfo);
		}

		public override string ToString()
		{
			var time = Time.ToString(Format, NumberFormatInfo.CurrentInfo);
			if (Type == ChatMessageType.Notification)
				return $"{time} {Message}";

			return $"{time} {Nick}: {Message}";
		}
	}

	public sealed class InternetRelayChat : IGlobalModData
	{
		public readonly string Hostname = "irc.freegamedev.net";
		public readonly int Port = 6697;
		public readonly string Channel = "openhv-lobby";
		public readonly string QuitMessage = "Battle control terminated!";
		public readonly string TimeStampFormat = "HH:mm";

		IrcClient client;
		string nickname;
		volatile Channel channel;

		public readonly ObservableSortedDictionary<string, ChatUser> Users = new(StringComparer.InvariantCultureIgnoreCase);
		public readonly ObservableList<ChatMessage> History = new();

		volatile string topic;
		public string Topic { get { return topic; } }

		volatile ChatConnectionStatus connectionStatus = ChatConnectionStatus.Disconnected;
		public ChatConnectionStatus ConnectionStatus { get { return connectionStatus; } }

		public InternetRelayChat()
		{
			Log.AddChannel("irc", "irc.log");
		}

		void Initialize()
		{
			client = new()
			{
				Encoding = System.Text.Encoding.UTF8,
				EnableUTF8Recode = true,
				SendDelay = 100,
				ActiveChannelSyncing = true
			};

			client.OnConnecting += OnConnecting;
			client.OnConnected += OnConnected;
			client.OnDisconnecting += OnDisconnecting;
			client.OnDisconnected += OnDisconnected;
			client.OnError += OnError;
			client.OnKick += OnKick;

			client.OnRawMessage += OnRawMessage;
			client.OnJoin += OnJoin;
			client.OnChannelActiveSynced += OnChannelActiveSynced;
			client.OnTopic += OnTopic;
			client.OnTopicChange += OnTopicChange;
			client.OnNickChange += OnNickChange;

			client.OnChannelMessage += OnChannelMessage;
			client.OnChannelNotice += OnChannelNotice;
			client.OnOp += OnOp;
			client.OnDeop += OnDeop;
			client.OnVoice += OnVoice;
			client.OnDevoice += OnDevoice;
			client.OnPart += OnPart;
			client.OnQuit += OnQuit;
		}

		void OnChannelMessage(object sender, IrcEventArgs e)
		{
			AddMessage(e.Data.Nick, e.Data.Message, e.Data.RawMessageArray[1]);
		}

		void OnChannelNotice(object sender, IrcEventArgs e)
		{
			AddNotification(e.Data.Message);
		}

		void OnOp(object sender, OpEventArgs e)
		{
			SetUserOp(e.Whom, true);
		}

		void OnDeop(object sender, DeopEventArgs e)
		{
			SetUserOp(e.Whom, false);
		}

		void SetUserOp(string whom, bool isOp)
		{
			Game.RunAfterTick(() =>
			{
				if (Users.TryGetValue(whom, out var user))
					user.IsOp = isOp;
			});
		}

		void OnVoice(object sender, VoiceEventArgs e)
		{
			SetUserVoiced(e.Whom, true);
		}

		void OnDevoice(object sender, DevoiceEventArgs e)
		{
			SetUserVoiced(e.Whom, false);
		}

		void SetUserVoiced(string whom, bool isVoiced)
		{
			Game.RunAfterTick(() =>
			{
				if (Users.TryGetValue(whom, out var user))
					user.IsVoiced = isVoiced;
			});
		}

		public void Connect(string nickname)
		{
			if (client == null)
				Initialize();

			if (client.IsConnected || !IsValidNickname(nickname))
				return;

			this.nickname = nickname;

			new Thread(() =>
			{
				try
				{
					client.UseSsl = true;
					client.Connect(Hostname, Port);
				}
				catch (Exception e)
				{
					connectionStatus = ChatConnectionStatus.Error;
					AddNotification(e.Message);
					Game.RunAfterTick(() => Log.Write("irc", e));

					return;
				}

				client.Listen();
			})
			{ Name = "IrcListenThread", IsBackground = true }.Start();
		}

		void AddNotification(string text)
		{
			if (text == "----" || text.StartsWith("**", StringComparison.Ordinal) || text.StartsWith("<", StringComparison.Ordinal))
				return;

			if (text.StartsWith("Image", StringComparison.Ordinal))
				return;

			if (text.StartsWith("#####", StringComparison.Ordinal))
				text = text.Replace("#####", "").Trim();

			var message = new ChatMessage(DateTime.Now, TimeStampFormat, ChatMessageType.Notification, null, text);
			Game.RunAfterTick(() =>
			{
				History.Add(message);
				Log.Write("irc", text);
			});
		}

		void AddMessage(string nick, string text, string code = "")
		{
			var type = code == "PRIVMSG" ? ChatMessageType.PrivateMessage : ChatMessageType.Message;
			var message = new ChatMessage(DateTime.Now, TimeStampFormat, type, nick, text);
			Game.RunAfterTick(() =>
			{
				History.Add(message);
				Log.Write("irc", text);
			});
		}

		void OnConnecting(object sender, EventArgs e)
		{
			AddNotification($"Connecting to {Hostname}:{Port}...");
			connectionStatus = ChatConnectionStatus.Connecting;
		}

		void OnConnected(object sender, EventArgs e)
		{
			AddNotification("Connected.");
			connectionStatus = ChatConnectionStatus.Connected;

			client.Login(nickname, "in-game IRC client", 0, "OpenHV");
			client.RfcJoin("#" + Channel);
		}

		void OnDisconnecting(object sender, EventArgs e)
		{
			if (connectionStatus != ChatConnectionStatus.Error)
				connectionStatus = ChatConnectionStatus.Disconnecting;
		}

		void OnDisconnected(object sender, EventArgs e)
		{
			// Keep the chat window open if there is an error
			// It will be cleared by the Disconnect button
			if (connectionStatus != ChatConnectionStatus.Error)
			{
				Game.RunAfterTick(History.Clear);
				topic = null;
				connectionStatus = ChatConnectionStatus.Disconnected;
			}
		}

		void OnError(object sender, ErrorEventArgs e)
		{
			// Ignore any errors that happen during disconnect
			if (connectionStatus != ChatConnectionStatus.Disconnecting)
			{
				connectionStatus = ChatConnectionStatus.Error;
				AddNotification("Error: " + e.ErrorMessage);
			}
		}

		void OnKick(object sender, KickEventArgs e)
		{
			if (e.Whom == client.Nickname)
			{
				Disconnect();
				connectionStatus = ChatConnectionStatus.Error;
				AddNotification($"You were kicked from the chat by {e.Who}. ({e.KickReason})");
			}
			else
			{
				Users.Remove(e.Whom);
				AddNotification($"{e.Whom} was kicked from the chat by {e.Who}. ({e.KickReason})");
			}
		}

		void OnJoin(object sender, JoinEventArgs e)
		{
			if (e.Who == client.Nickname || channel == null || e.Channel != channel.Name)
				return;

			AddNotification($"{e.Who} joined the chat.");
			Game.RunAfterTick(() => Users.Add(e.Who, new ChatUser(e.Who, false, false)));
		}

		void OnChannelActiveSynced(object sender, IrcEventArgs e)
		{
			channel = client.GetChannel(e.Data.Channel);
			AddNotification($"{channel.Users.Count} users online");
			connectionStatus = ChatConnectionStatus.Joined;

			foreach (var user in channel.Users.Values)
				Game.RunAfterTick(() =>
					Users.Add(user.Nick, new ChatUser(user.Nick, user.IsOp, user.IsVoice)));

			client.WriteLine($"history {channel.Name} 24h");
		}

		void OnTopic(object sender, TopicEventArgs e)
		{
			topic = e.Topic;
		}

		void OnTopicChange(object sender, TopicChangeEventArgs e)
		{
			topic = e.NewTopic;
		}

		void OnNickChange(object sender, NickChangeEventArgs e)
		{
			AddNotification($"{e.OldNickname} is now known as {e.NewNickname}.");

			Game.RunAfterTick(() =>
			{
				if (!Users.TryGetValue(e.OldNickname, out var user))
					return;

				Users.Remove(e.OldNickname);
				Users.Add(e.NewNickname, new ChatUser(e.NewNickname, user.IsOp, user.IsVoiced));
			});
		}

		void OnRawMessage(object sender, IrcEventArgs e)
		{
			Game.RunAfterTick(() => Log.Write("irc", e.Data.RawMessage));
		}

		void OnQuit(object sender, QuitEventArgs e)
		{
			AddNotification($"{e.Who} left the chat.");
			Game.RunAfterTick(() => Users.Remove(e.Who));
		}

		void OnPart(object sender, PartEventArgs e)
		{
			if (channel == null || e.Data.Channel != channel.Name)
				return;

			AddNotification($"{e.Who} left the chat.");
			Game.RunAfterTick(() => Users.Remove(e.Who));
		}

		public string SanitizedName(string dirty)
		{
			if (string.IsNullOrEmpty(dirty))
				return null;

			// There is no need to mangle the nick if it is already valid
			if (Rfc2812.IsValidNickname(dirty))
				return dirty;

			// TODO: some special chars are allowed as well, but not at every position
			var clean = new string(dirty.Where(c => char.IsLetterOrDigit(c)).ToArray());

			if (string.IsNullOrEmpty(clean))
				return null;

			if (char.IsDigit(clean[0]))
				return SanitizedName(clean[1..]);

			// Source: https://tools.ietf.org/html/rfc2812#section-1.2.1
			if (clean.Length > 9)
				clean = clean[..9];

			return clean;
		}

		public bool IsValidNickname(string name)
		{
			return Rfc2812.IsValidNickname(name);
		}

		public void SendMessage(string text)
		{
			if (connectionStatus != ChatConnectionStatus.Joined)
				return;

			// Guard against a last-moment disconnection
			try
			{
				client.SendMessage(SendType.Message, channel.Name, text);
				AddMessage(client.Nickname, text);
			}
			catch (NotConnectedException) { }
		}

		public bool TrySetNickname(string nick)
		{
			// TODO: This is inconsistent with the other check
			if (Rfc2812.IsValidNickname(nick))
			{
				client.RfcNick(nick);
				return true;
			}

			return false;
		}

		public void Disconnect()
		{
			// Error is an alias for disconnect, but keeps the panel open
			// so that clients can see the error
			if (connectionStatus == ChatConnectionStatus.Error)
			{
				Game.RunAfterTick(History.Clear);
				topic = null;
				connectionStatus = ChatConnectionStatus.Disconnected;
			}
			else
				connectionStatus = ChatConnectionStatus.Disconnecting;

			if (!client.IsConnected)
				return;

			client.RfcQuit(QuitMessage);

			AddNotification($"Disconnecting from {client.Address}...");
			client.Disconnect();
			Dispose();
		}

		void Unsubsribe()
		{
			if (client == null)
				return;

			client.OnConnecting -= OnConnecting;
			client.OnConnected -= OnConnected;
			client.OnDisconnecting -= OnDisconnecting;
			client.OnDisconnected -= OnDisconnected;
			client.OnError -= OnError;
			client.OnKick -= OnKick;

			client.OnRawMessage -= OnRawMessage;
			client.OnJoin -= OnJoin;
			client.OnChannelActiveSynced -= OnChannelActiveSynced;
			client.OnTopic -= OnTopic;
			client.OnTopicChange -= OnTopicChange;
			client.OnNickChange -= OnNickChange;

			client.OnChannelMessage -= OnChannelMessage;
			client.OnChannelNotice -= OnChannelNotice;
			client.OnOp -= OnOp;
			client.OnDeop -= OnDeop;
			client.OnVoice -= OnVoice;
			client.OnDevoice -= OnDevoice;
			client.OnPart -= OnPart;
			client.OnQuit -= OnQuit;
		}

		public void Dispose()
		{
			Users.Clear();
			Unsubsribe();
			client = null;
		}
	}
}
