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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;

namespace OpenRA.Mods.HV
{
	/// <summary>An editable representation of a .ftl file that can be written back to disk.</summary>
	public sealed class FluentDocument
	{
		public sealed class Entry
		{
			/// <summary>Verbatim text for comments and unparsable junk. Null for messages and terms.</summary>
			public string Raw;

			/// <summary>Message identifier. Terms are prefixed with a dash.</summary>
			public string Id;

			/// <summary>Comment directly attached to a message.</summary>
			public string Comment;

			public string Value;
			public readonly List<KeyValuePair<string, string>> Attributes = [];
		}

		public readonly List<Entry> Entries = [];

		public static FluentDocument Parse(string text)
		{
			var document = new FluentDocument();
			var resource = new LinguiniParser(text).ParseWithComments();
			foreach (var item in resource.Entries)
			{
				switch (item)
				{
					case AstMessage message:
						document.Entries.Add(CreateEntry(message.Id.Name.ToString(), message.Value, message.Attributes, message.Comment));
						break;
					case AstTerm term:
						document.Entries.Add(CreateEntry("-" + term.Id.Name, term.Value, term.Attributes, term.Comment));
						break;
					case AstComment comment:
						document.Entries.Add(new Entry { Raw = SerializeComment(comment) });
						break;
					case Junk junk:
						document.Entries.Add(new Entry { Raw = junk.Content.ToString().TrimEnd('\r', '\n') });
						break;
				}
			}

			return document;
		}

		static Entry CreateEntry(string id, Pattern value, List<Linguini.Syntax.Ast.Attribute> attributes, AstComment comment)
		{
			var entry = new Entry
			{
				Id = id,
				Value = value != null ? SerializePattern(value) : null,
				Comment = comment != null ? SerializeComment(comment) : null,
			};

			foreach (var attribute in attributes)
				entry.Attributes.Add(new KeyValuePair<string, string>(attribute.Id.Name.ToString(), SerializePattern(attribute.Value)));

			return entry;
		}

		/// <summary>Returns all values as "id" and "id.attribute" keys in file order.</summary>
		public IEnumerable<KeyValuePair<string, string>> Messages()
		{
			foreach (var entry in Entries)
			{
				if (entry.Id == null)
					continue;

				if (entry.Value != null)
					yield return new KeyValuePair<string, string>(entry.Id, entry.Value);

				foreach (var attribute in entry.Attributes)
					yield return new KeyValuePair<string, string>(entry.Id + "." + attribute.Key, attribute.Value);
			}
		}

		public static void SplitKey(string key, out string id, out string attribute)
		{
			var dot = key.IndexOf('.');
			id = dot < 0 ? key : key[..dot];
			attribute = dot < 0 ? null : key[(dot + 1)..];
		}

		/// <summary>
		/// Sets or removes (when value is empty) a message value or attribute.
		/// New entries are placed following the order of keys in the reference.
		/// </summary>
		public void SetMessage(string key, string value, IReadOnlyList<string> referenceOrder)
		{
			if (string.IsNullOrEmpty(value))
				value = null;

			SplitKey(key, out var id, out var attribute);
			var entry = Entries.FirstOrDefault(e => e.Id == id);
			if (entry == null)
			{
				if (value == null)
					return;

				entry = new Entry { Id = id };
				Entries.Insert(InsertionIndex(id, referenceOrder), entry);
			}

			if (attribute == null)
				entry.Value = value;
			else
			{
				var index = entry.Attributes.FindIndex(a => a.Key == attribute);
				if (index >= 0)
				{
					if (value == null)
						entry.Attributes.RemoveAt(index);
					else
						entry.Attributes[index] = new KeyValuePair<string, string>(attribute, value);
				}
				else if (value != null)
					entry.Attributes.Insert(AttributeInsertionIndex(entry, key, referenceOrder), new KeyValuePair<string, string>(attribute, value));
			}

			if (entry.Value == null && entry.Attributes.Count == 0)
			{
				// Keep the attached comment around when the message is removed.
				var index = Entries.IndexOf(entry);
				if (entry.Comment != null)
					Entries[index] = new Entry { Raw = entry.Comment };
				else
					Entries.RemoveAt(index);
			}
		}

		int InsertionIndex(string id, IReadOnlyList<string> referenceOrder)
		{
			// Place the new entry after the closest preceding reference message that already exists.
			var position = referenceOrder.TakeWhile(k => !IsSameId(k, id)).ToList();
			for (var i = position.Count - 1; i >= 0; i--)
			{
				SplitKey(position[i], out var previousId, out _);
				var index = Entries.FindIndex(e => e.Id == previousId);
				if (index >= 0)
					return index + 1;
			}

			return position.Count == referenceOrder.Count ? Entries.Count : 0;
		}

		static int AttributeInsertionIndex(Entry entry, string key, IReadOnlyList<string> referenceOrder)
		{
			var position = referenceOrder.TakeWhile(k => k != key).ToList();
			for (var i = position.Count - 1; i >= 0; i--)
			{
				SplitKey(position[i], out var previousId, out var previousAttribute);
				if (previousId != entry.Id || previousAttribute == null)
					break;

				var index = entry.Attributes.FindIndex(a => a.Key == previousAttribute);
				if (index >= 0)
					return index + 1;
			}

			return position.Count == referenceOrder.Count ? entry.Attributes.Count : 0;
		}

