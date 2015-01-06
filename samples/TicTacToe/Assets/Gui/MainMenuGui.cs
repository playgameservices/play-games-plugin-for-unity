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

using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;

public class MainMenuGui : BaseGui {
    WidgetConfig TitleCfg = new WidgetConfig(0.0f, -0.4f, 1.0f, 0.2f, 100, "Tic Tac Toss");
    WidgetConfig QuickMatchCfg = new WidgetConfig(0.0f, -0.2f, 0.8f, 0.2f, 60, "Random opponent!");
    WidgetConfig InviteCfg = new WidgetConfig(0.0f, 0.05f, 0.8f, 0.2f, 60, "Invite someone!");
    WidgetConfig InboxCfg = new WidgetConfig(0.0f, 0.30f, 0.8f, 0.2f, 60, "Show my inbox!");
    WidgetConfig SignOutCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Bottom, 0.2f, -0.15f, 0.4f, 0.15f,
            TextAnchor.MiddleCenter, 45, "Sign Out");
    WidgetConfig OkButtonCfg = new WidgetConfig(0.0f, 0.4f, 0.4f, 0.2f, 60, "OK");
    WidgetConfig AcceptButtonCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Bottom, 0.25f, -0.3f, 0.4f, 0.2f,
                                                    TextAnchor.MiddleCenter, 60, "Accept");
    WidgetConfig DeclineButtonCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Bottom, -0.25f, -0.3f, 0.4f, 0.2f,
                                                    TextAnchor.MiddleCenter, 60, "Decline");
    WidgetConfig PlayButtonCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Bottom, 0.25f, -0.3f, 0.4f, 0.2f,
                                                    TextAnchor.MiddleCenter, 60, "Play!");
    WidgetConfig NotNowButtonCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Bottom, -0.25f, -0.3f, 0.4f, 0.2f,
                                                    TextAnchor.MiddleCenter, 60, "Not Now");

    private string mErrorMessage = null;

    private const int Opponents = 1;
    private const int Variant = 0;

    // the match the player is being offered to play right now
    TurnBasedMatch mIncomingMatch = null;
    Invitation mIncomingInvite = null;

    public void Start() {
        PlayGamesPlatform.Instance.TurnBased.RegisterMatchDelegate(OnGotMatch);
        PlayGamesPlatform.Instance.RegisterInvitationDelegate(OnGotInvitation);
    }

    public void Update() {

    }

    protected void OnMatchStarted(bool success, TurnBasedMatch match) {
        EndStandBy();
        if (!success) {
            mErrorMessage = "There was a problem setting up the match.\nPlease try again.";
            return;
        }

        gameObject.GetComponent<PlayGui>().LaunchMatch(match);
    }


    public void HandleMatchTurn(TurnBasedMatch match, bool shouldAutoLaunch) {
        MakeActive();
        OnGotMatch(match, shouldAutoLaunch);
    }

    protected void OnGotMatch(TurnBasedMatch match, bool shouldAutoLaunch) {
        if (shouldAutoLaunch) {
          // if shouldAutoLaunch is true, we know the user has indicated (via an external UI)
          // that they wish to play this match right now, so we take the user to the
          // game screen without further delay:
            OnMatchStarted(true, match);
        } else {
          // if shouldAutoLaunch is false, this means it's not clear that the user
          // wants to jump into the game right away (for example, we might have received
          // this match from a background push notification). So, instead, we will
          // calmly hold on to the match and show a prompt so they can decide
          mIncomingMatch = match;
        }
    }

    public void HandleInvitation(Invitation invitation, bool shouldAutoAccept) {
        MakeActive();
        OnGotInvitation(invitation, shouldAutoAccept);
    }

    protected void OnGotInvitation(Invitation invitation, bool shouldAutoAccept) {
      if (invitation.InvitationType != Invitation.InvType.TurnBased) {
        // wrong type of invitation!
        return;
      }

      if (shouldAutoAccept) {
        // if shouldAutoAccept is true, we know the user has indicated (via an external UI)
        // that they wish to accept this invitation right now, so we take the user to the
        // game screen without further delay:
        SetStandBy("Accepting invitation...");
        PlayGamesPlatform.Instance.TurnBased.AcceptInvitation(invitation.InvitationId, OnMatchStarted);

      } else {
        // if shouldAutoAccept is false, we got this invitation in the background, so
        // we should not jump directly into the game
        mIncomingInvite = invitation;
      }
    }

    protected override void DoGUI() {
        GuiLabel(TitleCfg);

        if (mErrorMessage != null) {
            GuiLabel(CenterLabelCfg, mErrorMessage);
            if (GuiButton(OkButtonCfg)) {
                mErrorMessage = null;
            }
            return;
        }

        if (mIncomingMatch != null) {
          ShowIncomingMatchUi();
          return;
        } else if (mIncomingInvite != null) {
          ShowIncomingInviteUi();
          return;
        }

        if (GuiButton(QuickMatchCfg)) {
            SetStandBy("Creating match...");
            PlayGamesPlatform.Instance.TurnBased.CreateQuickMatch(Opponents, Opponents,
                    Variant, OnMatchStarted);
        } else if (GuiButton(InviteCfg)) {
            SetStandBy("Inviting...");
            PlayGamesPlatform.Instance.TurnBased.CreateWithInvitationScreen(Opponents, Opponents,
                    Variant, OnMatchStarted);
        } else if (GuiButton(InboxCfg)) {
            SetStandBy("Showing inbox...");
            PlayGamesPlatform.Instance.TurnBased.AcceptFromInbox(OnMatchStarted);
        } else if (GuiButton(SignOutCfg)) {
            DoSignOut();
        }
    }

    void ShowIncomingMatchUi() {

        switch (mIncomingMatch.Status) {
        case TurnBasedMatch.MatchStatus.Cancelled:
          GuiLabel (CenterLabelCfg, Util.GetOpponentName (mIncomingMatch) + " declined your invitation");
          if (GuiButton (OkButtonCfg)) {
            mIncomingMatch = null;
          }
        break;
        case TurnBasedMatch.MatchStatus.Complete:
          GuiLabel (CenterLabelCfg, "Your match with " + Util.GetOpponentName (mIncomingMatch) + " is over...");
          if (GuiButton (OkButtonCfg)) {
            TurnBasedMatch match = mIncomingMatch;
            mIncomingMatch = null;
            OnMatchStarted (true, match);
          }
        break;

        default:
          switch (mIncomingMatch.TurnStatus) {
          case TurnBasedMatch.MatchTurnStatus.MyTurn:
            GuiLabel (CenterLabelCfg, "It's your turn against " + Util.GetOpponentName (mIncomingMatch));
            if (GuiButton (PlayButtonCfg)) {
              TurnBasedMatch match = mIncomingMatch;
              mIncomingMatch = null;
              OnMatchStarted (true, match);
            } else if (GuiButton (NotNowButtonCfg)) {
              mIncomingMatch = null;
            }
          break;
          default:
            GuiLabel (CenterLabelCfg, Util.GetOpponentName (mIncomingMatch) + " accepted your invitation");
            if (GuiButton (OkButtonCfg)) {
              mIncomingMatch = null;
            }
          break;
          }
        break;

    } // end match status
  }

  void ShowIncomingInviteUi() {
      string inviterName = mIncomingInvite.Inviter == null ? "Someone" :
      mIncomingInvite.Inviter.DisplayName == null ? "Someone" :
      mIncomingInvite.Inviter.DisplayName;
      GuiLabel(CenterLabelCfg, inviterName + " is challenging you to a match!");
      Invitation inv = mIncomingInvite;
      if (GuiButton(AcceptButtonCfg)) {
        mIncomingInvite = null;
        SetStandBy("Accepting invitation...");
        PlayGamesPlatform.Instance.TurnBased.AcceptInvitation(inv.InvitationId, OnMatchStarted);
      } else if (GuiButton(DeclineButtonCfg)) {
        mIncomingInvite = null;
        PlayGamesPlatform.Instance.TurnBased.DeclineInvitation(inv.InvitationId);
      }
    }

    void DoSignOut() {
        PlayGamesPlatform.Instance.SignOut();
        gameObject.GetComponent<WelcomeGui>().SetAuthOnStart(false);
        gameObject.GetComponent<WelcomeGui>().MakeActive();
    }
}
