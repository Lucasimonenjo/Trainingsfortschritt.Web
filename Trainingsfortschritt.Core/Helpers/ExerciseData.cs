using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Trainingsfortschritt.Core.Helpers
{
    public static class ExerciseData
    {
        // -----------------------------------------
        // Equipment-Wörter ignorieren
        // -----------------------------------------
        private static readonly HashSet<string> EquipmentWords = new()
        {
            "machine", "maschine", "smith", "dumbbell", "db", "kettlebell", "kb", "cable"
        };

        // -----------------------------------------
        // Übungsfamilien (GRUPPEN)
        // -----------------------------------------
        public static readonly Dictionary<string, string> Families = new()
{
    // SQUATS
    { "front squat", "Squats" },
    { "back squat", "Squats" },
    { "bulgarian split squat", "Squats" },
    { "hack squat", "Squats" },
    { "smith machine squat", "Squats" },
    { "goblet squat", "Squats" },
    { "leg press", "Squats" },
    { "lunge", "Squats" },
    { "step up", "Squats" },
{ "squats", "Squats" },

    // BENCH PRESS
    { "bench press", "Bench Press" },
    { "flat bench press", "Bench Press" },
    { "incline bench press", "Bench Press" },
    { "decline bench press", "Bench Press" },
    { "dumbbell bench press", "Bench Press" },
    { "machine chest press", "Bench Press" },
    { "chest press", "Bench Press" },
    { "cable fly", "Bench Press" },
    { "pec deck", "Bench Press" },
    { "push up", "Bench Press" },
    { "dip", "Bench Press" },

    // PULLDOWN
    { "lat pulldown", "Pulldown" },
    { "pull-up", "Pulldown" },
    { "pullup", "Pulldown" },
    { "chin-up", "Pulldown" },
    { "neutral grip pull-up", "Pulldown" },
    { "muscle up", "Pulldown" },
    { "klimmzug", "Pulldown" },
            {"latzug","Pulldown" },
            {"lat", "Pulldown" },

    // ROW
    { "bent over row", "Row" },
    { "pendlay row", "Row" },
    { "cable row", "Row" },
    { "machine row", "Row" },
    { "t-bar row", "Row" },
    { "dumbbell row", "Row" },

    // DEADLIFT
    { "deadlift", "Deadlift" },
    { "conventional deadlift", "Deadlift" },
    { "sumo deadlift", "Deadlift" },
    { "romanian deadlift", "Deadlift" },
    { "stiff-leg deadlift", "Deadlift" },
    { "trap bar deadlift", "Deadlift" },
    { "rack pull", "Deadlift" },
    { "rdl", "Deadlift" },
    { "kreuzheben", "Deadlift" },

    // SHOULDERS
    { "overhead press", "Shoulder Press" },
    { "military press", "Shoulder Press" },
    { "arnold press", "Shoulder Press" },
    { "lateral raise", "Shoulder Press" },
    { "front raise", "Shoulder Press" },
    { "rear delt fly", "Shoulder Press" },
    { "face pull", "Shoulder Press" },
    { "shoulder press", "Shoulder Press" },
    { "ohp", "Shoulder Press" },

    // CURL
    { "biceps curl", "Curl" },
    { "hammer curl", "Curl" },
    { "incline curl", "Curl" },
    { "cable curl", "Curl" },
    { "preacher curl", "Curl" },

    // TRICEP
    { "triceps pushdown", "Tricep" },
    { "skullcrusher", "Tricep" },
    { "overhead extension", "Tricep" },
    { "french press", "Tricep" },
    { "jm press", "Tricep" },

    // ABS
    { "crunch", "Abs" },
    { "sit up", "Abs" },
    { "leg raise", "Abs" },
    { "plank", "Abs" },
    { "ab wheel", "Abs" },
    { "russian twist", "Abs" },
    { "hanging leg raise", "Abs" },

    // CARDIO
    { "treadmill", "Cardio" },
    { "cycling", "Cardio" },
    { "rowing machine", "Cardio" },
    { "stairmaster", "Cardio" },
    { "cross trainer", "Cardio" },

    // FUNCTIONAL
    { "kettlebell swing", "Functional" },
    { "battle rope", "Functional" },
    { "box jump", "Functional" },
    { "sled push", "Functional" },
    { "burpee", "Functional" },
    { "farmer walk", "Functional" },
    { "sandbag carry", "Functional" },
    { "wall ball", "Functional" },
    { "trx", "Functional" },
    { "medicine ball", "Functional" },
    { "kettlebell", "Functional" }
};

        // -----------------------------------------
        // Varianten
        // -----------------------------------------
        public static readonly Dictionary<string, List<string>> Variants =
    new(StringComparer.OrdinalIgnoreCase)
{
    // =========================
    // SQUATS
    // =========================
    { "Squats", new List<string>
    {
        "Front Squat",
        "Back Squat",
        "Bulgarian Split Squat",
        "Hack Squat",
        "Smith Machine Squat",
        "Goblet Squat",
        "Leg Press",
        "Lunge",
        "Step Up"
    }},

    // =========================
    // BENCH PRESS
    // =========================
    { "Bench Press", new List<string>
    {
        "Flat Bench Press",
        "Incline Bench Press",
        "Decline Bench Press",
        "Dumbbell Bench Press",
        "Machine Chest Press",
        "Cable Fly",
        "Pec Deck",
        "Push Up",
        "Dip"
    }},

    // =========================
    // PULLDOWN
    // =========================
    { "Pulldown", new List<string>
    {
        "Lat Pulldown",
        "Pull-Up",
        "Chin-Up",
        "Neutral Grip Pull-Up",
        "Muscle Up",
        "Latzug",
        "lat"
    }},

    // =========================
    // ROW
    // =========================
    { "Row", new List<string>
    {
        "Bent Over Row",
        "Pendlay Row",
        "Cable Row",
        "Machine Row",
        "T-Bar Row",
        "Dumbbell Row"
    }},

    // =========================
    // DEADLIFT
    // =========================
    { "Deadlift", new List<string>
    {
        "Conventional Deadlift",
        "Sumo Deadlift",
        "Romanian Deadlift",
        "Stiff-Leg Deadlift",
        "Trap Bar Deadlift",
        "Rack Pull"
    }},

    // =========================
    // SHOULDERS
    // =========================
    { "Shoulder Press", new List<string>
    {
        "Overhead Press",
        "Military Press",
        "Arnold Press",
        "Lateral Raise",
        "Front Raise",
        "Rear Delt Fly",
        "Face Pull"
    }},

    // =========================
    // CURLS
    // =========================
    { "Curl", new List<string>
    {
        "Biceps Curl",
        "Hammer Curl",
        "Incline Curl",
        "Cable Curl",
        "Preacher Curl"
    }},

    // =========================
    // TRICEPS
    // =========================
    { "Tricep", new List<string>
    {
        "Triceps Pushdown",
        "Skullcrusher",
        "Overhead Extension",
        "French Press",
        "JM Press"
    }},

    // =========================
    // ABS
    // =========================
    { "Abs", new List<string>
    {
        "Crunch",
        "Sit Up",
        "Leg Raise",
        "Plank",
        "Ab Wheel",
        "Russian Twist",
        "Hanging Leg Raise"
    }},

    // =========================
    // CARDIO
    // =========================
    { "Cardio", new List<string>
    {
        "Treadmill",
        "Cycling",
        "Rowing Machine",
        "Stairmaster",
        "Cross Trainer"
    }},

    // =========================
    // FUNCTIONAL
    // =========================
    { "Functional", new List<string>
    {
        "Kettlebell Swing",
        "Battle Rope",
        "Box Jump",
        "Sled Push",
        "Burpee",
        "Farmer Walk",
        "Sandbag Carry",
        "Wall Ball",
        "TRX",
        "Medicine Ball",
        "Kettlebell"
    }}
};

        // -----------------------------------------
        // GRUPPE ERMITTELN
        // -----------------------------------------
        public static string GetFamily(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Other";

            name = name.ToLowerInvariant();

            // =========================
            // 1. HÖCHSTE PRIORITÄT: CARDIO (verhindert Functional False Positives)
            // =========================
            if (name.Contains("treadmill") ||
                name.Contains("laufband") ||
                name.Contains("bike") ||
                name.Contains("rowing machine") ||
                name.Contains("cross trainer") ||
                name.Contains("stairmaster"))
                return "Cardio";

            // =========================
            // 2. FUNCTIONAL
            // =========================
            if (name.Contains("kettlebell") ||
                name.Contains("battle rope") ||
                name.Contains("sled") ||
                name.Contains("burpee") ||
                name.Contains("farmer walk") ||
                name.Contains("trx") ||
                name.Contains("medicine ball"))
                return "Functional";

            if (name.Contains("squat"))
                return "Squats";

            if (name.Contains("bench"))
                return "Bench Press";

            if (name.Contains("deadlift"))
                return "Deadlift";

            if (name.Contains("row"))
                return "Row";

            if (name.Contains("pull"))
                return "Pulldown";

            if (name.Contains("curl"))
                return "Curl";

            if (name.Contains("tricep"))
                return "Tricep";

            if (name.Contains("cardio"))
                return "Cardio";

            // =========================
            // 3. STRENGTH (Rest)
            // =========================
            foreach (var kv in Families)
            {
                if (name.Contains(kv.Key))
                    return kv.Value;
            }

            return "Other";
        }

        // -----------------------------------------
        // ÜBUNG NORMALISIEREN (KEIN PLURAL!)
        // -----------------------------------------
        public static string NormalizeExerciseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            var cleaned = Clean(name);

            foreach (var key in Families.Keys.OrderByDescending(k => k.Length))
            {
                if (cleaned.Contains(key))
                    return Families[key];
            }

            var parts = cleaned.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return "";

            return Capitalize(parts[0]);
        }

        // -----------------------------------------
        // VARIANTEN
        // -----------------------------------------
        public static List<string> GetVariants(string family)
        {
            if (Variants.ContainsKey(family))
                return Variants[family];

            return new List<string>();
        }

        // -----------------------------------------
        // CLEANING HELPER
        // -----------------------------------------
        private static string Clean(string input)
        {
            input = input.ToLower().Trim();

            input = Regex.Replace(input, @"[\(\)\[\]\{\}_\-\.]", " ");
            input = Regex.Replace(input, @"\s+", " ");

            var cleaned = string.Join(" ",
                input.Split(' ')
                    .Where(w => !EquipmentWords.Contains(w))
            );

            return cleaned;
        }

        // -----------------------------------------
        // CAPITALIZE HELPER
        // -----------------------------------------
        private static string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            return char.ToUpper(input[0]) + input.Substring(1);
        }
        public static bool IsVariant(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
                return false;

            var family = GetFamily(exerciseName);
            if (family == "Other")
                return false;

            return Variants.TryGetValue(family, out var list) &&
                   list.Any(v => v.Equals(exerciseName, StringComparison.OrdinalIgnoreCase));
        }
        public static string GetFamilySafe(string name)
        {
            return GetFamily(name);
        }
        public static bool IsBaseExercise(string exerciseName)
        {
            if (string.IsNullOrWhiteSpace(exerciseName))
                return false;

            var family = GetFamily(exerciseName);

            if (family == "Other")
                return true;

            // Base = genau der Family-Name als Standard-Keyword
            return Variants.ContainsKey(family) &&
                   Variants[family].Any(v =>
                       v.Equals(exerciseName, StringComparison.OrdinalIgnoreCase)) == false;
        }
    }
}