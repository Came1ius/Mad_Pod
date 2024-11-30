using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
public class PodInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public Coordinates PosCoordinates => new Coordinates() { X = X, Y = Y };
    public int Vx { get; set; }
    public int Vy { get; set; }
    public int Angle { get; set; }
    public int NextCheckPointId { get; set; }
    public int LapNumber { get; set; }
    public override string ToString()
    {
        return $"x:{X}, y:{Y}, vx:{Vx}, vy:{Vy}, angle:{Angle}, Dest:{NextCheckPointId}";
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
    public List<PodInfo> MyBots = new List<PodInfo> { };
    public List<PodInfo> EnemyBots = new List<PodInfo> { };
    public override string ToString()
    {
        var response = "";

        for (var bot = 0; bot < MyBots.Count; bot++)
        {
            response += $"MyBot{bot}: {MyBots[bot]} \n";
        }

        for (var bot = 0; bot < EnemyBots.Count; bot++)
        {
            response += $"EBot{bot}: {EnemyBots[bot]} \n";
        }
        return response;
    }
}

public class PodInstance
{
    GameInfo gameInfo;
    public bool _engagedInDefense = false;
    RunInfo PrevRunInfo = null;
    private RunInfo _runInfo = null;
    public RunInfo runInfo
    {
        get => _runInfo;
        set
        {
            PrevRunInfo = _runInfo;
            _runInfo = value;
        }
    }

    int podIndex = -1;


    public PodInstance(int podIndex, GameInfo game)
    {
        this.podIndex = podIndex;
        gameInfo = game;
    }

    private int EnemyToDefend()
    {
        var enemy1Pod = runInfo.EnemyBots[0];
        var enemy2Pod = runInfo.EnemyBots[1];
        var enemy1DistToTarget = GetDistanceToLocation(enemy1Pod, gameInfo.Checkpoints[enemy1Pod.NextCheckPointId]);
        var enemy2DistToTarget = GetDistanceToLocation(enemy2Pod, gameInfo.Checkpoints[enemy2Pod.NextCheckPointId]);

        if (enemy1Pod.LapNumber == enemy2Pod.LapNumber)
        {
            if (enemy1Pod.NextCheckPointId == enemy2Pod.NextCheckPointId)
            {
                return enemy1DistToTarget < enemy2DistToTarget ? 0 : 1;
            }

            //var nextIdValue1 = (enemy1Pod.NextCheckPointId + 1) % gameInfo.CheckpointCount;
            //var nextIdValue2 = (enemy2Pod.NextCheckPointId + 1) % gameInfo.CheckpointCount;
            //Console.Error.WriteLine($"next1 = {nextIdValue1}, next2 = {nextIdValue2} {enemy1Pod.NextCheckPointId }:{enemy2Pod.NextCheckPointId }");
            var checkpoint1 = enemy1Pod.NextCheckPointId == 0 ? 100 : enemy1Pod.NextCheckPointId;
            var checkpoint2 = enemy2Pod.NextCheckPointId;
            //Console.Error.WriteLine($"next1 = {checkpoint1}, next2 = {checkpoint2} {enemy1Pod.NextCheckPointId }:{enemy2Pod.NextCheckPointId }");
            if (checkpoint1 == 0) return 0;
            if (checkpoint2 == 0) return 1;
            return checkpoint1 > checkpoint2 ? 0 : 1;
        }

        var toDefend = enemy1Pod.LapNumber > enemy2Pod.LapNumber ? 0 : 1;
        //Console.Error.WriteLine($"e1Laps = {enemy1Pod.LapNumber}, e2Laps = {enemy2Pod.LapNumber},d: {toDefend}, ");
        return toDefend;
    }

