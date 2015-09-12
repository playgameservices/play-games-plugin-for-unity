/*
 * Copyright (C) 2014 Google Inc.
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

namespace QuizRacer.GameLogic
{
    using GooglePlayGames;
    using GooglePlayGames.BasicApi.Multiplayer;

    public class InvitationManager
    {
        private static InvitationManager sInstance = new InvitationManager();

        public static InvitationManager Instance
        {
            get
            {
                return sInstance;
            }
        }

        private Invitation mInvitation = null;
        private bool mShouldAutoAccept = false;

        public void OnInvitationReceived(Invitation inv, bool shouldAutoAccept)
        {
            mInvitation = inv;
            mShouldAutoAccept = shouldAutoAccept;
        }

        public Invitation Invitation
        {
            get
            {
                return mInvitation;
            }
        }

        public bool ShouldAutoAccept
        {
            get
            {
                return mShouldAutoAccept;
            }
        }

        public void DeclineInvitation()
        {
            if (mInvitation != null)
            {
                PlayGamesPlatform.Instance.RealTime.DeclineInvitation(mInvitation.InvitationId);
            }
            Clear();
        }

        public void Clear()
        {
            mInvitation = null;
            mShouldAutoAccept = false;
        }
    }
}