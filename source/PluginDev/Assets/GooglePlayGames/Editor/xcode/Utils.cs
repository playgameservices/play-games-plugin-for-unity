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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.IO;

    internal class GUIDToCommentMap
    {
        private Dictionary<string, string> m_Dict = new Dictionary<string, string>();

        public string this[string guid]
        {
            get {
                if (m_Dict.ContainsKey(guid))
                    return m_Dict[guid];
                return null;
            }
        }

        public void Add(string guid, string comment)
        {
            if (m_Dict.ContainsKey(guid))
                return;
            m_Dict.Add(guid, comment);
        }

        public void Remove(string guid)
        {
            m_Dict.Remove(guid);
        }

        public string Write(string guid)
        {
            string comment = this[guid];
            if (comment == null)
                return guid;
            return String.Format("{0} /* {1} */", guid, comment);
        }

        public void WriteStringBuilder(StringBuilder sb, string guid)
        {
            string comment = this[guid];
            if (comment == null)
                sb.Append(guid);
            else
            {
                // {0} /* {1} */
                sb.Append(guid).Append(" /* ").Append(comment).Append(" */");
            }
        }
    }

    internal class PBXGUID
    {
        internal delegate string GuidGenerator();

        // We allow changing Guid generator to make testing of PBXProject possible
        private static GuidGenerator guidGenerator = DefaultGuidGenerator;

        internal static string DefaultGuidGenerator()
        {
            return Guid.NewGuid().ToString("N").Substring(8).ToUpper();
        }

        internal static void SetGuidGenerator(GuidGenerator generator)
        {
            guidGenerator = generator;
        }

        // Generates a GUID.
        public static string Generate()
        {
            return guidGenerator();
        }
    }

    internal class PBXRegex
    {
        public static string GuidRegexString = "[A-Fa-f0-9]{24}";
    }

    internal class PBXStream
    {
        static bool DontNeedQuotes(string src)
        {
            // using a regex instead of explicit matching slows down common cases by 40%
            if (src.Length == 0)
                return false;

            bool hasSlash = false;
            for (int i = 0; i < src.Length; ++i)
            {
                char c = src[i];
                if (Char.IsLetterOrDigit(c) || c == '.' || c == '*' || c == '_')
                    continue;
                if (c == '/')
                {
                    hasSlash = true;
                    continue;
                }
                return false;
            }
            if (hasSlash)
            {
                if (src.Contains("//") || src.Contains("/*") || src.Contains("*/"))
                    return false;
            }
            return true;
        }

        // Quotes the given string if it contains special characters. Note: if the string already
        // contains quotes, then they are escaped and the entire string quoted again
        public static string QuoteStringIfNeeded(string src)
        {
            if (DontNeedQuotes(src))
                return src;
            return "\"" + src.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
        }

        // If the given string is quoted, removes the quotes and unescapes any quotes within the string
        public static string UnquoteString(string src)
        {
            if (!src.StartsWith("\"") || !src.EndsWith("\""))
                return src;
            return src.Substring(1, src.Length - 2).Replace("\\\\", "\u569f").Replace("\\\"", "\"")
                .Replace("\\n", "\n").Replace("\u569f", "\\"); // U+569f is a rarely used Chinese character
        }
    }

    internal enum PBXFileType
    {
        NotBuildable,
        Framework,
        Source,
        Resource,
        CopyFile
    }

    internal class FileTypeUtils
    {
        internal class FileTypeDesc
        {
            public FileTypeDesc(string typeName, PBXFileType type)
            {
                this.name = typeName;
                this.type = type;
                this.isExplicit = false;
            }

            public FileTypeDesc(string typeName, PBXFileType type, bool isExplicit)
            {
                this.name = typeName;
                this.type = type;
                this.isExplicit = isExplicit;
            }

            public string name;
            public PBXFileType type;
            public bool isExplicit;
        }

        private static readonly Dictionary<string, FileTypeDesc> types =
            new Dictionary<string, FileTypeDesc>
        {
            { ".a",         new FileTypeDesc("archive.ar",              PBXFileType.Framework) },
            { ".app",       new FileTypeDesc("wrapper.application",     PBXFileType.NotBuildable, true) },
            { ".appex",     new FileTypeDesc("wrapper.app-extension",   PBXFileType.CopyFile) },
            { ".bin",       new FileTypeDesc("archive.macbinary",       PBXFileType.Resource) },
            { ".s",         new FileTypeDesc("sourcecode.asm",          PBXFileType.Source) },
            { ".c",         new FileTypeDesc("sourcecode.c.c",          PBXFileType.Source) },
            { ".cc",        new FileTypeDesc("sourcecode.cpp.cpp",      PBXFileType.Source) },
            { ".cpp",       new FileTypeDesc("sourcecode.cpp.cpp",      PBXFileType.Source) },
            { ".swift",     new FileTypeDesc("sourcecode.swift",        PBXFileType.Source) },
            { ".dll",       new FileTypeDesc("file",                    PBXFileType.NotBuildable) },
            { ".framework", new FileTypeDesc("wrapper.framework",       PBXFileType.Framework) },
            { ".h",         new FileTypeDesc("sourcecode.c.h",          PBXFileType.NotBuildable) },
            { ".pch",       new FileTypeDesc("sourcecode.c.h",          PBXFileType.NotBuildable) },
            { ".icns",      new FileTypeDesc("image.icns",              PBXFileType.Resource) },
            { ".xcassets",  new FileTypeDesc("folder.assetcatalog",     PBXFileType.Resource) },
            { ".inc",       new FileTypeDesc("sourcecode.inc",          PBXFileType.NotBuildable) },
            { ".m",         new FileTypeDesc("sourcecode.c.objc",       PBXFileType.Source) },
            { ".mm",        new FileTypeDesc("sourcecode.cpp.objcpp",   PBXFileType.Source ) },
            { ".nib",       new FileTypeDesc("wrapper.nib",             PBXFileType.Resource) },
            { ".plist",     new FileTypeDesc("text.plist.xml",          PBXFileType.Resource) },
            { ".png",       new FileTypeDesc("image.png",               PBXFileType.Resource) },
            { ".rtf",       new FileTypeDesc("text.rtf",                PBXFileType.Resource) },
            { ".tiff",      new FileTypeDesc("image.tiff",              PBXFileType.Resource) },
            { ".txt",       new FileTypeDesc("text",                    PBXFileType.Resource) },
            { ".json",      new FileTypeDesc("text.json",               PBXFileType.Resource) },
            { ".xcodeproj", new FileTypeDesc("wrapper.pb-project",      PBXFileType.NotBuildable) },
            { ".xib",       new FileTypeDesc("file.xib",                PBXFileType.Resource) },
            { ".strings",   new FileTypeDesc("text.plist.strings",      PBXFileType.Resource) },
            { ".storyboard",new FileTypeDesc("file.storyboard",         PBXFileType.Resource) },
            { ".bundle",    new FileTypeDesc("wrapper.plug-in",         PBXFileType.Resource) },
            { ".dylib",     new FileTypeDesc("compiled.mach-o.dylib",   PBXFileType.Framework) },
            { ".dat",       new FileTypeDesc("file",                    PBXFileType.NotBuildable) }
        };

        public static bool IsKnownExtension(string ext)
        {
            return types.ContainsKey(ext);
        }

        internal static bool IsFileTypeExplicit(string ext)
        {
            if (types.ContainsKey(ext))
                return types[ext].isExplicit;
            return false;
        }

        public static PBXFileType GetFileType(string ext)
        {
            if (!types.ContainsKey(ext))
                return PBXFileType.Resource;
            return types[ext].type;
        }

        public static string GetTypeName(string ext)
        {
            if (types.ContainsKey(ext))
                return types[ext].name;
            // Xcode actually checks the file contents to determine the file type.
            // Text files have "text" type and all other files have "file" type.
            // Since we can't reasonably determine whether the file in question is
            // a text file, we just take the safe route and return "file" type.
            return "file";
        }

        public static bool IsBuildable(string ext)
        {
            if (!types.ContainsKey(ext))
                return true;
            if (types[ext].type != PBXFileType.NotBuildable)
                return true;
            return false;
        }

        private static readonly Dictionary<PBXSourceTree, string> sourceTree = new Dictionary<PBXSourceTree, string>
        {
            { PBXSourceTree.Absolute,   "<absolute>" },
            { PBXSourceTree.Group,      "<group>" },
            { PBXSourceTree.Build,      "BUILT_PRODUCTS_DIR" },
            { PBXSourceTree.Developer,  "DEVELOPER_DIR" },
            { PBXSourceTree.Sdk,        "SDKROOT" },
            { PBXSourceTree.Source,     "SOURCE_ROOT" },
        };

        private static readonly Dictionary<string, PBXSourceTree> stringToSourceTreeMap = new Dictionary<string, PBXSourceTree>
        {
            { "<absolute>",         PBXSourceTree.Absolute },
            { "<group>",            PBXSourceTree.Group },
            { "BUILT_PRODUCTS_DIR", PBXSourceTree.Build },
            { "DEVELOPER_DIR",      PBXSourceTree.Developer },
            { "SDKROOT",            PBXSourceTree.Sdk },
            { "SOURCE_ROOT",        PBXSourceTree.Source },
        };

        internal static string SourceTreeDesc(PBXSourceTree tree)
        {
            return sourceTree[tree];
        }

        // returns PBXSourceTree.Source on error
        internal static PBXSourceTree ParseSourceTree(string tree)
        {
            if (stringToSourceTreeMap.ContainsKey(tree))
                return stringToSourceTreeMap[tree];
            return PBXSourceTree.Source;
        }

        internal static List<PBXSourceTree> AllAbsoluteSourceTrees()
        {
            return new List<PBXSourceTree>{PBXSourceTree.Absolute, PBXSourceTree.Build,
                PBXSourceTree.Developer, PBXSourceTree.Sdk, PBXSourceTree.Source};
        }
    }

    internal class Utils
    {
        /// Replaces '\' with '/'. We need to apply this function to all paths that come from the user
        /// of the API because we store paths to pbxproj and on windows we may get path with '\' slashes
        /// instead of '/' slashes
        public static string FixSlashesInPath(string path)
        {
            if (path == null)
                return null;
            return path.Replace('\\', '/');
        }

        public static void CombinePaths(string path1, PBXSourceTree tree1, string path2, PBXSourceTree tree2,
                                        out string resPath, out PBXSourceTree resTree)
        {
            if (tree2 == PBXSourceTree.Group)
            {
                resPath = CombinePaths(path1, path2);
                resTree = tree1;
                return;
            }

            resPath = path2;
            resTree = tree2;
        }

        public static string CombinePaths(string path1, string path2)
        {
            if (path2.StartsWith("/"))
                return path2;
            if (path1.EndsWith("/"))
                return path1 + path2;
            if (path1 == "")
                return path2;
            if (path2 == "")
                return path1;
            return path1 + "/" + path2;
        }

        public static string GetDirectoryFromPath(string path)
        {
            int pos = path.LastIndexOf('/');
            if (pos == -1)
                return "";
            else
                return path.Substring(0, pos);
        }

        public static string GetFilenameFromPath(string path)
        {
            int pos = path.LastIndexOf('/');
            if (pos == -1)
                return path;
            else
                return path.Substring(pos + 1);
        }
    }
}
#endif
