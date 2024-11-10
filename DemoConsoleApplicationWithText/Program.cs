namespace DemoConsoleApplicationWithText;

internal class Program
{
    private static void OnProcessExit(object? sender, EventArgs e)
    {
        // Dispose of the LiteDatabase when the process exits.
        LiteDatabaseManager.Instance.Dispose();
    }

    private static void Main(string[] args)
    {
        // Hook into the process exit event to ensure disposal.
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        // Display available languages, including 'Developer'
        Console.WriteLine("Available Languages:");
        foreach (var lang in Trs.AvailableLanguages)
        {
            Console.WriteLine("- {0}",lang);
        }


        // Set the desired language to 'Developer'
        Trs.CurrentLanguage = "Developer";

        // Create a Trs instance
        var inputOpen = Trs.On("26E25DC1", "Input open"); //DevText okay got translation id from external Tool
        var selfTest  = Trs.On("979C84DA", "Self-test"); //DevText okay got translation id from external Tool
        var ethernetTransceiver = Trs.On("AB0B6A64", "Ethernet transceiver"); //DevText okay got translation id from external Tool
        var initiation = Trs.ToDo ("Initiation"); //is in DB as pending translation
        var fullAreaAnimation = Trs.ToDo("Full area animation");  //not in DB
        var redWarningLamp = Trs.On("56711F47", "Red warning lamp"); //DevText okay got translation id from external Tool
        var bluetooth = Trs.Off("Bluetooth"); //No need for translation


        // Use Trs as a string (developer text)
        Console.WriteLine(redWarningLamp); // Outputs: "Red warning lamp"   -> Dev Text

        // Get the display name should return developer text because no language set
        Console.WriteLine(redWarningLamp.DisplayName); // Outputs: "Red warning lamp"

        // Change the language to German
        Trs.CurrentLanguage = "German";

        // The cached translation is invalidated due to language change
        Console.WriteLine(redWarningLamp.DisplayName); // Outputs: "Rote Warnlampe" or falls back appropriately

        // Change the language to Spanish
        Trs.CurrentLanguage = "Spanish";

        // DisplayName now returns the Spanish text
        Console.WriteLine(redWarningLamp.DisplayName); // Outputs: "Testigo de advertencia rojo"



        ////(Trs\.\s*ToDo\s*\(\s*")(([^"]*))("\s*\))
       
        }
    }

