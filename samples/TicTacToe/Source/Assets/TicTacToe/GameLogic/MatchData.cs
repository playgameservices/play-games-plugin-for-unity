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
using System.Collections.Generic;
using System.IO;

public class MatchData {
    private const int Header = 600673; // sanity check for serialization

    public const char MarkNone = ' ';
    public const char MarkO = 'o';
    public const char MarkX = 'x';
    public const char MarkConflict = '!';

    public const int BoardSize = 3;
    private char[][] mBoard = new char[][] {
        new char[] { MarkNone, MarkNone, MarkNone },
        new char[] { MarkNone, MarkNone, MarkNone },
        new char[] { MarkNone, MarkNone, MarkNone }
    };

    private List<BlockDesc> mBlockDescs = new List<BlockDesc>();

    private bool mXWins = false;
    private bool mOWins = false;

    // the participant ID that plays as 'X' (the other one plays as 'O')
    private string mParticipantIdX = "";

    public MatchData() {

    }

    public MatchData(byte[] b) : this() {
        if (b != null) {
            ReadFromBytes(b);
            ComputeWinner();
        }
    }

    public char GetMark(int x, int y) {
        return (x >= 0 && x < mBoard.Length && y >= 0 && y < mBoard.Length) ?
            mBoard[x][y] : MarkNone;
    }

    public void SetMark(int x, int y, char mark) {
        if (x >= 0 && x < mBoard.Length && y >= 0 && y < mBoard.Length) {
            mBoard[x][y] = mark;
        }
        ComputeWinner();
    }

    public void ResetMarks() {
        int x, y;
        for (x = 0; x < mBoard.Length; x++) {
            for (y = 0; y < mBoard.Length; y++) {
                mBoard[x][y] = MarkNone;
            }
        }
    }

    public void ClearBlockDescs() {
        mBlockDescs.Clear();
    }

    public void AddBlockDesc(char mark, Vector3 position, Quaternion orientation) {
        mBlockDescs.Add(new BlockDesc(mark, position, orientation));
    }

    public List<BlockDesc> BlockDescs {
        get {
            return mBlockDescs;
        }
    }

    private static bool AllEqual(char[] arr) {
        foreach (char c in arr) {
            if (c != arr[0]) {
                return false;
            }
        }
        return true;
    }

    public char Winner {
        get {
            return (mOWins && mXWins) ? MarkConflict :
                mOWins ? MarkO : mXWins ? MarkX : MarkNone;
        }
    }

    public bool HasWinner {
        get {
            return (mXWins && !mOWins) || (!mXWins && mOWins);
        }
    }

    private void AddWinner(char mark) {
        if (MarkO == mark) {
            mOWins = true;
        } else if (MarkX == mark) {
            mXWins = true;
        }
    }

    private void ComputeWinner() {
        int x, y;
        char[] a = new char[mBoard.Length];

        mXWins = mOWins = false;

        // check columns
        for (x = 0; x < mBoard.Length; x++) {
            for (y = 0; y < mBoard.Length; y++) {
                a[y] = mBoard[x][y];
            }
            if (AllEqual(a)) {
                AddWinner(a[0]);
            }
        }

        // check rows
        for (y = 0; y < mBoard.Length; y++) {
            for (x = 0; x < mBoard.Length; x++) {
                a[x] = mBoard[x][y];
            }
            if (AllEqual(a)) {
                AddWinner(a[0]);
            }
        }

        // check diagonals
        for (x = 0; x < mBoard.Length; x++) {
              a[x] = mBoard[x][x];
            if (AllEqual(a)) {
                AddWinner(a[0]);
            }
        }
        for (x = 0; x < mBoard.Length; x++) {
            a[x] = mBoard[x][mBoard.Length - 1 - x];
            if (AllEqual(a)) {
                AddWinner(a[0]);
            }
        }
    }

    public byte[] ToBytes() {
        MemoryStream memStream = new MemoryStream();
        BinaryWriter w = new BinaryWriter(memStream);
        w.Write(Header);
        w.Write((byte)mParticipantIdX.Length);
        w.Write(mParticipantIdX.ToCharArray());
        int x;
        for (x = 0; x < mBoard.Length; x++) {
            w.Write(mBoard[x]);
        }
        w.Write(mBlockDescs.Count);
        foreach (BlockDesc b in mBlockDescs) {
            w.Write(b.mark);
            w.Write(b.position.x);
            w.Write(b.position.y);
            w.Write(b.position.z);
            w.Write(b.rotation.x);
            w.Write(b.rotation.y);
            w.Write(b.rotation.z);
            w.Write(b.rotation.w);
        }
        w.Close();
        byte[] buf = memStream.GetBuffer();
        memStream.Close();
        return buf;
    }

    private void ReadFromBytes(byte[] b) {
        BinaryReader r = new BinaryReader(new MemoryStream(b));
        int header = r.ReadInt32();
        if (header != Header) {
            // we don't know how to parse this version; user has to upgrade game
            throw new UnsupportedMatchFormatException("Board data header " + header +
                    " not recognized.");
        }

        int len = (int)r.ReadByte();
        mParticipantIdX = new string(r.ReadChars(len));

        int x;
        for (x = 0; x < mBoard.Length; x++) {
            mBoard[x] = r.ReadChars(mBoard.Length);
        }
        ComputeWinner();

        mBlockDescs.Clear();
        int blockDescs = r.ReadInt32(), i;
        for (i = 0; i < blockDescs; i++) {
            float px, py, pz, rx, ry, rz, rw;
            char mark = r.ReadChar();
            px = r.ReadSingle();
            py = r.ReadSingle();
            pz = r.ReadSingle();
            rx = r.ReadSingle();
            ry = r.ReadSingle();
            rz = r.ReadSingle();
            rw = r.ReadSingle();
            mBlockDescs.Add(new BlockDesc(mark, new Vector3(px, py, pz),
                    new Quaternion(rx, ry, rz, rw)));
        }
    }

    public char GetMyMark(string myParticipantId) {
        if (mParticipantIdX.Equals("")) {
            // if X is unclaimed, claim it!
            mParticipantIdX = myParticipantId;
        }
        return mParticipantIdX.Equals(myParticipantId) ? MarkX : MarkO;
    }

    public struct BlockDesc {
        public char mark;
        public Vector3 position;
        public Quaternion rotation;
        public BlockDesc(char mark, Vector3 position, Quaternion rotation) {
            this.mark = mark;
            this.position = position;
            this.rotation = rotation;
        }
        public override string ToString () {
            return "[BlockDesc: '" + mark + "', pos=" + position + ", rot=" + rotation + "]";
        }
    };

    public class UnsupportedMatchFormatException : System.Exception {
        public UnsupportedMatchFormatException(string message) : base(message) {}
    }
}
