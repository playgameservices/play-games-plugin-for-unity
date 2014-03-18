/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if UNITY_ANDROID
using System;

namespace GooglePlayGames.Android {
    internal class JavaConsts {
        // GameHelper client request flags
        public const int GAMEHELPER_CLIENT_ALL = 7;    
    
        // achievement states
        public const int STATE_HIDDEN = 2;
        public const int STATE_REVEALED = 1;
        public const int STATE_UNLOCKED = 0;

        // achievement types
        public const int TYPE_INCREMENTAL = 1;
        public const int TYPE_STANDARD = 0;

        // status codes
        public const int STATUS_OK = 0;
        public const int STATUS_STALE_DATA = 3;
        public const int STATUS_NO_DATA = 4;
        public const int STATUS_DEFERRED = 5;
        public const int STATUS_KEY_NOT_FOUND = 2002;
        public const int STATUS_CONFLICT = 2000;
        
        public const int SDK_VARIANT = 37143;
        
        // GmsCore base package name
        public const string GmsPkg = "com.google.android.gms";
        
        // Useful classes in GmsCore:
        public const string ResultCallbackClass = GmsPkg + ".common.api.ResultCallback";
        public const string RoomStatusUpdateListenerClass = 
                GmsPkg + ".games.multiplayer.realtime.RoomStatusUpdateListener";
        public const string RoomUpdateListenerClass = 
                GmsPkg + ".games.multiplayer.realtime.RoomUpdateListener";
        public const string RealTimeMessageReceivedListenerClass =
                GmsPkg + ".games.multiplayer.realtime.RealTimeMessageReceivedListener";
        public const string OnInvitationReceivedListenerClass =
                GmsPkg + ".games.multiplayer.OnInvitationReceivedListener";
        public const string ParticipantResultClass = 
                GmsPkg + ".games.multiplayer.ParticipantResult";
        
        // Plugin support package name
        public const string PluginSupportPkg = "com.google.example.games.pluginsupport";
        
        // Useful classes in plugin support library
        public const string SupportRtmpUtilsClass = PluginSupportPkg + ".RtmpUtils";
        public const string SupportTbmpUtilsClass = PluginSupportPkg + ".TbmpUtils";
        public const string SupportSelectOpponentsHelperActivity = PluginSupportPkg + 
                ".SelectOpponentsHelperActivity";
        public const string SupportSelectOpponentsHelperActivityListener =
                SupportSelectOpponentsHelperActivity + "$Listener";
        public const string SupportInvitationInboxHelperActivity = PluginSupportPkg + 
            ".InvitationInboxHelperActivity";
        public const string SupportInvitationInboxHelperActivityListener =
            SupportInvitationInboxHelperActivity + "$Listener";
        public const string SignInHelperManagerClass = PluginSupportPkg + ".SignInHelperManager";
        
        // participant status
        public const int STATUS_NOT_INVITED_YET = 0;
        public const int STATUS_INVITED = 1;
        public const int STATUS_JOINED = 2;
        public const int STATUS_DECLINED = 3;
        public const int STATUS_LEFT = 4;
        public const int STATUS_FINISHED = 5;
        public const int STATUS_UNRESPONSIVE = 6;
        
        // invitation types
        public const int INVITATION_TYPE_REAL_TIME = 0;
        public const int INVITATION_TYPE_TURN_BASED = 1;

        // match status
        public const int MATCH_STATUS_AUTO_MATCHING = 0;
        public const int MATCH_STATUS_ACTIVE = 1;
        public const int MATCH_STATUS_COMPLETE = 2;
        public const int MATCH_STATUS_EXPIRED = 3;
        public const int MATCH_STATUS_CANCELED = 4;

        // match turn status
        public const int MATCH_TURN_STATUS_INVITED = 0;
        public const int MATCH_TURN_STATUS_MY_TURN = 1;
        public const int MATCH_TURN_STATUS_THEIR_TURN = 2;
        public const int MATCH_TURN_STATUS_COMPLETE = 3;

        // match variant
        public const int MATCH_VARIANT_ANY = -1;

        // match participant result codes
        public const int MATCH_RESULT_UNINITIALIZED = -1;
        public const int PLACING_UNINITIALIZED = -1;
        public const int MATCH_RESULT_WIN = 0;
        public const int MATCH_RESULT_LOSS = 1;
        public const int MATCH_RESULT_TIE = 2;
        public const int MATCH_RESULT_NONE = 3;
        public const int MATCH_RESULT_DISCONNECT = 4;
        public const int MATCH_RESULT_DISAGREED = 5;
    }
}
#endif
