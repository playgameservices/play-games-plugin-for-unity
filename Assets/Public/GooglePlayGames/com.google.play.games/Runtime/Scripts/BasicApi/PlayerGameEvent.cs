// <copyright file="PlayerGameEvent.cs" company="Google Inc.">
// Copyright (C) 2026 Google Inc. All Rights Reserved.
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

namespace GooglePlayGames.BasicApi
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.OurUtils;

    /// <summary>
    /// Represents a custom game event or discrete player action (e.g., "player killed monster", "level completed", etc.).
    /// This class allows developers to define and log game-specific events with associated properties.
    /// </summary>
    public class PlayerGameEvent
    {
        // These limits are enforced by the Play Games Services backend API for event logging.
        // Exceeding these limits will result in the event being rejected by the service.
        private const int MaxEventNameLength = 100;
        private const int MaxKeyLength = 100;
        private const int MaxStringValueLength = 1024;
        private const int MaxProperties = 25;

        public string EventName { get; private set; }
        public IReadOnlyDictionary<string, object> EventProperties { get; private set; }

        private PlayerGameEvent(string eventName, IReadOnlyDictionary<string, object> eventProperties)
        {
            EventName = eventName;
            EventProperties = new Dictionary<string, object>(eventProperties);
        }

        /// <summary>
        /// Builder for <see cref="PlayerGameEvent"/>.
        /// </summary>
        public class Builder
        {
            private readonly string eventName;
            private readonly Dictionary<string, object> eventProperties = new Dictionary<string, object>();
            private readonly object lockObject = new object();

            public Builder(string eventName)
            {
                if (string.IsNullOrEmpty(eventName))
                {
                    throw new ArgumentException("eventName cannot be null or empty");
                }
                if (eventName.Length > MaxEventNameLength)
                {
                    throw new ArgumentException($"eventName cannot be longer than {MaxEventNameLength} characters");
                }
                this.eventName = eventName;
            }

            private void CheckKey(string key)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("Key cannot be null or empty.");
                }
                if (key.Length > MaxKeyLength)
                {
                    throw new ArgumentException($"Key cannot exceed {MaxKeyLength} characters.");
                }
            }

            public Builder AddProperty(string key, long value)
            {
                lock (lockObject)
                {
                    CheckKey(key);
                    if (eventProperties.Count >= MaxProperties && !eventProperties.ContainsKey(key))
                    {
                        throw new InvalidOperationException($"Cannot add more than {MaxProperties} properties.");
                    }
                    eventProperties[key] = value;
                }
                return this;
            }

            public Builder AddProperty(string key, int value)
            {
                return AddProperty(key, (long)value);
            }

            public Builder AddProperty(string key, double value)
            {
                lock (lockObject)
                {
                    CheckKey(key);
                    if (eventProperties.Count >= MaxProperties && !eventProperties.ContainsKey(key))
                    {
                        throw new InvalidOperationException($"Cannot add more than {MaxProperties} properties.");
                    }
                    eventProperties[key] = value;
                }
                return this;
            }

            public Builder AddProperty(string key, string value)
            {
                lock (lockObject)
                {
                    CheckKey(key);
                    if (eventProperties.Count >= MaxProperties && !eventProperties.ContainsKey(key))
                    {
                        throw new InvalidOperationException($"Cannot add more than {MaxProperties} properties.");
                    }
                    Misc.CheckNotNull(value, "value cannot be null");
                    if (value.Length > MaxStringValueLength)
                    {
                        throw new ArgumentException($"Property's string value cannot exceed {MaxStringValueLength} characters.");
                    }
                    eventProperties[key] = value;
                }
                return this;
            }

            public Builder AddProperty(string key, bool value)
            {
                lock (lockObject)
                {
                    CheckKey(key);
                    if (eventProperties.Count >= MaxProperties && !eventProperties.ContainsKey(key))
                    {
                        throw new InvalidOperationException($"Cannot add more than {MaxProperties} properties.");
                    }
                    eventProperties[key] = value;
                }
                return this;
            }

            public PlayerGameEvent Build()
            {
                lock (lockObject)
                {
                    return new PlayerGameEvent(eventName, eventProperties);
                }
            }
        }
    }
}
#endif