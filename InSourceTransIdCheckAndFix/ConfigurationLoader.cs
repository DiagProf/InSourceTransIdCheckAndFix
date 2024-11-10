
using System.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text.Json;

namespace InSourceTransIdCheckAndFix
{
    public static class ConfigurationLoader
    {
        private static readonly object LockObject = new object();
        private static volatile bool _isInitialized = false;

        private static volatile DevTextToKeyMapperConfig? _configBackUp = null;


        // Method to initialize configuration using Additional Files
        public static bool TryLoad(AnalyzerOptions options, out DevTextToKeyMapperConfig? loadedConfig)
        {
            
            loadedConfig = null;

            if (_isInitialized)
            {
                loadedConfig = _configBackUp;
                return true; // Already initialized
            }

            lock (LockObject)
            {
                if (_isInitialized)
                {
                    loadedConfig = _configBackUp;
                    return true; // Double-check after locking
                }

                // Load the configuration from Additional Files
                //Here, the JSON file located under the solution is loaded and set as AdditionalFiles using the Directory.Build.props.
                /// YourSolution
                //    ├── InSourceTransIdCheckAndFixConfig.json
                //    ├── Directory.Build.props
                //    ├── YourSolution.sln
                //    ├── Project1
                //    └── ... (other projects and files)


                const string configFileName = "InSourceTransIdCheckAndFixConfig.json"; // Replace with your actual configuration file name
                
                var additionalFile = options.AdditionalFiles.FirstOrDefault(file => Path.GetFileName(file.Path) == configFileName);

                if (additionalFile != null)
                {
                    try
                    {
                        var configText = additionalFile.GetText()?.ToString();
                        if (configText != null && !string.IsNullOrEmpty(configText))
                        {
                            loadedConfig = JsonSerializer.Deserialize<DevTextToKeyMapperConfig>(configText);
                            _configBackUp = loadedConfig;

                            if (_configBackUp != null)
                            {
                                _configBackUp.DevTextManager = new LiteDbDevTextManager(_configBackUp.DbDevTextManagerImple);
                                _isInitialized = true; // Mark as initialized
                                return true;
                            }

                            Debug.WriteLine($"Failed to deserialize DevTextToKeyMapperConfig from Additional File: {configFileName}");
                            return false;
                        }

                        Debug.WriteLine($"The Additional File '{configFileName}' is empty or could not be read.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to load DevTextToKeyMapperConfig from Additional File '{configFileName}': {ex.Message}");
                        return false;
                    }
                }

                Debug.WriteLine($"Configuration file '{configFileName}' not found in Additional Files. Using default configuration.");
                return false;
            }
        }
    }

}
