using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using RecommendationEngine;

namespace Recommender.Console
{
    public class ArticleRecommender : RecommenderBase 
    {
        public ArticleRecommender(string fileName) : base()
        {
            Tags = new List<Tag>();
            Downloads = new List<UserAction>();
            Views = new List<UserAction>();
            LoadData(fileName, "UpVote", "DownVote");
        }

        public List<Tag> Tags { get; set; }
        public List<UserAction> Downloads { get; set; }
        public List<UserAction> Views { get; set; }

        public void GenerateSimilaritiesByTags()
        {
            foreach(User user1 in this.Raters)
            {
                user1.Similarities = new List<Similarity>();

                foreach (User user2 in this.Raters )
                {
                    if (user1.Id == user2.Id)
                        continue;

                    List<KeyValuePair<Tag, int>> tagValuesUser1Likes = GetKeyValuePairTagValueByRatees(user1.Likes);
                    List<KeyValuePair<Tag, int>> tagValuesUser2Likes = GetKeyValuePairTagValueByRatees(user2.Likes);
                    List<KeyValuePair<Tag, int>> tagValuesUser1Dislikes = GetKeyValuePairTagValueByRatees(user1.Dislikes);
                    List<KeyValuePair<Tag, int>> tagValuesUser2DisLikes = GetKeyValuePairTagValueByRatees(user2.Dislikes);

                    int L1IL2 = GetTagCountIntersection(tagValuesUser1Likes, tagValuesUser2Likes);
                    int D1ID2 = GetTagCountIntersection(tagValuesUser1Dislikes, tagValuesUser2DisLikes);
                    int L1ID2 = GetTagCountIntersection(tagValuesUser1Likes, tagValuesUser2DisLikes);
                    int L2ID1 = GetTagCountIntersection(tagValuesUser2Likes, tagValuesUser1Dislikes);

                    double sim = RunSimilarityFormula(L1IL2, D1ID2, L1ID2, L2ID1, L1IL2 + D1ID2 + L1ID2 + L2ID1);

                    // not enough data to figure out a similarity...
                    if (sim != 0.0)
                        user1.Similarities.Add(new Similarity() { Rater = user2, Value = sim });
                }
                // public double RunSimilarityFormula(int L1IL2, int D1ID2, int L1ID2, int L2ID1, int L1UL2UD1UD2)
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagsUser1"></param>
        /// <param name="tagsUser2"></param>
        /// <param name="listUser1"></param>
        /// <param name="listUser2"></param>
        /// <returns></returns>
        public int GetTagCountIntersection(List<KeyValuePair<Tag, int>> listUser1, List<KeyValuePair<Tag, int>> listUser2)
        {
            var U1TagsIU2Tags = listUser1.Select(s => s.Key).Intersect(listUser2.Select(s => s.Key)).ToList();

            int tagCount1 = GetTagCountFromTagList(U1TagsIU2Tags, listUser1);
            int tagCount2 = GetTagCountFromTagList(U1TagsIU2Tags, listUser2);

            return tagCount1 + tagCount2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int GetTagCountFromTagList(List<Tag> tags, List<KeyValuePair<Tag, int>> list)
        {
            var distinctTags = tags.Distinct();
            int tagCount = 0; ;
            foreach (Tag tag in distinctTags)
            {
                KeyValuePair<Tag, int> pair = list.FirstOrDefault(s => s.Key.Id == tag.Id);
                tagCount += pair.Value;
            }

            return tagCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int GetTagCountTotal(List<KeyValuePair<Tag, int>> list)
        {
            int tagCount = 0;
            foreach (KeyValuePair<Tag, int> pair in list)
                tagCount += pair.Value;

            return tagCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<KeyValuePair<Tag, int>> GetKeyValuePairTagValueByRatees(List<RateeBase> list)
        {
            Dictionary<Tag, int> dictionary = new Dictionary<Tag, int>();

            foreach (RateeBase ratee in list)
            {
                foreach (Tag tag in ((Article)ratee).Tags)
                {
                    if (dictionary.ContainsKey(tag) == false)
                        dictionary.Add(tag, 1);
                    else
                        dictionary[tag]++;
                }
            }

            var myList = dictionary.ToList();
            myList.Sort((pair1, pair2) => (pair1.Value.CompareTo(pair2.Value) * -1));

            return myList;
        }


        public override void LoadData(string fileName, string actionLike, string actionDislike)
        {
            DateTime now = DateTime.Now;
            System.Console.WriteLine("LoadData() start time:  " + now);

            string[] contents = System.IO.File.ReadAllLines(fileName);

            bool tags = false, article = false, user = false, ratings = false;

            foreach (string content in contents)
            {
                if (content.StartsWith("# Tags"))
                {
                    tags = true;
                    continue;
                }

                if (content.StartsWith("# Articles"))
                {
                    article = true;
                    continue;
                }
                if (content.StartsWith("# Users"))
                {
                    user = true;
                    article = false;
                    tags = false;
                    continue;
                }


                if (content.StartsWith("# User actions"))
                {
                    ratings = true;
                    user = false;
                    article = false;
                    tags = false;
                    continue;
                }

                if (string.IsNullOrEmpty(content) == true || content.StartsWith("#"))
                {
                    continue;
                }

                string[] splits = content.Split(",");

                if (tags == true)
                {
                    int number = 1;
                    foreach (string split in splits)
                    {
                        Tags.Add(new Tag() { Id = number++, Name = split.Trim() });
                    }

                    tags = false;
                }

                if (article == true)
                {
                    Article ratee = new Article()
                    {
                        Tags = new List<Tag>(),
                        Name = splits[1].Trim(),
                        Id = int.Parse(splits[0]),
                        Likes = new List<RaterBase>(),
                        Dislikes = new List<RaterBase>(),
                        Views = new List<RaterBase>(),
                        Downloads = new List<RaterBase>()
                    };

                    Ratees.Add(ratee);

                    for (int index = 2; index < splits.Count(); index++)
                    {
                        ratee.Tags.Add(this.Tags.FirstOrDefault(s => s.Name == splits[index].Trim()));
                    }
                }

                if (user == true)
                {
                    Raters.Add(new User
                    {
                        Id = int.Parse(splits[0]),
                        Name = splits[1].Trim(),
                        Likes = new List<RateeBase>(),
                        Dislikes = new List<RateeBase>(),
                        Views = new List<RateeBase>(),
                        Downloads = new List<RateeBase>() 
                    });
                }

                if (ratings == true)
                {
                    UserAction rating = new UserAction
                    {
                        Action = splits[1],
                        Ratee = this.Ratees.FirstOrDefault(s => s.Name == splits[5].Trim()),
                        Rater = this.Raters.FirstOrDefault(s => s.Name == splits[3].Trim()),
                    };

                    Ratings.Add(rating);
                    if (rating.Action.Equals(actionLike))
                    {
                        rating.Ratee.Likes.Add(rating.Rater);
                        rating.Rater.Likes.Add(rating.Ratee);
                    }
                    else if (rating.Action.Equals(actionDislike))
                    {
                        rating.Ratee.Dislikes.Add(rating.Rater);
                        rating.Rater.Dislikes.Add(rating.Ratee);
                    }
                    else if (rating.Action.Equals("Download"))
                    {
                        Downloads.Add(rating);
                        ((Article)rating.Ratee).Downloads.Add(rating.Rater);
                        ((User)rating.Rater).Downloads.Add(rating.Ratee);

                        ((Article)rating.Ratee).Likes.Add(rating.Rater);
                        ((User)rating.Rater).Likes.Add(rating.Ratee);
                    }
                    else if (rating.Action.Equals("View"))
                    {
                        Views.Add(rating);
                        ((Article)rating.Ratee).Views.Add(rating.Rater);
                        ((User)rating.Rater).Views.Add(rating.Ratee);
                    }
                }
            }
            System.Console.WriteLine("LoadData() time to process:  " + DateTime.Now.Subtract(now));

            InitializeStatistics("UpVote", "DownVote");
        }

        //public void PopulatedSuggestionsByTags()
        //{
        //    DateTime now = DateTime.Now;
        //    System.Console.WriteLine("PopulatedSuggestionsByTags() start time:  " + now);
        //    Parallel.ForEach(this.Raters, (rater) =>
        //    {
        //        ((User)rater).SuggestionsCalculatedByTags = ((User)rater).GetSuggestionsByTagsCorrelation(((User)rater).Views, this.Ratees);
        //    });

        //    System.Console.WriteLine("PopulatedSuggestionsByTags() time to process:  " + DateTime.Now.Subtract(now));
        //}




    }
}
