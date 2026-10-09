using System.Collections.Generic;
using DataStructure;

namespace Data
{
    class PlayerScoreComparer : IComparer<PlayerInfo>
    {
        public int Compare(PlayerInfo a, PlayerInfo b)
        {
            return b.score.CompareTo(a.score);  
        }
    }
}