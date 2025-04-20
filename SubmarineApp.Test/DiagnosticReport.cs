namespace SubmarineApp.DiagnosticReportTest.UnitTests
{
    using System.Diagnostics;
    using System.Numerics;
    using DiagnosticReport;

    [TestClass]
    public class DiagnosticReportTest
    {
#pragma warning disable CS8618
        ///
        /// CS8618: TestContext may be null after constructor.
        /// Reason for disabling: TestContext is set by MSTest after the constructor.
        ///
        public TestContext TestContext { get; set; }
#pragma warning restore CS8618

        [TestInitialize]
        public void InitializeTest()
        {
            if (TestContext.DeploymentDirectory is string dir && TestContext.TestName is string testName)
            {
                var fileName = Path.Combine(dir, testName + "_Trace.txt");
                Trace.Listeners.Add(new TextWriterTraceListener(fileName));
            }
        }

        [TestCleanup]
        public void CleanupTest()
        {
            Trace.Close();
        }

        ///
        /// Checks diagnostic report instantiation with null or empty data.
        ///
        [TestMethod]
        public void DataIsNotEmpty()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new DiagnosticReport([]),
                "Should fail if data is empty."
            );

            Assert.ThrowsException<ArgumentException>(
                () => new DiagnosticReport([[]]),
                "Should fail if data is empty."
            );

            Assert.ThrowsException<ArgumentException>(
                () => DiagnosticReport.FromText(""),
                "Should fail if text is empty."
            );
        }

        ///
        /// Checks diagnostic report instantiation with badly formatted (invalid) data.
        /// - Data should be only 1s and 0s.
        ///
        [TestMethod]
        [DataRow("2")]
        [DataRow("A")]
        [DataRow("10\n01\n09")]
        [DataRow("10\n01\nZ0")]
        public void DataIsValid_ValidCharacters(string text)
        {
            var data = DiagnosticReport.DataFromText(text);

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new DiagnosticReport(data),
                "Should fail if data contains invalid bits."
            );

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => DiagnosticReport.FromText(text),
                "Should fail if text contains invalid  chars."
            );
        }

        ///
        /// Checks diagnostic report instantiation with badly formatted (invalid) data.
        /// - Data should have the same dimensions (same amount of bits for every reading).
        ///
        [TestMethod]
        [DataRow("0101\n1010\n010\n1111\n0000")]
        public void DataIsValid_ReadingsDimensions(string text)
        {
            var data = DiagnosticReport.DataFromText(text);

            Assert.ThrowsException<ArgumentException>(
                () => new DiagnosticReport(data),
                "Should fail if data dimensions do not match."
            );

            Assert.ThrowsException<ArgumentException>(
                () => DiagnosticReport.FromText(text),
                "Should fail if text readings dimensions do not match."
            );
        }

        ///
        /// Should compute gama rate, epsilon rate and energy consumption correctly on
        /// valid data.
        ///
        [TestMethod]
        [DataRow("TestData/Dataset1.txt")]
        [DataRow("TestData/Dataset2.txt")]
        public void GamaAndEpsilonAreCorrect_CanCompute(string inputFilePath)
        {
            var text = File.ReadAllText(inputFilePath);
            var report = DiagnosticReport.FromText(text);

            var gamaRate = report.GetGamaRate();
            Assert.IsTrue(gamaRate >= 0);

            Trace.WriteLine($"Gama rate: {gamaRate}");

            var epsilonRate = report.GetEpsilonRate();
            Assert.IsTrue(epsilonRate >= 0);

            Trace.WriteLine($"Epsilon rate: {epsilonRate}");

            try
            {
                ulong energyConsumption = report.GetEnergyConsumption();
                Assert.IsTrue(energyConsumption >= 0);

                Trace.WriteLine($"Energy consumption: {energyConsumption}");
            }
            catch (OverflowException)
            {
                BigInteger energyConsumption = report.GetEnergyConsumptionBig();
                Assert.IsTrue(energyConsumption >= 0);

                Trace.WriteLine($"Energy consumption (big): {energyConsumption}");
            }
        }
    }
}
