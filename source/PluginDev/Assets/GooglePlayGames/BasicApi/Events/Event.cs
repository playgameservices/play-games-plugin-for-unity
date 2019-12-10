namespace GooglePlayGames.BasicApi.Events
{
    internal class Event : IEvent
    {
        private string mId;
        private string mName;
        private string mDescription;
        private string mImageUrl;
        private ulong mCurrentCount;
        private EventVisibility mVisibility;

        internal Event(string id, string name, string description, string imageUrl,
            ulong currentCount, EventVisibility visibility)
        {
            mId = id;
            mName = name;
            mDescription = description;
            mImageUrl = imageUrl;
            mCurrentCount = currentCount;
            mVisibility = visibility;
        }

        public string Id
        {
            get { return mId; }
        }

        public string Name
        {
            get { return mName; }
        }

        public string Description
        {
            get { return mDescription; }
        }

        public string ImageUrl
        {
            get { return mImageUrl; }
        }

        public ulong CurrentCount
        {
            get { return mCurrentCount; }
        }

        public EventVisibility Visibility
        {
            get { return mVisibility; }
        }
    }
}