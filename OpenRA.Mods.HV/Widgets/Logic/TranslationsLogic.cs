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
using System.IO;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class TranslationsLogic : ChromeLogic
	{
		[FluentReference]
		const string SelectLanguage = "dropdownbutton-translations-language";

		[FluentReference]
		const string SelectFile = "dropdownbutton-translations-file";

		[FluentReference("translated", "total")]
		const string Progress = "label-translations-progress";

		[FluentReference("file")]
		const string NewFile = "label-translations-new-file";

		[FluentReference]
		const string Unsaved = "label-translations-unsaved";

		[FluentReference]
		const string Invalid = "label-translations-invalid";

		[FluentReference("file")]
		const string Saved = "label-translations-saved";

		[FluentReference("error")]
		const string SaveFailed = "label-translations-save-failed";

		[FluentReference]
		const string DiscardTitle = "dialog-translations-discard.title";

		[FluentReference("file")]
		const string DiscardPrompt = "dialog-translations-discard.prompt";

		[FluentReference]
		const string DiscardConfirm = "dialog-translations-discard.confirm";

		[FluentReference]
		const string Cancel = "button-cancel";

		readonly ModData modData;

		// Folders containing installed mods and the one of them that contains the running mod.
		readonly List<string> modDirectories;
		readonly string modsDirectory;
		readonly ScrollPanelWidget messageList;
		readonly ScrollItemWidget messageTemplate;
		readonly TextFieldWidget sourceField;
		readonly TextFieldWidget translationField;

		readonly List<KeyValuePair<string, Language>> languages;
		readonly List<string> sourceFiles;

		readonly List<KeyValuePair<string, string>> sourceMessages = [];
		readonly List<string> sourceKeys = [];
		readonly Dictionary<string, string> translations = [];
		readonly HashSet<string> invalidKeys = [];

		KeyValuePair<string, Language>? selectedLanguage;
		string selectedSource;
		string selectedKey;

		FluentDocument translationDocument;
		string translationPath;
		bool translationExists;
		bool dirty;
		string status = "";

		[ObjectCreator.UseCtor]
		public TranslationsLogic(Widget widget, ModData modData, Action onExit)
		{
			this.modData = modData;
			modsDirectory = ParentDirectory(modData.Manifest);
			modDirectories = Game.Mods.Values.Select(ParentDirectory).Distinct().ToList();

			var localisation = modData.GetOrCreate<Localisation>();
			languages = localisation.Languages?.OrderBy(l => l.Key).ToList() ?? [];
			sourceFiles = FindSourceFiles(localisation);

			var panel = widget;
			var languageDropdown = panel.Get<DropDownButtonWidget>("LANGUAGE_DROPDOWN");
			var selectLanguageText = FluentProvider.GetMessage(SelectLanguage);
			languageDropdown.GetText = () => selectedLanguage.HasValue ? LanguageName(selectedLanguage.Value) : selectLanguageText;
			languageDropdown.OnMouseDown = _ => ShowLanguageDropdown(languageDropdown);
			languageDropdown.IsDisabled = () => languages.Count == 0;

			var fileDropdown = panel.Get<DropDownButtonWidget>("FILE_DROPDOWN");
			var selectFileText = FluentProvider.GetMessage(SelectFile);
			fileDropdown.GetText = () => selectedSource ?? selectFileText;
			fileDropdown.OnMouseDown = _ => ShowFileDropdown(fileDropdown);
			fileDropdown.IsDisabled = () => sourceFiles.Count == 0;

			messageList = panel.Get<ScrollPanelWidget>("MESSAGE_LIST");
			messageTemplate = messageList.Get<ScrollItemWidget>("MESSAGE_TEMPLATE");
			messageList.RemoveChildren();

			sourceField = panel.Get<TextFieldWidget>("SOURCE_TEXT");
			sourceField.IsDisabled = () => selectedKey == null;

			translationField = panel.Get<TextFieldWidget>("TRANSLATION_TEXT");
			translationField.IsDisabled = () => selectedKey == null || translationPath == null;
			var validation = new CachedTransform<string, bool>(t => FluentDocument.IsValidPattern(FromField(t)));
			translationField.IsValid = () => validation.Update(translationField.Text);
			translationField.OnTextEdited = OnTranslationEdited;
			translationField.OnEnterKey = _ => SelectRelative(1);
			translationField.OnArrowDown = _ => SelectRelative(1);
			translationField.OnArrowUp = _ => SelectRelative(-1);
			translationField.OnEscKey = _ => translationField.YieldKeyboardFocus();

			panel.Get<LabelWidget>("STATUS").GetText = () => status;

			var saveButton = panel.Get<ButtonWidget>("SAVE_BUTTON");
			saveButton.IsDisabled = () => !dirty || invalidKeys.Count > 0 || translationPath == null;
			saveButton.OnClick = Save;

			panel.Get<ButtonWidget>("BACK_BUTTON").OnClick = () => ConfirmDiscard(() =>
			{
				Ui.CloseWindow();
				onExit();
			});

			if (languages.Count > 0)
				selectedLanguage = languages[0];

			if (sourceFiles.Count > 0)
				selectedSource = sourceFiles[0];

			Load();
		}

		static string LanguageName(KeyValuePair<string, Language> language)
		{
			return $"{language.Key} ({language.Value.Locale.Name})";
		}

		string DisplayPath(string path)
		{
			// Relative to the game directory that contains the mods folder.
			return Path.GetRelativePath(Path.GetDirectoryName(modsDirectory), path);
		}

		// Fluent values may span multiple lines, but text fields only show a single one.
		static string ToField(string value) { return value?.Replace("\n", "\\n") ?? ""; }
		static string FromField(string text) { return text.Replace("\\n", "\n"); }

		List<string> FindSourceFiles(Localisation localisation)
		{
			var translationFiles = localisation.Languages?.Values.SelectMany(l => l.FluentMessages).ToHashSet() ?? [];

			// Strings of this development mod itself are not meant to be translated.
			var ownPackage = modData.Manifest.Id + "|";
			var files = modData.Manifest.FluentMessages
				.Where(p => !translationFiles.Contains(p) && !p.StartsWith(ownPackage, StringComparison.Ordinal))
				.Distinct()
				.ToList();

			// Also offer files that are not referenced by the mod manifest, e.g. map or Lua strings.
			foreach (var folder in files.Select(p => p[..(p.LastIndexOf('/') + 1)]).Distinct().ToList())
			{
				var directory = ResolveDiskPath(folder);
				if (directory == null || !Directory.Exists(directory))
					continue;

				foreach (var file in Directory.GetFiles(directory, "*.ftl").Order())
				{
					var path = folder + Path.GetFileName(file);
					if (!files.Contains(path))
						files.Add(path);
				}
			}

			return files;
		}

		string ResolveDiskPath(string path)
		{
			var fileSystem = modData.ModFiles;
			if (path.Contains('|'))
			{
				if (fileSystem.TryGetPackageContaining(path, out var package, out var filename) && package is Folder)
					return Path.GetFullPath(Path.Combine(package.Name, filename));

				return null;
			}

			var folder = fileSystem.MountedPackages.LastOrDefault(p => p is Folder && p.Contains(path));
			return folder != null ? Path.GetFullPath(Path.Combine(folder.Name, path)) : null;
		}

		static string PackageOf(string path)
		{
			var split = path.IndexOf('|');
			return split > 0 ? path[..split] : null;
		}

		static bool BelongsToPackage(string path, string package)
		{
			return package == null || path.StartsWith(package + "|", StringComparison.Ordinal) || path.Contains("/" + package + "/");
		}

		string FindTranslationPath(Language language, string source, out bool exists)
		{
			var package = PackageOf(source);
			var filename = Path.GetFileName(source);
			var candidates = language.FluentMessages.Where(p => BelongsToPackage(p, package)).ToList();

			var match = candidates.FirstOrDefault(p => Path.GetFileName(p) == filename);
			if (match != null)
			{
				var path = ResolveDiskPath(match);
				exists = path != null && File.Exists(path);
				return path;
			}

			// Place new files next to the existing translations of the same package.
			string target = null;
			var sibling = candidates.Select(ResolveDiskPath).FirstOrDefault(p => p != null);
			if (sibling != null)
				target = Path.Combine(Path.GetDirectoryName(sibling), filename);
			else
			{
				var sourcePath = ResolveDiskPath(source);
				if (sourcePath != null)
					target = OutsideEngine(Path.Combine(Path.GetDirectoryName(sourcePath), language.Locale.Name, filename));
			}

			// The file may have been created earlier without being added to the mod's language list yet.
			exists = target != null && File.Exists(target);
			return target;
		}

		/// <summary>
		/// The engine is replaced on updates, so translations of files from other mod folders
		/// are kept at the same relative location inside the folder of the running mod instead.
		/// </summary>
		string OutsideEngine(string path)
		{
			foreach (var directory in modDirectories)
			{
				if (directory != modsDirectory && IsInside(path, directory))
					return Path.Combine(modsDirectory, Path.GetRelativePath(directory, path));
			}

			return path;
		}

		static bool IsInside(string path, string directory)
		{
			return path.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.Ordinal);
		}

		/// <summary>The folder that contains the given mod, e.g. the mods folder of the game or the engine.</summary>
		static string ParentDirectory(Manifest manifest)
		{
			return Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(manifest.Package.Name)));
		}

		void ConfirmDiscard(Action onConfirm)
		{
			if (!dirty)
			{
				onConfirm();
				return;
			}

			ConfirmationDialogs.ButtonPrompt(modData,
				title: DiscardTitle,
				text: DiscardPrompt,
				textArguments: ["file", Path.GetFileName(translationPath)],
				onConfirm: onConfirm,
				confirmText: DiscardConfirm,
				onCancel: () => { },
				cancelText: Cancel);
		}

		bool ShowLanguageDropdown(DropDownButtonWidget dropdown)
		{
			ScrollItemWidget SetupItem(KeyValuePair<string, Language> language, ScrollItemWidget template)
			{
				var item = ScrollItemWidget.Setup(template,
					() => selectedLanguage?.Key == language.Key,
					() => ConfirmDiscard(() =>
					{
						selectedLanguage = language;
						Load();
					}));

				var name = LanguageName(language);
				item.Get<LabelWidget>("LABEL").GetText = () => name;
				return item;
			}

			dropdown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 280, languages, SetupItem);
			return true;
		}

		bool ShowFileDropdown(DropDownButtonWidget dropdown)
		{
			ScrollItemWidget SetupItem(string file, ScrollItemWidget template)
			{
				var item = ScrollItemWidget.Setup(template,
					() => selectedSource == file,
					() => ConfirmDiscard(() =>
					{
						selectedSource = file;
						Load();
					}));

				item.Get<LabelWidget>("LABEL").GetText = () => file;
				return item;
			}

			dropdown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 400, sourceFiles, SetupItem);
			return true;
		}

		void Load()
		{
			sourceMessages.Clear();
			sourceKeys.Clear();
			translations.Clear();
			invalidKeys.Clear();
			selectedKey = null;
			translationDocument = null;
			translationPath = null;
			translationExists = false;
			dirty = false;
			sourceField.Text = "";
			translationField.Text = "";

			if (selectedSource != null)
			{
				using (var stream = modData.DefaultFileSystem.Open(selectedSource))
					sourceMessages.AddRange(FluentDocument.Parse(stream.ReadAllText()).Messages());

				sourceKeys.AddRange(sourceMessages.Select(m => m.Key));
			}

			if (selectedSource != null && selectedLanguage.HasValue)
			{
				translationPath = FindTranslationPath(selectedLanguage.Value.Value, selectedSource, out translationExists);
				translationDocument = translationExists ? FluentDocument.Parse(File.ReadAllText(translationPath)) : new FluentDocument();
				foreach (var message in translationDocument.Messages())
					translations[message.Key] = message.Value;
			}

			PopulateMessageList();
			UpdateStatus();
		}

		void PopulateMessageList()
		{
			messageList.RemoveChildren();

			var sourceLabelTemplate = messageTemplate.Get<LabelWidget>("SOURCE");
			var translationLabelTemplate = messageTemplate.Get<LabelWidget>("TRANSLATION");
			var font = Game.Renderer.Fonts[sourceLabelTemplate.Font];

			foreach (var message in sourceMessages)
			{
				var key = message.Key;
				var item = ScrollItemWidget.Setup(key, messageTemplate, () => selectedKey == key, () => Select(key), null);

				var sourceText = WidgetUtils.TruncateText(message.Value.Replace('\n', ' '), sourceLabelTemplate.Bounds.Width, font);
				item.Get<LabelWidget>("SOURCE").GetText = () => sourceText;

				var translationText = new CachedTransform<string, string>(t =>
					t == null ? "" : WidgetUtils.TruncateText(t.Replace('\n', ' '), translationLabelTemplate.Bounds.Width, font));
				item.Get<LabelWidget>("TRANSLATION").GetText = () => translationText.Update(translations.GetValueOrDefault(key));

				messageList.AddChild(item);
			}

			messageList.ScrollToTop();
		}

		void Select(string key)
		{
			selectedKey = key;
			sourceField.Text = ToField(sourceMessages.First(m => m.Key == key).Value);
			translationField.Text = ToField(translations.GetValueOrDefault(key));
			translationField.CursorPosition = translationField.Text.Length;
			translationField.TakeKeyboardFocus();
		}

		bool SelectRelative(int offset)
		{
			var index = selectedKey != null ? sourceKeys.IndexOf(selectedKey) + offset : 0;
			if (index < 0 || index >= sourceKeys.Count)
				return true;

			Select(sourceKeys[index]);
			messageList.ScrollToItem(sourceKeys[index]);
			return true;
		}

		void OnTranslationEdited()
		{
			if (selectedKey == null || translationDocument == null)
				return;

			var value = FromField(translationField.Text);
			if (FluentDocument.IsValidPattern(value))
				invalidKeys.Remove(selectedKey);
			else
				invalidKeys.Add(selectedKey);

			if (string.IsNullOrEmpty(value))
				translations.Remove(selectedKey);
			else
				translations[selectedKey] = value;

			translationDocument.SetMessage(selectedKey, value, sourceKeys);
			dirty = true;
			UpdateStatus();
		}

		void Save()
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(translationPath));
				File.WriteAllText(translationPath, translationDocument.Serialize());
				dirty = false;
				translationExists = true;
				UpdateStatus();
				status = FluentProvider.GetMessage(Saved, "file", DisplayPath(translationPath));
			}
			catch (Exception e)
			{
				Log.Write("debug", e);
				status = FluentProvider.GetMessage(SaveFailed, "error", e.Message);
			}
		}

		void UpdateStatus()
		{
			if (invalidKeys.Count > 0)
				status = FluentProvider.GetMessage(Invalid);
			else if (dirty)
				status = FluentProvider.GetMessage(Unsaved);
			else if (translationPath != null && !translationExists)
				status = FluentProvider.GetMessage(NewFile, "file", DisplayPath(translationPath));
			else if (translationPath != null)
				status = FluentProvider.GetMessage(Progress, "translated", sourceKeys.Count(translations.ContainsKey), "total", sourceKeys.Count);
			else
				status = "";
		}
	}
}
