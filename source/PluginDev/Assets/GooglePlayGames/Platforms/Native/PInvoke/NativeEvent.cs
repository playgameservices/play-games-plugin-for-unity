// <copyright file="NativeEvent.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

#if UNITY_ANDROID

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using GooglePlayGames.BasicApi.Events;
    using GooglePlayGames.Native.PInvoke;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using C = GooglePlayGames.Native.Cwrapper.Event;

    internal class NativeEvent : BaseReferenceHolder, IEvent
    {

        internal NativeEvent(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        public string Id
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Event_Id(SelfPtr(), out_string, out_size));
            }
        }

        public string Name
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Event_Name(SelfPtr(), out_string, out_size));
            }
        }

        public string Description
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Event_Description(SelfPtr(), out_string, out_size));
            }
        }

        public string ImageUrl
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Event_ImageUrl(SelfPtr(), out_string, out_size));
            }
        }

        public ulong CurrentCount
        {
            get
            {
                return C.Event_Count(SelfPtr());
            }
        }

        public EventVisibility Visibility
        {
            get
            {
                var visibility = C.Event_Visibility(SelfPtr());
                switch (visibility)
                {
                    case Types.EventVisibility.HIDDEN:
                        return EventVisibility.Hidden;
                    case Types.EventVisibility.REVEALED:
                        return EventVisibility.Revealed;
                    default:
                        throw new InvalidOperationException("Unknown visibility: " + visibility);
                }
            }
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.Event_Dispose(selfPointer);
        }

        public override string ToString()
        {
            if (IsDisposed())
            {
                return "[NativeEvent: DELETED]";
            }

            return string.Format("[NativeEvent: Id={0}, Name={1}, Description={2}, " +
                "ImageUrl={3}, CurrentCount={4}, Visibility={5}]",
                Id, Name, Description, ImageUrl, CurrentCount, Visibility);
        }
    }
}
#endif //UNITY_ANDROID

