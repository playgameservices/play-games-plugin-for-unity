// <copyright file="NativeEventClient.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.Native
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Events;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.Native.PInvoke;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal class NativeEventClient : IEventsClient
    {
        private readonly EventManager mEventManager;

        internal NativeEventClient(EventManager manager)
        {
            this.mEventManager = Misc.CheckNotNull(manager);
        }

        public void FetchAllEvents(DataSource source, Action<ResponseStatus, List<IEvent>> callback)
        {
            Misc.CheckNotNull(callback);
            callback = CallbackUtils.ToOnGameThread<ResponseStatus, List<IEvent>>(callback);

            mEventManager.FetchAll(ConversionUtils.AsDataSource(source),
                response =>
                {
                    var responseStatus =
                        ConversionUtils.ConvertResponseStatus(response.ResponseStatus());

                    if (!response.RequestSucceeded())
                    {
                        callback(responseStatus, new List<IEvent>());
                    }
                    else
                    {
                        callback(responseStatus, response.Data().Cast<IEvent>().ToList());
                    }
                });
        }

        public void FetchEvent(DataSource source, string eventId,
                           Action<ResponseStatus, IEvent> callback)
        {
            Misc.CheckNotNull(eventId);
            Misc.CheckNotNull(callback);

            mEventManager.Fetch(ConversionUtils.AsDataSource(source), eventId,
                response =>
                {
                    var responseStatus =
                        ConversionUtils.ConvertResponseStatus(response.ResponseStatus());

                    if (!response.RequestSucceeded())
                    {
                        callback(responseStatus, null);
                    }
                    else
                    {
                        callback(responseStatus, response.Data());
                    }
                });
        }

        public void IncrementEvent(string eventId, uint stepsToIncrement)
        {
            Misc.CheckNotNull(eventId);
            mEventManager.Increment(eventId, stepsToIncrement);
        }

    }
}
#endif // #if (UNITY_ANDROID || UNITY_IPHONE)
