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
using System;
using NUnit.Framework;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.UnitTests.OurUtils {
    [TestFixture]
    public class MiscTests {
        [Test]
        public void BuffersAreIdenticalHandlesNull() {
            byte[] nonNull = {0, 1, 2};

            AssertBuffersIdenticalInBothOrders(false, nonNull, null);
            Assert.IsTrue(Misc.BuffersAreIdentical(null, null));
        }

        [Test]
        public void BuffersAreIdenticalHandlesDifferentLengthArrays() {
            byte[] one = {0};
            byte[] two = {0, 1};
            byte[] three = {0, 1, 2};

            AssertBuffersIdenticalInBothOrders(false, one, two);
            AssertBuffersIdenticalInBothOrders(false, one, three);
            AssertBuffersIdenticalInBothOrders(false, two, three);
        }

        [Test]
        public void BuffersAreIdenticalHandlesSameLengthArrays() {
            byte[] arrayOne = {0};
            byte[] arrayTwo = {0};
            byte[] different = {1};

            AssertBuffersIdenticalInBothOrders(true, arrayOne, arrayTwo);
            AssertBuffersIdenticalInBothOrders(false, arrayOne, different);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubsetBytesDisallowsNullArray() {
            Misc.GetSubsetBytes(null, 0, 100);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetSubsetBytesDisallowsNegativeOffset() {
            byte[] sentinel = {10};
            Misc.GetSubsetBytes(sentinel, -10, 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetSubsetBytesDisallowsToLargeLength() {
            byte[] sentinel = {10};
            Misc.GetSubsetBytes(sentinel, 0, 10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetSubsetBytesDisallowsOverflowingLength() {
            byte[] sentinel = {1, 2};
            Misc.GetSubsetBytes(sentinel, 1, 2);
        }

        [Test]
        public void GetSubsetBytesWorksWithNoOffset() {
            byte[] exampleArray = {1, 2, 3};
            Assert.AreEqual(new byte[] {1}, Misc.GetSubsetBytes(exampleArray, 0, 1));
            Assert.AreEqual(new byte[] {1, 2}, Misc.GetSubsetBytes(exampleArray, 0, 2));
            Assert.AreEqual(exampleArray, Misc.GetSubsetBytes(exampleArray, 0, 3));
        }

        [Test]
        public void GetSubsetBytesWorksWithOffset() {
            byte[] exampleArray = {1, 2, 3};
            Assert.AreEqual(new byte[] {2}, Misc.GetSubsetBytes(exampleArray, 1, 1));
            Assert.AreEqual(new byte[] {3}, Misc.GetSubsetBytes(exampleArray, 2, 1));
            Assert.AreEqual(new byte[] {2, 3}, Misc.GetSubsetBytes(exampleArray, 1, 2));
        }

        private static void AssertBuffersIdenticalInBothOrders(
            bool expected, byte[] arrayOne, byte[] arrayTwo) {
            Assert.AreEqual(expected, Misc.BuffersAreIdentical(arrayOne, arrayTwo));
            Assert.AreEqual(expected, Misc.BuffersAreIdentical(arrayTwo, arrayOne));
        }
    }
}

