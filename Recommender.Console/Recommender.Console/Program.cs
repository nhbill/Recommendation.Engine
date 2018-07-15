using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using RecommendationEngine; 

namespace Recommender.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ArticleRecommender recommendationEngine = new ArticleRecommender("Userbehavior.txt");

            // cut just using UpVotes and DownVotes
            ShowMaxAndMinCorrelationFromUsers(recommendationEngine);

            System.Console.WriteLine("Run results tags to correlate similarities on UpVotes and DownVotes only...");
            recommendationEngine.GenerateSimilaritiesByTags();
            recommendationEngine.GenerateRatingsProbabilitiesForUsers();
            ShowMaxAndMinCorrelationFromUsers(recommendationEngine);

            // lets add the downloads to the likes and see what that does to our numbers ---
            System.Console.WriteLine("Added Downloads as likes to result set, no tags for correlation...");
            recommendationEngine.AddUserActionsToLikes("Download");
            recommendationEngine.GenerateSimilarityValuesForUsers();
            recommendationEngine.GenerateRatingsProbabilitiesForUsers();
            ShowMaxAndMinCorrelationFromUsers(recommendationEngine);

            System.Console.WriteLine("Run results tags to correlate similarities on UpVotes and Downloads as Likes and DownVotes as Dislikes...");
            recommendationEngine.GenerateSimilaritiesByTags();
            recommendationEngine.GenerateRatingsProbabilitiesForUsers();
            ShowMaxAndMinCorrelationFromUsers(recommendationEngine);

            System.Console.WriteLine("Added Views as likes to result set, no tags...");
            recommendationEngine.AddUserActionsToLikes("View");
            recommendationEngine.GenerateSimilarityValuesForUsers();
            recommendationEngine.GenerateRatingsProbabilitiesForUsers();
            ShowMaxAndMinCorrelationFromUsers(recommendationEngine);

            System.Console.WriteLine("Run results tags to correlate similarities on UpVotes, Views and Downloads as Likes and DownVotes as Dislikes...");
            recommendationEngine.GenerateSimilaritiesByTags();
            recommendationEngine.GenerateRatingsProbabilitiesForUsers();
            ShowMaxAndMinCorrelationFromUsers(recommendationEngine);
        }

        public static void ShowMaxAndMinCorrelationFromUsers(ArticleRecommender recommendationEngine)
        {
        double maxCorrelation = -1.0 ;
        double minCorrelation = 1.0;
        RateeBase rateeMax = null; 
        RaterBase raterMax = null;
        RateeBase rateeMin = null;
        RaterBase raterMin = null; 

          foreach (User rater in recommendationEngine.Raters)
            {
                List<KeyValuePair<RateeBase, double>> list = rater.GetSuggestions();

                if (list.Any() && list[0].Value > maxCorrelation)
                {
                    maxCorrelation = list[0].Value;
                    raterMax = rater;
                    rateeMax = list[0].Key; 
                }

                if (list.Any() && list[list.Count() - 1].Value < minCorrelation)
                {
                    minCorrelation = list[list.Count() - 1].Value;
                    raterMin = rater;
                    rateeMin = list[list.Count() - 1].Key;
                }
            }

            if (raterMax != null)
            {
                System.Console.WriteLine("Max Correlation was " + maxCorrelation);
                System.Console.WriteLine("for user " + raterMax.Name);
                System.Console.WriteLine("for article " + rateeMax.Name);
            }

            if (raterMin != null)
            {
                System.Console.WriteLine("Min Correlation was " + minCorrelation);
                System.Console.WriteLine("for user " + raterMin.Name);
                System.Console.WriteLine("for article " + rateeMin.Name);
            }
        }
    }
}
