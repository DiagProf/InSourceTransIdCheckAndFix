using LiteDB;

namespace DemoConsoleApplicationWithText
{
    /// Represents a translation record in the database.
    /// </summary>
    public class RtTranslationRecord
    {
        //[BsonId]
        public required string TextId { get; set; }
        public required string English { get; set; }
        public required string German { get; set; }
        public required string French { get; set; }
        public required string Spanish { get; set; }
        public required string Italian { get; set; }
        public required string Danish { get; set; }
        public required string Polish { get; set; }
        public required string Russian { get; set; }
        public required string Chinese { get; set; }
    }
}
