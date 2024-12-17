using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


/*
 * TODO:
 * - Print your prediction of the next move based on move command
 * -- useful to have one that translates based on command entered by opponent
 * --- i cannot see it live but i can reverse engineer commands
 * -- probably deciding my move like i was doing, based on angle and thrust rather than position ok
 * --- i then translate the angle into coordinates that now can be arbitrary, helps hide my intentions
 * -- See if you can calculate best solution rather than check all possibilities
 * --- could be faster and more accurate
 * --- having all possibilities check is useful to estimate likely future position/vector of enemy so this is needed anyways
 * ---- my current prediction of enemy was not adding any angle and keeping thrust the same, maybe adjust this
 *
 * - Predict collisions correctly
 * -- is going to be weird when there are multiple collisions, wonder how they are ordered
 * --- if two pods collide first but their bounce hits another, do you just add up all vectors of overlapping future pods?
 * -- start by crashing your two pods and predicting outcome since you know your commands
 *
 * - it will be useful to make pod commands as a team rather than individually like before so you can coordinate
 * -- try to make units of calculation that can be optimized through machine learning or neural nets easily
 * --- i wonder how i can create a neural net that has extra info like the physics involved hybrid between regression and NN
 * --- something like given podInfo on both pod and target dest as parameters to NN and info about physics, output best commands
 * -- logic something like:
 * --- given both podInfos
 * ---- If team timeout > 50
 * ----- collaborate to make pod ranked higher move closer to its next target
 * ------ collide enemy near friend ranked higher so that resulting vectors favor your higher ranked friend
 * ----- maybe move any pod to reset timeout if unobstructed
 * ---- If team timeout < 50
 * ----- defend enemy to increase its timeout (might not be good idea because if you are ahead then you lose the lead)
 *
 * - Once you can predict collisions correctly move on to animation
 * -- this will be learning experience of graphics
 * -- leads to reinforcement learning learning => reward faster move to a position or whole map targets
 * --- maybe adversarial learning => battle pods to each other improve attach and defense
 * -- could make into tiktok battle channel with my own figures
 * -- much easier to test outcomes and techniques 
 *
 * - Track if pod has used its 650 thrust, not sure if one per team or one each, test it
 */

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

/// <summary>
/// Snapshot information of a pod received at each iteration of the game
/// </summary>
public class PodInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public Coordinates PosCoordinates => new Coordinates() { X = X, Y = Y };
    public int Vx { get; set; }
    public int Vy { get; set; }
    public int Angle { get; set; }
    public int NextCheckPointId { get; set; }
    public override string ToString()
    {
        return $"x:{X}, y:{Y}, vx:{Vx}, vy:{Vy}, angle:{Angle}, DestChkpnt:{NextCheckPointId}";
    }
}

public class GameInfo
{
    public int Laps { get; set; }
    public int CheckpointCount { get; set; }
    public List<Coordinates> Checkpoints = new List<Coordinates>();
}

public class Coordinates
{
    public int X { get; set; }
    public int Y { get; set; }
    public override string ToString()
    {
        return $"x:{X}, y:{Y}";
    }
}

public class Command
{
    public Coordinates Destination { get; set; }
    public int Thrust { get; set; }
    public PodInfo FuturePodAfterCommand { get; set; }
    public override string ToString()
    {
        var thrustStr = Thrust == 650 ? "BOOST" : Thrust == -1 ? "SHIELD" : Thrust.ToString();
        return $"{Destination.X} {Destination.Y} {thrustStr}";
    }
}

public class RunInfo
{
    public Dictionary<Pods, PodInfo> Pods = new Dictionary<Pods, PodInfo> { };
    public override string ToString()
    {
        var response = "";
        foreach (var bot in Pods)
        {
            response += $"Bot:{bot.Key}: Details:{bot.Value} \n";
        }
        return response;
    }
}

public enum Pods
{
    MyPod1,
    MyPod2,
    EnemyPod1,
    EnemyPod2
}

/// <summary>
/// Permanent instance of a pod
/// </summary>
public class PodInstance
{
    public int LapNumber { get; private set; }
    public int TimeOut { get; private set; } = 100;
    public int RankingInGame { get; set; }
    public int DistanceToNextCheckpoint { get; private set; }
    public int CheckPointsReached { get; private set; } = 1; //checkPointCount received is 1 based
    public GameInfo GameInfo { get; set; }

