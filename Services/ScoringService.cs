﻿using CRICXI.Models;

namespace CRICXI.Services
{
    public class ScoringService
    {
        public int CalculatePoints(PlayerPerformance p, bool isCaptain = false, bool isViceCaptain = false)
        {
            int pts = 0;

            // Basic Batting
            pts += p.Runs;
            pts += p.Fours * 1;
            pts += p.Sixes * 2;
            if (p.Runs >= 30) pts += 4;
            if (p.Runs >= 50) pts += 8;
            if (p.Runs >= 100) pts += 16;
            if (p.Runs == 0 && p.BallsFaced > 0) pts -= 2;

            // Bowling
            pts += p.Wickets * 25;
            if (p.Wickets >= 3) pts += 4;
            if (p.Wickets >= 5) pts += 8;

            // Fielding
            pts += p.Catches * 8;
            pts += p.Stumpings * 12;
            pts += p.RunOutsDirect * 12;
            pts += p.RunOutsIndirect * 6;

            // Economy (min 2 overs)
            if (p.OversBowled >= 2)
            {
                double econ = (double)p.RunsConceded / p.OversBowled;
                if (econ < 4) pts += 6;
                else if (econ < 6) pts += 4;
                else if (econ < 7) pts += 2;
                else if (econ > 9) pts -= 6;
                else if (econ > 8) pts -= 4;
                else if (econ > 7) pts -= 2;
            }

            // Strike Rate (min 10 balls)
            if (p.BallsFaced >= 10)
            {
                double sr = ((double)p.Runs / p.BallsFaced) * 100;
                if (sr > 170) pts += 6;
                else if (sr > 150) pts += 4;
                else if (sr > 130) pts += 2;
                else if (sr < 50) pts -= 6;
                else if (sr < 60) pts -= 4;
                else if (sr < 70) pts -= 2;
            }

            // Captain/Vice Captain
            if (isCaptain) pts *= 2;
            else if (isViceCaptain) pts = (int)(pts * 1.5);

            return pts;
        }
    }

}
