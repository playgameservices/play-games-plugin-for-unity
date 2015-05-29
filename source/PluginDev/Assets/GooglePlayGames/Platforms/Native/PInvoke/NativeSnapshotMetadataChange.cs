﻿/*
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
using GooglePlayGames.OurUtils;


#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
using System;
using System.Runtime.InteropServices;
using System.Text;
using GooglePlayGames.Native.PInvoke;
using GooglePlayGames.BasicApi.Multiplayer;
using Types = GooglePlayGames.Native.Cwrapper.Types;

using C = GooglePlayGames.Native.Cwrapper.SnapshotMetadataChange;
using B = GooglePlayGames.Native.Cwrapper.SnapshotMetadataChangeBuilder;

namespace GooglePlayGames.Native {
internal class NativeSnapshotMetadataChange : BaseReferenceHolder {
    internal NativeSnapshotMetadataChange(IntPtr selfPointer) : base(selfPointer) {
    }


    protected override void CallDispose(HandleRef selfPointer) {
        C.SnapshotMetadataChange_Dispose(selfPointer);
    }


    internal static NativeSnapshotMetadataChange FromPointer(IntPtr pointer) {
        if (pointer.Equals(IntPtr.Zero)) {
            return null;
        }

        return new NativeSnapshotMetadataChange(pointer);
    }

    internal class Builder : BaseReferenceHolder {
        internal Builder() : base(B.SnapshotMetadataChange_Builder_Construct()) {
        }

        protected override void CallDispose(HandleRef selfPointer) {
            B.SnapshotMetadataChange_Builder_Dispose(selfPointer);
        }

        internal Builder SetDescription(string description) {
            B.SnapshotMetadataChange_Builder_SetDescription(SelfPtr(), description);
            return this;
        }

        internal Builder SetPlayedTime(ulong playedTime) {
            B.SnapshotMetadataChange_Builder_SetPlayedTime(SelfPtr(), playedTime);
            return this;
        }

        internal Builder SetCoverImageFromPngData(byte[] pngData) {
            Misc.CheckNotNull(pngData);
            B.SnapshotMetadataChange_Builder_SetCoverImageFromPngData(SelfPtr(),
                pngData, new UIntPtr((ulong) pngData.LongLength));
            return this;
        }

        internal NativeSnapshotMetadataChange Build() {
            return FromPointer(B.SnapshotMetadataChange_Builder_Create(SelfPtr()));
        }
    }
}
}

#endif // (UNITY_ANDROID || UNITY_IPHONE)
