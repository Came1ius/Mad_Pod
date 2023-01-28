using System;
using System.Collections.Generic;

namespace TestGround
{
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
    //testing branch
    public class PodInfo
    {
        public int x { get; set; }
        public int y { get; set; }
        public int vx { get; set; }
        public int vy { get; set; }
        public int angle { get; set; }
        public int nextCheckPointId { get; set; }
        public int lapNumber { get; set; }
        public override string ToString()
        {
            return $"x:{x}, y:{y}, vx:{vx}, vy:{vy}, angle:{angle}, Dest:{nextCheckPointId}";
        }

    }

    public class GameInfo
    {
        public int laps { get; set; }
        public int checkpointCount { get; set; }
        public List<Coordinates> Checkpoints = new List<Coordinates>();

    }

    public class Coordinates
    {
        public int x { get; set; }
        public int y { get; set; }
        public override string ToString()
        {
            return $"x:{x}, y:{y}";
        }
    }

    public class Command
    {
        public Coordinates destination { get; set; }
        public int thrust { get; set; }
        public PodInfo futurePodAfterCommand { get; set; }
        public override string ToString()
        {
            var thrustStr = thrust == 650 ? "BOOST" : thrust.ToString();
            return $"{destination.x} {destination.y} {thrustStr}";
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
            var enemy1DistToTarget = GetDistanceToLocation(enemy1Pod, gameInfo.Checkpoints[enemy1Pod.nextCheckPointId]);
            var enemy2DistToTarget = GetDistanceToLocation(enemy2Pod, gameInfo.Checkpoints[enemy2Pod.nextCheckPointId]);

            if (enemy1Pod.lapNumber == enemy2Pod.lapNumber)
            {
                if (enemy1Pod.nextCheckPointId == enemy2Pod.nextCheckPointId)
                {
                    return enemy1DistToTarget < enemy2DistToTarget ? 0 : 1;
                }

                var nextIdValue1 = (enemy1Pod.nextCheckPointId + 1) % gameInfo.checkpointCount;
                var nextIdValue2 = (enemy2Pod.nextCheckPointId + 1) % gameInfo.checkpointCount;
                Console.Error.WriteLine($"next1 = {nextIdValue1}, next2 = {nextIdValue2} {enemy1Pod.nextCheckPointId }:{enemy2Pod.nextCheckPointId }");
                if (enemy2Pod.nextCheckPointId == nextIdValue1)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            var toDefend = enemy1Pod.lapNumber > enemy2Pod.lapNumber ? 0 : 1;
            Console.Error.WriteLine($"e1Laps = {enemy1Pod.lapNumber}, e2Laps = {enemy2Pod.lapNumber},d: {toDefend}, ");
            return toDefend;
        }

        public string GetDefenseCommand2(bool boostAvailable, string friendCmd)
        {
            var friendIndex = (podIndex + 1) % 2;
            var thisPod = runInfo.MyBots[podIndex];
            var friend = runInfo.MyBots[friendIndex];
            var enemy1 = runInfo.EnemyBots[0];
            var enemy2 = runInfo.EnemyBots[1];
            var enemyPos1 = CalculateFuturePosition(enemy1, 800, 0, true);
            var enemyPos2 = CalculateFuturePosition(enemy2, 800, 0, true);
            var myPos = CalculateFuturePosition(thisPod, 200, 0, true);
            var distanceToE = (int)GetDeltaBetweenPods(myPos, enemyPos1) + 1;
            var distanceToE2 = (int)GetDeltaBetweenPods(myPos, enemyPos2) + 1;
            var distanceToFriend = (int)GetDeltaBetweenPods(myPos, friend) + 1;
            var closestEnemy = distanceToE < distanceToE2 ? 0 : 1;

            var enemy1Speed = (int)GetSpeedMagnitude(enemy1);
            var enemy2Speed = (int)GetSpeedMagnitude(enemy2);
            var mySpeed = (int)GetSpeedMagnitude(thisPod);
            var speedToDefend = 500;
            if (distanceToE <= 900 && (enemy1Speed > speedToDefend || mySpeed > speedToDefend))
            {
                // return $"{cmd.destination.x} {cmd.destination.x} SHIELD {distanceToE1} {distanceToE2}";
                return $"{enemyPos1.x} {enemyPos1.y} SHIELD  v:{enemy1Speed}, {enemy2Speed}, {mySpeed}";
            }

            if (distanceToE2 <= 900 && (enemy1Speed > speedToDefend || mySpeed > speedToDefend))
            {
                // return $"{cmd.destination.x} {cmd.destination.x} SHIELD {distanceToE1} {distanceToE2}";
                return $"{enemyPos2.x} {enemyPos2.y} SHIELD  v:{enemy1Speed}, {enemy2Speed}, {mySpeed}";
            }

            var dist = 3000;
            var enemyToDefendIndex = EnemyToDefend();
            if (distanceToFriend < dist || distanceToE < dist || distanceToE2 < dist)
            {
                var enemyCoords = new Coordinates() { x = runInfo.EnemyBots[closestEnemy].x, y = runInfo.EnemyBots[closestEnemy].y };
                return GetOptimizedCommand(enemyCoords, boostAvailable).ToString() + $" enemy{enemyToDefendIndex}";
            }
            //return friendCmd + $"f:{distanceToFriend}";
            //var checkPoint = gameInfo.Checkpoints[((enemy1.nextCheckPointId + 2) % gameInfo.checkpointCount)];
            //var enemy = new Coordinates(){x = enemyPos1.x, y = enemyPos1.y };
            var enemyToDefend = enemyToDefendIndex == 0 ? enemyPos1 : enemyPos2;
            var enemy = new Coordinates() { x = enemyToDefend.x, y = enemyToDefend.y };

            //half way through path
            var checkPoint = gameInfo.Checkpoints[((enemyToDefend.nextCheckPointId + 2) % gameInfo.checkpointCount)];
            var newX = (int)(3 * (enemyToDefend.x - checkPoint.x) / 4) + enemyToDefend.x;
            var newY = (int)(3 * (enemyToDefend.y - checkPoint.y) / 4) + enemyToDefend.y;
            var coords = new Coordinates() { x = newX, y = newY };
            //var coords = gameInfo.Checkpoints[0];


            //return GetOptimizedCommand(coords,false).ToString() + $" enemy{enemyToDefendIndex} v:{enemy1Speed}, {enemy2Speed}, {mySpeed}";
            return GetOptimizedCommand(enemy, false).ToString() + $" enemy{enemyToDefendIndex}";
        }

        public string GetDefenseCommand(bool boostAvailable)
        {
            var thisPod = runInfo.MyBots[podIndex];
            var friendIndex = (podIndex + 1) % 2;
            var indexEnemyBotAhead = EnemyToDefend();

            var enemiesDestination = new List<Coordinates>();
            for (var e = 0; e < runInfo.EnemyBots.Count; e++)
            {
                //ONLY WORK ON ENEMY AHEAD
                if (e != indexEnemyBotAhead) continue;

                var enemy = runInfo.EnemyBots[e];
                var friend = runInfo.MyBots[friendIndex];
                //var destination = CalculateDestinationFromSpeedVector(enemy, 10);
                //var destinationFriend = CalculateDestinationFromSpeedVector(friend, 10);

                var enemyPos = CalculateFuturePosition(enemy, 200, 0, true);
                var myPos = CalculateFuturePosition(thisPod, 200, 0, true);
                var distanceToE = (int)GetDeltaBetweenPods(myPos, enemyPos) + 1;

                var nextCheckP = gameInfo.Checkpoints[enemy.nextCheckPointId];
                var nextNextCheckP = gameInfo.Checkpoints[((enemy.nextCheckPointId + 1) % gameInfo.checkpointCount)];

                var meDistanceToCheckpoint = GetDistanceToLocation(thisPod, nextCheckP);
                var enemyDistanceToCheckpoint = GetDistanceToLocation(enemy, nextCheckP);

                if ((distanceToE <= 800))
                {
                    // return $"{cmd.destination.x} {cmd.destination.x} SHIELD {distanceToE1} {distanceToE2}";
                    return $"{enemyPos.x} {enemyPos.y} SHIELD";
                }

                if (enemyDistanceToCheckpoint < meDistanceToCheckpoint)
                {
                    //not gonna catch him, move on
                    return GetOptimizedCommand(nextNextCheckP, boostAvailable).ToString();
                }

                //i should be between checkpoint and enemy
                var enemyCoords = new Coordinates() { x = enemyPos.x, y = enemyPos.y };
                return GetOptimizedCommand(enemyCoords, boostAvailable).ToString();




                //var cmd = GetOptimizedCommand(destination,boostAvailable);

                //var deltaAngle = CalculateAngleFaceVsSpeed(enemy);
                //if(deltaAngle > 40 ){
                //return $"{destination.x} {destination.y} SHIELD";
                //    cmd = GetOptimizedCommand(nextCheckP,boostAvailable);  
                //} 


                //var x = enemy.x + ((nextCheckP.x - destination.x) / 2.0);
                //var y = enemy.y + ((nextCheckP.y - destination.y) / 2.0);
                //var destination2 = new Coordinates() { x = (int)x, y = (int)y };


                var cmd = GetCommand(boostAvailable);


                //check if enemy moving towards you
                //var isEnemyCloser = GetDistanceToLocation(thisPod, destination) < GetDeltaBetweenPods(thisPod, enemy);
                var isEnemyCollision = GetDeltaBetweenPods(thisPod, enemy) <= 1200;//800;


                //attack if dealing with enemy ahead
                return cmd.ToString();
            }
            //Standby

            return $"{thisPod.x + thisPod.vx} {thisPod.y + thisPod.vy} 30";

            //if(boost){
            //    return $"{cmd.destination.x} {cmd.destination.y} BOOST";
            //}
        }

        public double GetSpeedMagnitude(PodInfo pod)
        {
            return Math.Sqrt(Math.Pow(pod.vx, 2) + Math.Pow(pod.vy, 2));
        }

        public Command WouldPredictedMotionArriveAtTarget(int predictionNumbers, bool isBoostAvailable, PodInfo pod, Coordinates aimDestination, Coordinates actualDestination)
        {
            var singleBoostForPrediction = isBoostAvailable;
            var currentPod = pod;
            var predictionList = new List<Command>();
            for (var i = 0; i < predictionNumbers; i++)
            {
                //Console.Error.WriteLine($"--- calculating {i}th prediction");
                var prediction = GetOptimizedCommand(currentPod, aimDestination, singleBoostForPrediction);
                if (prediction.thrust == 650) singleBoostForPrediction = false;
                predictionList.Add(prediction);
                var distanceToDestination = GetDistanceToLocation(prediction.futurePodAfterCommand, actualDestination);
                currentPod = prediction.futurePodAfterCommand;
                if (distanceToDestination <= 600)
                {
                    return predictionList[0];
                }
            }
            return null;
        }

        public string GetCommand(bool isBoostAvailable)
        {
            var thisPod = runInfo.MyBots[podIndex];
            var checkPoint = thisPod.nextCheckPointId;
            var nextCheckpointPosition = gameInfo.Checkpoints[checkPoint];

            //Get following checkpoint
            var followingCheckPointIndex = (checkPoint + 1) % gameInfo.checkpointCount;
            var checkpointAhead = gameInfo.Checkpoints[followingCheckPointIndex];

            //prediction going to actual destination
            var cmdNext = WouldPredictedMotionArriveAtTarget(20, isBoostAvailable, thisPod, nextCheckpointPosition, nextCheckpointPosition);

            if (cmdNext == null)
            {
                //var turnAngle = CalculateAnglefromCoordinates(nextCheckpointPosition, thisPod);
                //var modAngle = turnAngle > 0 ? 18 : -18;
                //slow down
                var thrust = 10;
                var slowerPod = CalculateFuturePosition(thisPod, thrust, 0, true);
                //var destination = new Coordinates(){x = slowerPod.x , y =  slowerPod.y};

                cmdNext = new Command() { destination = nextCheckpointPosition, thrust = thrust, futurePodAfterCommand = slowerPod };
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

            var distanceToE1 = (int)GetDeltaBetweenPods(cmd.futurePodAfterCommand, enemy1Pos) + 1;
            var distanceToE2 = (int)GetDeltaBetweenPods(cmd.futurePodAfterCommand, enemy2Pos) + 1;

            var speedE1 = (int)GetSpeedMagnitude(enemy1Pos) + 1;
            var speedE2 = (int)GetSpeedMagnitude(enemy2Pos) + 1;
            var thisSpeed = (int)GetSpeedMagnitude(cmd.futurePodAfterCommand) + 1;
            var speedThreshold = 500;
            var dontShieldCollisionE1 = speedE1 < speedThreshold && thisSpeed < speedThreshold;
            var dontShieldCollisionE2 = speedE2 < speedThreshold && thisSpeed < speedThreshold;
            if ((distanceToE1 <= 800 && !dontShieldCollisionE1) || (distanceToE2 <= 800 && !dontShieldCollisionE2))
            {
                // return $"{cmd.destination.x} {cmd.destination.x} SHIELD {distanceToE1} {distanceToE2}";
                //return $"0 0 SHIELD {speedE1},{speedE2}:{thisSpeed} = SHIELD";
                return $"{cmd.destination.x} {cmd.destination.y} SHIELD";
            }
            return cmd.ToString(); //+ $" {speedE1},{speedE2}:{thisSpeed}";

        }

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
            for (var angle = -18; angle <= 18; angle += 18)
            {
                foreach (var thrust in listOfThrusts)
                {
                    var futurePod = CalculateFuturePosition(startingPodInfo, thrust, angle, true);
                    var angleDistance = CalculateAnglefromCoordinates(nextCheckpointPosition, futurePod);
                    var distance = GetDistanceToLocation(futurePod, nextCheckpointPosition);

                    var closerDist = distance < shortestDistance;
                    var sameDistLessRotate = (distance == shortestDistance) && (angleDistance < shortestAngleDistance);

                    var toPrint = $"";
                    if (closerDist || sameDistLessRotate)
                    {
                        shortestDistance = distance;
                        bestThrust = thrust;
                        bestAngle = angle;
                        bestFuturePod = futurePod;
                        shortestAngleDistance = angleDistance;
                    }
                }
            }

            //calculate x y coors
            var idealAimPosition = CalculateCoordinatesFromAngle(startingPodInfo, bestAngle);

            var command = new Command() { destination = idealAimPosition, thrust = bestThrust, futurePodAfterCommand = bestFuturePod };
            return command;
        }



        #region utilities
        public static bool IsPosBetweenBotAndTarget(PodInfo pod, Coordinates position, Coordinates target)
        {
            var deltaPosTargX = target.x - position.x;
            var deltaPosTargY = target.y - position.y;

            var isDeltaPosTargXPos = deltaPosTargX > 0;
            var isDeltaPosTargYPos = deltaPosTargY > 0;

            var deltaPodTargX = target.x - pod.x;
            var deltaPodTargY = target.x - pod.y;
            var isDeltaPodTargXPos = deltaPodTargX > 0;
            var isDeltaPodTargYPos = deltaPodTargY > 0;

            return Math.Abs(deltaPosTargX) < Math.Abs(deltaPodTargX) &&
                    Math.Abs(deltaPosTargY) < Math.Abs(deltaPodTargY) &&
                    isDeltaPosTargXPos == isDeltaPodTargXPos &&
                    isDeltaPosTargYPos == isDeltaPodTargYPos;

        }

        public static Coordinates CalculateDestinationFromSpeedVector(PodInfo pod, int thrustMultiplier)
        {
            return new Coordinates() { x = (int)(pod.vx * thrustMultiplier * 0.85) + pod.x, y = (int)(pod.vy * thrustMultiplier * 0.85) + pod.y };
        }

        public static Coordinates CalculateCoordinatesFromAngle(PodInfo pod, int angle)
        {
            //assume hypotenuse of 100 to give it some room
            var hyp = 1500;
            var angleRad = ConvertDegreesToRadians(angle + pod.angle);
            var destinationY = Math.Sin(angleRad) * hyp;
            var destinationX = Math.Cos(angleRad) * hyp;
            var result = new Coordinates();
            result.x = (int)destinationX + pod.x;
            result.y = (int)destinationY + pod.y;
            return result;
        }

        public static double CalculateAnglefromCoordinates(Coordinates coords, PodInfo pod)
        {
            var xDelta = coords.x - pod.x;
            var yDelta = coords.y - pod.y;

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

            var angleToTarg = Math.Abs(pod.angle - angleDegrees);
            var finalAngle = angleToTarg <= 180 ? angleToTarg : 360 - angleToTarg;


            //Console.Error.WriteLine($"point2pointAngle {angleDegrees}, finalAngle {finalAngle} quad {quadrant} faceAngle {pod.angle}");
            return finalAngle;

        }

        public static double CalculateAngleFaceVsSpeed(PodInfo pod)
        {
            //in Rad
            var ratio = Math.Abs(pod.vy) / (Math.Abs(pod.vx) * 1.0);
            var angle = Math.Atan(ratio);
            var angleDegrees = ConvertRadsToDegrees(angle);

            var yDelta = pod.vy;
            var xDelta = pod.vx;

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

            var angleToTarg = Math.Abs(pod.angle - angleDegrees);
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
            var a = pod.angle + proposedFacingAngleToAdd;
            var angleAbsolute = a >= 0 ? a : 360 + a; //plus since a negative
            var angleRad = ConvertDegreesToRadians(angleAbsolute);
            var facingX = Math.Cos(angleRad) * proposedThrust;
            var facingY = Math.Sin(angleRad) * proposedThrust;

            var speedVectorX = pod.vx + facingX;
            var speedVectorY = pod.vy + facingY;

            var result = new PodInfo();
            result.x = pod.x + (int)Math.Round(speedVectorX);
            result.y = pod.y + (int)Math.Round(speedVectorY);
            result.angle = angleAbsolute;
            result.vx = (int)(speedVectorX * 0.85);
            result.vy = (int)(speedVectorY * 0.85);
            return result;
        }


        public static double GetDistanceToLocation(PodInfo pod, Coordinates coords)
        {
            var c1 = new Coordinates();
            c1.x = pod.x;
            c1.y = pod.y;

            return GetDistanceBetweenCoords(c1, coords);
        }

        public static double GetDeltaBetweenPods(PodInfo pod1, PodInfo pod2)
        {
            var c1 = new Coordinates();
            c1.x = pod1.x;
            c1.y = pod1.y;

            var c2 = new Coordinates();
            c2.x = pod2.x;
            c2.y = pod2.y;

            return GetDistanceBetweenCoords(c1, c2);
        }

        public static double GetDistanceBetweenCoords(Coordinates c1, Coordinates c2)
        {
            var deltaX = c1.x - c2.x;
            var deltaY = c1.y - c2.y;

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
            var lapsE0 = 0;
            var lapsE1 = 0;
            var lastCheckPointEnemy1 = -1;
            while (true)
            {
                iter++;

                if (iter < 100)
                {
                    isBoostAvailable0 = false;
                    isBoostAvailable1 = false;
                }

                if (iter == 100)
                {
                    //isBoostAvailable0 = true;
                }

                if (iter == 120)
                {
                    isBoostAvailable1 = true;
                }
                //Fetch info
                var RunInfo = GetRunInfo();

                var n0 = RunInfo.EnemyBots[0].nextCheckPointId;
                var n1 = RunInfo.EnemyBots[1].nextCheckPointId;

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

                RunInfo.EnemyBots[1].lapNumber = lapsE1;
                RunInfo.EnemyBots[0].lapNumber = lapsE0;
                lastCheckPointEnemy0 = n0;
                lastCheckPointEnemy1 = n1;

                //Console.Error.WriteLine($"nextCheckPoint e1 {checkPointHistoryEnemy0[checkPointHistoryEnemy0.Count()-1]} e2 {checkPointHistoryEnemy1[checkPointHistoryEnemy1.Count()-1]}");
                //Update bots
                myPod0.runInfo = RunInfo;
                myPod1.runInfo = RunInfo;

                if (iter == 1)
                {
                    var coors0 = GameInfo.Checkpoints[RunInfo.MyBots[0].nextCheckPointId];
                    var coors1 = GameInfo.Checkpoints[RunInfo.MyBots[1].nextCheckPointId];
                    Console.WriteLine($"{coors0.x} {coors0.y} BOOST");
                    Console.WriteLine($"{coors1.x} {coors1.y} 100");
                    continue;
                }

                //win
                var cmd0 = myPod0.GetCommand(isBoostAvailable0);
                var cmd1 = myPod1.GetCommand(isBoostAvailable1);
                //var cmd1 = myPod1.GetDefenseCommand2(isBoostAvailable1, cmd0);
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
            result.x = int.Parse(podInfoArr[0]);
            result.y = int.Parse(podInfoArr[1]);
            result.vx = int.Parse(podInfoArr[2]);
            result.vy = int.Parse(podInfoArr[3]);
            result.angle = int.Parse(podInfoArr[4]);
            result.nextCheckPointId = int.Parse(podInfoArr[5]);
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
            response.laps = int.Parse(Console.ReadLine());
            response.checkpointCount = int.Parse(Console.ReadLine());
            for (var i = 0; i < response.checkpointCount; i++)
            {
                var checkPoints = Console.ReadLine().Split(' ');
                var coordinates = new Coordinates();
                coordinates.x = int.Parse(checkPoints[0]);
                coordinates.y = int.Parse(checkPoints[1]);
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
}
