#region Copyright & License Information
/*
 * Copyright 2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Widgets;

namespace OpenRA.Mods.HV.Widgets
{
	/// <summary>A text field that allows selecting and copying, but not changing its text.</summary>
	public class ReadOnlyTextFieldWidget : TextFieldWidget
	{
		public ReadOnlyTextFieldWidget() { }

		protected ReadOnlyTextFieldWidget(ReadOnlyTextFieldWidget widget)
			: base(widget) { }

		public override ReadOnlyTextFieldWidget Clone() { return new ReadOnlyTextFieldWidget(this); }

		public override bool HandleKeyPress(KeyInput e)
		{
			if (IsModifyingKey(e))
				return HasKeyboardFocus && !IsDisabled();

			return base.HandleKeyPress(e);
		}

		public override bool HandleTextInput(string input)
		{
			return HasKeyboardFocus && !IsDisabled();
		}

		static bool IsModifyingKey(KeyInput e)
		{
			switch (e.Key)
			{
				case Keycode.BACKSPACE:
				case Keycode.DELETE:
				case Keycode.V:
				case Keycode.X:
				case Keycode.D:
				case Keycode.K:
				case Keycode.U:
					return true;
				default:
					return false;
			}
		}
	}
}