    public string GetDefenseCommand2(bool boostAvailable, string friendCmd)
    {
        var friendIndex = (podIndex + 1) % 2;
        var thisPod = runInfo.MyBots[podIndex];
        var friend = runInfo.MyBots[friendIndex];
        var enemy1 = runInfo.EnemyBots[0];
        var enemy2 = runInfo.EnemyBots[1];
        var enemyPos1 = CalculateFuturePosition(enemy1, 400, 0, true);
        var enemyPos2 = CalculateFuturePosition(enemy2, 400, 0, true);
        var enemyPos1R = CalculateFuturePosition(enemy1, 200, 0, true);
        var enemyPos2R = CalculateFuturePosition(enemy2, 200, 0, true);
        var myPos = CalculateFuturePosition(thisPod, 200, 0, true);
        var distanceToE = (int)GetDeltaBetweenPods(myPos, enemyPos1R) + 1;
        var distanceToE2 = (int)GetDeltaBetweenPods(myPos, enemyPos2R) + 1;
        var distanceToFriend = (int)GetDeltaBetweenPods(myPos, friend) + 1;
        var closestEnemy = distanceToE < distanceToE2 ? 0 : 1;

        var enemy1Speed = (int)GetSpeedMagnitude(enemy1);
        var enemy2Speed = (int)GetSpeedMagnitude(enemy2);
        var mySpeed = (int)GetSpeedMagnitude(thisPod);
        var speedToDefend = 10;
        var distToDefend = 1250;
        if (distanceToE <= distToDefend && (enemy1Speed > speedToDefend || mySpeed > speedToDefend))
        {
            _engagedInDefense = true;
            // return $"{cmd.destination.x} {cmd.destination.x} SHIELD {distanceToE1} {distanceToE2}";
            return $"{enemyPos1.X} {enemyPos1.Y} SHIELD  v:{enemy1Speed}, {mySpeed}";
        }

        if (distanceToE2 <= distToDefend && (enemy2Speed > speedToDefend || mySpeed > speedToDefend))
        {
            _engagedInDefense = true;
            // return $"{cmd.destination.x} {cmd.destination.x} SHIELD {distanceToE1} {distanceToE2}";
            return $"{enemyPos2.X} {enemyPos2.Y} SHIELD  v2:{enemy2Speed}, {mySpeed}";
        }

        var distClose = 2500;
        var distCloseF = 900;
        var enemyToDefendIndex = EnemyToDefend();
        var enemyToDefend = enemyToDefendIndex == 0 ? enemyPos1 : enemyPos2;
        var chpId = (enemyToDefend.NextCheckPointId); //% gameInfo.checkpointCount;
        var checkPoint = gameInfo.Checkpoints[(chpId)];

        if (distanceToFriend < distCloseF)
        {
            var c = GetOptimizedCommand(thisPod.PosCoordinates, false);
            c.Thrust = -1;
            return GetOptimizedCommand(thisPod.PosCoordinates, false) + $" F";
        }

        //var enemyAttack = closestEnemy == 0 ? enemyPos1 : enemyPos2;
        var enemyAttack = enemyToDefend;
        var distanceToEdefend = (int)GetDeltaBetweenPods(myPos, enemyAttack) + 1;
        //if ( distanceToE < distClose )//|| distanceToE2 < distClose)
        if (distanceToEdefend < distClose)//|| distanceToE2 < distClose)
        {
            var ex = enemyAttack.X - (int)((enemyAttack.X - checkPoint.X) * 0.2);
            var ey = enemyAttack.Y - (int)((enemyAttack.Y - checkPoint.Y) * 0.2);
            var c = new Coordinates() { X = ex, Y = ey };
            _engagedInDefense = true;
            //return GetOptimizedCommand(enemyAttack.PosCoordinates, boostAvailable) + $" enemy{enemyToDefendIndex} CEF, {enemyAttack.PosCoordinates}";
            return GetOptimizedCommand(c, boostAvailable) + $" enemy{enemyToDefendIndex} CEF, {enemyAttack.PosCoordinates}";
        }
        _engagedInDefense = false;
        //return friendCmd + $"f:{distanceToFriend}";
        var checkPoint2 = gameInfo.Checkpoints[((chpId) % gameInfo.CheckpointCount)];


        //half way through path

        //var newX = enemyToDefend.X - (int)((enemyToDefend.X - checkPoint.X) / 1.75);
        //var newY = enemyToDefend.Y - (int)((enemyToDefend.Y - checkPoint.Y) / 1.75);
        var newX = enemyToDefend.X - (int)((enemyToDefend.X - checkPoint.X) * 0.8);
        var newY = enemyToDefend.Y - (int)((enemyToDefend.Y - checkPoint.Y) * 0.8);
        var coords = new Coordinates() { X = newX, Y = newY };
        var fChk = runInfo.MyBots[friendIndex].NextCheckPointId == 0 ? gameInfo.CheckpointCount : runInfo.MyBots[friendIndex].NextCheckPointId;
        //var coords = gameInfo.Checkpoints[0];
        //var coords = checkPoint;

        if (GetDistanceToLocation(enemyToDefend, coords) < GetDistanceToLocation(thisPod, coords))
        {

            if (chpId == 0 || (chpId + 1) % gameInfo.CheckpointCount == 0)
            {
                coords = gameInfo.Checkpoints[0];
            }
            else
            {
                var centroidX = 0;
                var centroidY = 0;
                foreach (var c in gameInfo.Checkpoints)
                {
                    centroidX += c.X;
                    centroidY += c.Y;
                }
                centroidX = centroidX / gameInfo.CheckpointCount;
                centroidY = centroidY / gameInfo.CheckpointCount;
                coords.X = centroidX;
                coords.Y = centroidY;
            }
        }

        if (runInfo.MyBots[friendIndex].NextCheckPointId == 0)
        {
            //coords = new Coordinates() {X = coords.X, Y = coords.Y - 1000} ;
        }


        var distanceToTarget = GetDistanceToLocation(thisPod, coords);
        var cmd = GetOptimizedCommand(coords, false);
        var speed = Math.Sqrt(Math.Pow(thisPod.Vx, 2) + Math.Pow(thisPod.Vy, 2));

        if (distanceToTarget < 1000 && thisPod.Vx + thisPod.Vx < 10)
        {
            cmd = GetOptimizedCommand(new Coordinates() { X = newX, Y = newY }, false);
            cmd.Thrust = 1;
        }

        else if (distanceToTarget / (speed + 1) < 7)
        {
            cmd.Thrust = 0;
            //Console.Error.WriteLine($"thr={cmd.Thrust},d{(int)distanceToTarget}, sp: {speed}");
        }
        else if (cmd.Thrust > 100 && cmd.Thrust != 650)
        {
            //cmd.Thrust = 100;
        }
        return cmd.ToString() + $" enemy{enemyToDefendIndex} MH {coords},chp{chpId}, th{cmd.Thrust}, d:{(int)distanceToTarget}";

        //return GetOptimizedCommand(coords, false) + $" enemy{enemyToDefendIndex} MH {coords},chp{chpId}";
        //return GetOptimizedCommand(enemy,false).ToString() + $" enemy{enemyToDefendIndex} MH {coords},chp{chpId}";
    }

