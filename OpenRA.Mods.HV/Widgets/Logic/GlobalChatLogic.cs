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

			var textLabel = chatTemplate.Get<LabelWidget>("TEXT");
			textLabel.GetColor = () => textColor;

			autoConnect = FieldLoader.GetValue<bool>("AutoConnect", logicArgs.First().Value.Value);

			internetRelayChat = modData.GetOrCreate<InternetRelayChat>();

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
			var template = chatTemplate.Clone();
			SetupChatNotification(template, message, historyPanel.Bounds.Width - historyPanel.ScrollbarWidth);

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

		void SetupChatNotification(Widget notificationWidget, ChatMessage chatMessage, int boxWidth)
		{
			var timeLabel = notificationWidget.GetOrNull<LabelWidget>("TIME");
			var prefixLabel = notificationWidget.GetOrNull<LabelWidget>("PREFIX");
			var textLabel = notificationWidget.Get<LabelWidget>("TEXT");

			var from = chatMessage.Type == ChatMessageType.Notification ? "Battlefield Control" : chatMessage.Nick;
			var prefixColor = chatMessage.Type == ChatMessageType.Notification ? notificationColor : playerColor;

			var textFont = Game.Renderer.Fonts[textLabel.Font];
			var textWidth = boxWidth - notificationWidget.Bounds.X - textLabel.Bounds.X;

			var timeOffset = 0;

			if (timeLabel != null)
			{
				var time = $"{chatMessage.Time.Hour:D2}:{chatMessage.Time.Minute:D2}";
				timeOffset = timeLabel.Bounds.Width + timeLabel.Bounds.X;

				timeLabel.GetText = () => time;

				textWidth -= timeOffset;
				textLabel.Bounds.X += timeOffset;

				prefixLabel.Bounds.X += timeOffset;
			}

			var prefix = from + ":";
			var prefixSize = Game.Renderer.Fonts[prefixLabel.Font].Measure(prefix);
			var prefixOffset = prefixSize.X + prefixLabel.Bounds.X;

			prefixLabel.GetColor = () => prefixColor;
			prefixLabel.GetText = () => prefix;
			prefixLabel.Bounds.Width = prefixSize.X;

			textWidth -= prefixOffset;
			textLabel.Bounds.X += prefixOffset - timeOffset;

			textLabel.GetColor = () => textColor;
			textLabel.Bounds.Width = textWidth;

			// Hack around our hacky wordwrap behavior: need to resize the widget to fit the text
			var text = WidgetUtils.WrapText(chatMessage.Message, textLabel.Bounds.Width, textFont);
			textLabel.GetText = () => text;
			var deltaHeight = textFont.Measure(text).Y - textLabel.Bounds.Height;
			if (deltaHeight > 0)
			{
				textLabel.Bounds.Height += deltaHeight;
				notificationWidget.Bounds.Height += deltaHeight;
			}

			notificationWidget.Bounds.Width = boxWidth - notificationWidget.Bounds.X;
		}
	}
}
