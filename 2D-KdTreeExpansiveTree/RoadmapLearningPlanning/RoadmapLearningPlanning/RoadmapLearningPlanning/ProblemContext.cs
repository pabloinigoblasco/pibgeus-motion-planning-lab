using System;
using System.Drawing;
using System.Collections.Generic;

namespace RLP
{
    public class ProblemContext
    {
        public static int NodesPerArea = 5;
        public static double MinAreaSize = 70;
        public static int MaxGenerations = 10000;
        public static int MaxAttemptsPerNodeExpansion = 5;

        public static Vector2? LocalPlanner(Vector2 origin, Vector2 dest)
        {
            Vector2? obstacle = null;
            Vector2 incr = dest - origin;
            double module = incr.Module();
            incr.X = incr.X / (module*15);
            incr.Y = incr.Y / (module * 15);

            bool directionNorth = dest.Y > origin.Y;
            bool directionEast = dest.X > origin.X;

            Vector2 currentPoint;
            for ( currentPoint = origin; !obstacle.HasValue; currentPoint += incr)
            {
                if (directionNorth && currentPoint.Y > dest.Y ||
                    !directionNorth && currentPoint.Y<dest.Y ||
                    directionEast && currentPoint.X>dest.X ||
                    !directionEast && currentPoint.X<dest.X)
                    break;

                if (checkObstacle(currentPoint.X, currentPoint.Y))
                {
                    obstacle = new Vector2 { X = currentPoint.X, Y = currentPoint.Y } - incr * 25;
                }
                
            }

            return obstacle;

        }

        //null if obstacle
        public static bool checkObstacle(double x, double y)
        {
            if(x>= 0 && x < data.Width && y>= 0 && y< data.Height)
            {
                int val = data.GetPixel((int)x, (int)y).R;
                if (val == 0)
                    return true;
                else
                    return false;
            }
            return true;
        }

        static Bitmap data { get; set; }
        public static void LoadFromImage(Bitmap data)
        {
            ProblemContext.data = data;
        }

        public static event Action<Node> NewNode;
        public static void onNewNode(Node n)
        {
            if (NewNode != null)
                NewNode(n);
        }

        public static event Action<Node, Vector2, IEnumerable<Vector2> , Vector2> Collision;
        public static void onColision(Node n, Vector2 localRepulsiveForce, IEnumerable<Vector2> candidates, Vector2 collision)
        {
            if (Collision != null)
                Collision(n, localRepulsiveForce, candidates, collision);
        }

        public static event Action<Node, Vector2> Candidate;
        public static void onCandidate(Node node, Vector2 displacement)
        {
            if (Candidate != null)
                Candidate(node, displacement);
        }

        public static event Action<QuadTreeArea> NewArea;
        public static void onNewArea(QuadTreeArea quadTreeArea)
        {
            if (NewArea != null)
                NewArea(quadTreeArea);
        }
    }
}