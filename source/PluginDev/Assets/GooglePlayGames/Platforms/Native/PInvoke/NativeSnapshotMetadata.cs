
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
using System;
using System.Runtime.InteropServices;
using System.Text;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.Native.PInvoke;
using GooglePlayGames.OurUtils;
using Types = GooglePlayGames.Native.Cwrapper.Types;

using C = GooglePlayGames.Native.Cwrapper.SnapshotMetadata;

namespace GooglePlayGames.Native {
internal class NativeSnapshotMetadata : BaseReferenceHolder, ISavedGameMetadata {
    internal NativeSnapshotMetadata(IntPtr selfPointer) : base(selfPointer) {
    }

    public bool IsOpen {
        get {
            return C.SnapshotMetadata_IsOpen(SelfPtr());
        }
    }

    public string Filename {
        get {
            return PInvokeUtilities.OutParamsToString(
                (out_string, out_size) => C.SnapshotMetadata_FileName(
                    SelfPtr(), out_string, out_size));
        }
    }

    public string Description {
        get {
            return PInvokeUtilities.OutParamsToString(
                (out_string, out_size) => C.SnapshotMetadata_Description(
                    SelfPtr(), out_string, out_size));
        }
    }

    public string CoverImageURL {
        get {
            return PInvokeUtilities.OutParamsToString(
                (out_string, out_size) => C.SnapshotMetadata_CoverImageURL(
                    SelfPtr(), out_string, out_size));
        }
    }

    public TimeSpan TotalTimePlayed {
        get {
            var playedTime = C.SnapshotMetadata_PlayedTime(SelfPtr());

            // In the case of an unknown play time, we received -1 here. Use 0 instead.
            if (playedTime < 0) {
                return TimeSpan.FromMilliseconds(0);
            }

            return TimeSpan.FromMilliseconds(playedTime);
        }
    }

    public DateTime LastModifiedTimestamp {
        get {
            return PInvokeUtilities.FromMillisSinceUnixEpoch(
                C.SnapshotMetadata_LastModifiedTime(SelfPtr()));
        }
    }

    public override string ToString() {
        if (IsDisposed()) {
            return "[NativeSnapshotMetadata: DELETED]";
        }

        return string.Format("[NativeSnapshotMetadata: IsOpen={0}, Filename={1}, " +
            "Description={2}, CoverImageUrl={3}, TotalTimePlayed={4}, LastModifiedTimestamp={5}]",
            IsOpen, Filename, Description, CoverImageURL, TotalTimePlayed, LastModifiedTimestamp
        );
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.SnapshotMetadata_Dispose(SelfPtr());
    }

}
}

#endif // (UNITY_ANDROID || UNITY_IPHONE)
