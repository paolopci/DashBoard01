namespace DashboardOrders.Models.ViewModels;

public class PhonePrefixComboboxModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhonePrefix { get; set; } = string.Empty;
    public string CountryIso2 { get; set; } = string.Empty;
    public string FlagPath { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string PrefixFieldName { get; set; } = "PhonePrefix";
    public string Iso2FieldName { get; set; } = "PhoneCountryIso2";
    public string PhoneNumberFieldName { get; set; } = "PhoneNumber";
    public bool Required { get; set; }
    public bool TwoColumns { get; set; } = true;

    /// <summary>
    /// Crea un'istanza con label predefinita.
    /// </summary>
    public static PhonePrefixComboboxModel Create(
        string prefixFieldName,
        string iso2FieldName,
        string phoneNumberFieldName,
        string? prefix = null,
        string? iso2 = null,
        string? number = null,
        string? flagPath = null,
        bool required = false,
        bool twoColumns = true)
    {
        var model = new PhonePrefixComboboxModel
        {
            PhonePrefix = prefix ?? string.Empty,
            CountryIso2 = iso2 ?? string.Empty,
            PhoneNumber = number ?? string.Empty,
            FlagPath = flagPath ?? string.Empty,
            PrefixFieldName = prefixFieldName,
            Iso2FieldName = iso2FieldName,
            PhoneNumberFieldName = phoneNumberFieldName,
            Required = required,
            TwoColumns = twoColumns
        };
        model.Label = string.IsNullOrEmpty(model.PhonePrefix) && string.IsNullOrEmpty(model.CountryIso2)
            ? "Prefisso"
            : $"{model.CountryIso2} {model.PhonePrefix}";
        return model;
    }
}
