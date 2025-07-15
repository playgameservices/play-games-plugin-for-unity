using System;

namespace GooglePlayGames.BasicApi.Events
{
    /// <summary>
    /// @deprecated This class will be removed in the future. We recommend that you migrate to the Play Games Services Unity Plugin (v2).
    /// </summary>
    internal class Event : IEvent
    {
        private string mId;
        private string mName;
        private string mDescription;
        private string mImageUrl;
        private ulong mCurrentCount;
        private EventVisibility mVisibility;

        /// <summary>
        /// Initializes a new instance of the Event class.
        /// </summary>
        /// <remarks>
        /// @deprecated This constructor will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
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

        /// <summary>
        /// Gets the ID of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string Id
        {
            get { return mId; }
        }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string Name
        {
            get { return mName; }
        }

        /// <summary>
        /// Gets the description of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string Description
        {
            get { return mDescription; }
        }

        /// <summary>
        /// Gets the image URL associated with the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string ImageUrl
        {
            get { return mImageUrl; }
        }

        /// <summary>
        /// Gets the current count of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public ulong CurrentCount
        {
            get { return mCurrentCount; }
        }

        /// <summary>
        /// Gets the visibility of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public EventVisibility Visibility
        {
            get { return mVisibility; }
        }
    }
}
