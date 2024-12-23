using System.Collections;
using System.Net;
using FluentAssertions;
using FluentAssertions.Json;
using IVFSuccessCalculator.Domain;
using IVFSuccessCalculator.Tests.Integration.Fixtures;
using Newtonsoft.Json.Linq;

namespace IVFSuccessCalculator.Tests.Integration
{
    public class IVFSuccessCalculatorIntegrationTests : IClassFixture<ServerFixture>
    {
        private readonly ServerFixture _server;

        public IVFSuccessCalculatorIntegrationTests(ServerFixture server)
        {
            _server = server;
        }

        [Theory]
        [ClassData(typeof(IVFSuccessRateCalculations))]
        public async Task Calculates_Success_Rate_For_Requests(SuccessRateCalculationRequest request, double expectedSuccessRate)
        {
            var result = await _server.CalculateSuccessRate(request);

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var successRate = double.Parse(await result.Content.ReadAsStringAsync());
            successRate.Should().BeApproximately(expectedSuccessRate, 1e-3);
        }

        [Theory]
        [ClassData(typeof(IVFSuccessRateCalculationInvalidRequests))]
        public async Task Calculation_NotProcessed_When_Request_Payload_IsInvalid(SuccessRateCalculationRequest request, JToken expectedValidationErrors)
        {
            var result = await _server.CalculateSuccessRate(request);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var validationErrors = JToken.Parse(await result.Content.ReadAsStringAsync());
            validationErrors.Should().BeEquivalentTo(expectedValidationErrors);
        }

        [Fact]
        public async Task Calculation_NotProcessed_When_Required_Request_Parameters_Omitted()
        {
            // ARRANGE
            var request = new SuccessRateCalculationRequest
            {
                Age = 25,
                // Height is omitted
                Weight = 100,

                UsingOwnEggs = false,
                UsedIvfBefore = true,

                ReasonForInfertilityKnown = true,
                InfertilityDiagnosis = new InfertilityFactors
                {
                    MaleFactorInf = true
                },

                // Num prior pregnancies is omitted
                NumLiveBirths = 0
            };

            // ACT
            var result = await _server.CalculateSuccessRate(request);

            // ASSERT
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var errors = JToken.Parse(await result.Content.ReadAsStringAsync())["errors"];
            errors.Should().BeEquivalentTo(JToken.FromObject(new
            {
                Height = new[] { "The Height field is required." },
                NumPriorPregnancies = new[] { "The NumPriorPregnancies field is required." }
            }));
        }

        private class IVFSuccessRateCalculations : IEnumerable<object[]>
        {
            private const int FT = 12;

            public IEnumerator<object[]> GetEnumerator()
            {
                // Using CDC Formula 1-3
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Height = 5*FT + 8,
                        Weight = 150,

                        UsingOwnEggs = true,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = true,
                        InfertilityDiagnosis = new InfertilityFactors {
                            Endometriosis = true,
                            OvulatoryDisorder = true
                        },

                        NumPriorPregnancies = 1,
                        NumLiveBirths = 1
                    },

                    0.622 // 62 %
                ];

                // Using CDC Formula 4-6
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Height = 5*FT + 8,
                        Weight = 150,

                        UsingOwnEggs = true,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 1,
                        NumLiveBirths = 1
                    },

