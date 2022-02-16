﻿using CommunityToolkit.Core.Platform;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Handlers;

namespace CommunityToolkit.Core.Handlers;
public partial class PopupViewHandler : ElementHandler<IPopup, MCTPopup>
{
	/// <summary>
	/// Action that's triggered when the Popup is Dismissed.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupViewHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	/// <param name="result">The result that should return from this Popup.</param>
	public static async void MapOnDismissed(PopupViewHandler handler, IPopup view, object? result)
	{
		if (handler.NativeView is null)
		{
			return;
		}

		var vc = handler.NativeView.ViewController;
		if (vc is not null)
		{
			await vc.DismissViewControllerAsync(true);
		}

		handler.DisconnectHandler(handler.NativeView);
	}

	/// <summary>
	/// Action that's triggered when the Popup is Opened.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupViewHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	/// <param name="result">We don't need to provide the result parameter here.</param>
	public static void MapOnOpened(PopupViewHandler handler, IPopup view, object? result)
	{
		view.OnOpened();
	}

	/// <summary>
	/// Action that's triggered when the Popup is LightDismissed.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupViewHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	/// <param name="result">The result that should return from this Popup.</param>
	public static void MapOnLightDismiss(PopupViewHandler handler, IPopup view, object? result)
	{
		if (handler.NativeView is not MCTPopup popupRenderer)
		{
			return;
		}

		if (popupRenderer.IsViewLoaded && view.IsLightDismissEnabled)
		{
			view.LightDismiss();
		}
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.Anchor"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupViewHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>

	public static void MapAnchor(PopupViewHandler handler, IPopup view)
	{
		handler.NativeView?.SetSize(view);
		handler.NativeView?.SetLayout(view);
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.IsLightDismissEnabled"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupViewHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapLightDismiss(PopupViewHandler handler, IPopup view)
	{
		handler.NativeView?.SetLightDismiss(view);
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.Color"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupViewHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapColor(PopupViewHandler handler, IPopup view)
	{
		handler.NativeView?.SetBackgroundColor(view);
	}

	/// <summary>
	/// Action that's triggered when the Popup <see cref="IPopup.Size"/> property changes.
	/// </summary>
	/// <param name="handler">An instance of <see cref="PopupViewHandler"/>.</param>
	/// <param name="view">An instance of <see cref="IPopup"/>.</param>
	public static void MapSize(PopupViewHandler handler, IPopup view)
	{
		handler.NativeView?.SetSize(view);
		handler.NativeView?.SetLayout(view);
	}

	/// <inheritdoc/>
	protected override void ConnectHandler(MCTPopup nativeView)
	{
		base.ConnectHandler(nativeView);
		nativeView.SetElement(VirtualView);
	}

	/// <inheritdoc/>
	protected override MCTPopup CreateNativeElement()
	{
		return new MCTPopup(MauiContext ?? throw new NullReferenceException(nameof(MauiContext)));
	}

	/// <inheritdoc/>
	protected override void DisconnectHandler(MCTPopup nativeView)
	{
		base.DisconnectHandler(nativeView);
		nativeView?.CleanUp();
	}
}