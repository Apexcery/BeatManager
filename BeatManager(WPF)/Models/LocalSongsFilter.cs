using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatManager_WPF_.Models
{
    public class LocalSongsFilter
    {
        public string SearchQuery { get; set; } = null;
        public DifficultyFilter? Difficulty { get; set; } = null;
    }

    public enum DifficultyFilter
    {
        Easy,
        Normal,
        Hard,
        Expert,
        ExpertPlus
    }
}
