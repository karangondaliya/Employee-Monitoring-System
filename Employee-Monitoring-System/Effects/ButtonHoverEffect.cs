using Microsoft.Maui.Handlers;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
#endif

namespace Employee_Monitoring_System.Effects
{
    public static class ButtonHoverEffect
    {
        public static void Initialize()
        {
#if WINDOWS
            ButtonHandler.Mapper.AppendToMapping("HoverButton", (handler, view) =>
            {
                if (handler.PlatformView is Microsoft.UI.Xaml.Controls.Button button)
                {
                    button.PointerEntered += OnPointerEntered;
                    button.PointerExited += OnPointerExited;
                }
            });
#endif
        }

#if WINDOWS
        private static void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Button button)
            {
                // Apply hover effect
                button.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.ForestGreen);
                button.Scale = new System.Numerics.Vector3(1.05f, 1.05f, 1);
            }
        }

        private static void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.Button button)
            {
                // Restore original appearance
                button.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.MediumSeaGreen);
                button.Scale = new System.Numerics.Vector3(1f, 1f, 1);
            }
        }
#endif
    }
}