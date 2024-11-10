using LiteDB;

namespace DemoConsoleApplicationWithText
{
    /// <summary>
    /// Singleton class to manage the LiteDatabase instance with proper disposal.
    /// </summary>
    public sealed class LiteDatabaseManager : IDisposable
    {
        private static readonly Lazy<LiteDatabaseManager> instance = new(() => new LiteDatabaseManager());

        public static LiteDatabaseManager Instance => instance.Value;

        public LiteDatabase Database { get; }

        public ILiteCollection<RtTranslationRecord> Collection { get; }

        private bool disposed = false;

        private LiteDatabaseManager()
        {
            // Initialize LiteDB.
            // Replace the connection string with your actual database path or configuration.
            string path = @"..\..\..\RtTranslation.db";
            Database = new LiteDatabase($"Filename={path};ReadOnly=true");
            // Get the translations collection from LiteDB.
         
            Collection = Database.GetCollection<RtTranslationRecord>("RtTranslations");
        }

        ~LiteDatabaseManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // Suppress finalization since resources have been released.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    Database?.Dispose();
                }
                // Dispose unmanaged resources if any.
                disposed = true;
            }
        }
    }
}
