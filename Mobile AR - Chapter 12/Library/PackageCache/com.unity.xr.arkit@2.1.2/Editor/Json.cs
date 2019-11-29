using System;

namespace UnityEditor.XR.ARKit
{
    internal static class Json
    {
        [Serializable]
        public struct AuthorInfo
        {
            public int version;
            public string author;
        }

        [Serializable]
        public struct Filename
        {
            public string filename;
        }

        [Serializable]
        public struct ResourceGroup
        {
            public AuthorInfo info;
            public Filename[] resources;
        }

        [Serializable]
        public struct ImageProperties
        {
            public float width;
        }

        [Serializable]
        public struct ReferenceImage
        {
            public AuthorInfo info;
            public FilenameWithIdiom[] images;
            public ImageProperties properties;
        }

        [Serializable]
        public struct FilenameWithIdiom
        {
            public string filename;
            public string idiom;
        }
    }
}