    public double GetSpeedMagnitude(PodInfo pod)
    {
        return Math.Sqrt(Math.Pow(pod.Vx, 2) + Math.Pow(pod.Vy, 2));
    }

    public Command WouldPredictedMotionArriveAtTarget(int predictionNumbers, bool isBoostAvailable, PodInfo pod, Coordinates aimDestination, Coordinates actualDestination)
    {
        var singleBoostForPrediction = isBoostAvailable;
        var currentPod = pod;
        var predictionList = new List<Command>();
        var enemy1 = runInfo.EnemyBots[0];
        var enemy2 = runInfo.EnemyBots[1];
        for (var i = 0; i < predictionNumbers; i++)
        {
            //Console.Error.WriteLine($"--- calculating {i}th prediction");
            var prediction = GetOptimizedCommand(currentPod, aimDestination, singleBoostForPrediction);
            if (prediction.Thrust == 650) singleBoostForPrediction = false;
            predictionList.Add(prediction);
            var distanceToDestination = GetDistanceToLocation(prediction.FuturePodAfterCommand, actualDestination);
            currentPod = prediction.FuturePodAfterCommand;

            //evade
            var distanceToE1 = (int)GetDeltaBetweenPods(currentPod, enemy1) + 1;
            var distanceToE2 = (int)GetDeltaBetweenPods(currentPod, enemy2) + 1;
            var evadeDistance = 400;
            if ((distanceToE1 <= evadeDistance || distanceToE2 <= evadeDistance) && i > 0)
            {
                var mod = predictionList[0];
                mod.Thrust = 200;
                //mod.Destination = new Coordinates(){X = 7000, Y = 4000};
                //return mod;
            }

            if (distanceToDestination <= 600)
            {
                return predictionList[0];
            }
        }
        return null;
    }

