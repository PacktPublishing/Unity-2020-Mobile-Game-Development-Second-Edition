using UnityEngine.TestTools;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRCameraSubsystemImpl : XRCameraSubsystem
    {
        class ProviderImpl : IProvider
        {
            public override void Start() {}
            public override void Stop() {}
            public override void Destroy() {}
            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                cameraFrame = default(XRCameraFrame);
                return false;
            }
            public override string shaderName { get { return null; } }
            public override bool TrySetFocusMode(CameraFocusMode cameraFocusMode) { return false; }
            public override bool TrySetLightEstimationMode(LightEstimationMode lightEstimationMode) { return false; }
            public override bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics) { cameraIntrinsics = default(XRCameraIntrinsics); return false; }
            public override bool permissionGranted { get { return false; } }
        }

        protected override IProvider CreateProvider()
        {
            return new ProviderImpl();
        }
    }

    [TestFixture]
    public class XRCameraSubsystemTestFixture
    {
         [Test]
        public void RunningStateTests()
        {
            XRCameraSubsystem subsystem = new XRCameraSubsystemImpl();
            
            // Initial state is not running
            Assert.That(subsystem.running == false);

            // After start subsystem is running
            subsystem.Start();
            Assert.That(subsystem.running == true);

            // After start subsystem is running
            subsystem.Stop();
            Assert.That(subsystem.running == false);
        }
    }
}