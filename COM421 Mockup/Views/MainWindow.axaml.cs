using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using Mapsui.Widgets;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using Location = Mockup.Models.Location;

namespace Mockup.Views;

public partial class MainWindow : Window
{
	private IEnumerable<Location> _locations;
	private MyLocationLayer _locationLayer;

	public MainWindow()
	{
		InitializeComponent();
		MapControl mapControl = this.FindControl<MapControl>("MapControl") ?? new MapControl();
		mapControl.Map = CreateMap();
		// mapControl.Map.Navigator.ViewportChanged += NavigatorOnViewportChanged;
		mapControl.PointerReleased += NavigatorOnViewportChanged;
	}

	private void NavigatorOnViewportChanged(object? sender, PointerReleasedEventArgs e)
	{
		Viewport viewport;
		if (sender is Viewport viewport1)
		{
			viewport = viewport1;
		}
		else if (sender is Navigator navigator)
		{
			viewport = navigator.Viewport;
		}
		else if (sender is MapControl mapControl)
		{
			viewport = mapControl.Map.Navigator.Viewport;
		}
		else
		{
			return;
		}

		MPoint center = new MPoint(viewport.CenterX, viewport.CenterY);
		MPoint lonLat = SphericalMercator.ToLonLat(center);
		Location.Center = lonLat;
		// _locationLayer.UpdateMyLocation(lonLat);
	}

	public Map CreateMap()
	{
		Map map = new Map();

		map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
		map.Layers.Add(new Layer
		{
			Name = "Point",
			DataSource = new MemoryProvider(GetFeatures()),
			IsMapInfoLayer = true
		});
		_locationLayer = new MyLocationLayer(map, Location.CenterIIT)
		{
			Name = "User Location",
		};
		// FIXME: Get the user's dot on the map working, it keeps glitching to (0, 0) on the map
		// map.Layers.Add(_locationLayer);

		// TODO: Set this to the device's Location to zoom in it where the person is, also set the resolutions to ^2 for it to be closer
		map.Home = n => n.CenterOnAndZoomTo(Location.CenterIIT, n.Resolutions[^3]);
		map.Widgets.Add(new MapInfoWidget(map));

		return map;
	}

	public IEnumerable<PointFeature> GetFeatures()
	{
		return GetLocations().Select<Location, PointFeature>(location =>
		{
			PointFeature feature = new PointFeature(SphericalMercator.FromLonLat(location.Longitude, location.Latitude).ToMPoint());

			feature["Name"] = location.Name;
			feature["ID"] = location.ID;
			MemoryStream callbackImage = CreateCallbackImage(location.Name);

			feature.Styles.Add(new CalloutStyle
			{
				Content = BitmapRegistry.Instance.Register(callbackImage),
				ArrowPosition = 0.1f,
				RotateWithMap = true,
				Type = CalloutType.Custom,
				ArrowAlignment = ArrowAlignment.Bottom,
				Offset = new Offset(0, SymbolStyle.DefaultHeight * 0.5f),
				RectRadius = 10,
				ShadowWidth = 4,
				StrokeWidth = 0
			});

			return feature;
		});
	}

	public IEnumerable<Location> GetLocations()
	{
		if (_locations != null)
		{
			return _locations;
		}

		string json = System.Text.Encoding.UTF8.GetString(Properties.Resources.locations);
		List<Location> locations = JsonSerializer.Deserialize<List<Location>>(json);

		_locations = locations;
		return locations;
	}

	// https://mapsui.com/samples/
	private static MemoryStream CreateCallbackImage(string name)
	{
		using var paint = new SKPaint
		{
			Color = new SKColor(204, 0, 0),
			Typeface = SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal,
				SKFontStyleSlant.Upright),
			TextSize = 12
		};

		SKRect bounds;
		using (var textPath = paint.GetTextPath(name, 0, 0))
		{
			// Set transform to center and enlarge clip path to window height
			textPath.GetTightBounds(out bounds);
		}