    public bool IsCollisionPath(PodInfo podA, PodInfo podB)
    {
        //no collision sum of vectors
        var vA = Math.Sqrt(Math.Pow(podA.Vx, 2) + Math.Pow(podA.Vy, 2));
        var vB = Math.Sqrt(Math.Pow(podB.Vx, 2) + Math.Pow(podB.Vy, 2));
        var v = vA + vB;

        //collission
        var vx = podA.Vx + podB.Vx;
        var vy = podA.Vy + podB.Vy;
        var mag = Math.Sqrt(Math.Pow(vx, 2) + Math.Pow(vy, 2));
        var ratio = Math.Abs(mag - v) / mag;
        Console.Error.WriteLine($"mag:{(int)mag}, v:{(int)v}, ratio: {ratio}");
        return (ratio > 0.4);
        //return true;
    }
    public string GetCommand(bool isBoostAvailable)
    {
        isBoostAvailable = false; //boost only available for defense
        var thisPod = runInfo.MyBots[podIndex];
        var checkPoint = thisPod.NextCheckPointId;
        var nextCheckpointPosition = gameInfo.Checkpoints[checkPoint];

        //Get following checkpoint
        var followingCheckPointIndex = (checkPoint + 1) % gameInfo.CheckpointCount;
        var checkpointAhead = gameInfo.Checkpoints[followingCheckPointIndex];

        //prediction going to actual destination
        var cmdNext = WouldPredictedMotionArriveAtTarget(20, isBoostAvailable, thisPod, nextCheckpointPosition, nextCheckpointPosition);
        //var cmdNext = GetOptimizedCommand(nextCheckpointPosition,isBoostAvailable);

        if (cmdNext == null)
        {
            //var turnAngle = CalculateAnglefromCoordinates(nextCheckpointPosition, thisPod);
            //var modAngle = turnAngle > 0 ? 18 : -18;
            //slow down
            var thrust = 0;
            var slowerPod = CalculateFuturePosition(thisPod, thrust, 0, true);
            //var destination = new Coordinates(){x = slowerPod.x , y =  slowerPod.y};

            cmdNext = new Command() { Destination = nextCheckpointPosition, Thrust = thrust, FuturePodAfterCommand = slowerPod };
        }

        //next prediction
        var cmdAnticipating = WouldPredictedMotionArriveAtTarget(10, isBoostAvailable, thisPod, checkpointAhead, nextCheckpointPosition);

        //Console.Error.WriteLine($"Tot Predictions calc: {predictionList.Count()}, wouldReachTarget? {wouldReachTarget} ");

        var cmd = cmdAnticipating != null ? cmdAnticipating : cmdNext;

        //predict future position for enemy bots and friend
        var enemy1 = runInfo.EnemyBots[0];
        var enemy2 = runInfo.EnemyBots[1];
        var enemy1Pos = CalculateFuturePosition(enemy1, 200, 0, true);
        var enemy2Pos = CalculateFuturePosition(enemy2, 200, 0, true);

        var distanceToE1 = (int)GetDeltaBetweenPods(cmd.FuturePodAfterCommand, enemy1Pos) + 1;
        var distanceToE2 = (int)GetDeltaBetweenPods(cmd.FuturePodAfterCommand, enemy2Pos) + 1;

        var speedE1 = (int)GetSpeedMagnitude(enemy1Pos) + 1;
        var speedE2 = (int)GetSpeedMagnitude(enemy2Pos) + 1;
        var thisSpeed = (int)GetSpeedMagnitude(cmd.FuturePodAfterCommand) + 1;
        var speedThreshold = 650;
        var shieldCollisionE1 = speedE1 > speedThreshold || thisSpeed > speedThreshold;
        var shieldCollisionE2 = speedE2 > speedThreshold || thisSpeed > speedThreshold;
        var distToDefend = 650;
        if (distanceToE1 <= distToDefend && shieldCollisionE1 && IsCollisionPath(enemy1Pos, thisPod))
        {
            // return $"{cmd.destination.x} {cmd.destination.x} SHIELD {distanceToE1} {distanceToE2}";
            //return $"0 0 SHIELD {speedE1},{speedE2}:{thisSpeed} = SHIELD";
            return $"{cmd.Destination.X} {cmd.Destination.Y} SHIELD";
        }

        if (distanceToE2 <= distToDefend && shieldCollisionE2 && IsCollisionPath(enemy2Pos, thisPod))
        {
            return $"{cmd.Destination.X} {cmd.Destination.Y} SHIELD";
        }

        //if (distanceToE1 <= distToDefend+ (distToDefend*0.1) || distanceToE2 <= distToDefend+ (distToDefend*0.1))
        if (distanceToE1 <= distToDefend || distanceToE2 <= distToDefend)
        {
            //cmd.Destination.X += 1000;
            //cmd.Destination.Y += 1000;
            //cmd.Thrust = 200;

            return cmd.ToString() + $" {speedE1},{speedE2}:{thisSpeed}";
        }

        //if(cmd != cmdAnticipating) Console.Error.WriteLine(" Not anticipating");
        return cmd.ToString();// + " " +cmd.Destination; //+ $" {speedE1},{speedE2}:{thisSpeed}";

    }
    /*
    public bool IsTrajectoryCollision(PodInfo thisPod, PodInfo enemyPod)
    {

    }
    */
    public Command GetOptimizedCommand(Coordinates nextCheckpointPosition, bool isBoostAvailable)
    {
        return GetOptimizedCommand(runInfo.MyBots[podIndex], nextCheckpointPosition, isBoostAvailable);
    }

