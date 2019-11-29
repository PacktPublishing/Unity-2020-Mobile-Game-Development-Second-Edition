using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRDepthSubsystemImpl : XRDepthSubsystem
    {
        protected override IDepthApi GetInterface()
        {
            return new IDepthApi();
        }
    }

    [TestFixture]
    public class XRDepthSubsystemTestFixture
    {
         [Test]
        public void RunningStateTests()
        {
            XRDepthSubsystem subsystem = new XRDepthSubsystemImpl();
            
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