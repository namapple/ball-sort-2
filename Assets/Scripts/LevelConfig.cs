using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameTown.MiniGame.BallSort
{

    public class LevelConfig
    {
        public List<UserLevelConfig> user_level;
        public List<LevelDifficultyConfig> level_dificulty;
        public List<RewardConfig> rewards;
    }
    
    public class UserLevelConfig
    {
        public int level_from;
        public int level_to;
        public List<string> level_difficulty;
    }

    public class LevelDifficultyConfig
    {
        public string level_difficulty;
        public int time;
        public int point_each_tube;
    }



    public class RewardConfig
    {
        public int id;
        public int point_min;
        public List<RewardItem> reward;
    }

    public class RewardItem
    {
        public int hotel_id;
        public int type;
        public int number;
    }
    
}


