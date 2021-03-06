﻿using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syndll2.Data;

namespace Syndll2.FunctionalTests.StatusTests
{
    [TestClass]
    public class GetFingerprintUnitStatusTests
    {
        [TestMethod]
        public void Can_Get_Fingerprint_Unit_Status()
        {
            using (var client = TestSettings.Connect())
            using (var p = client.Terminal.Programming())
            {
                var status = p.Fingerprint.GetUnitStatus();
                Assert.IsNotNull(status);
                DisplayStatus(status);
            }
        }

        [TestMethod]
        public async Task Can_Get_Fingerprint_Unit_Status_Async()
        {
            using (var client = await TestSettings.ConnectAsync())
            using (var p = client.Terminal.Programming())
            {
                var status = await p.Fingerprint.GetUnitStatusAsync();
                Assert.IsNotNull(status);
                DisplayStatus(status);
            }
        }

        private static void DisplayStatus(FingerprintUnitStatus status)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Comparison Mode:   {0}", status.ComparisonMode);
            Debug.WriteLine("Kernel Version:    {0}", new object[] { status.KernelVersion });
            Debug.WriteLine("Loaded Templates:  {0}", status.LoadedTemplates);
            Debug.WriteLine("Maximum Templates: {0}", status.MaximumTemplates);
            Debug.WriteLine("FPU Mode:          {0}", status.FingerprintUnitMode);
            Debug.WriteLine("Global Threshold:  {0}", status.GlobalThreshold);
            Debug.WriteLine("Enroll Mode:       {0}", status.EnrollMode);
            Debug.WriteLine("");
        }
    }
}
