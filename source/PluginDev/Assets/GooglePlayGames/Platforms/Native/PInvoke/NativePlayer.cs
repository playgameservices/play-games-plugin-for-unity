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
using GooglePlayGames.Native.PInvoke;
using GooglePlayGames.BasicApi.Multiplayer;
using Types = GooglePlayGames.Native.Cwrapper.Types;

using C = GooglePlayGames.Native.Cwrapper.Player;

namespace GooglePlayGames.Native {
internal class NativePlayer : BaseReferenceHolder {

    internal NativePlayer (IntPtr selfPointer) : base(selfPointer) {
    }

    internal string Id() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, out_size) => C.Player_Id(SelfPtr(), out_string, out_size));
    }

    internal string Name() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, out_size) => C.Player_Name(SelfPtr(), out_string, out_size));
    }

    internal string AvatarURL() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, out_size) => C.Player_AvatarUrl(SelfPtr(),
                    Types.ImageResolution.ICON, out_string, out_size));
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.Player_Dispose(selfPointer);
    }

    internal Player AsPlayer() {
        return new Player(Name(), Id(), AvatarURL());
    }
}
}


#endif
