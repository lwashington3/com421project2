using System.Collections.Immutable;
using System.Linq;
using System.Collections.ObjectModel;
using Mockup.Models;

namespace Mockup.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public ObservableCollection<Location> Locations { get; set; }

	public MainWindowViewModel()
	{
		Location.OnCreated += LocationAdded;
		Locations = new ObservableCollection<Location>();
		Sort();
	}

	private void LocationAdded(Location location)
	{
		Locations.Add(location);
		Sort();
	}

	public void Sort()
	{
		var sorted = Locations.OrderBy(location => location.DistanceTo(Location.Center)).ToImmutableArray();
		Locations.Clear();
		foreach(Location location in sorted)
		{
			Locations.Add(location);
		}
	}
}