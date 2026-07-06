using System;
using System.Collections.Generic;
using System.Linq;
using StargateGalacticCommand.Core.Models;

namespace StargateGalacticCommand.Core.Services
{
    public class GalaxyGeneratorService
    {
        private static readonly char[] CodeLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private static readonly string[] Biomes =
        {
            "Wüstenwelt", "Dschungelmond", "Eisplanet", "Ozeanwelt", "Vulkanische Ödnis",
            "Steppenwelt", "Nebelsumpf", "Kristallhöhlenwelt", "Tundraplanet", "Kargwelt",
            "Regenwaldkolonie", "Salzwüste", "Tiefseewelt", "Kanyonwelt", "Graslandebene",
            "Kargmond", "Basaltwelt", "Gezeitenwelt", "Steppenmond", "Dünenwelt"
        };

        private static readonly string[] Features =
        {
            "mit stabiler Gate-Lichtung",
            "mit verlassenen Goa'uld-Ruinen",
            "unter Jaffa-Patrouillenkontrolle",
            "mit Replikator-Kontamination",
            "mit Ori-Kult-Präsenz",
            "mit Unas-Stammesgebiet",
            "mit alten Ausgrabungsstätten",
            "mit instabiler Atmosphäre",
            "mit reichen Naquadah-Vorkommen",
            "mit Trinium-Adern",
            "mit neutralem Handelsposten",
            "mit verlassener Außenstation",
            "mit gefährlicher Wildnis",
            "mit rätselhaften Antiker-Relikten"
        };

        private static readonly int[] FeatureRiskBase = { 1, 4, 6, 9, 8, 5, 3, 7, 2, 2, 1, 3, 5, 3 };

        public IList<GateAddress> GenerateWorlds(int count, IEnumerable<string> existingCodes, int seed)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            var random = new Random(seed);
            var usedCodes = new HashSet<string>(existingCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var result = new List<GateAddress>(count);

            for (int i = 0; i < count; i++)
            {
                string code = NextUniqueCode(random, usedCodes);
                usedCodes.Add(code);

                int featureIndex = random.Next(Features.Length);
                string biome = Biomes[random.Next(Biomes.Length)];
                string feature = Features[featureIndex];
                int risk = Math.Clamp(FeatureRiskBase[featureIndex] + random.Next(-1, 2), 1, 10);

                result.Add(new GateAddress
                {
                    Code = code,
                    WorldName = code,
                    Description = biome + " " + feature + ".",
                    IsNeutralPve = true,
                    RiskLevel = risk
                });
            }

            return result;
        }

        private static string NextUniqueCode(Random random, HashSet<string> usedCodes)
        {
            string code;
            do
            {
                code = NextCode(random);
            } while (usedCodes.Contains(code));
            return code;
        }

        private static string NextCode(Random random)
        {
            char letter1 = CodeLetters[random.Next(CodeLetters.Length)];
            int digit = random.Next(1, 10);
            char letter2 = CodeLetters[random.Next(CodeLetters.Length)];
            int number = random.Next(100, 1000);
            return string.Concat(letter1, digit.ToString(), letter2.ToString(), "-", number.ToString());
        }
    }
}