    private PodInfo _currentPodInfo = null;

    public PodInfo CurrentPodInfo
    {
        get => _currentPodInfo;
        set
        {
            PreviousPodInfo = _currentPodInfo;
            _currentPodInfo = value;

            CalculateDistanceToNextCheckpoint();
            TrackCheckPointTimeoutAndLap();
        }
    }

    public PodInfo PreviousPodInfo { get; private set; }

    private void TrackCheckPointTimeoutAndLap()
    {
        //If next check point changed, i must have hit the target
        if (PreviousPodInfo != null && PreviousPodInfo.NextCheckPointId != CurrentPodInfo.NextCheckPointId)
        {
            CheckPointsReached++;
            TimeOut = 99; //Reset timeout

            //when zero a lap has been completed
            if (CheckPointsReached % GameInfo.CheckpointCount != 0) return;

            //Console.Error.WriteLine($"Reached:{CheckPointsReached},Mod:{CheckPointsReached % GameInfo.CheckpointCount}");
            Console.Error.WriteLine($"Reached:{CheckPointsReached},Mod:{CheckPointsReached % GameInfo.CheckpointCount}");
            LapNumber++;
        }
        else
        {
            TimeOut--; //Running out of time to get to checkpoint
        }
    }


    private void CalculateDistanceToNextCheckpoint()
    {
        //TODO: this could reveal an important problem, i get the coordinate of the previous move compared to what the system
        //considers. For this reason i cannot check if pod over nextTarget because when it is over, the nextTarget changes
        //this is bad if i want to know if I am over the target because it could miss it by just checking the distance to target
        //it is like you have to estimate future position and then that is what the system considers
        //this is important to figure out collisions i think

        //Console.Error.WriteLine($"D:{CurrentPodInfo.PosCoordinates},Ch:{GameInfo.Checkpoints[CurrentPodInfo.NextCheckPointId]}");
        DistanceToNextCheckpoint = (int)Utilities.GetPodDistanceToCoordinates(
            CurrentPodInfo,
            GameInfo.Checkpoints[CurrentPodInfo.NextCheckPointId]);
    }

    public override string ToString()
    {
        return $"Timeout:{TimeOut},Rank:{RankingInGame},Lap:{LapNumber},Dist:{DistanceToNextCheckpoint},Cur:{CurrentPodInfo.PosCoordinates},Ch:{GameInfo.Checkpoints[CurrentPodInfo.NextCheckPointId]}";
    }
}

public static class Utilities
{
    public const int CheckPointRadius = 600;
    public static double ConvertDegreesToRadians(double degrees)
    {
        var radians = (Math.PI / 180) * degrees;
        return radians;
    }

    public static double ConvertRadsToDegrees(double rads)
    {
        return rads / (Math.PI / 180);
    }

    public static PodInfo CalculateFuturePosition(PodInfo pod, int proposedThrust, int proposedFacingAngleToAdd)
    {
        if (Math.Abs(proposedFacingAngleToAdd) > 18) throw new Exception($"Angle to add too large {proposedFacingAngleToAdd}");
        var a = pod.Angle + proposedFacingAngleToAdd;
        var angleAbsolute = a >= 0 ? a : 360 + a; //plus since a negative
        var angleRad = ConvertDegreesToRadians(angleAbsolute);
        var facingX = Math.Cos(angleRad) * proposedThrust;
        var facingY = Math.Sin(angleRad) * proposedThrust;

        var speedVectorX = pod.Vx + facingX;
        var speedVectorY = pod.Vy + facingY;

        var result = new PodInfo();
        result.X = pod.X + (int)Math.Round(speedVectorX);
        result.Y = pod.Y + (int)Math.Round(speedVectorY);
        result.Angle = angleAbsolute;
        result.Vx = (int)(speedVectorX * 0.85);
        result.Vy = (int)(speedVectorY * 0.85);
        result.NextCheckPointId = -1; //needs to be calculated, here we just calculate position and stuff
        return result;
    }

    public static double GetPodDistanceToCoordinates(PodInfo pod, Coordinates coords)
    {
        return GetDistanceBetweenCoords(pod.PosCoordinates, coords);
    }