    public Command GetOptimizedCommand(PodInfo startingPodInfo, Coordinates nextCheckpointPosition, bool isBoostAvailable, bool isPrint = false)
    {
        //check all angles and thrust combinations, brute force it for now
        var shortestDistance = double.MaxValue;
        var shortestAngleDistance = 360.0;
        var bestThrust = -1;
        var bestAngle = -20;
        PodInfo bestFuturePod = null;
        var listOfThrusts = new List<int>();
        for (var thrust = 0; thrust <= 20; thrust++)
        {
            listOfThrusts.Add(thrust * 10);
        }
        if (isBoostAvailable) listOfThrusts.Add(650);
        for (var angle = -18; angle <= 18; angle += 2)
        {
            foreach (var thrust in listOfThrusts)
            {
                var futurePod = CalculateFuturePosition(startingPodInfo, thrust, angle, true);
                var angleDistance = CalculateAnglefromCoordinates(nextCheckpointPosition, futurePod);
                var distance = GetDistanceToLocation(futurePod, nextCheckpointPosition);

                var closerDist = distance < shortestDistance;
                var sameDistLessRotate = (distance == shortestDistance) && (angleDistance < shortestAngleDistance);

                //if(angleDistance < shortestAngleDistance)
                //Console.Error.WriteLine($"{distance},{angle},{thrust}");
                var toPrint = $"";
                if (closerDist || sameDistLessRotate)
                {
                    shortestDistance = distance;
                    bestThrust = thrust;
                    bestAngle = angle;
                    bestFuturePod = futurePod;
                    shortestAngleDistance = angleDistance;
                    if (angleDistance > shortestAngleDistance)
                    {
                        //bestThrust = 0;
                    }
                }
            }
        }
        //Console.Error.WriteLine($"Best:{(int)shortestDistance},{bestAngle},{bestThrust}");
        //calculate x y coors
        var idealAimPosition = CalculateCoordinatesFromAngle(startingPodInfo, bestAngle);

        var command = new Command() { Destination = idealAimPosition, Thrust = bestThrust, FuturePodAfterCommand = bestFuturePod };

        return command;
    }



    #region utilities
    public static bool IsPosBetweenBotAndTarget(PodInfo pod, Coordinates position, Coordinates target)
    {
        var deltaPosTargX = target.X - position.X;
        var deltaPosTargY = target.Y - position.Y;

        var isDeltaPosTargXPos = deltaPosTargX > 0;
        var isDeltaPosTargYPos = deltaPosTargY > 0;

        var deltaPodTargX = target.X - pod.X;
        var deltaPodTargY = target.Y - pod.Y;
        var isDeltaPodTargXPos = deltaPodTargX > 0;
        var isDeltaPodTargYPos = deltaPodTargY > 0;

        return Math.Abs(deltaPosTargX) < Math.Abs(deltaPodTargX) &&
                Math.Abs(deltaPosTargY) < Math.Abs(deltaPodTargY) &&
                isDeltaPosTargXPos == isDeltaPodTargXPos &&
                isDeltaPosTargYPos == isDeltaPodTargYPos;

    }

    public static Coordinates CalculateDestinationFromSpeedVector(PodInfo pod, int thrustMultiplier)
    {
        return new Coordinates() { X = (int)(pod.Vx * thrustMultiplier * 0.85) + pod.X, Y = (int)(pod.Vy * thrustMultiplier * 0.85) + pod.Y };
    }

    public static Coordinates CalculateCoordinatesFromAngle(PodInfo pod, int angle)
    {
        //assume hypotenuse of 100 to give it some room
        var hyp = 1500;
        var angleRad = ConvertDegreesToRadians(angle + pod.Angle);
        var destinationY = Math.Sin(angleRad) * hyp;
        var destinationX = Math.Cos(angleRad) * hyp;
        var result = new Coordinates();
        result.X = (int)destinationX + pod.X;
        result.Y = (int)destinationY + pod.Y;
        return result;
    }

