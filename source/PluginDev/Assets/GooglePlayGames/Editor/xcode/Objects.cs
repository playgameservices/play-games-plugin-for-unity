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
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Linq;
    using System;

    internal class PBXObject
    {
        public string guid;
        protected PBXElementDict m_Properties = new PBXElementDict();

        internal void SetPropertiesWhenSerializing(PBXElementDict props)
        {
            m_Properties = props;
        }

        internal PBXElementDict GetPropertiesWhenSerializing()
        {
            return m_Properties;
        }

        // returns null if it does not exist
        protected string GetPropertyString(string name)
        {
            var prop = m_Properties[name];
            if (prop == null)
                return null;

            return prop.AsString();
        }

        protected void SetPropertyString(string name, string value)
        {
            if (value == null)
                m_Properties.Remove(name);
            else
                m_Properties.SetString(name, value);
        }

        protected List<string> GetPropertyList(string name)
        {
            var prop = m_Properties[name];
            if (prop == null)
                return null;

            var list = new List<string>();
            foreach (var el in prop.AsArray().values)
                list.Add(el.AsString());
            return list;
        }

        protected void SetPropertyList(string name, List<string> value)
        {
            if (value == null)
                m_Properties.Remove(name);
            else
            {
                var array = m_Properties.CreateArray(name);
                foreach (string val in value)
                    array.AddString(val);
            }
        }

        private static PropertyCommentChecker checkerData = new PropertyCommentChecker();
        internal virtual PropertyCommentChecker checker { get { return checkerData; } }
        internal virtual bool shouldCompact { get { return false; } }

        public virtual void UpdateProps() {}      // Updates the props from cached variables
        public virtual void UpdateVars() {}       // Updates the cached variables from underlying props
    }

    internal class PBXBuildFile : PBXObject
    {
        public string fileRef;
        public string compileFlags;
        public bool weak;

        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "fileRef/*"
        });
        internal override PropertyCommentChecker checker { get { return checkerData; } }
        internal override bool shouldCompact { get { return true; } }

        public static PBXBuildFile CreateFromFile(string fileRefGUID, bool weak,
                                                  string compileFlags)
        {
            PBXBuildFile buildFile = new PBXBuildFile();
            buildFile.guid = PBXGUID.Generate();
            buildFile.SetPropertyString("isa", "PBXBuildFile");
            buildFile.fileRef = fileRefGUID;
            buildFile.compileFlags = compileFlags;
            buildFile.weak = weak;
            return buildFile;
        }

        PBXElementDict GetSettingsDictOptional()
        {
            if (m_Properties.Contains("settings"))
                return m_Properties["settings"].AsDict();
            return null;
        }

        PBXElementDict GetSettingsDict()
        {
            if (m_Properties.Contains("settings"))
                return m_Properties["settings"].AsDict();
            else
                return m_Properties.CreateDict("settings");
        }

        public override void UpdateProps()
        {
            SetPropertyString("fileRef", fileRef);
            if (compileFlags != null && compileFlags != "")
            {
                GetSettingsDict().SetString("COMPILER_FLAGS", compileFlags);
            }
            else
            {
                var dict = GetSettingsDictOptional();
                if (dict != null)
                    dict.Remove("COMPILER_FLAGS");
            }

            if (weak)
            {
                var dict = GetSettingsDict();
                PBXElementArray attrs = null;
                if (dict.Contains("ATTRIBUTES"))
                    attrs = dict["ATTRIBUTES"].AsArray();
                else
                    attrs = dict.CreateArray("ATTRIBUTES");

                bool exists = false;
                foreach (var value in attrs.values)
                {
                    if (value is PBXElementString && value.AsString() == "Weak")
                        exists = true;
                }
                if (!exists)
                    attrs.AddString("Weak");
            }
            else
            {
                var dict = GetSettingsDictOptional();
                if (dict != null && dict.Contains("ATTRIBUTES"))
                {
                    var attrs = dict["ATTRIBUTES"].AsArray();
                    attrs.values.RemoveAll(el => (el is PBXElementString && el.AsString() == "Weak"));
                    if (attrs.values.Count == 0)
                        dict.Remove("ATTRIBUTES");
                    if (dict.values.Count == 0)
                        m_Properties.Remove("settings");
                }
            }
        }

        public override void UpdateVars()
        {
            fileRef = GetPropertyString("fileRef");
            compileFlags = null;
            weak = false;
            if (m_Properties.Contains("settings"))
            {
                var dict = m_Properties["settings"].AsDict();
                if (dict.Contains("COMPILER_FLAGS"))
                    compileFlags = dict["COMPILER_FLAGS"].AsString();

                if (dict.Contains("ATTRIBUTES"))
                {
                    var attrs = dict["ATTRIBUTES"].AsArray();
                    foreach (var value in attrs.values)
                    {
                        if (value is PBXElementString && value.AsString() == "Weak")
                            weak = true;
                    }
                }
            }
        }
    }

    internal class PBXFileReference : PBXObject
    {
        string m_Path = null;
        string m_ExplicitFileType = null;
        string m_LastKnownFileType = null;

        public string path
        {
            get { return m_Path; }
            set { m_ExplicitFileType = null; m_LastKnownFileType = null; m_Path = value; }
        }

        public string name;
        public PBXSourceTree tree;

        internal override bool shouldCompact { get { return true; } }

        public static PBXFileReference CreateFromFile(string path, string projectFileName,
                                                      PBXSourceTree tree)
        {
            string guid = PBXGUID.Generate();

            PBXFileReference fileRef = new PBXFileReference();
            fileRef.SetPropertyString("isa", "PBXFileReference");
            fileRef.guid = guid;
            fileRef.path = path;
            fileRef.name = projectFileName;
            fileRef.tree = tree;
            return fileRef;
        }

        public override void UpdateProps()
        {
            string ext = null;
            if (m_ExplicitFileType != null)
                SetPropertyString("explicitFileType", m_ExplicitFileType);
            else if (m_LastKnownFileType != null)
                SetPropertyString("lastKnownFileType", m_LastKnownFileType);
            else
            {
                if (name != null)
                    ext = Path.GetExtension(name);
                else if (m_Path != null)
                    ext = Path.GetExtension(m_Path);
                if (ext != null)
                {
                    if (FileTypeUtils.IsFileTypeExplicit(ext))
                        SetPropertyString("explicitFileType", FileTypeUtils.GetTypeName(ext));
                    else
                        SetPropertyString("lastKnownFileType", FileTypeUtils.GetTypeName(ext));
                }
            }
            if (m_Path == name)
                SetPropertyString("name", null);
            else
                SetPropertyString("name", name);
            if (m_Path == null)
                SetPropertyString("path", "");
            else
                SetPropertyString("path", m_Path);
            SetPropertyString("sourceTree", FileTypeUtils.SourceTreeDesc(tree));
        }

        public override void UpdateVars()
        {
            name = GetPropertyString("name");
            m_Path = GetPropertyString("path");
            if (name == null)
                name = m_Path;
            if (m_Path == null)
                m_Path = "";
            tree = FileTypeUtils.ParseSourceTree(GetPropertyString("sourceTree"));
            m_ExplicitFileType = GetPropertyString("explicitFileType");
            m_LastKnownFileType = GetPropertyString("lastKnownFileType");
        }
    }

    class GUIDList : IEnumerable<string>
    {
        private List<string> m_List = new List<string>();

        public GUIDList() {}
        public GUIDList(List<string> data)
        {
            m_List = data;
        }

        public static implicit operator List<string>(GUIDList list) { return list.m_List; }
        public static implicit operator GUIDList(List<string> data) { return new GUIDList(data); }

        public void AddGUID(string guid)        { m_List.Add(guid); }
        public void RemoveGUID(string guid)     { m_List.RemoveAll(x => x == guid); }
        public bool Contains(string guid)       { return m_List.Contains(guid); }
        public int Count                        { get { return m_List.Count; } }
        public void Clear()                     { m_List.Clear(); }
        IEnumerator<string> IEnumerable<string>.GetEnumerator() { return m_List.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return m_List.GetEnumerator(); }
    }

    internal class XCConfigurationList : PBXObject
    {
        public GUIDList buildConfigs;

        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "buildConfigurations/*"
        });
        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public static XCConfigurationList Create()
        {
            var res = new XCConfigurationList();
            res.guid = PBXGUID.Generate();

            res.SetPropertyString("isa", "XCConfigurationList");
            res.buildConfigs = new GUIDList();
            res.SetPropertyString("defaultConfigurationIsVisible", "0");

            return res;
        }

        public override void UpdateProps()
        {
            SetPropertyList("buildConfigurations", buildConfigs);
        }
        public override void UpdateVars()
        {
            buildConfigs = GetPropertyList("buildConfigurations");
        }
    }

    internal class PBXGroup : PBXObject
    {
        public GUIDList children;
        public PBXSourceTree tree;
        public string name, path;

        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "children/*"
        });
        internal override PropertyCommentChecker checker { get { return checkerData; } }

        // name must not contain '/'
        public static PBXGroup Create(string name, string path, PBXSourceTree tree)
        {
            if (name.Contains("/"))
                throw new Exception("Group name must not contain '/'");

            PBXGroup gr = new PBXGroup();
            gr.guid = PBXGUID.Generate();
            gr.SetPropertyString("isa", "PBXGroup");
            gr.name = name;
            gr.path = path;
            gr.tree = PBXSourceTree.Group;
            gr.children = new GUIDList();

            return gr;
        }

        public static PBXGroup CreateRelative(string name)
        {
            return Create(name, name, PBXSourceTree.Group);
        }

        public override void UpdateProps()
        {
            // The name property is set only if it is different from the path property
            SetPropertyList("children", children);
            if (name == path)
                SetPropertyString("name", null);
            else
                SetPropertyString("name", name);
            if (path == "")
                SetPropertyString("path", null);
            else
                SetPropertyString("path", path);
            SetPropertyString("sourceTree", FileTypeUtils.SourceTreeDesc(tree));
        }
        public override void UpdateVars()
        {
            children = GetPropertyList("children");
            path = GetPropertyString("path");
            name = GetPropertyString("name");
            if (name == null)
                name = path;
            if (path == null)
                path = "";
            tree = FileTypeUtils.ParseSourceTree(GetPropertyString("sourceTree"));
        }
    }

    internal class PBXVariantGroup : PBXGroup
    {
    }

    internal class PBXNativeTarget : PBXObject
    {
        public GUIDList phases;

        public string buildConfigList; // guid
        public string name;
        public GUIDList dependencies;

        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "buildPhases/*",
            "buildRules/*",
            "dependencies/*",
            "productReference/*",
            "buildConfigurationList/*"
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public static PBXNativeTarget Create(string name, string productRef, string productType, string buildConfigList)
        {
            var res = new PBXNativeTarget();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXNativeTarget");
            res.buildConfigList = buildConfigList;
            res.phases = new GUIDList();
            res.SetPropertyList("buildRules", new List<string>());
            res.dependencies = new GUIDList();
            res.name = name;
            res.SetPropertyString("productName", name);
            res.SetPropertyString("productReference", productRef);
            res.SetPropertyString("productType", productType);
            return res;
        }

        public override void UpdateProps()
        {
            SetPropertyString("buildConfigurationList", buildConfigList);
            SetPropertyString("name", name);
            SetPropertyList("buildPhases", phases);
            SetPropertyList("dependencies", dependencies);
        }
        public override void UpdateVars()
        {
            buildConfigList = GetPropertyString("buildConfigurationList");
            name = GetPropertyString("name");
            phases = GetPropertyList("buildPhases");
            dependencies = GetPropertyList("dependencies");
        }
    }


    internal class FileGUIDListBase : PBXObject
    {
        public GUIDList files;

        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "files/*",
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public override void UpdateProps()
        {
            SetPropertyList("files", files);
        }
        public override void UpdateVars()
        {
            files = GetPropertyList("files");
        }
    }

    internal class PBXSourcesBuildPhase : FileGUIDListBase
    {
        public static PBXSourcesBuildPhase Create()
        {
            var res = new PBXSourcesBuildPhase();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXSourcesBuildPhase");
            res.SetPropertyString("buildActionMask", "2147483647");
            res.files = new List<string>();
            res.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
            return res;
        }
    }

    internal class PBXFrameworksBuildPhase : FileGUIDListBase
    {
        public static PBXFrameworksBuildPhase Create()
        {
            var res = new PBXFrameworksBuildPhase();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXFrameworksBuildPhase");
            res.SetPropertyString("buildActionMask", "2147483647");
            res.files = new List<string>();
            res.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
            return res;
        }
    }

    internal class PBXResourcesBuildPhase : FileGUIDListBase
    {
        public static PBXResourcesBuildPhase Create()
        {
            var res = new PBXResourcesBuildPhase();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXResourcesBuildPhase");
            res.SetPropertyString("buildActionMask", "2147483647");
            res.files = new List<string>();
            res.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
            return res;
        }
    }

    internal class PBXCopyFilesBuildPhase : FileGUIDListBase
    {
        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "files/*",
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public string name;

        // name may be null
        public static PBXCopyFilesBuildPhase Create(string name, string subfolderSpec)
        {
            var res = new PBXCopyFilesBuildPhase();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXCopyFilesBuildPhase");
            res.SetPropertyString("buildActionMask", "2147483647");
            res.SetPropertyString("dstPath", "");
            res.SetPropertyString("dstSubfolderSpec", subfolderSpec);
            res.files = new List<string>();
            res.SetPropertyString("runOnlyForDeploymentPostprocessing", "0");
            res.name = name;
            return res;
        }

        public override void UpdateProps()
        {
            SetPropertyList("files", files);
            SetPropertyString("name", name);
        }
        public override void UpdateVars()
        {
            files = GetPropertyList("files");
            name = GetPropertyString("name");
        }
    }

    internal class PBXShellScriptBuildPhase : PBXObject
    {
        public GUIDList files;

        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "files/*",
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public override void UpdateProps()
        {
            SetPropertyList("files", files);
        }
        public override void UpdateVars()
        {
            files = GetPropertyList("files");
        }
    }

    internal class BuildConfigEntry
    {
        public string name;
        public List<string> val = new List<string>();

        public static string ExtractValue(string src)
        {
            return PBXStream.UnquoteString(src.Trim().TrimEnd(','));
        }

        public void AddValue(string value)
        {
            if (!val.Contains(value))
                val.Add(value);
        }

        public void RemoveValue(string value)
        {
            val.RemoveAll(v => v == value);
        }

        public static BuildConfigEntry FromNameValue(string name, string value)
        {
            BuildConfigEntry ret = new BuildConfigEntry();
            ret.name = name;
            ret.AddValue(value);
            return ret;
        }
    }

    internal class XCBuildConfiguration : PBXObject
    {
        protected SortedDictionary<string, BuildConfigEntry> entries = new SortedDictionary<string, BuildConfigEntry>();
        public string name { get { return GetPropertyString("name"); } }

        // Note that QuoteStringIfNeeded does its own escaping. Double-escaping with quotes is
        // required to please Xcode that does not handle paths with spaces if they are not
        // enclosed in quotes.
        static string EscapeWithQuotesIfNeeded(string name, string value)
        {
            if (name != "LIBRARY_SEARCH_PATHS")
                return value;
            if (!value.Contains(" "))
                return value;
            if (value.First() == '\"' && value.Last() == '\"')
                return value;
            return "\"" + value + "\"";
        }

        public void SetProperty(string name, string value)
        {
            entries[name] = BuildConfigEntry.FromNameValue(name, EscapeWithQuotesIfNeeded(name, value));
        }

        public void AddProperty(string name, string value)
        {
            if (entries.ContainsKey(name))
                entries[name].AddValue(EscapeWithQuotesIfNeeded(name, value));
            else
                SetProperty(name, value);
        }

        public void RemoveProperty(string name)
        {
            if (entries.ContainsKey(name))
                entries.Remove(name);
        }

        public void RemovePropertyValue(string name, string value)
        {
            if (entries.ContainsKey(name))
                entries[name].RemoveValue(EscapeWithQuotesIfNeeded(name, value));
        }

        // name should be either release or debug
        public static XCBuildConfiguration Create(string name)
        {
            var res = new XCBuildConfiguration();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "XCBuildConfiguration");
            res.SetPropertyString("name", name);
            return res;
        }

        public override void UpdateProps()
        {
            var dict = m_Properties.CreateDict("buildSettings");
            foreach (var kv in entries)
            {
                if (kv.Value.val.Count == 0)
                    continue;
                else if (kv.Value.val.Count == 1)
                    dict.SetString(kv.Key, kv.Value.val[0]);
                else  // kv.Value.val.Count > 1
                {
                    var array = dict.CreateArray(kv.Key);
                    foreach (var value in kv.Value.val)
                        array.AddString(value);
                }
            }
        }
        public override void UpdateVars()
        {
            entries = new SortedDictionary<string, BuildConfigEntry>();
            if (m_Properties.Contains("buildSettings"))
            {
                var dict = m_Properties["buildSettings"].AsDict();
                foreach (var key in dict.values.Keys)
                {
                    var value = dict[key];
                    if (value is PBXElementString)
                    {
                        if (entries.ContainsKey(key))
                            entries[key].val.Add(value.AsString());
                        else
                            entries.Add(key, BuildConfigEntry.FromNameValue(key, value.AsString()));
                    }
                    else if (value is PBXElementArray)
                    {
                        foreach (var pvalue in value.AsArray().values)
                        {
                            if (pvalue is PBXElementString)
                            {
                                if (entries.ContainsKey(key))
                                    entries[key].val.Add(pvalue.AsString());
                                else
                                    entries.Add(key, BuildConfigEntry.FromNameValue(key, pvalue.AsString()));
                            }
                        }
                    }
                }
            }
        }
    }

    internal class PBXContainerItemProxy : PBXObject
    {
        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "containerPortal/*"
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public static PBXContainerItemProxy Create(string containerRef, string proxyType,
                                                   string remoteGlobalGUID, string remoteInfo)
        {
            var res = new PBXContainerItemProxy();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXContainerItemProxy");
            res.SetPropertyString("containerPortal", containerRef); // guid
            res.SetPropertyString("proxyType", proxyType);
            res.SetPropertyString("remoteGlobalIDString", remoteGlobalGUID); // guid
            res.SetPropertyString("remoteInfo", remoteInfo);
            return res;
        }
    }

    internal class PBXReferenceProxy : PBXObject
    {
        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "remoteRef/*"
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public string path { get { return GetPropertyString("path"); } }

        public static PBXReferenceProxy Create(string path, string fileType,
                                               string remoteRef, string sourceTree)
        {
            var res = new PBXReferenceProxy();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXReferenceProxy");
            res.SetPropertyString("path", path);
            res.SetPropertyString("fileType", fileType);
            res.SetPropertyString("remoteRef", remoteRef);
            res.SetPropertyString("sourceTree", sourceTree);
            return res;
        }
    }

    internal class PBXTargetDependency : PBXObject
    {
        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "target/*",
            "targetProxy/*"
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public static PBXTargetDependency Create(string target, string targetProxy)
        {
            var res = new PBXTargetDependency();
            res.guid = PBXGUID.Generate();
            res.SetPropertyString("isa", "PBXTargetDependency");
            res.SetPropertyString("target", target);
            res.SetPropertyString("targetProxy", targetProxy);
            return res;
        }
    }

    internal class ProjectReference
    {
        public string group;      // guid
        public string projectRef; // guid

        public static ProjectReference Create(string group, string projectRef)
        {
            var res = new ProjectReference();
            res.group = group;
            res.projectRef = projectRef;
            return res;
        }
    }

    internal class PBXProjectObject : PBXObject
    {
        private static PropertyCommentChecker checkerData = new PropertyCommentChecker(new string[]{
            "buildConfigurationList/*",
            "mainGroup/*",
            "projectReferences/*/ProductGroup/*",
            "projectReferences/*/ProjectRef/*",
            "targets/*"
        });

        internal override PropertyCommentChecker checker { get { return checkerData; } }

        public List<ProjectReference> projectReferences = new List<ProjectReference>();
        public string mainGroup { get { return GetPropertyString("mainGroup"); } }
        public List<string> targets { get { return GetPropertyList("targets"); } }
        public string buildConfigList;

        public void AddReference(string productGroup, string projectRef)
        {
            projectReferences.Add(ProjectReference.Create(productGroup, projectRef));
        }

        public override void UpdateProps()
        {
            m_Properties.values.Remove("projectReferences");
            if (projectReferences.Count > 0)
            {
                var array = m_Properties.CreateArray("projectReferences");
                foreach (var value in projectReferences)
                {
                    var dict = array.AddDict();
                    dict.SetString("ProductGroup", value.group);
                    dict.SetString("ProjectRef", value.projectRef);
                }
            };
            SetPropertyString("buildConfigurationList", buildConfigList);
        }
        public override void UpdateVars()
        {
            projectReferences = new List<ProjectReference>();
            if (m_Properties.Contains("projectReferences"))
            {
                var el = m_Properties["projectReferences"].AsArray();
                foreach (var value in el.values)
                {
                    PBXElementDict dict = value.AsDict();
                    if (dict.Contains("ProductGroup") && dict.Contains("ProjectRef"))
                    {
                        string group = dict["ProductGroup"].AsString();
                        string projectRef = dict["ProjectRef"].AsString();
                        projectReferences.Add(ProjectReference.Create(group, projectRef));
                    }
                }
            }
            buildConfigList = GetPropertyString("buildConfigurationList");
        }
    }

}
#endif
