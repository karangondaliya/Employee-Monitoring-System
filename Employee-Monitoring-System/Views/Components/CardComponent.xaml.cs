namespace Employee_Monitoring_System.Views.Components
{
    public partial class CardComponent : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CardComponent), string.Empty);

        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(string), typeof(CardComponent), string.Empty);

        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(CardComponent), string.Empty);

        public static readonly BindableProperty AdditionalTextProperty =
            BindableProperty.Create(nameof(AdditionalText), typeof(string), typeof(CardComponent), string.Empty);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string AdditionalText
        {
            get => (string)GetValue(AdditionalTextProperty);
            set => SetValue(AdditionalTextProperty, value);
        }

        public CardComponent()
        {
            InitializeComponent();
            this.BindingContextChanged += (s, e) =>
            {
                Console.WriteLine($"CardComponent Data - Title: {Title}, Value: {Value}, Icon: {Icon}, AdditionalText: {AdditionalText}");
            };
        }
    }
}