using SubmarineApp.DiagnosticReport;

internal class Program
{
    private static void Main(string[] args)
    {
        var inputText = "";
        using (var sr = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
        {
            inputText = sr.ReadToEnd();
        }

        var report = DiagnosticReport.FromText(inputText);
        try
        {
            var value = report.GetEnergyConsumption();
            Console.WriteLine(value);
        }
        catch (OverflowException)
        {
            var value = report.GetEnergyConsumptionBig();
            Console.WriteLine(value);
        }
    }
}