    public static double CalculateAnglefromCoordinates(Coordinates coords, PodInfo pod)
    {
        var xDelta = coords.X - pod.X;
        var yDelta = coords.Y - pod.Y;

        //in Rad
        var ratio = Math.Abs(yDelta) / (Math.Abs(xDelta) * 1.0);
        var angle = Math.Atan(ratio);
        var angleDegrees = ConvertRadsToDegrees(angle);


        //Console.Error.WriteLine($"calcAngle {angle}, degAngle {angleDegrees}, y {yDelta}, x {xDelta}, ratio {ratio}");
        //find out quadrant
        var quadrant = 0;
        if (yDelta >= 0 && xDelta >= 0)
        {
            quadrant = 0;
        }
        else if (yDelta >= 0 && xDelta < 0)
        {
            quadrant = 1;
            angleDegrees = (90 - angleDegrees) + 90;
        }
        else if (yDelta < 0 && xDelta < 0)
        {
            quadrant = 2;
            angleDegrees += 180;
        }
        else
        {
            quadrant = 3;
            angleDegrees = (360 - angleDegrees);
        }

        var angleToTarg = Math.Abs(pod.Angle - angleDegrees);
        var finalAngle = angleToTarg <= 180 ? angleToTarg : 360 - angleToTarg;


        //Console.Error.WriteLine($"point2pointAngle {angleDegrees}, finalAngle {finalAngle} quad {quadrant} faceAngle {pod.angle}");
        return finalAngle;

    }

    public static double CalculateAngleFaceVsSpeed(PodInfo pod)
    {
        //in Rad
        var ratio = Math.Abs(pod.Vy) / (Math.Abs(pod.Vx) * 1.0);
        var angle = Math.Atan(ratio);
        var angleDegrees = ConvertRadsToDegrees(angle);

        var yDelta = pod.Vy;
        var xDelta = pod.Vx;

        //Console.Error.WriteLine($"calcAngle {angle}, degAngle {angleDegrees}, y {yDelta}, x {xDelta}, ratio {ratio}");
        //find out quadrant
        var quadrant = 0;
        if (yDelta >= 0 && xDelta >= 0)
        {
            quadrant = 0;
        }
        else if (yDelta >= 0 && xDelta < 0)
        {
            quadrant = 1;
            angleDegrees = (90 - angleDegrees) + 90;
        }
        else if (yDelta < 0 && xDelta < 0)
        {
            quadrant = 2;
            angleDegrees += 180;
        }
        else
        {
            quadrant = 3;
            angleDegrees = (360 - angleDegrees);
        }

        var angleToTarg = Math.Abs(pod.Angle - angleDegrees);
        var finalAngle = angleToTarg <= 180 ? angleToTarg : 360 - angleToTarg;


        //Console.Error.WriteLine($"point2pointAngle {angleDegrees}, finalAngle {finalAngle} quad {quadrant} faceAngle {pod.angle}");
        return finalAngle;

    }

    public static double ConvertDegreesToRadians(double degrees)
    {
        double radians = (Math.PI / 180) * degrees;
        return (radians);
    }

    public static double ConvertRadsToDegrees(double rads)
    {
        return rads / (Math.PI / 180);
    }

    public static PodInfo CalculateFuturePosition(PodInfo pod, int proposedThrust, int proposedFacingAngleToAdd, bool print)
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
        result.NextCheckPointId = pod.NextCheckPointId;
        return result;
    }


    public static double GetDistanceToLocation(PodInfo pod, Coordinates coords)
    {
        var c1 = new Coordinates();
        c1.X = pod.X;
        c1.Y = pod.Y;

        return GetDistanceBetweenCoords(c1, coords);
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

        return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
    }
    #endregion
}

class Player
{

