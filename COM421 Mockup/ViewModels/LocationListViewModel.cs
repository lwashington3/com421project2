using System;
using System.Collections.ObjectModel;
using Mockup.Models;

namespace Mockup.ViewModels;

public class LocationListViewModel : ViewModelBase
{
	public ObservableCollection<Location> Locations { get; set; }

	public LocationListViewModel()
	{
		Location.OnCreated += LocationAdded;
		Locations = new ObservableCollection<Location>();
	}

	private void LocationAdded(Location location)
	{
		Locations.Add(location);
	}
}