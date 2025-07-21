#region Copyright & License Information
/*
 * Copyright 2023-2025 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class GlobalChatLogic : ChromeLogic
	{
		readonly ScrollPanelWidget historyPanel;
		readonly ContainerWidget chatTemplate;
		readonly ScrollPanelWidget nicknamePanel;
		readonly Widget nicknameTemplate;
		readonly TextFieldWidget inputBox;
		readonly InternetRelayChat internetRelayChat;

		readonly bool autoConnect;

		readonly Color textColor;
		readonly Color notificationColor;
		readonly Color playerColor;
		readonly Color historyColor;

		[ObjectCreator.UseCtor]
		public GlobalChatLogic(Widget widget, ModData modData, Dictionary<string, MiniYaml> logicArgs)
		{
			historyPanel = widget.Get<ScrollPanelWidget>("HISTORY_PANEL");
			chatTemplate = historyPanel.Get<ContainerWidget>("CHAT_TEMPLATE");
			nicknamePanel = widget.Get<ScrollPanelWidget>("NICKNAME_PANEL");
			nicknameTemplate = nicknamePanel.Get("NICKNAME_TEMPLATE");

			textColor = ChromeMetrics.Get<Color>("GlobalChatTextColor");
			notificationColor = ChromeMetrics.Get<Color>("GlobalChatNotificationColor");
			playerColor = ChromeMetrics.Get<Color>("GlobalChatPlayerNameColor");
			historyColor = ChromeMetrics.Get<Color>("GlobalChatHistoryColor");

			var textLabel = chatTemplate.Get<LabelWidget>("TEXT");
			textLabel.GetColor = () => textColor;

			autoConnect = FieldLoader.GetValue<bool>("AutoConnect", logicArgs.First().Value.Value);

			internetRelayChat = modData.Manifest.Get<InternetRelayChat>();

			historyPanel.Bind(internetRelayChat.History, MakeHistoryWidget, HistoryWidgetEquals, true);
			nicknamePanel.Bind(internetRelayChat.Users, MakeUserWidget, UserWidgetEquals, false);

			inputBox = widget.Get<TextFieldWidget>("CHAT_TEXTFIELD");
			inputBox.IsDisabled = () => internetRelayChat.ConnectionStatus != ChatConnectionStatus.Joined;
			inputBox.OnEnterKey = _ =>
			{
				if (inputBox.Text.Length == 0)
					return true;

				if (inputBox.Text.StartsWith("/nick ", StringComparison.Ordinal))
				{
					var nick = inputBox.Text.Replace("/nick ", string.Empty);
					internetRelayChat.TrySetNickname(nick);
				}
				else
					internetRelayChat.SendMessage(inputBox.Text);

				inputBox.Text = "";

				return true;
			};

			// IRC protocol limits messages to 510 characters + CRLF
			inputBox.MaxLength = 510;

			var nickName = internetRelayChat.SanitizedName(Game.Settings.Player.Name);
			var nicknameBox = widget.Get<TextFieldWidget>("NICKNAME_TEXTFIELD");
			nicknameBox.Text = nickName;
			nicknameBox.OnTextEdited = () => nicknameBox.Text = internetRelayChat.SanitizedName(nicknameBox.Text);

			var connectPanel = widget.Get("GLOBALCHAT_CONNECT_PANEL");
			connectPanel.IsVisible = () => internetRelayChat.ConnectionStatus == ChatConnectionStatus.Disconnected;

			var disconnectButton = widget.Get<ButtonWidget>("DISCONNECT_BUTTON");
			disconnectButton.OnClick = internetRelayChat.Disconnect;

			var connectButton = connectPanel.Get<ButtonWidget>("CONNECT_BUTTON");
			connectButton.IsDisabled = () => !internetRelayChat.IsValidNickname(nicknameBox.Text);
			connectButton.OnClick = () => internetRelayChat.Connect(nicknameBox.Text);

			var mainPanel = widget.Get("GLOBALCHAT_MAIN_PANEL");
			mainPanel.IsVisible = () => internetRelayChat.ConnectionStatus != ChatConnectionStatus.Disconnected;

			mainPanel.Get<LabelWidget>("CHANNEL_TOPIC").GetText = () => internetRelayChat.Topic;

			if (autoConnect)
				internetRelayChat.Connect(nickName);
		}

		Widget MakeHistoryWidget(object o)
		{
			var message = (ChatMessage)o;
			var from = message.Type == ChatMessageType.Notification ? "Battlefield Control" : message.Nick;
			var prefixColor = message.Type == ChatMessageType.Notification ? notificationColor : playerColor;
			prefixColor = message.Type == ChatMessageType.PrivateMessage ? historyColor : prefixColor;
			var messageColor = message.Type == ChatMessageType.PrivateMessage ? historyColor : textColor;
			var template = chatTemplate.Clone();
			var notification = new TextNotification(TextNotificationPool.Chat, -1, from, message.Message, prefixColor, messageColor);
			var timestamp = message.Type != ChatMessageType.PrivateMessage;
			WidgetUtils.SetupTextNotification(template, notification, historyPanel.Bounds.Width - historyPanel.ScrollbarWidth, timestamp);

			template.Id = message.UID;
			return template;
		}

		bool HistoryWidgetEquals(Widget widget, object o)
		{
			return widget.Id == ((ChatMessage)o).UID;
		}

		Widget MakeUserWidget(object o)
		{
			var nick = (string)o;
			var client = internetRelayChat.Users[nick];

			var item = nicknameTemplate.Clone();
			item.Id = client.Name;
			item.IsVisible = () => true;
			var name = item.Get<LabelWidget>("NICK");
			name.GetText = () => client.Name;
			name.IsVisible = () => true;

			// TODO: Add custom image for voice
			var indicator = item.Get<ImageWidget>("INDICATOR");
			indicator.IsVisible = () => client.IsOp || client.IsVoiced;
			indicator.GetImageName = () => client.IsOp || client.IsVoiced ? "admin" : "";

			return item;
		}

		bool UserWidgetEquals(Widget widget, object o)
		{
			var nick = (string)o;
			return widget.Id == nick;
		}

		bool disposed;
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposed)
				return;

			historyPanel.Unbind();
			nicknamePanel.Unbind();

			disposed = true;
		}
	}
}
