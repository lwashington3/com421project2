using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;

namespace Mockup.Models;

public class Location : INotifyPropertyChanged
{
	public delegate void LocationHandler(Location assignment);
	public static event LocationHandler OnCreated;
	public static MPoint CenterIIT => SphericalMercator.FromLonLat(-87.626607, 41.834685).ToMPoint();
	public static MPoint Center { get; set; } = CenterIIT;

	private int _id;

	[JsonPropertyName("name")]
	public string Name { get; private set; }

	[JsonPropertyName("id")]
	public int ID
	{
		get => _id;
		private set
		{
			_id = value;
			OnPropertyChanged(nameof(AbbreviatedID));
		}
	}

	// [JsonPropertyName("longitude")]
	public double Longitude { get; private set; }

	// [JsonPropertyName("latitude")]
	public double Latitude { get; private set; }

	// These are only here to deal with null lon/lat values since I have not added all the locations. Once they're all in, these 2 repeated properties can be deleted.
	[JsonPropertyName("longitude")]
	public double? LongitudeSetter
	{
		set => Longitude = value ?? 0;
	} //?? -87.626607 + ((Random.Shared.NextDouble() - 0.5) / 10000d); }

	[JsonPropertyName("latitude")]
	public double? LatitudeSetter
	{
		set => Latitude = value ?? 0;
	} // ?? 41.834685 + ((Random.Shared.NextDouble() - 0.5) / 1000d); }

	public string AbbreviatedID => (ID % 10000).ToString().PadLeft(4, '0');

	public Location(string name, int id, double longitude, double latitude)
	{
		Name = name;
		ID = id;
		LongitudeSetter = longitude;
		LatitudeSetter = latitude;
		OnCreated?.Invoke(this);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public override bool Equals(object? obj)
	{
		if (obj is not Location location)
		{
			return false;
		}
		return location.Longitude == Longitude && location.Latitude == Latitude;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Longitude, Latitude);
	}

	public override string ToString()
	{
		return $"{Name} [{ID}] ({Longitude}, {Latitude})";
	}

	public double DistanceTo(MPoint point)
	{
		return Math.Sqrt(Math.Pow(point.X - Longitude, 2) + Math.Pow(point.Y - Latitude, 2));
	}

	public static bool operator <(Location a, Location b)
	{
		return a.DistanceTo(Center) < b.DistanceTo(Center);
	}

	public static bool operator >(Location a, Location b)
	{
		return a.DistanceTo(Center) > b.DistanceTo(Center);
	}
}
