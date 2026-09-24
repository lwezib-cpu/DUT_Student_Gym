using System;
using System.Collections.Generic;
using System.Linq;
using GymNet.ViewModels;

namespace GymNet.Helpers
{
    /// <summary>
    /// A static list of recommended exercises per body part. No database table
    /// behind this on purpose - it's reference content, not member data, so it
    /// ships with the app and doesn't need a migration to add or edit exercises.
    /// To change the list, edit the arrays below and rebuild.
    /// </summary>
    public static class ExerciseCatalog
    {
        public static readonly List<string> BodyParts = new List<string>
        {
            "Chest", "Back", "Shoulders", "Arms", "Legs", "Core", "Cardio / Full Body"
        };

        private static readonly List<ExerciseViewModel> All = new List<ExerciseViewModel>
        {
            // ---- Chest ----
            new ExerciseViewModel { BodyPart = "Chest", Name = "Barbell Bench Press", Equipment = "Barbell", Difficulty = "Intermediate", SetsReps = "4 x 6-10",
                Instructions = "Lie on a flat bench, grip slightly wider than shoulder-width, lower the bar to your chest with control, press back up. Keep feet flat and shoulder blades pulled together." },
            new ExerciseViewModel { BodyPart = "Chest", Name = "Push-Up", Equipment = "Bodyweight", Difficulty = "Beginner", SetsReps = "3 x 12-20",
                Instructions = "Hands slightly wider than shoulders, body in a straight line from head to heels. Lower until your chest nearly touches the floor, push back up." },
            new ExerciseViewModel { BodyPart = "Chest", Name = "Dumbbell Incline Press", Equipment = "Dumbbell", Difficulty = "Intermediate", SetsReps = "3 x 8-12",
                Instructions = "Set bench to 30-45 degrees. Press dumbbells up and slightly together above your upper chest, lower under control." },
            new ExerciseViewModel { BodyPart = "Chest", Name = "Cable Chest Fly", Equipment = "Cable", Difficulty = "Beginner", SetsReps = "3 x 12-15",
                Instructions = "Stand between two cable stacks, slight bend in elbows, bring hands together in front of your chest in a wide arc." },

            // ---- Back ----
            new ExerciseViewModel { BodyPart = "Back", Name = "Lat Pulldown", Equipment = "Machine", Difficulty = "Beginner", SetsReps = "3 x 10-12",
                Instructions = "Grip the bar wider than shoulders, pull down to your upper chest while keeping your chest up, control it back up." },
            new ExerciseViewModel { BodyPart = "Back", Name = "Bent-Over Barbell Row", Equipment = "Barbell", Difficulty = "Intermediate", SetsReps = "4 x 8-10",
                Instructions = "Hinge at the hips, back flat, pull the bar to your lower ribs, squeeze your shoulder blades together, lower with control." },
            new ExerciseViewModel { BodyPart = "Back", Name = "Pull-Up", Equipment = "Bodyweight", Difficulty = "Advanced", SetsReps = "3 x max reps",
                Instructions = "Grip slightly wider than shoulders, pull your chin above the bar, lower fully with control. Use an assisted machine if needed." },
            new ExerciseViewModel { BodyPart = "Back", Name = "Seated Cable Row", Equipment = "Cable", Difficulty = "Beginner", SetsReps = "3 x 10-12",
                Instructions = "Sit tall, pull the handle to your torso keeping elbows close, squeeze your back at the end, extend arms fully to reset." },

            // ---- Shoulders ----
            new ExerciseViewModel { BodyPart = "Shoulders", Name = "Overhead Press", Equipment = "Barbell", Difficulty = "Intermediate", SetsReps = "4 x 6-10",
                Instructions = "Stand tall, press the bar from shoulder height straight overhead, avoid arching your lower back." },
            new ExerciseViewModel { BodyPart = "Shoulders", Name = "Dumbbell Lateral Raise", Equipment = "Dumbbell", Difficulty = "Beginner", SetsReps = "3 x 12-15",
                Instructions = "Slight bend in elbows, raise dumbbells out to the sides to shoulder height, lower slowly." },
            new ExerciseViewModel { BodyPart = "Shoulders", Name = "Face Pull", Equipment = "Cable", Difficulty = "Beginner", SetsReps = "3 x 15",
                Instructions = "Pull a rope attachment toward your face, elbows high, squeezing your rear shoulders and upper back." },

            // ---- Arms ----
            new ExerciseViewModel { BodyPart = "Arms", Name = "Dumbbell Bicep Curl", Equipment = "Dumbbell", Difficulty = "Beginner", SetsReps = "3 x 10-12",
                Instructions = "Elbows pinned to your sides, curl the weights up without swinging, lower slowly." },
            new ExerciseViewModel { BodyPart = "Arms", Name = "Triceps Pushdown", Equipment = "Cable", Difficulty = "Beginner", SetsReps = "3 x 12-15",
                Instructions = "Elbows tucked at your sides, push the bar/rope down until your arms are straight, control the return." },
            new ExerciseViewModel { BodyPart = "Arms", Name = "Close-Grip Bench Press", Equipment = "Barbell", Difficulty = "Intermediate", SetsReps = "3 x 8-10",
                Instructions = "Hands shoulder-width apart, lower the bar to your lower chest, press up focusing on your triceps." },
            new ExerciseViewModel { BodyPart = "Arms", Name = "Dip", Equipment = "Bodyweight", Difficulty = "Advanced", SetsReps = "3 x 8-12",
                Instructions = "Lower your body between parallel bars until your upper arms are about parallel to the floor, press back up." },

            // ---- Legs ----
            new ExerciseViewModel { BodyPart = "Legs", Name = "Barbell Back Squat", Equipment = "Barbell", Difficulty = "Intermediate", SetsReps = "4 x 6-10",
                Instructions = "Bar on your upper back, feet shoulder-width, sit back and down keeping your chest up, drive through your heels to stand." },
            new ExerciseViewModel { BodyPart = "Legs", Name = "Leg Press", Equipment = "Machine", Difficulty = "Beginner", SetsReps = "3 x 10-15",
                Instructions = "Feet shoulder-width on the platform, lower until your knees reach about 90 degrees, press back up without locking your knees hard." },
            new ExerciseViewModel { BodyPart = "Legs", Name = "Walking Lunge", Equipment = "Bodyweight", Difficulty = "Beginner", SetsReps = "3 x 10 per leg",
                Instructions = "Step forward, lower your back knee toward the floor, push through your front heel to step into the next lunge." },
            new ExerciseViewModel { BodyPart = "Legs", Name = "Romanian Deadlift", Equipment = "Barbell", Difficulty = "Intermediate", SetsReps = "3 x 8-10",
                Instructions = "Slight knee bend, hinge at the hips lowering the bar down your legs, feel a stretch in your hamstrings, return to standing." },

            // ---- Core ----
            new ExerciseViewModel { BodyPart = "Core", Name = "Plank", Equipment = "Bodyweight", Difficulty = "Beginner", SetsReps = "3 x 30-60s",
                Instructions = "Forearms and toes on the floor, body in a straight line, brace your abs and hold without letting your hips sag." },
            new ExerciseViewModel { BodyPart = "Core", Name = "Hanging Leg Raise", Equipment = "Bodyweight", Difficulty = "Advanced", SetsReps = "3 x 10-15",
                Instructions = "Hang from a bar, raise your legs to hip height or higher without swinging, lower with control." },
            new ExerciseViewModel { BodyPart = "Core", Name = "Cable Woodchopper", Equipment = "Cable", Difficulty = "Intermediate", SetsReps = "3 x 12 per side",
                Instructions = "Pull the cable diagonally across your body from high to low (or low to high), rotating through your torso, not just your arms." },

            // ---- Cardio / Full Body ----
            new ExerciseViewModel { BodyPart = "Cardio / Full Body", Name = "Treadmill Intervals", Equipment = "Machine", Difficulty = "Beginner", SetsReps = "20-25 min",
                Instructions = "Alternate 1 minute fast pace with 2 minutes moderate pace. Adjust speed to your fitness level." },
            new ExerciseViewModel { BodyPart = "Cardio / Full Body", Name = "Burpee", Equipment = "Bodyweight", Difficulty = "Intermediate", SetsReps = "4 x 45s",
                Instructions = "Drop into a squat, kick your feet back to a plank, do a push-up, jump your feet back in, then jump up." },
            new ExerciseViewModel { BodyPart = "Cardio / Full Body", Name = "Rowing Machine", Equipment = "Machine", Difficulty = "Beginner", SetsReps = "15-20 min",
                Instructions = "Drive with your legs first, then lean back and pull the handle to your torso, reverse the order on the way forward." },
            new ExerciseViewModel { BodyPart = "Cardio / Full Body", Name = "Kettlebell Swing", Equipment = "Kettlebell", Difficulty = "Intermediate", SetsReps = "4 x 15-20",
                Instructions = "Hinge at the hips, swing the kettlebell between your legs then drive your hips forward to swing it to chest height." },
        };