		using var bitmap = new SKBitmap((int)(bounds.Width + 1), (int)(bounds.Height + 1));
		using var canvas = new SKCanvas(bitmap);
		canvas.Clear();
		canvas.DrawText(name, -bounds.Left, -bounds.Top, paint);
		var memStream = new MemoryStream();
		using (var wStream = new SKManagedWStream(memStream))
		{
			bitmap.Encode(wStream, SKEncodedImageFormat.Png, 100);
		}
		return memStream;
	}

	private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
	{
		AdjustPanelColor((StackPanel)sender, new SolidColorBrush(new Color(255, 248, 248, 248)), new SolidColorBrush(new Color(255, 7, 7, 7)));
	}

	private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		AdjustPanelColor((StackPanel)sender, new SolidColorBrush(new Color(255, 248, 248, 248)), new SolidColorBrush(new Color(255, 7, 7, 7)));
	}

	private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		AdjustPanelColor((StackPanel)sender, new SolidColorBrush(new Color(255, 7, 7, 7)), new SolidColorBrush(new Color(255, 248, 248, 248)));
	}

	private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
	{
		AdjustPanelColor((StackPanel)sender, new SolidColorBrush(new Color(255, 7, 7, 7)), new SolidColorBrush(new Color(255, 248, 248, 248)));
	}

	private void AdjustPanelColor(StackPanel panel, IBrush foreground, IBrush background)
	{
		panel.Background = background;
		StackPanel innerPanel = (StackPanel)((Border)panel.Children[1]).Child;
		((TextBlock)innerPanel.Children[0]).Foreground = foreground;
		((TextBlock)innerPanel.Children[1]).Foreground = foreground;
	}

	private void CorrectCircles(object? sender, RoutedEventArgs e)
	{
		Canvas canvas = (Canvas)sender;

		foreach (Control control in canvas.Children)
		{
			if (control is TextBlock textBlock)
			{
				TextShaper textShaper = TextShaper.Current;
				Typeface typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
				ShapedBuffer shaped = textShaper.ShapeText(textBlock.Text, new TextShaperOptions(typeface.GlyphTypeface, textBlock.FontSize));
				ShapedTextRun run = new ShapedTextRun(shaped, new GenericTextRunProperties(typeface, textBlock.FontSize));
				Canvas.SetLeft(control, (canvas.Width - run.Size.Width) / 2);
				Canvas.SetTop(control, (canvas.Height - run.Size.Height) / 2);
			}
			else
			{
				Canvas.SetLeft(control, (canvas.Width - control.Width) / 2);
				Canvas.SetTop(control, (canvas.Height - control.Height) / 2);
			}
		}

		// ItemsControl itemsControl = this.GetControl<ItemsControl>("LocationsControl");
		// foreach (ILogical child in itemsControl.GetLogicalChildren())
		// {
		// 	IAvaloniaReadOnlyList<ILogical> logicalChildren = child.LogicalChildren;
		// 	Canvas canvas = (Canvas) logicalChildren[0].LogicalChildren[0];
		// 	foreach (Control control in canvas.Children)
		// 	{
		// 		if (control is TextBlock textBlock)
		// 		{
		// 			TextShaper textShaper = TextShaper.Current;
		// 			Typeface typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
		// 			ShapedBuffer shaped = textShaper.ShapeText(textBlock.Text, new TextShaperOptions(typeface.GlyphTypeface, textBlock.FontSize));
		// 			ShapedTextRun run = new ShapedTextRun(shaped, new GenericTextRunProperties(typeface, textBlock.FontSize));
		// 			Canvas.SetLeft(control, (canvas.Width - run.Size.Width) / 2);
		// 			Canvas.SetTop(control, (canvas.Height - run.Size.Height) / 2);
		// 		}
		// 		else
		// 		{
		// 			Canvas.SetLeft(control, (canvas.Width - control.Width) / 2);
		// 			Canvas.SetTop(control, (canvas.Height - control.Height) / 2);
		// 		}
		// 	}
		// }
	}
}