    public static double GetDeltaBetweenPods(PodInfo pod1, PodInfo pod2)
    {
        var c1 = new Coordinates();
        c1.X = pod1.X;
        c1.Y = pod1.Y;

        var c2 = new Coordinates();
        c2.X = pod2.X;
        c2.Y = pod2.Y;

        return GetDistanceBetweenCoords(c1, c2);
    }

    public static double GetDistanceBetweenCoords(Coordinates c1, Coordinates c2)
    {
        var deltaX = c1.X - c2.X;
        var deltaY = c1.Y - c2.Y;

        return GetHypotenuseBetweenValues(deltaX, deltaY);
    }

    public static double GetHypotenuseBetweenValues(int x, int y)
    {
        return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
    }
}

class Player
{
    static void Main(string[] args)
    {
        //Fetch game input
        var gameInfo = GetGameInfo();

        //Instantiate instances for pods and pass the GameInfo
        var podInstances = new Dictionary<Pods, PodInstance>();
        foreach (Pods enumEntry in Enum.GetValues(typeof(Pods)))
        {
            podInstances.Add(enumEntry, new PodInstance { GameInfo = gameInfo });
        }

        while (true)
        {
            //Fetch info for run
            var runInfo = GetRunInfo();

            //Populate current pod info for each pod, this triggers populating most internal variables tracking its status
            foreach (var (key, value) in runInfo.Pods)
            {
                if (podInstances.ContainsKey(key))
                {
                    podInstances[key].CurrentPodInfo = value;
                }
                else
                {
                    var podInstance = new PodInstance { CurrentPodInfo = value };
                    podInstances.Add(key, podInstance);
                }
            }

            //Calculate the ranking of each pod
            var sortedRankList = podInstances
                .OrderByDescending(podInstanceEntry => podInstanceEntry.Value.CheckPointsReached) // sort by most checkpoints
                .ThenBy(podInstanceEntry => podInstanceEntry.Value.DistanceToNextCheckpoint)      // Then by smallest DistanceToNextCheckpoint
                .ToList();

            var rank = 1;
            foreach (var (rankKey, _) in sortedRankList)
            {
                podInstances[rankKey].RankingInGame = rank;
                rank++;
                Console.Error.WriteLine($"PodName:{rankKey},{podInstances[rankKey]}");
            }


            //win
            var pod1Destination = gameInfo.Checkpoints[runInfo.Pods[Pods.MyPod1].NextCheckPointId];
            var pod2Destination = gameInfo.Checkpoints[runInfo.Pods[Pods.MyPod2].NextCheckPointId];
            var cmd1 = new Command { Destination = pod1Destination, Thrust = 100, FuturePodAfterCommand = null };
            var cmd2 = new Command { Destination = pod2Destination, Thrust = 100, FuturePodAfterCommand = null };
            Console.WriteLine(cmd1);
            Console.WriteLine(cmd2);

        }
    }


    public static PodInfo GetPodInfo()
    {
        var result = new PodInfo();
        var podInfoArr = Console.ReadLine().Split(' ');
        result.X = int.Parse(podInfoArr[0]);
        result.Y = int.Parse(podInfoArr[1]);
        result.Vx = int.Parse(podInfoArr[2]);
        result.Vy = int.Parse(podInfoArr[3]);
        result.Angle = int.Parse(podInfoArr[4]);
        result.NextCheckPointId = int.Parse(podInfoArr[5]);
        return result;
    }

    public static RunInfo GetRunInfo()
    {
        var response = new RunInfo();

        //two my bots
        response.Pods.Add(Pods.MyPod1, GetPodInfo());
        response.Pods.Add(Pods.MyPod2, GetPodInfo());

        //two enemy bots
        response.Pods.Add(Pods.EnemyPod1, GetPodInfo());
        response.Pods.Add(Pods.EnemyPod2, GetPodInfo());

        return response;
    }

    public static GameInfo GetGameInfo()
    {
        var response = new GameInfo();
        response.Laps = int.Parse(Console.ReadLine());
        response.CheckpointCount = int.Parse(Console.ReadLine());
        for (var i = 0; i < response.CheckpointCount; i++)
        {
            var checkPoints = Console.ReadLine().Split(' ');
            var coordinates = new Coordinates();
            coordinates.X = int.Parse(checkPoints[0]);
            coordinates.Y = int.Parse(checkPoints[1]);
            response.Checkpoints.Add(coordinates);
        }
        return response;
    }
}