                    0.5983 // 59%
                ];

                // Using CDC Formula 7-8
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Height = 5*FT + 8,
                        Weight = 150,

                        UsingOwnEggs = true,
                        UsedIvfBefore = true,

                        ReasonForInfertilityKnown = true,
                        InfertilityDiagnosis = new InfertilityFactors {
                            TubalFactor = true,
                            DiminishedOvarianReserve = true,
                        },

                        NumPriorPregnancies = 1,
                        NumLiveBirths = 1
                    },

                    0.4089 // 41%
                ];

                // Using CDC Formula 9-10
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 29,
                        Height = 5*FT + 2,
                        Weight = 130,

                        UsingOwnEggs = true,
                        UsedIvfBefore = true,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 2,
                        NumLiveBirths = 1
                    },

                    0.563 // 56%
                ];

                // Using CDC Formula 11-13
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 25,
                        Height = 4*FT + 9,
                        Weight = 100,

                        UsingOwnEggs = false,
                        UsedIvfBefore = true,

                        ReasonForInfertilityKnown = true,
                        InfertilityDiagnosis = new InfertilityFactors {
                            MaleFactorInf = true
                        },

                        NumPriorPregnancies = 0,
                        NumLiveBirths = 0
                    },

                    0.585 // 59%
                ];

                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 25,
                        Height = 4*FT + 9,
                        Weight = 100,

                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = true,
                        InfertilityDiagnosis = new InfertilityFactors {
                            MaleFactorInf = true
                        },

                        NumPriorPregnancies = 0,
                        NumLiveBirths = 0
                    },

                    0.585 // 59%
                ];

                // Using CDC Formula 14-16
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 45,
                        Height = 5*FT + 7,
                        Weight = 130,

                        UsingOwnEggs = false,
                        UsedIvfBefore = true,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 2,
                        NumLiveBirths = 1
                    },

                    0.529 // 53%
                ];

                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 45,
                        Height = 5*FT + 7,
                        Weight = 130,

                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 2,
                        NumLiveBirths = 1
                    },

                    0.529 // 53%
                ];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class IVFSuccessRateCalculationInvalidRequests : IEnumerable<object[]>
        {
            private const int FT = 12;

            public IEnumerator<object[]> GetEnumerator()
            {
                // Age and weight are outside of the current range
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 80,
                        Weight = 301,

                        Height = 5*FT + 2,
                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 0,
                        NumLiveBirths = 0
                    },

                    JToken.FromObject(new {
                        Age = new[] { "'Age' must be between 20 and 50. You entered 80." },
                        Weight = new[] { "'Weight' must be between 80 and 300. You entered 301." }
                    })
                ];

                // Height is outside of the allowed range
                yield return [
                    new SuccessRateCalculationRequest {
                        Height = 4*FT + 3,

                        Age = 32,
                        Weight = 150,
                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 0,
                        NumLiveBirths = 0
                    },

                    JToken.FromObject(new {
                        Height = new[] { "'Height' must be between 4'6\" and 6'. You entered 4'3\"" }
                    })
                ];

                // Number of prior pregnancies and live births is an invalid quantity (negative).
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Weight = 150,
                        Height = 5*FT + 8,

                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = -1,
                        NumLiveBirths = -1
                    },

                    JToken.FromObject(new {
                        NumPriorPregnancies = new[] { "'Num Prior Pregnancies' must be greater than or equal to '0'." },
                        NumLiveBirths = new[] { "'Num Live Births' must be greater than or equal to '0'." }
                    })
                ];

                // Number of live births exceeds number of prior pregnancies specified.
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Weight = 150,
                        Height = 5*FT + 8,

                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 1,
                        NumLiveBirths = 2
                    },

                    JToken.FromObject(new {
                        NumLiveBirths = new[] { "The number of live births cannot exceed the number of prior pregnancies." }
                    })
                ];

                // Infertility factors specified when reason for infertility is not known
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Weight = 150,
                        Height = 5*FT + 8,

                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = false,
                        InfertilityDiagnosis = new InfertilityFactors {
                             MaleFactorInf = true
                        },

                        NumPriorPregnancies = 0,
                        NumLiveBirths = 0
                    },

                    JToken.FromObject(new {
                        InfertilityDiagnosis = new[] { "Infertility factors cannot be specified when reason for infertility is not known." }
                    })
                ];

                // Infertility factors specified when diagnosis has yielded unexplainable infertility.
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Weight = 150,
                        Height = 5*FT + 8,

                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = true,
                        InfertilityDiagnosis = new InfertilityFactors {
                            Endometriosis = true,
                            UnexplainedInf = true
                        },

                        NumPriorPregnancies = 0,
                        NumLiveBirths = 0
                    },

                    JToken.FromObject(new {
                        InfertilityDiagnosis = new[] { "Infertility factors cannot be specified if infertility is unexplained." }
                    })
                ];

                // No infertility factors specified when reason is known.
                yield return [
                    new SuccessRateCalculationRequest {
                        Age = 32,
                        Weight = 150,
                        Height = 5*FT + 8,

                        UsingOwnEggs = false,
                        UsedIvfBefore = false,

                        ReasonForInfertilityKnown = true,
                        InfertilityDiagnosis = InfertilityFactors.None,

                        NumPriorPregnancies = 0,
                        NumLiveBirths = 0
                    },

                    JToken.FromObject(new {
                        InfertilityDiagnosis = new[] { "At least one infertility factor must be specified." }
                    })
                ];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
