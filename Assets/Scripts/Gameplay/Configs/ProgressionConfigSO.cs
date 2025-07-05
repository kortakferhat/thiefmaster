using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Configs
{
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "TowerClicker/Progression Config", order = 2)]
    public class ProgressionConfigSO : ScriptableObject
    {
        public List<int> LevelToExperience = new List<int>();

        public int GetExperienceToLevelUp(int totalExperience)
        {
            var calculatedLevel = 0;
            var calculatedTotalExperienceToLevel = 0;
            
            while (calculatedTotalExperienceToLevel < totalExperience)
            {
                var levelToExperience = LevelToExperience[^1] * 2;
                if (calculatedLevel < LevelToExperience.Count)
                {
                    levelToExperience = LevelToExperience[calculatedLevel];
                }
                
                calculatedTotalExperienceToLevel += levelToExperience;
                calculatedLevel++;
                
                if (calculatedLevel >= LevelToExperience.Count)
                {
                    break;
                }
            }

            return calculatedTotalExperienceToLevel;
        }
    }
}