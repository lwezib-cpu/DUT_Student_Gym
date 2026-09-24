using System.Collections.Generic;

namespace GymNet.Helpers
{
    public class FoodItem
    {
        public string Emoji { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }

    public class MealSection
    {
        public string Name { get; set; }
        public string Emoji { get; set; }
        public List<FoodItem> Items { get; set; }
    }

    public class MealPlan
    {
        public string GoalLabel { get; set; }
        public string Summary { get; set; }
        public List<MealSection> Meals { get; set; }
        public List<FoodItem> Fruits { get; set; }
    }

    /// <summary>
    /// One ingredient in the Build Your Own Meal calculator. Calorie figures are
    /// rounded, typical values for a standard serving size (in the Name), based
    /// on commonly published nutrition references (e.g. USDA FoodData Central) -
    /// they're estimates for planning, not lab-measured amounts for any specific
    /// product.
    /// </summary>
    public class Ingredient
    {
        public string Emoji { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public string Category { get; set; }
        public string Benefit { get; set; }
    }

    /// <summary>
    /// General, non-personalized meal ideas by goal type - illustrative examples,
    /// not a prescribed diet plan or precise calorie/macro targets. Always paired
    /// with a note recommending a registered dietitian for anything personalized.
    /// Uses emoji rather than photos - renders natively everywhere, no external
    /// image/font dependency, and no copyright question.
    /// </summary>
    public static class NutritionGuide
    {
        public static MealPlan For(string goalType)
        {
            switch (goalType)
            {
                case "LoseWeight": return LoseWeightPlan();
                case "GainWeight": return GainWeightPlan();
                default: return MaintainWeightPlan();
            }
        }

