namespace Mockup.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	public LocationListViewModel LocationList { get; set;  }

	public MainWindowViewModel()
	{
		LocationList = new LocationListViewModel();
	}
}