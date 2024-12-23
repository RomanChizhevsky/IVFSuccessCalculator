using IVFSuccessCalculator.Models;

namespace IVFSuccessCalculator.Data
{
    public class SuccessRateFormulaRepository
    {
        public static IEnumerable<SuccessRateFormulaParameters> FetchParameters()
        {
            using var formulaSheet = File.OpenRead("ivf_success_formulas.csv");
            using var sheetReader = new StreamReader(formulaSheet);

            var header = Array.Empty<string>();

            while (sheetReader.ReadLine() is string line)
            {
                var columns = line.Split(',');
                if (header.Length == 0)
                {
                    header = columns;
                    continue;
                }

                yield return FromCsvData(header, columns);
            }
        }

        private static SuccessRateFormulaParameters FromCsvData(IEnumerable<string> headerRow, IEnumerable<string> row)
        {
            var csvLookup = headerRow.Zip(row).ToDictionary();

            var formulaParams = csvLookup
                                    .Where(col => col.Key.StartsWith("formula"))
                                    .ToDictionary(k => k.Key, v => double.Parse(v.Value));

            // Obtain the selection criteria for the formula
            var usingOwnEggs = bool.Parse(csvLookup["param_using_own_eggs"]);
            bool? prevAttemptedIvf = usingOwnEggs ? bool.Parse(csvLookup["param_attempted_ivf_previously"]) : null;
            var reasonForInfKnown = bool.Parse(csvLookup["param_is_reason_for_infertility_known"]);

            return new SuccessRateFormulaParameters(usingOwnEggs, prevAttemptedIvf, reasonForInfKnown)
            {
                Intercept = formulaParams["formula_intercept"],

                AgeLinearCoefficient = formulaParams["formula_age_linear_coefficient"],
                AgePowerCoefficient = formulaParams["formula_age_power_coefficient"],
                AgePowerFactor = formulaParams["formula_age_power_factor"],

                BmiLinearCoefficient = formulaParams["formula_bmi_linear_coefficient"],
                BmiPowerCoefficient = formulaParams["formula_bmi_power_coefficient"],
                BmiPowerFactor = formulaParams["formula_bmi_power_factor"],

                TubalFactor = formulaParams["formula_tubal_factor_true_value"],
                MaleInfertilityFactor = formulaParams["formula_male_factor_infertility_true_value"],
                EndometriosisFactor = formulaParams["formula_endometriosis_true_value"],
                OvulatoryDisorderFactor = formulaParams["formula_ovulatory_disorder_true_value"],
                DiminishedOvarianReserveFactor = formulaParams["formula_diminished_ovarian_reserve_true_value"],
                UterineFactor = formulaParams["formula_uterine_factor_true_value"],
                SupplementalFactor = formulaParams["formula_other_reason_true_value"],

                UnexplainedInfertilityFactor = formulaParams["formula_unexplained_infertility_true_value"],

                PriorPregnancyFactor = formulaParams["formula_prior_pregnancies_1_value"],
                MultiplePriorPregnanciesFactor = formulaParams["formula_prior_pregnancies_2+_value"],

                LiveBirthFactor = formulaParams["formula_prior_live_births_1_value"],
                MultipleLiveBirthsFactor = formulaParams["formula_prior_live_births_2+_value"],
            };
        }
    }
}