		static bool IsSameId(string key, string id)
		{
			return key == id || (key.StartsWith(id, StringComparison.Ordinal) && key.Length > id.Length && key[id.Length] == '.');
		}

		public string Serialize()
		{
			var builder = new StringBuilder();
			foreach (var entry in Entries)
			{
				if (entry.Raw != null)
				{
					builder.Append(entry.Raw).Append('\n');
					continue;
				}

				if (entry.Comment != null)
					builder.Append(entry.Comment).Append('\n');

				builder.Append(entry.Id).Append(" =");
				AppendPattern(builder, entry.Value, 4);
				foreach (var attribute in entry.Attributes)
				{
					builder.Append("\n    .").Append(attribute.Key).Append(" =");
					AppendPattern(builder, attribute.Value, 8);
				}

				builder.Append('\n');
			}

			return builder.ToString();
		}

		static void AppendPattern(StringBuilder builder, string pattern, int indent)
		{
			if (pattern == null)
				return;

			if (!pattern.Contains('\n'))
			{
				builder.Append(' ').Append(pattern);
				return;
			}

			foreach (var line in pattern.Split('\n'))
			{
				builder.Append('\n');
				if (line.Length > 0)
					builder.Append(' ', indent).Append(line);
			}
		}

		/// <summary>Checks whether a pattern in Fluent syntax would parse as a message value.</summary>
		public static bool IsValidPattern(string pattern)
		{
			if (string.IsNullOrEmpty(pattern))
				return true;

			var builder = new StringBuilder("validate =");
			AppendPattern(builder, pattern, 4);
			builder.Append('\n');

			var resource = new LinguiniParser(builder.ToString()).Parse();
			return resource.Errors.Count == 0 && resource.Entries.Count == 1 && resource.Entries[0] is AstMessage message && message.Value != null;
		}

		static string SerializeComment(AstComment comment)
		{
			var prefix = comment.CommentLevel switch
			{
				CommentLevel.GroupComment => "##",
				CommentLevel.ResourceComment => "###",
				_ => "#",
			};

			return string.Join("\n", comment.Content.Select(l => l.Length > 0 ? prefix + " " + l : prefix));
		}

		/// <summary>Converts a pattern back to Fluent syntax. Multi-line constructs are separated by '\n' with relative indentation.</summary>
		public static string SerializePattern(Pattern pattern)
		{
			var builder = new StringBuilder();
			foreach (var element in pattern.Elements)
			{
				if (element is TextLiteral text)
					builder.Append(text.Value.Span);
				else if (element is Placeable placeable)
					builder.Append(SerializePlaceable(placeable));
			}

			return builder.ToString();
		}

		static string SerializePlaceable(Placeable placeable)
		{
			if (placeable.Expression is SelectExpression select)
				return "{ " + SerializeSelect(select) + "}";

			return "{ " + SerializeExpression(placeable.Expression) + " }";
		}

		static string SerializeSelect(SelectExpression select)
		{
			var builder = new StringBuilder();
			builder.Append(SerializeExpression(select.Selector)).Append(" ->");
			foreach (var variant in select.Variants)
			{
				builder.Append('\n').Append(variant.IsDefault ? "   *[" : "    [").Append(variant.Key.Span).Append(']');
				var value = SerializePattern(variant.Value);
				if (value.Contains('\n'))
				{
					foreach (var line in value.Split('\n'))
					{
						builder.Append('\n');
						if (line.Length > 0)
							builder.Append(' ', 8).Append(line);
					}
				}
				else if (value.Length > 0)
					builder.Append(' ').Append(value);
			}

			builder.Append('\n');
			return builder.ToString();
		}

		static string SerializeExpression(IExpression expression)
		{
			switch (expression)
			{
				case SelectExpression select:
					return SerializeSelect(select);
				case Placeable placeable:
					return SerializePlaceable(placeable);
				case TextLiteral text:
					return "\"" + text.Value + "\"";
				case NumberLiteral number:
					return number.Value.ToString();
				case VariableReference variable:
					return "$" + variable.Id.Name;
				case MessageReference message:
					return message.Id.Name + (message.Attribute != null ? "." + message.Attribute.Name : "");
				case TermReference term:
					return "-" + term.Id.Name + (term.Attribute != null ? "." + term.Attribute.Name : "") +
						(term.Arguments.HasValue ? SerializeArguments(term.Arguments.Value) : "");
				case FunctionReference function:
					return function.Id.Name + SerializeArguments(function.Arguments);
				case DynamicReference dynamic:
					return "$$" + dynamic.Id.Name + (dynamic.Attribute != null ? "." + dynamic.Attribute.Name : "") +
						(dynamic.Arguments.HasValue ? SerializeArguments(dynamic.Arguments.Value) : "");
				default:
					throw new NotSupportedException($"Unsupported Fluent expression {expression.GetType().Name}");
			}
		}

		static string SerializeArguments(CallArguments arguments)
		{
			var positional = arguments.PositionalArgs.Select(SerializeExpression);
			var named = arguments.NamedArgs.Select(a => a.Name.Name + ": " + SerializeExpression(a.Value));
			return "(" + string.Join(", ", positional.Concat(named)) + ")";
		}
	}
}
