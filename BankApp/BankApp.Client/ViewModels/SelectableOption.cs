namespace BankApp.Client.ViewModels
{
    public class SelectableOption
    {
        public SelectableOption(string value, string label)
        {
            Value = value;
            Label = label;
        }

        public string Value { get; }

        public string Label { get; }
    }
}
