using System.Globalization;
using System.Text.RegularExpressions;
using Vela.Application.Interfaces.External.MealDb;

namespace Vela.Infrastructure.External.MealDb;

public class MeasureParser : IMeasureParser
{
    public (double Quantity, string Unit) Parse(string measure)
    {
        if (string.IsNullOrWhiteSpace(measure))
            return (0, "");
        
        measure = measure.Trim();
        var match = Regex.Match(measure, @"^([\d\.,/]+)\s*(.*)");
        
        if (!match.Success)
            return (0,measure);
        
        string numberPart = match.Groups[1].Value;
        double Quantity = ParseNumber(numberPart);
        string unit = match.Groups[2].Value.Trim();
        return (Quantity, unit);
    }

    private double ParseNumber(string numberPart)
    {
        if (numberPart.Contains("/"))
        {
            var parts = numberPart.Split('/');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double numerator) &&
                double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double denominator) &&
                denominator != 0)
            {
                return numerator / denominator;
            }

            return 0;
        }
        double.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);
        return result;
    }
}