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
using GooglePlayGames.Native.PInvoke;
using System.Runtime.InteropServices;
using GooglePlayGames.OurUtils;
using System.Collections.Generic;
using GooglePlayGames.Native.Cwrapper;

using C = GooglePlayGames.Native.Cwrapper.RealTimeRoom;
using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native.PInvoke {
internal class NativeRealTimeRoom : BaseReferenceHolder {

    internal NativeRealTimeRoom(IntPtr selfPointer) : base(selfPointer) {
    }

    internal string Id() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, size) => C.RealTimeRoom_Id(SelfPtr(), out_string, size));
    }

    internal IEnumerable<MultiplayerParticipant> Participants() {
        return PInvokeUtilities.ToEnumerable(
            C.RealTimeRoom_Participants_Length(SelfPtr()),
            (index) => new MultiplayerParticipant(
                C.RealTimeRoom_Participants_GetElement(SelfPtr(), index)));
    }

    internal uint ParticipantCount() {
        return C.RealTimeRoom_Participants_Length(SelfPtr()).ToUInt32();
    }

    internal Types.RealTimeRoomStatus Status() {
        return C.RealTimeRoom_Status(SelfPtr());
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.RealTimeRoom_Dispose(selfPointer);
    }

    internal static NativeRealTimeRoom FromPointer(IntPtr selfPointer) {
        if (selfPointer.Equals(IntPtr.Zero)) {
            return null;
        }

        return new NativeRealTimeRoom(selfPointer);
    }
}
}

#endif
