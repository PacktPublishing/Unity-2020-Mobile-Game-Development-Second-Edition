using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.TestTools;
using UnityEngine.XR.ARSubsystems;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
#endif

namespace UnityEngine.XR.ARKit.Tests
{
    [TestFixture]
    public class ARKitTestFixture
    {
        [Test]
        public void DepthSubsystemRegistered()
        {
#if !UNITY_EDITOR
            Assert.That(SubsystemDescriptorRegistered<XRDepthSubsystemDescriptor>("ARKit-Depth"));
#endif
        }

        [Test]
        public void SessionSubsystemRegistered()
        {
#if !UNITY_EDITOR
            Assert.That(SubsystemDescriptorRegistered<XRSessionSubsystemDescriptor>("ARKit-Session"));
#endif
        }

        [Test]
        public void PlaneSubsystemRegistered()
        {
#if !UNITY_EDITOR
            Assert.That(SubsystemDescriptorRegistered<XRPlaneSubsystemDescriptor>("ARKit-Plane"));
#endif
        }

        [Test]
        public void RaycastSubsystemRegistered()
        {
#if !UNITY_EDITOR
            Assert.That(SubsystemDescriptorRegistered<XRRaycastSubsystemDescriptor>("ARKit-Raycast"));
#endif
        }

        [Test]
        public void ReferencePointSubsystemRegistered()
        {
#if !UNITY_EDITOR
            Assert.That(SubsystemDescriptorRegistered<XRReferencePointSubsystemDescriptor>("ARKit-ReferencePoint"));
#endif
        }

        [Test]
        public void CameraSubsystemRegistered()
        {
#if !UNITY_EDITOR
            Assert.That(SubsystemDescriptorRegistered<XRCameraSubsystemDescriptor>("ARKit-Camera"));
#endif
        }
        bool SubsystemDescriptorRegistered<T>(string id) where T : SubsystemDescriptor
        {
            List<T> descriptors = new List<T>();

            SubsystemManager.GetSubsystemDescriptors<T>(descriptors);

            foreach(T descriptor in descriptors)
            {
                if (descriptor.id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
