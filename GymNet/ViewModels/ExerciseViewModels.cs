using System.Collections.Generic;

namespace GymNet.ViewModels
{
    public class ExerciseViewModel
    {
        public string Name { get; set; }
        public string BodyPart { get; set; }
        public string Equipment { get; set; }       // e.g. "Barbell", "Dumbbell", "Bodyweight", "Machine", "Cable"
        public string Difficulty { get; set; }       // "Beginner", "Intermediate", "Advanced"
        public string SetsReps { get; set; }         // e.g. "3 x 10-12"
        public string Instructions { get; set; }

        // Optional: a LottieFiles animation URL (e.g. https://assets*.lottiefiles.com/packages/lf20_xxx.json).
        // Set this per exercise in Helpers/ExerciseCatalog.cs once you have a URL from lottiefiles.com.
        // Left blank, the card falls back to the CSS-animated icon.
        public string LottieUrl { get; set; }

        // Local photo/diagram (Content/images/exercises/...). When set, the card
        // shows this instead of the icon/Lottie fallback.
        public string ImageUrl { get; set; }
    }

    public class ExerciseRecommendationViewModel
    {
        public List<string> BodyParts { get; set; } = new List<string>();
        public string SelectedBodyPart { get; set; }
        public List<ExerciseViewModel> Exercises { get; set; } = new List<ExerciseViewModel>();
    }
}
