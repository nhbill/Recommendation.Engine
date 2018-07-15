using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using RecommendationEngine; 

namespace RecommendationEngine
{
    public abstract class RecommenderBase
    {
        //public enum 
        public abstract void LoadData(string fileName, string actionLike, string actionDislike);
        public List<RaterBase> Raters { get; set; }
        public List<RateeBase> Ratees { get; set; }
        public List<UserAction> Ratings { get; set; }
        public List<UserAction> Likes { get; set; }
        public List<UserAction> Dislikes { get; set; }

        public RecommenderBase()
        {
            Raters = new List<RaterBase>();
            Ratees = new List<RateeBase>();
            Ratings = new List<UserAction>();
        }
        
        /// <summary>
        /// S(U1, U2) = ((L1 intersection L2) + (D1 intersection d2) - (L1 intersection D2) - (L2 intersection D1)) / (L1 union L2 union D1 union D2)
        /// </summary>
        /// <param name="L1IL2"></param>
        /// <param name="D1ID2"></param>
        /// <param name="L1ID2"></param>
        /// <param name="L2ID1"></param>
        /// <param name="L1UL2UD1UD2"></param>
        /// <returns></returns>
        public double RunSimilarityFormula(int L1IL2, int D1ID2, int L1ID2, int L2ID1, int L1UL2UD1UD2)
        {
            double probability = (L1IL2 + D1ID2 - L1ID2 - L2ID1) / (double)L1UL2UD1UD2; 

            if (double.IsNaN(probability) == true || double.IsNegativeInfinity(probability) || double.IsPositiveInfinity(probability))
                probability = 0.0;

            return probability ;
        }

        /// <summary>
        /// 
        /// P(U,A) = (Zl - Zd) / (Ml + Md)
        /// 
        /// </summary>
        /// <param name="Zl"></param>
        /// <param name="Zd"></param>
        /// <param name="Ml"></param>
        /// <param name="Md"></param>
        /// <returns></returns>
        public double RunProbabilityFormula(double Zl, double Zd, int Ml, int Md)
        {
            double probability = (Zl - Zd) / (double)(Ml + Md);

            if (double.IsNaN(probability) == true || double.IsNegativeInfinity(probability) || double.IsPositiveInfinity(probability))
                probability = 0.0;

            return probability; 
        }

        private Similarity GetSimilarityToRater(RaterBase rater1, RaterBase rater2)
        {
            // do some linq magic to get our variables for the forumula -- this builds the similarity value between rater1 and rater2
            //S(U1, U2) = ((L1 intersection L2) + (D1 intersection d2) - (L1 intersection D2) - (L2 intersection D1)) / (L1 union L2 union D1 union D2)

            // (L1 intersection L2)
            var listRater1LikeRater2LikeIntersection = rater1.Likes.Intersect(rater2.Likes);

            //  (D1 intersection d2)
            var listRater1DisLikeRater2DisLikeIntersection = rater1.Dislikes.Intersect(rater2.Dislikes);

            // (L1 intersection D2)
            var listRater1LikeRater2DisLikeIntersection = rater1.Likes.Intersect(rater2.Dislikes);

            // (L2 intersection D1)
            var listRater1DisLikeRater2LikeIntersection = rater2.Likes.Intersect(rater1.Dislikes);

            var divisor = rater1.Likes.Union(rater2.Likes).Union(rater1.Dislikes).Union(rater2.Dislikes);

            double similarity = RunSimilarityFormula(listRater1LikeRater2LikeIntersection.Count(), listRater1DisLikeRater2DisLikeIntersection.Count(),
                listRater1LikeRater2DisLikeIntersection.Count(), listRater1DisLikeRater2LikeIntersection.Count(),
                divisor.Count());

            if (similarity == double.NaN)
                similarity = 0.0; 
            
            return new Similarity() { Rater = rater2, Value = similarity } ; 
        }
        
        private List<Similarity> GetSimularities(RaterBase rater)
        {
            List<Similarity> simularities = new List<Similarity>();

            foreach ( RaterBase rater2 in Raters)
            {
                if (rater.Equals(rater2) == true)
                    continue;

                Similarity sim = GetSimilarityToRater(rater, rater2);
                if (sim.Value != 0.0)
                    simularities.Add(sim); 
            }

            return simularities;
        }

