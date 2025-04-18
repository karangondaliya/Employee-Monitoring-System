using Microsoft.Maui.Controls;

namespace Employee_Monitoring_System.Views
{
    public partial class ProjectsPage : ContentPage
    {
        public ProjectsPage()
        {
            InitializeComponent();

            // Force the CollectionView to always use span=2
            this.SizeChanged += (s, e) =>
            {
                if (ProjectsCollectionView?.ItemsLayout is GridItemsLayout gridLayout)
                {
                    // Always set span to 2, regardless of screen size
                    gridLayout.Span = 2;
                }
            };
        }
    }
}