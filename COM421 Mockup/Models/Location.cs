using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Mockup.Models;

public class Location : INotifyPropertyChanged
{
	public delegate void LocationHandler(Location assignment);
	public static event LocationHandler OnCreated;

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

	[JsonPropertyName("longitude")]
	public double? LongitudeSetter
	{
		set => Longitude = value ?? 0;
	} //?? -87.626607 + ((Random.Shared.NextDouble() - 0.5) / 10000d); }

	[JsonPropertyName("latitude")]
	public double? LatitudeSetter
	{
		set => Latitude = value ?? 0;
	}
	                                                  // ?? 41.834685 + ((Random.Shared.NextDouble() - 0.5) / 1000d); }

	public string AbbreviatedID => (ID % 10000).ToString().PadLeft(4, '0');

	private static HashSet<int> ids = new();
	public Location(string name, int id, double longitude, double latitude)
	{
		Name = name;
		ID = id;
		Longitude = longitude;
		Latitude = latitude;
		if (ids.Contains(id))
		{
			Console.WriteLine($"Duplicate ID: {id}.");
		}
		else
		{
			ids.Add(id);
		}
		OnCreated?.Invoke(this);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