        private static MealPlan LoseWeightPlan()
        {
            return new MealPlan
            {
                GoalLabel = "Lose Weight",
                Summary = "Built around lean protein, fibre and vegetables to help you feel full on fewer calories.",
                Meals = new List<MealSection>
                {
                    new MealSection { Name = "Breakfast", Emoji = "\U0001F373", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F963", Name = "Vegetable omelette", Note = "Eggs + spinach, peppers, onion" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Greek yoghurt with berries", Note = "High protein, lower sugar" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Oats with cinnamon", Note = "No added sugar, high fibre" },
                        new FoodItem { Emoji = "\U0001F345", Name = "Egg-white scramble with tomato", Note = "Very low fat, high protein" },
                        new FoodItem { Emoji = "\U0001F95D", Name = "Cottage cheese with kiwi", Note = "High protein, low calorie" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Chia pudding (unsweetened)", Note = "High fibre, keeps you full" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Smoothie: spinach, berries, protein powder", Note = "Low sugar, high protein" },
                        new FoodItem { Emoji = "\U0001F344", Name = "Mushroom & spinach egg muffins", Note = "Portioned, protein-rich" },
                    }},
                    new MealSection { Name = "Lunch", Emoji = "\U0001F957", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F957", Name = "Grilled chicken salad", Note = "Leafy greens, olive oil dressing" },
                        new FoodItem { Emoji = "\U0001F35C", Name = "Lentil soup", Note = "Plant protein, filling and low fat" },
                        new FoodItem { Emoji = "\U0001F35A", Name = "Quinoa & roasted veg bowl", Note = "Whole grain, high fibre" },
                        new FoodItem { Emoji = "\U0001F35B", Name = "Chickpea & vegetable salad", Note = "Plant protein, high fibre" },
                        new FoodItem { Emoji = "\U0001F35C", Name = "Vegetable & bean soup", Note = "Low calorie, very filling" },
                        new FoodItem { Emoji = "\U0001F32F", Name = "Turkey lettuce wraps", Note = "Lean protein, low carb" },
                        new FoodItem { Emoji = "\U0001F41F", Name = "Tuna salad (light mayo)", Note = "Lean protein" },
                        new FoodItem { Emoji = "\U0001F966", Name = "Steamed vegetable & tofu bowl", Note = "Low calorie, high fibre" },
                    }},
                    new MealSection { Name = "Dinner", Emoji = "\U0001F37D", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F41F", Name = "Baked fish + steamed veg", Note = "Lean protein, minimal oil" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Tofu & broccoli stir-fry", Note = "Light on oil, high in fibre" },
                        new FoodItem { Emoji = "\U0001F357", Name = "Grilled chicken + sweet potato", Note = "Balanced protein and carbs" },
                        new FoodItem { Emoji = "\U0001F35B", Name = "Zucchini noodles with turkey mince", Note = "Low carb, high protein" },
                        new FoodItem { Emoji = "\U0001F364", Name = "Grilled prawns + salad", Note = "Very lean protein" },
                        new FoodItem { Emoji = "\U0001F966", Name = "Roasted vegetable & lentil bake", Note = "Plant protein, high fibre" },
                        new FoodItem { Emoji = "\U0001F357", Name = "Chicken & vegetable soup", Note = "Filling, lower calorie" },
                        new FoodItem { Emoji = "\U0001F41F", Name = "Grilled salmon + asparagus", Note = "Lean protein, healthy fat" },
                    }},
                    new MealSection { Name = "Drinks", Emoji = "\U0001F964", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F4A7", Name = "Water", Note = "Aim for consistent hydration through the day" },
                        new FoodItem { Emoji = "\U0001F375", Name = "Green tea", Note = "No added sugar" },
                        new FoodItem { Emoji = "\u2615", Name = "Black coffee", Note = "No sugar or cream" },
                        new FoodItem { Emoji = "\U0001F34B", Name = "Lemon & mint infused water", Note = "Flavour without added sugar" },
                        new FoodItem { Emoji = "\U0001F375", Name = "Herbal tea (rooibos, chamomile)", Note = "Caffeine-free, no calories" },
                        new FoodItem { Emoji = "\U0001F9CA", Name = "Sparkling water", Note = "No sugar alternative to soda" },
                    }},
                },
                Fruits = new List<FoodItem> {
                    new FoodItem { Emoji = "\U0001F353", Name = "Berries", Note = "High fibre, lower sugar" },
                    new FoodItem { Emoji = "\U0001F34E", Name = "Apples", Note = "Filling, high in fibre" },
                    new FoodItem { Emoji = "\U0001F34A", Name = "Citrus (orange, naartjie)", Note = "Low calorie, high vitamin C" },
                    new FoodItem { Emoji = "\U0001F349", Name = "Watermelon", Note = "Very low calorie, hydrating" },
                    new FoodItem { Emoji = "\U0001F350", Name = "Pears", Note = "High fibre" },
                    new FoodItem { Emoji = "\U0001F95D", Name = "Kiwi", Note = "Low calorie, high vitamin C" },
                }
            };
        }

        private static MealPlan GainWeightPlan()
        {
            return new MealPlan
            {
                GoalLabel = "Gain Weight",
                Summary = "Built around calorie-dense, nutrient-rich foods to support muscle and weight gain.",
                Meals = new List<MealSection>
                {
                    new MealSection { Name = "Breakfast", Emoji = "\U0001F373", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F95C", Name = "Peanut butter oats", Note = "Extra oats + a spoon of peanut butter" },
                        new FoodItem { Emoji = "\U0001F35E", Name = "Eggs on avocado toast", Note = "Healthy fats + protein" },
                        new FoodItem { Emoji = "\U0001F964", Name = "Banana & oat smoothie", Note = "Add peanut butter or full-cream milk" },
                        new FoodItem { Emoji = "\U0001F95E", Name = "Pancakes with honey & nuts", Note = "Extra calories, add yoghurt" },
                        new FoodItem { Emoji = "\U0001F95C", Name = "Granola with full-cream milk", Note = "Add nuts and dried fruit" },
                        new FoodItem { Emoji = "\U0001F35E", Name = "Breakfast burrito (eggs, cheese, beans)", Note = "Calorie-dense, high protein" },
                        new FoodItem { Emoji = "\U0001F95D", Name = "Yoghurt parfait with granola & honey", Note = "Layered for extra calories" },
                        new FoodItem { Emoji = "\U0001F9C0", Name = "Bagel with cream cheese & eggs", Note = "High calorie breakfast" },
                    }},
                    new MealSection { Name = "Lunch", Emoji = "\U0001F957", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F357", Name = "Chicken & rice bowl", Note = "Larger portion, add avocado" },
                        new FoodItem { Emoji = "\U0001F35C", Name = "Beef stir-fry with noodles", Note = "Protein + carbs together" },
                        new FoodItem { Emoji = "\U0001F41F", Name = "Salmon with quinoa", Note = "Healthy fats and protein" },
                        new FoodItem { Emoji = "\U0001F32F", Name = "Loaded burrito bowl", Note = "Rice, beans, meat, cheese, avocado" },
                        new FoodItem { Emoji = "\U0001F35D", Name = "Pasta with chicken & cream sauce", Note = "Calorie-dense" },
                        new FoodItem { Emoji = "\U0001F356", Name = "Steak sandwich with cheese", Note = "High protein and calories" },
                        new FoodItem { Emoji = "\U0001F35A", Name = "Fried rice with egg & chicken", Note = "Filling and calorie-dense" },
                        new FoodItem { Emoji = "\U0001F32E", Name = "Chicken quesadilla", Note = "Cheese adds extra calories" },
                    }},
                    new MealSection { Name = "Dinner", Emoji = "\U0001F37D", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F969", Name = "Steak with baked potato", Note = "Add butter or olive oil" },
                        new FoodItem { Emoji = "\U0001F35D", Name = "Pasta with meat sauce", Note = "Good calorie density" },
                        new FoodItem { Emoji = "\U0001F357", Name = "Chicken, rice and vegetables", Note = "Larger portion size" },
                        new FoodItem { Emoji = "\U0001F355", Name = "Homemade pizza with extra toppings", Note = "Calorie-dense meal" },
                        new FoodItem { Emoji = "\U0001F35B", Name = "Beef curry with rice", Note = "Rich in calories and protein" },
                        new FoodItem { Emoji = "\U0001F35C", Name = "Creamy chicken pasta bake", Note = "High calorie comfort meal" },
                        new FoodItem { Emoji = "\U0001F364", Name = "Fish and chips (baked)", Note = "Carbs + protein" },
                        new FoodItem { Emoji = "\U0001F357", Name = "Roast chicken with stuffing & veg", Note = "Large, calorie-dense plate" },
                    }},
                    new MealSection { Name = "Drinks", Emoji = "\U0001F964", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F95B", Name = "Full-cream milk", Note = "Extra calories and protein" },
                        new FoodItem { Emoji = "\U0001F964", Name = "Protein or fruit smoothie", Note = "Add oats, peanut butter or yoghurt" },
                        new FoodItem { Emoji = "\U0001F9C3", Name = "Fruit juice", Note = "In addition to meals, not instead of" },
                        new FoodItem { Emoji = "\U0001F95B", Name = "Weight-gain / mass-gainer shake", Note = "Follow the product's own serving guidance" },
                        new FoodItem { Emoji = "\U0001F96B", Name = "Hot chocolate (full-cream milk)", Note = "Extra calories, occasional treat" },
                        new FoodItem { Emoji = "\U0001F964", Name = "Peanut butter banana smoothie", Note = "Calorie-dense, protein-rich" },
                    }},
                },
                Fruits = new List<FoodItem> {
                    new FoodItem { Emoji = "\U0001F34C", Name = "Bananas", Note = "Energy-dense, easy to add to meals" },
                    new FoodItem { Emoji = "\U0001F96D", Name = "Mangoes", Note = "Naturally calorie-dense" },
                    new FoodItem { Emoji = "\U0001F951", Name = "Avocado", Note = "Healthy fats, high calorie" },
                    new FoodItem { Emoji = "\U0001F347", Name = "Grapes", Note = "Easy snack, natural sugars" },
                    new FoodItem { Emoji = "\U0001F352", Name = "Dried fruit (raisins, dates)", Note = "Very calorie-dense in small portions" },
                    new FoodItem { Emoji = "\U0001F34D", Name = "Pineapple", Note = "Good with a protein source" },
                }
            };
        }

        private static MealPlan MaintainWeightPlan()
        {
            return new MealPlan
            {
                GoalLabel = "Maintain Weight",
                Summary = "A balanced mix of protein, whole grains, vegetables and fruit to support your current weight.",
                Meals = new List<MealSection>
                {
                    new MealSection { Name = "Breakfast", Emoji = "\U0001F373", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F35E", Name = "Whole-grain toast with eggs", Note = "Balanced protein and carbs" },
                        new FoodItem { Emoji = "\U0001F964", Name = "Fruit and yoghurt bowl", Note = "Mix of protein and natural sugars" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Oatmeal with fruit", Note = "Steady energy through the morning" },
                        new FoodItem { Emoji = "\U0001F95E", Name = "Whole-wheat pancakes with fruit", Note = "Balanced, not too heavy" },
                        new FoodItem { Emoji = "\U0001F95D", Name = "Smoothie bowl with granola", Note = "Fruit, yoghurt and a protein source" },
                        new FoodItem { Emoji = "\U0001F345", Name = "Avocado & tomato on rye toast", Note = "Healthy fats, fibre" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Muesli with milk", Note = "Balanced carbs and fibre" },
                        new FoodItem { Emoji = "\U0001F35E", Name = "Breakfast sandwich (egg & cheese)", Note = "Balanced protein and carbs" },
                    }},
                    new MealSection { Name = "Lunch", Emoji = "\U0001F957", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F32F", Name = "Grilled chicken wrap", Note = "Whole-wheat wrap, mixed veg" },
                        new FoodItem { Emoji = "\U0001F35B", Name = "Mixed bean salad", Note = "Plant protein and fibre" },
                        new FoodItem { Emoji = "\U0001F35A", Name = "Brown rice with veg and protein", Note = "Balanced plate" },
                        new FoodItem { Emoji = "\U0001F35C", Name = "Vegetable & chicken soup", Note = "Warming, balanced" },
                        new FoodItem { Emoji = "\U0001F35E", Name = "Chicken or egg salad sandwich", Note = "Whole-grain bread" },
                        new FoodItem { Emoji = "\U0001F963", Name = "Grain bowl with roasted veg", Note = "Quinoa or couscous base" },
                        new FoodItem { Emoji = "\U0001F41F", Name = "Tuna & salad sandwich", Note = "Lean protein, balanced" },
                        new FoodItem { Emoji = "\U0001F958", Name = "Falafel & hummus wrap", Note = "Plant protein, balanced carbs" },
                    }},
                    new MealSection { Name = "Dinner", Emoji = "\U0001F37D", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F41F", Name = "Grilled fish with vegetables", Note = "Light, balanced meal" },
                        new FoodItem { Emoji = "\U0001F357", Name = "Chicken stir-fry", Note = "Mixed vegetables, moderate oil" },
                        new FoodItem { Emoji = "\U0001F35B", Name = "Vegetable curry with rice", Note = "Balanced portion" },
                        new FoodItem { Emoji = "\U0001F35D", Name = "Pasta primavera", Note = "Vegetables + moderate portion of pasta" },
                        new FoodItem { Emoji = "\U0001F32E", Name = "Fish tacos with slaw", Note = "Balanced protein and carbs" },
                        new FoodItem { Emoji = "\U0001F357", Name = "Roast chicken with vegetables", Note = "Classic balanced plate" },
                        new FoodItem { Emoji = "\U0001F35B", Name = "Stir-fried tofu & vegetables with rice", Note = "Plant-based balanced option" },
                        new FoodItem { Emoji = "\U0001F364", Name = "Grilled prawns with rice & greens", Note = "Lean protein, balanced carbs" },
                    }},
                    new MealSection { Name = "Drinks", Emoji = "\U0001F964", Items = new List<FoodItem> {
                        new FoodItem { Emoji = "\U0001F4A7", Name = "Water", Note = "Your main drink through the day" },
                        new FoodItem { Emoji = "\U0001F375", Name = "Herbal tea", Note = "Caffeine-free option" },
                        new FoodItem { Emoji = "\U0001F9C3", Name = "Fresh fruit juice", Note = "In moderation, alongside meals" },
                        new FoodItem { Emoji = "\u2615", Name = "Coffee or tea (light milk)", Note = "In moderation" },
                        new FoodItem { Emoji = "\U0001F95B", Name = "Low-fat milk", Note = "With meals or snacks" },
                        new FoodItem { Emoji = "\U0001F9CA", Name = "Sparkling water with fruit", Note = "Refreshing, no added sugar" },
                    }},
                },
                Fruits = new List<FoodItem> {
                    new FoodItem { Emoji = "\U0001F34E", Name = "Apples", Note = "Great everyday snack" },
                    new FoodItem { Emoji = "\U0001F34A", Name = "Oranges", Note = "Vitamin C and fibre" },
                    new FoodItem { Emoji = "\U0001F34C", Name = "Bananas", Note = "Good pre/post workout snack" },
                    new FoodItem { Emoji = "\U0001F347", Name = "Grapes", Note = "Easy to portion, refreshing" },
                    new FoodItem { Emoji = "\U0001F351", Name = "Peaches", Note = "Seasonal, high in fibre" },
                    new FoodItem { Emoji = "\U0001F353", Name = "Berries", Note = "Antioxidant-rich, low calorie" },
                }
            };
        }

        /// <summary>
        /// Ingredient library for the Build Your Own Meal calculator. Calorie values
        /// are rounded, typical figures for the stated serving (informed by common
        /// nutrition references such as USDA FoodData Central) - estimates for
        /// planning purposes, not a lab measurement of a specific product.
        /// </summary>
        public static List<Ingredient> Ingredients()
        {
            return new List<Ingredient>
            {
                new Ingredient { Emoji = "\U0001F357", Name = "Chicken breast, grilled (100g)", Calories = 165, Category = "Protein", Benefit = "High protein" },
                new Ingredient { Emoji = "\U0001F41F", Name = "Salmon, baked (100g)", Calories = 208, Category = "Protein", Benefit = "High protein, healthy fat" },
                new Ingredient { Emoji = "\U0001F969", Name = "Lean beef, grilled (100g)", Calories = 217, Category = "Protein", Benefit = "High protein" },
                new Ingredient { Emoji = "\U0001F373", Name = "Egg, large (1)", Calories = 70, Category = "Protein", Benefit = "High protein" },
                new Ingredient { Emoji = "\U0001F41F", Name = "Tuna, canned in water (100g)", Calories = 116, Category = "Protein", Benefit = "Lean protein" },
                new Ingredient { Emoji = "\U0001F963", Name = "Tofu, firm (100g)", Calories = 76, Category = "Protein", Benefit = "Plant protein" },
                new Ingredient { Emoji = "\U0001F35B", Name = "Lentils, cooked (1 cup)", Calories = 230, Category = "Protein", Benefit = "Plant protein, high fibre" },
                new Ingredient { Emoji = "\U0001F958", Name = "Chickpeas, cooked (1 cup)", Calories = 269, Category = "Protein", Benefit = "Plant protein, high fibre" },
                new Ingredient { Emoji = "\U0001F95A", Name = "Cottage cheese (1 cup)", Calories = 220, Category = "Protein", Benefit = "High protein" },

                new Ingredient { Emoji = "\U0001F35A", Name = "Brown rice, cooked (1 cup)", Calories = 216, Category = "Carbs", Benefit = "Whole grain" },
                new Ingredient { Emoji = "\U0001F35A", Name = "White rice, cooked (1 cup)", Calories = 205, Category = "Carbs", Benefit = "Energy source" },
                new Ingredient { Emoji = "\U0001F963", Name = "Oats, cooked (1 cup)", Calories = 150, Category = "Carbs", Benefit = "High fibre" },
                new Ingredient { Emoji = "\U0001F35E", Name = "Whole-wheat bread (1 slice)", Calories = 80, Category = "Carbs", Benefit = "Fibre" },
                new Ingredient { Emoji = "\U0001F360", Name = "Sweet potato, baked (medium)", Calories = 112, Category = "Carbs", Benefit = "High fibre, vitamins" },
                new Ingredient { Emoji = "\U0001F954", Name = "Potato, baked (medium)", Calories = 161, Category = "Carbs", Benefit = "Energy source" },
                new Ingredient { Emoji = "\U0001F35A", Name = "Quinoa, cooked (1 cup)", Calories = 222, Category = "Carbs", Benefit = "Whole grain, protein" },
                new Ingredient { Emoji = "\U0001F35D", Name = "Pasta, cooked (1 cup)", Calories = 221, Category = "Carbs", Benefit = "Energy source" },

                new Ingredient { Emoji = "\U0001F966", Name = "Broccoli, steamed (1 cup)", Calories = 55, Category = "Vegetables", Benefit = "Low calorie, high fibre" },
                new Ingredient { Emoji = "\U0001F96C", Name = "Spinach, raw (1 cup)", Calories = 7, Category = "Vegetables", Benefit = "Very low calorie" },
                new Ingredient { Emoji = "\U0001F966", Name = "Mixed salad greens (2 cups)", Calories = 20, Category = "Vegetables", Benefit = "Very low calorie" },
                new Ingredient { Emoji = "\U0001F955", Name = "Carrots (1 cup)", Calories = 52, Category = "Vegetables", Benefit = "Low calorie, fibre" },
                new Ingredient { Emoji = "\U0001FAD1", Name = "Bell peppers (1 cup)", Calories = 30, Category = "Vegetables", Benefit = "Low calorie, vitamin C" },
                new Ingredient { Emoji = "\U0001FAD8", Name = "Green beans (1 cup)", Calories = 44, Category = "Vegetables", Benefit = "Low calorie, fibre" },

                new Ingredient { Emoji = "\U0001F34C", Name = "Banana (1 medium)", Calories = 105, Category = "Fruits", Benefit = "Energy, potassium" },
                new Ingredient { Emoji = "\U0001F34E", Name = "Apple (1 medium)", Calories = 95, Category = "Fruits", Benefit = "High fibre" },
                new Ingredient { Emoji = "\U0001F34A", Name = "Orange (1 medium)", Calories = 62, Category = "Fruits", Benefit = "Vitamin C" },
                new Ingredient { Emoji = "\U0001F353", Name = "Berries, mixed (1 cup)", Calories = 85, Category = "Fruits", Benefit = "Low calorie, antioxidants" },
                new Ingredient { Emoji = "\U0001F96D", Name = "Mango (1 cup)", Calories = 99, Category = "Fruits", Benefit = "Vitamins, natural sugars" },
                new Ingredient { Emoji = "\U0001F951", Name = "Avocado (half)", Calories = 120, Category = "Fruits", Benefit = "Healthy fat" },

                new Ingredient { Emoji = "\U0001FAD2", Name = "Olive oil (1 tbsp)", Calories = 119, Category = "Fats & Extras", Benefit = "Healthy fat" },
                new Ingredient { Emoji = "\U0001F95C", Name = "Peanut butter (1 tbsp)", Calories = 95, Category = "Fats & Extras", Benefit = "Healthy fat, protein" },
                new Ingredient { Emoji = "\U0001F330", Name = "Almonds (30g / small handful)", Calories = 170, Category = "Fats & Extras", Benefit = "Healthy fat" },
                new Ingredient { Emoji = "\U0001F9C0", Name = "Cheese (30g)", Calories = 110, Category = "Fats & Extras", Benefit = "Protein, calcium" },
                new Ingredient { Emoji = "\U0001F9C8", Name = "Butter (1 tbsp)", Calories = 102, Category = "Fats & Extras", Benefit = "Calorie-dense" },

                new Ingredient { Emoji = "\U0001F95B", Name = "Milk, full-cream (1 cup)", Calories = 149, Category = "Dairy & Drinks", Benefit = "Protein, calcium" },
                new Ingredient { Emoji = "\U0001F95B", Name = "Milk, low-fat (1 cup)", Calories = 102, Category = "Dairy & Drinks", Benefit = "Protein, calcium" },
                new Ingredient { Emoji = "\U0001F963", Name = "Greek yoghurt, plain (1 cup)", Calories = 130, Category = "Dairy & Drinks", Benefit = "High protein" },
                new Ingredient { Emoji = "\U0001F964", Name = "Protein shake (1 scoop + water)", Calories = 120, Category = "Dairy & Drinks", Benefit = "High protein" },
            };
        }
    }
}
