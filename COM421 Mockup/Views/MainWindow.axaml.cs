using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Avalonia.Controls;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using Mapsui.Widgets;
using SkiaSharp;
using Location = Mockup.Models.Location;

namespace Mockup.Views;

public partial class MainWindow : Window
{
	private IEnumerable<Location> _locations;

	public MainWindow()
	{
		InitializeComponent();
		MapControl mapControl = this.FindControl<MapControl>("MapControl") ?? new MapControl();
		mapControl.Map = CreateMap();
		// Content = mapControl;
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

		// TODO: Set this to the device's Location to zoom in it where the person is, also set the resolutions to ^2 for it to be closer
		map.Home = n => n.CenterOnAndZoomTo(SphericalMercator.FromLonLat(-87.626607, 41.834685).ToMPoint(), n.Resolutions[^3]);
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
}