        public static List<ExerciseViewModel> ForBodyPart(string bodyPart)
        {
            if (string.IsNullOrWhiteSpace(bodyPart)) return new List<ExerciseViewModel>();
            var results = All.Where(e => e.BodyPart == bodyPart).ToList();
            foreach (var ex in results)
            {
                ex.LottieUrl = LottieFor(ex.Name);
                ex.ImageUrl = ImageFor(ex.Name);
            }
            return results;
        }

        // Maps equipment type to a Bootstrap Icon class, used to render a simple
        // animated icon on each exercise card (see Views/Exercise/Index.cshtml).
        // No real photos/video here on purpose - see the note in that view for why.
        private static readonly Dictionary<string, string> IconByEquipment = new Dictionary<string, string>
        {
            { "Barbell", "bi-dash-lg" },
            { "Dumbbell", "bi-record-circle" },
            { "Kettlebell", "bi-droplet-fill" },
            { "Cable", "bi-distribute-vertical" },
            { "Machine", "bi-gear-fill" },
            { "Bodyweight", "bi-person-arms-up" },
        };

        public static string IconFor(string equipment)
        {
            return IconByEquipment.TryGetValue(equipment, out var icon) ? icon : "bi-lightning-charge-fill";
        }

        // What kind of guided session an exercise gets when a member starts a
        // workout session:
        //   "Timed"    - bodyweight moves (push-ups, plank...): 30s countdown,
        //                then ask how many reps were completed.
        //   "Cardio"   - machine-based cardio (treadmill, rowing machine): ask
        //                how many minutes, no countdown.
        //   "Standard" - everything else: ask sets, reps, and weight.
        public static string SessionCategory(string equipment, string bodyPart)
        {
            if (equipment == "Bodyweight") return "Timed";
            if (equipment == "Machine" && bodyPart == "Cardio / Full Body") return "Cardio";
            return "Standard";
        }

