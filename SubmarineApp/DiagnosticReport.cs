using System.Numerics;

namespace SubmarineApp.DiagnosticReport
{
    public class DiagnosticReport
    {
        private IEnumerable<ulong>? bitAcc;
        private int readingLength;
        private int readingCount;
        private DiagnosticData data = Enumerable.Empty<DiagnosticDataReading>();

        ///
        /// This stores the sum of the bits from every reading for every position
        /// of the reading. Should have the same length as a reading.
        ///
        private IEnumerable<ulong> BitAcc
        {
            get
            {
                if (bitAcc is not null)
                {
                    return bitAcc;
                }

                bitAcc = Enumerable.Repeat(0UL, ReadingLength).ToArray();
                var bitAccArray = (ulong[])bitAcc;

                foreach (var reading in Data)
                {
                    var i = 0;
                    foreach (var bit in reading)
                    {
                        bitAccArray[i++] += bit;
                    }
                }

                return bitAcc;
            }
        }

        ///
        /// Diagnostic data is a collection of readings (rows) with fixed
        /// length (reading length) arrays of bits (0 or 1).
        /// @throws ArgumentException
        /// @throws ArgumentNullException
        /// @throws ArgumentOutOfRangeException
        ///
        public DiagnosticData Data
        {
            get => data;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException("Diagnostic data can not be null.");
                }

                var readingLength = value.FirstOrDefault()?.Count() ?? 0;
                var readingCount = value.Count();
                if (readingLength == 0 || readingCount == 0)
                {
                    throw new ArgumentException("Diagnostic data can not be empty.");
                }

                if (value.Any((reading) => reading.Count() != readingLength))
                {
                    throw new ArgumentException("Every diagnostic reading should have the same amount of bits.");
                }

                if (value.SelectMany((reading) => reading).Any((bit) => bit != 0 && bit != 1))
                {
                    throw new ArgumentOutOfRangeException("Diagnostic bits should be 0 or 1.");
                }

                data = value;
                this.readingLength = value.First().Count();
                this.readingCount = value.Count();
                /* Sum is recalculated when BitAcc is accessed again */
                bitAcc = default;
            }
        }

        ///
        /// The amount of bits in a single reading. Should be the same for every reading.
        ///
        public int ReadingLength { get => readingLength; }

        ///
        /// The amount of readings (rows) in the diagnostic data.
        ///
        public int ReadingCount { get => readingCount; }

        ///
        /// Parses the input text into a DiagnosticData object.
        /// Only checks if the data is made of 0s and 1s.
        /// @throws ArgumentOutOfRangeException
        ///
        public static DiagnosticData DataFromText(string text)
        {
            var data = text
                .Trim()
                .ReplaceLineEndings()
                .Split(Environment.NewLine)
                .Select((line) =>
                    line.Select((c, index) => (c) switch
                        {
                            '0' => (byte)0,
                            '1' => (byte)1,
                            _ => throw new ArgumentOutOfRangeException("Diagnostic bits should be 0 or 1."),
                        }
                    )
                )
                .ToArray();

            return data;
        }

        ///
        /// Instantiates a new DiagnosticReport.
        /// The input text is parsed into the report's data.
        ///
        public static DiagnosticReport FromText(string text)
        {
            return new(DataFromText(text));
        }

        public DiagnosticReport(DiagnosticData data)
        {
            Data = data;
        }

        ///
        /// The gama rate is computed by generating a binary integer from
        /// the diagnostic data. For every reading position in the data, if
        /// for that position the amount of bits set to 1 is larger than
        /// half of the amount of readings, then set the gama rate bit at
        /// that position to 1, 0 otherwise.
        ///
        public ulong GetGamaRate()
        {
            var gamaRateBits = BitAcc.Select((value) => value > (ulong)ReadingCount / 2UL ? 1 : 0);
            return Convert.ToUInt64(string.Join("", gamaRateBits), 2);
        }

        ///
        /// The epsilon rate is computed by generating a binary integer from
        /// the diagnostic data. For every reading position in the data, if
        /// for that position the amount of bits set to 1 is smaller than
        /// half of the amount of readings, then set the epsilon rate bit at
        /// that position to 1, 0 otherwise.
        ///
        public ulong GetEpsilonRate()
        {
            var epsilonRateBits = BitAcc.Select((value) => value < (ulong)ReadingCount / 2UL ? 1 : 0);
            return Convert.ToUInt64(string.Join("", epsilonRateBits), 2);
        }

        ///
        /// Energy consumption is computed by multiplying the gama rate and epsilon rate.
        /// @throws OverflowException
        ///
        public ulong GetEnergyConsumption()
        {
            // ulong values are used for the report data and no overflow check is made
            // (but it could happen), but for energy consumptions, as it multiplies two
            // ulong values, it's way more likely to overflow on large input.
            // To deal with large input, use GetEnergyConsumptionBig();
            return checked(GetGamaRate() * GetEpsilonRate());
        }

        ///
        /// Energy consumption is computed by multiplying the gama rate and epsilon rate.
        /// Deals with large values.
        ///
        public BigInteger GetEnergyConsumptionBig()
        {
            return new BigInteger(GetGamaRate()) * new BigInteger(GetEpsilonRate());
        }
    }
}
