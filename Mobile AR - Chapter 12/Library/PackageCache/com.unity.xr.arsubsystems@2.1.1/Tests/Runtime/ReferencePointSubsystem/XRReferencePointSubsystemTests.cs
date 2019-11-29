using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARSubsystems.Tests
{
    public class XRReferencePointSubsystemImpl : XRReferencePointSubsystem
    {
        protected override IProvider CreateProvider()
        {
            return new IProvider();
        }
    }

    [TestFixture]
    public class XRReferencePointSubsystemTestFixture
    {
        [Test]
        public void RunningStateTests()
        {
            XRReferencePointSubsystem subsystem = new XRReferencePointSubsystemImpl();
            
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