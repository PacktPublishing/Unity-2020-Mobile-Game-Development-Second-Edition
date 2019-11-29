using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRPlaneSubsystemImpl : XRPlaneSubsystem
    {
        protected override IProvider CreateProvider()
        {
            return new IProvider();
        }
    }

    [TestFixture]
    public class XRPlaneSubsystemTestFixture
    {
         [Test]
        public void RunningStateTests()
        {
            XRPlaneSubsystem subsystem = new XRPlaneSubsystemImpl();
            
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