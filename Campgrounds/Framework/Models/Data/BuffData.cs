namespace Campgrounds.Framework.Models.Data
{
    public class BuffData
    {
        /// <summary>
        /// The default buff duration in milliseconds (7 minutes) applied when a buff does not specify its own duration
        /// </summary>
        public const int DEFAULT_DURATION = 420000;

        public string Id { get; set; }
        public int Duration { get; set; }
    }
}