        // Cycles through a few CSS animation styles so a page of exercises doesn't
        // all move in perfect unison - purely cosmetic variety.
        private static readonly string[] AnimationStyles = { "icon-anim-bounce", "icon-anim-pulse", "icon-anim-sway" };

        public static string AnimationFor(string exerciseName)
        {
            var index = Math.Abs((exerciseName ?? "").GetHashCode()) % AnimationStyles.Length;
            return AnimationStyles[index];
        }

        // Fill this in with real LottieFiles animation URLs once you have them
        // (lottiefiles.com -> search -> pick a Free one -> Embed/Download -> copy the .json URL).
        // Key must match an exercise's Name exactly, e.g.:
        // { "Barbell Bench Press", "https://assets2.lottiefiles.com/packages/lf20_xxxxx.json" }
        private static readonly Dictionary<string, string> LottieByExerciseName = new Dictionary<string, string>
        {
        };

        public static string LottieFor(string exerciseName)
        {
            return LottieByExerciseName.TryGetValue(exerciseName ?? "", out var url) ? url : null;
        }

        // Original flat-pictogram illustrations (Content/images/exercises/) - simple
        // geometric shapes, not based on any external reference image. Falls back
        // to the icon animation for any exercise without one yet.
        private static readonly Dictionary<string, string> ImageByExerciseName = new Dictionary<string, string>
        {
            { "Barbell Bench Press", "~/Content/images/exercises/barbell-bench-press.svg" },
            { "Push-Up", "~/Content/images/exercises/push-up.svg" },
            { "Dumbbell Incline Press", "~/Content/images/exercises/dumbbell-incline-press.svg" },
            { "Cable Chest Fly", "~/Content/images/exercises/cable-chest-fly.svg" },

            { "Lat Pulldown", "~/Content/images/exercises/lat-pulldown.svg" },
            { "Bent-Over Barbell Row", "~/Content/images/exercises/bent-over-row.svg" },
            { "Pull-Up", "~/Content/images/exercises/pull-up.svg" },
            { "Seated Cable Row", "~/Content/images/exercises/seated-cable-row.svg" },

            { "Overhead Press", "~/Content/images/exercises/overhead-press.svg" },
            { "Dumbbell Lateral Raise", "~/Content/images/exercises/lateral-raise.svg" },
            { "Face Pull", "~/Content/images/exercises/face-pull.svg" },

            { "Dumbbell Bicep Curl", "~/Content/images/exercises/bicep-curl.svg" },
            { "Triceps Pushdown", "~/Content/images/exercises/triceps-pushdown.svg" },
            { "Close-Grip Bench Press", "~/Content/images/exercises/close-grip-bench.svg" },
            { "Dip", "~/Content/images/exercises/dip.svg" },

            { "Barbell Back Squat", "~/Content/images/exercises/back-squat.svg" },
            { "Leg Press", "~/Content/images/exercises/leg-press.svg" },
            { "Walking Lunge", "~/Content/images/exercises/walking-lunge.svg" },
            { "Romanian Deadlift", "~/Content/images/exercises/romanian-deadlift.svg" },

            { "Plank", "~/Content/images/exercises/plank.svg" },
            { "Hanging Leg Raise", "~/Content/images/exercises/hanging-leg-raise.svg" },
            { "Cable Woodchopper", "~/Content/images/exercises/cable-woodchopper.svg" },

            { "Treadmill Intervals", "~/Content/images/exercises/treadmill-intervals.svg" },
            { "Burpee", "~/Content/images/exercises/burpee.svg" },
            { "Rowing Machine", "~/Content/images/exercises/rowing-machine.svg" },
            { "Kettlebell Swing", "~/Content/images/exercises/kettlebell-swing.svg" },
        };

        public static string ImageFor(string exerciseName)
        {
            return ImageByExerciseName.TryGetValue(exerciseName ?? "", out var url) ? url : null;
        }
    }
}
