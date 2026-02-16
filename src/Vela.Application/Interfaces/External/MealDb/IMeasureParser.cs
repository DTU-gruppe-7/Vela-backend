namespace Vela.Application.Interfaces.External.MealDb;

public interface IMeasureParser
{
    (double Quantity, string Unit) Parse(string measure);
}