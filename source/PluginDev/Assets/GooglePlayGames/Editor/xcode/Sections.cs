/// <summary>
/// Xcode PBX support library.  This is from the Unity open source.
/// https://bitbucket.org/Unity-Technologies/xcodeapi/overview
/// </summary>
///
/// The MIT License (MIT)

/// Copyright (c) 2014 Unity Technologies
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
///
#if !UNITY_5
namespace GooglePlayGames.xcode
{
    // Base classes for section handling
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.IO;

    // common base
    internal abstract class SectionBase
    {
        public abstract void AddObject(string key, PBXElementDict value);
        public abstract void WriteSection(StringBuilder sb, GUIDToCommentMap comments);
    }

    // known section: contains objects that we care about
    internal class KnownSectionBase<T> : SectionBase where T : PBXObject, new()
    {
        private Dictionary<string, T> m_Entries = new Dictionary<string, T>();

        private string m_Name;

        public KnownSectionBase(string sectionName)
        {
            m_Name = sectionName;
        }

        public IEnumerable<KeyValuePair<string, T>> GetEntries()
        {
            return m_Entries;
        }

        public IEnumerable<string> GetGuids()
        {
            return m_Entries.Keys;
        }

        public IEnumerable<T> GetObjects()
        {
            return m_Entries.Values;
        }

        public override void AddObject(string key, PBXElementDict value)
        {
            T obj = new T();
            obj.guid = key;
            obj.SetPropertiesWhenSerializing(value);
            obj.UpdateVars();
            m_Entries[obj.guid] = obj;
        }

        public override void WriteSection(StringBuilder sb, GUIDToCommentMap comments)
        {
            if (m_Entries.Count == 0)
                return;            // do not write empty sections

            sb.AppendFormat("\n\n/* Begin {0} section */", m_Name);
            var keys = new List<string>(m_Entries.Keys);
            keys.Sort(StringComparer.Ordinal);
            foreach (string key in keys)
            {
                T obj = m_Entries[key];
                obj.UpdateProps();
                sb.Append("\n\t\t");
                comments.WriteStringBuilder(sb, obj.guid);
                sb.Append(" = ");
                Serializer.WriteDict(sb, obj.GetPropertiesWhenSerializing(), 2,
                                     obj.shouldCompact, obj.checker, comments);
                sb.Append(";");
            }
            sb.AppendFormat("\n/* End {0} section */", m_Name);
        }

        // returns null if not found
        public T this[string guid]
        {
            get {
                if (m_Entries.ContainsKey(guid))
                    return m_Entries[guid];
                return null;
            }
        }

        public bool HasEntry(string guid)
        {
            return m_Entries.ContainsKey(guid);
        }

        public void AddEntry(T obj)
        {
            m_Entries[obj.guid] = obj;
        }

        public void RemoveEntry(string guid)
        {
            if (m_Entries.ContainsKey(guid))
                m_Entries.Remove(guid);
        }
    }

    // we assume there is only one PBXProject entry
    internal class PBXProjectSection : KnownSectionBase<PBXProjectObject>
    {
        public PBXProjectSection() : base("PBXProject")
        {
        }

        public PBXProjectObject project
        {
            get {
                foreach (var kv in GetEntries())
                    return kv.Value;
                return null;
            }
        }
    }

}
#endif
