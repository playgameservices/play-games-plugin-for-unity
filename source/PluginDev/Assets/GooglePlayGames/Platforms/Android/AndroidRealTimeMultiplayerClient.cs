#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.OurUtils;
    using UnityEngine;

    internal class AndroidRealTimeMultiplayerClient : IRealTimeMultiplayerClient
    {
        private volatile AndroidJavaObject mClient;

        public AndroidRealTimeMultiplayerClient(AndroidJavaObject account) 
        {
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games")) 
            {
                mClient = gamesClass.CallStatic<AndroidJavaObject>("getRealTimeMultiplayerClient", AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
                            RealTimeMultiplayerListener listener)
        {
        }

        public void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitMask,
            RealTimeMultiplayerListener listener)
        {
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOppponents, uint variant,
                                        RealTimeMultiplayerListener listener)
        {
        }

        public void ShowWaitingRoomUI()
        {
        }

        public void GetAllInvitations(Action<Invitation[]> callback)
        {
        }

        public void AcceptFromInbox(RealTimeMultiplayerListener listener)
        {
        }

        public void AcceptInvitation(string invitationId, RealTimeMultiplayerListener listener)
        {
        }

        public void SendMessageToAll(bool reliable, byte[] data)
        {
        }

        public void SendMessageToAll(bool reliable, byte[] data, int offset, int length)
        {
        }

        public void SendMessage(bool reliable, string participantId, byte[] data)
        {
        }

        public void SendMessage(bool reliable, string participantId, byte[] data, int offset, int length)
        {
        }

        public List<Participant> GetConnectedParticipants()
        {
            return null;
        }

        public Participant GetSelf()
        {
            return null;           
        }

        public Participant GetParticipant(string participantId)
        {
            return null;           
        }

        public Invitation GetInvitation()
        {
            return null;           
        }

        public void LeaveRoom()
        {
        }

        public bool IsRoomConnected()
        {
            return false;
        }

        public void DeclineInvitation(string invitationId)
        {
        }
    }
}
#endif