    static void Main(string[] args)
    {
        //Fetch game input
        var GameInfo = GetGameInfo();
        PodInstance myPod0 = new PodInstance(0, GameInfo);
        PodInstance myPod1 = new PodInstance(1, GameInfo);

        var isBoostAvailable0 = true;
        var isBoostAvailable1 = true;
        var iter = 0;


        var lastCheckPointEnemy0 = -1;
        var lastCheckPointF0 = -1;
        var lastCheckPointF1 = -1;
        var lapsE0 = 0;
        var lapsE1 = 0;
        var lapsF0 = 0;
        var lapsF1 = 0;
        var lastCheckPointEnemy1 = -1;
        var switchRole = false;
        while (true)
        {
            iter++;

            if (iter > 35)
            {
                isBoostAvailable0 = true;
                isBoostAvailable1 = true;
            }

            if (iter == 110)
            {
                //isBoostAvailable0 = true;
            }

            if (iter == 100)
            {
                //isBoostAvailable1 = true;
            }
            //Fetch info
            var RunInfo = GetRunInfo();

            var n0 = RunInfo.EnemyBots[0].NextCheckPointId;
            var n1 = RunInfo.EnemyBots[1].NextCheckPointId;
            var f0 = RunInfo.MyBots[0].NextCheckPointId;
            var f1 = RunInfo.MyBots[1].NextCheckPointId;

            if (n0 != lastCheckPointEnemy0 && n0 == 1)
            {
                ;
                //Console.Error.WriteLine($"mod in if{n0 % GameInfo.checkpointCount}");
                lapsE0++;
            }

            if (n1 != lastCheckPointEnemy1 && n1 == 1)
            {
                ;
                //Console.Error.WriteLine($"mod in if {n1},{n1 % GameInfo.checkpointCount}");
                lapsE1++;

            }

            if (f0 != lastCheckPointF0 && f0 == 1)
            {
                ;
                //Console.Error.WriteLine($"mod in if {n1},{n1 % GameInfo.checkpointCount}");
                lapsF0++;

            }

            if (f1 != lastCheckPointF1 && f1 == 1)
            {
                ;
                //Console.Error.WriteLine($"mod in if {n1},{n1 % GameInfo.checkpointCount}");
                lapsF1++;

            }
            RunInfo.EnemyBots[1].LapNumber = lapsE1;
            RunInfo.EnemyBots[0].LapNumber = lapsE0;
            RunInfo.MyBots[0].LapNumber = lapsF0;
            RunInfo.MyBots[1].LapNumber = lapsF1;
            lastCheckPointEnemy0 = n0;
            lastCheckPointEnemy1 = n1;
            lastCheckPointF0 = f0;
            lastCheckPointF1 = f1;

            //Console.Error.WriteLine($"nextCheckPoint e1 {checkPointHistoryEnemy0[checkPointHistoryEnemy0.Count()-1]} e2 {checkPointHistoryEnemy1[checkPointHistoryEnemy1.Count()-1]}");
            //Update bots
            myPod0.runInfo = RunInfo;
            myPod1.runInfo = RunInfo;

            if (iter == 1)
            {
                var coors0 = GameInfo.Checkpoints[RunInfo.MyBots[0].NextCheckPointId];
                var coors1 = GameInfo.Checkpoints[RunInfo.MyBots[1].NextCheckPointId];
                Console.WriteLine($"{coors0.X} {coors0.Y} 200");
                Console.WriteLine($"{coors1.X} {coors1.Y} 200");
                continue;
            }

            //win
            var cmd0 = myPod0.GetCommand(isBoostAvailable0);
            var cmd1 = myPod1.GetCommand(isBoostAvailable1);
            //var cmd0 = myPod0.GetDefenseCommand2(isBoostAvailable1, null);
            //var cmd1 = myPod1.GetDefenseCommand2(isBoostAvailable1, cmd0);

            /*
                        //if(iter % 75 == 0)  myPod1._engagedInDefense) switchRole = !switchRole;
                        if(myPod0._engagedInDefense && !myPod1._engagedInDefense){
                            cmd1 = myPod1.GetCommand(isBoostAvailable1);
                        }else if(myPod1._engagedInDefense && !myPod0._engagedInDefense){
                            cmd0 = myPod0.GetCommand(isBoostAvailable1);
                        }else if(myPod0._engagedInDefense && myPod1._engagedInDefense){
                            if(RunInfo.MyBots[0].NextCheckPointId > RunInfo.MyBots[1].NextCheckPointId){
                                cmd0 = myPod0.GetCommand(isBoostAvailable0);
                            }else{
                                cmd1 = myPod1.GetCommand(isBoostAvailable1);
                            }
                        }
            */

            /*
                        if(lapsF0 > lapsF1 && 
                            lapsF0 > lapsE0 &&
                            lapsF0 > lapsE1 ){
                                cmd0 = myPod0.GetCommand(isBoostAvailable0);
                            }

                        if(lapsF1 > lapsF0 && 
                            lapsF1 > lapsE0 &&
                            lapsF1 > lapsE1 ){
                                cmd1 = myPod1.GetCommand(isBoostAvailable0);
                            }
                            */

            /*
                        if(lapsF1 > lapsE0 && lapsF1 > lapsE1 &&
                        lapsF0 > lapsE0 && lapsF0 > lapsE1){
                            cmd1 = myPod1.GetCommand(isBoostAvailable0);
                            cmd0 = myPod0.GetCommand(isBoostAvailable0);
                        }
            */
            //if(iter % 50 == 0 && !switchRole) switchRole = !switchRole;
            if ((lapsE0 > 0 || lapsE1 > 0) && !switchRole) switchRole = !switchRole;
            if (switchRole)
            {

                if (lapsF0 < lapsF1)
                {
                    cmd0 = myPod0.GetDefenseCommand2(isBoostAvailable1, null);
                }
                else if (lapsF1 < lapsF0)
                {
                    cmd1 = myPod1.GetDefenseCommand2(isBoostAvailable1, null);
                }
                else
                {
                    var c0 = RunInfo.MyBots[0].NextCheckPointId == 0 ? 10 : RunInfo.MyBots[0].NextCheckPointId;
                    var c1 = RunInfo.MyBots[1].NextCheckPointId == 0 ? 10 : RunInfo.MyBots[1].NextCheckPointId;
                    if (c0 < c1)
                    {
                        cmd0 = myPod0.GetDefenseCommand2(isBoostAvailable0, null);
                    }
                    else
                    {
                        cmd1 = myPod1.GetDefenseCommand2(isBoostAvailable1, null);
                    }
                }

                /*
                if(lapsF1 > lapsF0 && 
                    lapsF1 > lapsE0 &&
                    lapsF1 > lapsE1 ){
                        cmd1 = myPod1.GetCommand(isBoostAvailable0);
                    }

                    if(RunInfo.MyBots[0].NextCheckPointId > RunInfo.MyBots[1].NextCheckPointId){
                    cmd0 = myPod0.GetDefenseCommand2(isBoostAvailable1, null);
                    }else{
                        cmd1 = myPod1.GetDefenseCommand2(isBoostAvailable1, null);
                    }
                 */
                //cmd0 = myPod0.GetCommand(isBoostAvailable0);
                //cmd1 = myPod1.GetCommand(isBoostAvailable1);

                //cmd0 = myPod0.GetDefenseCommand2(isBoostAvailable1, null);
                //cmd1 = myPod0.GetDefenseCommand2(isBoostAvailable1, null);
            }


            //var cmd1 = cmd0;

            if (isBoostAvailable0) isBoostAvailable0 = !cmd0.Contains("BOOST");
            if (isBoostAvailable1) isBoostAvailable1 = !cmd1.Contains("BOOST");

            //Console.Error.WriteLine($"#1 {RunInfo.MyBots[0]}: cmd {cmd0} ");
            //Console.Error.WriteLine($"#2 {RunInfo.MyBots[1]}: cmd {cmd1}");

            Console.WriteLine(cmd0);
            Console.WriteLine(cmd1);

        }
    }

