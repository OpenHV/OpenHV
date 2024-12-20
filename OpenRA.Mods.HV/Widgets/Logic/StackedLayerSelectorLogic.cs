#region Copyright & License Information
/*
 * Copyright 2019-2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class StackedLayerSelectorLogic : ChromeLogic
	{
		readonly EditorViewportControllerWidget editor;
		readonly WorldRenderer worldRenderer;

		readonly ScrollPanelWidget layerTemplateList;
		readonly ScrollItemWidget layerPreviewTemplate;

		[ObjectCreator.UseCtor]
		public StackedLayerSelectorLogic(Widget widget, WorldRenderer worldRenderer)
		{
			this.worldRenderer = worldRenderer;
			editor = widget.Parent.Parent.Get<EditorViewportControllerWidget>("MAP_EDITOR");
			layerTemplateList = widget.Get<ScrollPanelWidget>("LAYERTEMPLATE_LIST");
			layerTemplateList.Layout = new GridLayout(layerTemplateList);
			layerPreviewTemplate = layerTemplateList.Get<ScrollItemWidget>("LAYERPREVIEW_TEMPLATE");

			IntializeLayerPreview();
		}

		void IntializeLayerPreview()
		{
			layerTemplateList.RemoveChildren();
			foreach (var resourceRenderer in worldRenderer.World.WorldActor.TraitsImplementing<IResourceRenderer>())
			{
				foreach (var resourceType in resourceRenderer.ResourceTypes)
				{
					var newResourcePreviewTemplate = ScrollItemWidget.Setup(layerPreviewTemplate,
						() => editor.CurrentBrush is StackedEditorResourceBrush brush && brush.ResourceType == resourceType,
						() => editor.SetBrush(new StackedEditorResourceBrush(editor, resourceType, worldRenderer)));

					newResourcePreviewTemplate.Bounds.X = 0;
					newResourcePreviewTemplate.Bounds.Y = 0;

					var layerPreview = newResourcePreviewTemplate.Get<ResourcePreviewWidget>("LAYER_PREVIEW");
					var size = layerPreview.IdealPreviewSize;
					layerPreview.IsVisible = () => true;
					layerPreview.ResourceType = resourceType;
					layerPreview.Bounds.Width = size.Width;
					layerPreview.Bounds.Height = size.Height;
					newResourcePreviewTemplate.Bounds.Width = size.Width + layerPreview.Bounds.X * 2;
					newResourcePreviewTemplate.Bounds.Height = size.Height + layerPreview.Bounds.Y * 2;
					newResourcePreviewTemplate.IsVisible = () => true;
					newResourcePreviewTemplate.GetTooltipText = () => resourceType;

					layerTemplateList.AddChild(newResourcePreviewTemplate);
				}
			}
		}
	}
}
