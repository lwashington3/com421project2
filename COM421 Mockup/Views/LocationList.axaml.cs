using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Mockup.ViewModels;

namespace Mockup.Views;

public partial class LocationList : UserControl
{
	public LocationList()
	{
		InitializeComponent();
		DataContext = new LocationListViewModel();
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);
	}
}