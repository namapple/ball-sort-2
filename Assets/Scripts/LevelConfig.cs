using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GameTown.MiniGame.BallSort
{

    [System.Serializable]
    public class LevelConfig
    {
        public List<UserLevel> user_level;
        public List<LevelDifficulty> level_dificulty;
        public List<RewardData> rewards;
    }
    
    public class UserLevel
    {
        public int level_from;
        public int level_to;
        public List<string> level_difficulty;
    }

    public class LevelDifficulty
    {
        public string level_difficulty;
        public int time;
        public int max_level;
        public int point_each_tube;
    }



    public class RewardData
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


