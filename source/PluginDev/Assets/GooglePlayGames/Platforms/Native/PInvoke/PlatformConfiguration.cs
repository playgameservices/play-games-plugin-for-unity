// <copyright file="PlatformConfiguration.cs" company="Google Inc.">
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

#if (UNITY_ANDROID || UNITY_IPHONE)

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class PlatformConfiguration : BaseReferenceHolder
    {
        protected PlatformConfiguration(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        internal HandleRef AsHandle()
        {
            return SelfPtr();
        }
    }
}


#endif
