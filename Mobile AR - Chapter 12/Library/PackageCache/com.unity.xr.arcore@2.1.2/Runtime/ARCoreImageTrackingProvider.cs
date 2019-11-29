using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// The ARCore implementation of the <c>XRImageTrackingSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public sealed class ARCoreImageTrackingProvider : XRImageTrackingSubsystem
    {
        internal static readonly string k_StreamingAssetsPath =
        #if UNITY_EDITOR
            Path.Combine(Application.streamingAssetsPath, "HiddenARCore");
        #else
            "jar:file://" + Application.dataPath + "!/assets/HiddenARCore";
        #endif

        internal static string GetPathForLibrary(XRReferenceImageLibrary library)
        {
            if (library == null)
                throw new ArgumentNullException("library");

            return Path.Combine(k_StreamingAssetsPath, library.guid.ToString() + ".imgdb");
        }

        class Provider : IProvider
        {
            public unsafe override XRReferenceImageLibrary imageLibrary
            {
                set
                {
                    if (value == null)
                    {
                        UnityARCore_imageTracking_setDatabase(null, 0, null, 0);
                    }
                    else
                    {
                        using (var uwr = new UnityWebRequest(GetPathForLibrary(value)))
                        {
                            uwr.downloadHandler = new DownloadHandlerBuffer();
                            uwr.disposeDownloadHandlerOnDispose = true;
                            uwr.SendWebRequest();
                            while (!uwr.isDone) {}

                            byte[] libraryBlob = uwr.downloadHandler.data;
                            if (libraryBlob == null || libraryBlob.Length == 0)
                            {
                                throw new InvalidOperationException(string.Format(
                                    "Failed to load image library '{0}' - file was empty!", value.name));
                            }

                            var guids = new NativeArray<Guid>(value.count, Allocator.Temp);
                            try
                            {
                                for (int i = 0; i < value.count; ++i)
                                {
                                    guids[i] = value[i].guid;
                                }

                                fixed (byte* blob = libraryBlob)
                                {
                                    UnityARCore_imageTracking_setDatabase(
                                        blob,
                                        libraryBlob.Length,
                                        guids.GetUnsafePtr(),
                                        guids.Length);
                                }
                            }
                            finally
                            {
                                guids.Dispose();
                            }
                        }
                    }
                }
            }

            public unsafe override TrackableChanges<XRTrackedImage> GetChanges(
                XRTrackedImage defaultTrackedImage,
                Allocator allocator)
            {
                void* addedPtr, updatedPtr, removedPtr;
                int addedLength, updatedLength, removedLength, stride;

                var context = UnityARCore_imageTracking_acquireChanges(
                    out addedPtr, out addedLength,
                    out updatedPtr, out updatedLength,
                    out removedPtr, out removedLength,
                    out stride);

                try
                {
                    return new TrackableChanges<XRTrackedImage>(
                        addedPtr, addedLength,
                        updatedPtr, updatedLength,
                        removedPtr, removedLength,
                        defaultTrackedImage, stride,
                        allocator);
                }
                finally
                {
                    UnityARCore_imageTracking_releaseChanges(context);
                }
            }

            public override void Destroy()
            {
                UnityARCore_imageTracking_destroy();
            }

            // This must be implemented if supportsMovingImages is true.
            public override int maxNumberOfMovingImages
            {
                set { }
            }

            [DllImport("UnityARCore")]
            static unsafe extern void UnityARCore_imageTracking_setDatabase(
                void* databaseBytes, int databaseSize, void* sourceGuidBytes, int sourceGuidLength);

            [DllImport("UnityARCore")]
            static extern void UnityARCore_imageTracking_destroy();

            [DllImport("UnityARCore")]
            static extern unsafe void* UnityARCore_imageTracking_acquireChanges(
                out void* addedPtr, out int addedLength,
                out void* updatedPtr, out int updatedLength,
                out void* removedPtr, out int removedLength,
                out int stride);

            [DllImport("UnityARCore")]
            static extern unsafe void UnityARCore_imageTracking_releaseChanges(void* changes);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDescriptor()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            XRImageTrackingSubsystemDescriptor.Create(new XRImageTrackingSubsystemDescriptor.Cinfo
            {
                id = "ARCore-ImageTracking",
                subsystemImplementationType = typeof(ARCoreImageTrackingProvider),
                supportsMovingImages = true
            });
#endif
        }

        protected override IProvider CreateProvider()
        {
            return new Provider();
        }
    }
}
