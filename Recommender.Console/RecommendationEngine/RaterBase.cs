using System;
using System.Linq; 
using System.Collections.Generic;
using System.Text;

namespace RecommendationEngine
{
    public class RaterBase 
    {
        public List<RateeBase> Likes { get; set; }
        public List<RateeBase> Dislikes { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Similarity> Similarities { get; set; }
        public Dictionary<RateeBase, double> Ratees { get; set; }

        public List<KeyValuePair<RateeBase, double>> GetSuggestions()
        {
            var myList = Ratees.ToList();
            myList.Sort((pair1, pair2) => (pair1.Value.CompareTo(pair2.Value) * -1));

            return myList; 
        }
    }
}
