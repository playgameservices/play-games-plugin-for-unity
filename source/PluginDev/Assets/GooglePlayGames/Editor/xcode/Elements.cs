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
    using System.Collections.Generic;
    using System.Collections;
    using System;

  class PBXElement
  {
    protected PBXElement() {}

    // convenience methods
    public string AsString() { return ((PBXElementString)this).value; }
    public PBXElementArray AsArray() { return (PBXElementArray)this; }
    public PBXElementDict AsDict()   { return (PBXElementDict)this; }

    public PBXElement this[string key]
    {
      get { return AsDict()[key]; }
      set { AsDict()[key] = value; }
    }
  }

  class PBXElementString : PBXElement
  {
    public PBXElementString(string v) { value = v; }

    public string value;
  }

  class PBXElementDict : PBXElement
  {
    public PBXElementDict() : base() {}

    private Dictionary<string, PBXElement> m_PrivateValue = new Dictionary<string, PBXElement>();
    public IDictionary<string, PBXElement> values { get { return m_PrivateValue; }}

    new public PBXElement this[string key]
    {
      get {
        if (values.ContainsKey(key))
          return values[key];
        return null;
      }
      set { this.values[key] = value; }
    }

    public bool Contains(string key)
    {
      return values.ContainsKey(key);
    }

    public void Remove(string key)
    {
      values.Remove(key);
    }

    public void SetString(string key, string val)
    {
      values[key] = new PBXElementString(val);
    }

    public PBXElementArray CreateArray(string key)
    {
      var v = new PBXElementArray();
      values[key] = v;
      return v;
    }

    public PBXElementDict CreateDict(string key)
    {
      var v = new PBXElementDict();
      values[key] = v;
      return v;
    }
  }

  class PBXElementArray : PBXElement
  {
    public PBXElementArray() : base() {}
    public List<PBXElement> values = new List<PBXElement>();

    // convenience methods
    public void AddString(string val)
    {
      values.Add(new PBXElementString(val));
    }

    public PBXElementArray AddArray()
    {
      var v = new PBXElementArray();
      values.Add(v);
      return v;
    }

    public PBXElementDict AddDict()
    {
      var v = new PBXElementDict();
      values.Add(v);
      return v;
    }
  }

}
#endif