    public static void TrackPodPos(RunInfo run, GameInfo game)
    {

    }
    /*
        public static int CalculatePower(PodInfo info){
                //calculate speeds
                var ratio = Math.Abs(nextCheckpointAngle)/180.0;
                var pwrOut = (ratio * 100);
                var pwrPre = Math.Round(100 - pwrOut);
                //var pwr = pwrPre > 80 ? 100 :
                //          pwrPre < 50 ? 0 : pwrPre;
                var pwr = pwrPre > 70 ? 100 : pwrPre;
                //var pwr = pwrPre;

        }
       */

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
        //two my bots, could make a loop but lazy
        response.MyBots.Add(GetPodInfo());
        response.MyBots.Add(GetPodInfo());

        //two enemy bots, could make a loop but lazy
        response.EnemyBots.Add(GetPodInfo());
        response.EnemyBots.Add(GetPodInfo());

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

    public static bool IsOpponentClose(int x, int y, int oX, int oY)
    {
        var xPow = Math.Pow(Math.Abs(oX - x), 2);
        var yPow = Math.Pow(Math.Abs(oY - y), 2);
        var distance = Math.Sqrt(xPow + yPow);
        Console.Error.WriteLine($"Distance:{distance}");
        return distance < 600;
    }

    public static bool IsOpponentInMyWay(int x, int y, int oX, int oY, int chkX, int chkY)
    {
        return Math.Abs(oX - chkX) < Math.Abs(x - chkX) &&
                Math.Abs(oY - chkY) < Math.Abs(y - chkY);
    }


}