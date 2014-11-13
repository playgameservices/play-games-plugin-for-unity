
namespace GooglePlayGames.BasicApi {
	internal class QuestsConsts {
		// used to retrieve extended data from the intent
		public const string EXTRA_QUEST = "quest";
		
		// quest return ordering
		public const int SORT_ORDER_ENDING_SOON_FIRST      = 1;
		public const int SORT_ORDER_RECENTLY_UPDATED_FIRST = 0;
		
		// quest type selectors
		public const int SELECT_UPCOMING  = 0x01;
		public const int SELECT_OPEN      = 0x02;
		public const int SELECT_ACCEPTED  = 0x03;
		public const int SELECT_COMPLETED = 0x04;
		public const int SELECT_EXPIRED   = 0x05;
		public const int SELECT_FAILED    = 0x06;
		public const int SELECT_COMPLETED_UNCLAIMED = 0x65;
		public const int SELECT_ENDING_SOON         = 0x66;
		public const int SELECT_RECENTLY_FAILED     = 0x67;
		
		// every quest type
		public static readonly int[] SELECT_ALL_QUESTS = { 
			SELECT_UPCOMING,
			SELECT_OPEN,
			SELECT_ACCEPTED,
			SELECT_COMPLETED,
			SELECT_EXPIRED,
			SELECT_FAILED,
			SELECT_COMPLETED_UNCLAIMED,
			SELECT_ENDING_SOON,
			SELECT_RECENTLY_FAILED
		};
		
		// quest states
		public const int STATE_UPCOMING  = 0x01;
		public const int STATE_OPEN      = 0x02;
		public const int STATE_ACCEPTED  = 0x03;
		public const int STATE_COMPLETED = 0x04;
		public const int STATE_EXPIRED   = 0x05;
		public const int STATE_FAILED    = 0x06;
		
		// the default value for Quest related timestamps when they aren't set by the server. 
		public const int UNSET_QUEST_TIMESTAMP = -1;
		
		// every quest state
		public static readonly int[] QUEST_STATE_ALL = {
			STATE_UPCOMING,
			STATE_OPEN,
			STATE_ACCEPTED,
			STATE_COMPLETED,
			STATE_EXPIRED,
			STATE_FAILED
		};
		
		// milestone states
		public const int STATE_NOT_STARTED   = 0x01;
		public const int STATE_NOT_COMPLETED = 0x02;
		public const int STATE_COMPLETED_NOT_CLAIMED = 0x03;
		public const int STATE_CLAIMED = 0x04;
	}
}