        public void GenerateSimilarityValuesForUsers()
        {
            foreach ( RaterBase rater in Raters)
            {
                if (rater.Similarities != null)
                {
                    rater.Similarities.Clear();
                    rater.Ratees.Clear();
                }

                rater.Similarities = GetSimularities(rater);
            }
        }

        public void GenerateRatingsProbabilitiesForUsers()
        {
            Parallel.ForEach(this.Raters, (rater) =>
            {
                if (rater.Ratees != null )
                    rater.Ratees.Clear(); 

                Dictionary<RateeBase, double> dictionary = new Dictionary<RateeBase, double>();
                foreach (RateeBase ratee in this.Ratees)
                {
                    double probability = GetProbabilityUserLikesRatee(rater, ratee);
                    if (probability != 0.0)
                        dictionary.Add(ratee, probability);
                }

                rater.Ratees = dictionary;
            });
        }

        private double GetProbabilityUserLikesRatee(RaterBase user, RateeBase ratee)
        {
            // P(U,A) = (Zl - Zd) / (Ml + Md)

            // Zl computations
            // sum of user's similarities to our user who have liked the ratee in question
            var similaritesForRatersWhoLikedRatee = user.Similarities.Where(s => ratee.Likes.Contains(s.Rater));
            double Zl = 0.0;
            foreach (Similarity similar in similaritesForRatersWhoLikedRatee)
            {
                Zl += similar.Value;
            }

            // Ml computations
            // total who have liked ratee
            int Ml = ratee.Likes.Count();

            // Zd computations
            // sum of user's similarities to our user who have disliked the ratee in question
            var similaritesForRatersWhoDislikedRatee = user.Similarities.Where(s => ratee.Dislikes.Contains(s.Rater));
            double Zd = 0.0;
            foreach (Similarity similar in similaritesForRatersWhoDislikedRatee)
            {
                Zd += similar.Value;
            }

            // Md computations
            // total who have disliked ratee
            int Md = ratee.Dislikes.Count();

            double probability = 0.0;

            // if ( (Ml + Md) != 0.0 && (Zl - Zd) != 0)
            probability = RunProbabilityFormula(Zl, Zd, Ml, Md);

            if (probability == double.NaN)
                probability = 0.0 ; 

            return probability;
        }

        public void RemoveUserActionsFromLikes(string userAction)
        {
            List<UserAction> listUserActions = Ratings.Where(s => s.Action.Equals(userAction)).ToList();

            for ( int index = listUserActions.Count() - 1, count = 0; count < listUserActions.Count() - 1; index --, count ++)
            {
                this.Likes.RemoveAt(index); 
            }
            
            for ( int index = listUserActions.Count() -1 ; index >= 0; index --)
            {
                UserAction uA = listUserActions[index];
                RaterBase rater = Raters.FirstOrDefault(s => s.Id == uA.Rater.Id);

                if (rater != null)
                    rater.Likes.RemoveAt(rater.Likes.Count() - 1);
            }
        }

        public void AddUserActionsToLikes(string userAction)
        {
            List<UserAction> listUserActions = Ratings.Where(s => s.Action.Equals(userAction)).ToList();

            this.Likes.AddRange(listUserActions);

            foreach (UserAction uA in listUserActions)
            {
                RaterBase rater = Raters.FirstOrDefault(s => s.Id == uA.Rater.Id);

                if (rater != null)
                {
                    rater.Likes.Add(uA.Ratee);
                }
            }
        }

        public void InitializeStatistics(string actionLike, string actionDislike)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine("InitializeStatistics() start time:  " + now);

            this.Likes = Ratings.Where(s => s.Action.Equals(actionLike)).ToList();
            this.Dislikes = Ratings.Where(s => s.Action.Equals(actionLike)).ToList();

            GenerateSimilarityValuesForUsers();
            GenerateRatingsProbabilitiesForUsers();

            Console.WriteLine("InitializeStatistics() time to process:  " + DateTime.Now.Subtract(now));
        }
    }
}
