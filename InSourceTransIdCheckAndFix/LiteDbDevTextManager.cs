using LiteDB;

namespace InSourceTransIdCheckAndFix
{
    public class LiteDbDevTextManager : IDevTextManager
    {
        private static readonly object LockObject = new();
        private readonly string _databasePath;

        public LiteDbDevTextManager(string databasePath)
        {
            _databasePath = databasePath;
        }

        public string? GetKeyForText(string text)
        {
            lock ( LockObject )
            {
                using ( var db = new LiteDatabase(_databasePath) )
                {
                    var collection = db.GetCollection<DevTextToKeyMapperRecord>("DevTextToKeyMapper");
                    var record = collection.FindOne(r => r.DeveloperText.Equals(text));
                    return record?.TextId;
                }
            }
        }

        public bool VerifyKeyMatchesText(string key, string text)
        {
            lock ( LockObject )
            {
                using ( var db = new LiteDatabase(_databasePath) )
                {
                    var collection = db.GetCollection<DevTextToKeyMapperRecord>("DevTextToKeyMapper");
                    var record = collection.FindOne(r => r.TextId.Equals(key));
                    if ( record != null )
                    {
                        record.DateLastDeveloperTextSeen = DateTime.Now;
                        collection.Update(record);
                    }

                    return record != null && record.DeveloperText == text;
                }
            }
        }

        public void AddNewTranslationRequest(string text)
        {
            lock ( LockObject )
            {
                using ( var db = new LiteDatabase(_databasePath) )
                {
                    var collection = db.GetCollection<NewDeveloperTextRecord>("NewDeveloperText");

                    // Check if the text already exists in the pending translations
                    if ( collection.Exists(r => r.DeveloperText == text) )
                    {
                        return;
                    }

                    var newRecord = new NewDeveloperTextRecord
                    {
                        DeveloperText = text
                    };

                    collection.Insert(newRecord);
                }
            }
        }

        public List<string> GetPendingTranslations()
        {
            lock ( LockObject )
            {
                using ( var db = new LiteDatabase(_databasePath) )
                {
                    var collection = db.GetCollection<NewDeveloperTextRecord>("NewDeveloperText");
                    var records = collection.FindAll();

                    var pendingTexts = new List<string>();
                    foreach ( var record in records )
                    {
                        pendingTexts.Add(record.DeveloperText);
                    }

                    return pendingTexts;
                }
            }
        }

        public bool IsPendingTranslationDevText(string devText)
        {
            lock ( LockObject )
            {
                using ( var db = new LiteDatabase(_databasePath) )
                {
                    var collection = db.GetCollection<NewDeveloperTextRecord>("NewDeveloperText");
                    var record = collection.FindOne(r => r.DeveloperText.Equals(devText));
                    return record != null;
                }
            }
        }

        public void UpdateTranslation(string key, string text)
        {
            lock ( LockObject )
            {
                using ( var db = new LiteDatabase(_databasePath) )
                {
                    var collection = db.GetCollection<DevTextToKeyMapperRecord>("DevTextToKeyMapper");
                    var existingRecord = collection.FindOne(r => r.TextId.Equals(key));

                    if ( existingRecord != null )
                    {
                        existingRecord.DeveloperText = text;
                        collection.Update(existingRecord);
                    }
                    else
                    {
                        var newRecord = new DevTextToKeyMapperRecord
                        {
                            TextId = key,
                            DeveloperText = text
                        };
                        collection.Insert(newRecord);
                    }
                }
            }
        }

        // class to represent records in the LiteDB collections
        private class DevTextToKeyMapperRecord
        {
            [BsonId] 
            internal ObjectId Id { get; set; }

            public string TextId { get; set; } = string.Empty;
            public string DeveloperText { get; set; } = string.Empty;
            public DateTime DateLastDeveloperTextSeen { get; set; } // Used to remove unused developer texts later.
        }

        private class NewDeveloperTextRecord
        {
            [BsonId] 
            internal ObjectId Id { get; set; }

            public string DeveloperText { get; set; } = string.Empty;
        }
    }
}
