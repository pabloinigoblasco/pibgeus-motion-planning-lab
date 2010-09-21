using System;
using System.Collections.Generic;
using System.Linq;

namespace RLP
{
    public class Node : IComparable
    {
        public Node()
        {
            Cost = 0;
            RepulsiveForce.X = 0;
            RepulsiveForce.Y = 0;
            Penalty = 1;
            ChildrenCount = 1;
        }
        private randomExpansionIterator candidateDisplacementGenerator;
        private IEnumerator<Vector2> candidateDisplacement;
        public Node Parent { get; set; }
        public Vector2 Position;
        public Vector2 RepulsiveForce;
        public int ChildrenCount { get; private set; }
        public int Penalty { get; set; }

        public double Cost { get; set; }
        public double Heuristic { get; set; }
        public double PathQualityEstimation { get { return Cost; } }
        public QuadTreeArea Area { get; set; }
        public static Random r = new Random();

        public Node TryExpand(Node qend)
        {
            bool found = false;
            Node n = new Node();
            bool solution = false;
            Random r=new Random(Environment.TickCount);


            if (candidateDisplacement == null)
            {
                candidateDisplacementGenerator= new randomExpansionIterator { q = this.Position, qend = qend.Position, maxVectorModule = 50 };
                candidateDisplacement = candidateDisplacementGenerator
                            .GetVectors()
                            .GetEnumerator();
            }

            

            int fails = 0;
            candidateDisplacement.MoveNext();
                    

            while (!found && fails < ProblemContext.MaxAttemptsPerNodeExpansion)
            {
                Vector2 targetPoint;
                Vector2? collisionPoint;

                if (r.Next() % 3 == 0)
                    targetPoint = qend.Position;
                else
                {
                    QuadTreeArea CandidateArea;
                    do{
                        targetPoint = this.Position + candidateDisplacement.Current;
                        CandidateArea = this.Area.FindArea(targetPoint);
                        this.candidateDisplacement.MoveNext();
                    }
                    while (CandidateArea != null && CandidateArea.Density > this.Area.Density);
                }



                ProblemContext.onCandidate(this, candidateDisplacement.Current);
                if (!ProblemContext.checkObstacle(targetPoint.X , targetPoint.Y))
                {
                   collisionPoint= ProblemContext.LocalPlanner(Position,targetPoint );
                   
                    //no collision
                    if (collisionPoint == null)
                    {
                        Node parentForNewChild = this;

                        //OK
                        n.Position = targetPoint;
                        n.Heuristic = 0;
                        n.Cost = parentForNewChild.Cost + targetPoint.Module();
                        n.Parent = parentForNewChild;
                        parentForNewChild.ChildrenCount++;

                        
                        
                        parentForNewChild.Area.Add(n);
                        ProblemContext.onNewNode(n);

                        found = true;

                        if (qend.Position.X == n.Position.X && qend.Position.Y == n.Position.Y)
                            solution = true;
                    
                    }
                }
                if (!found)
                {
                    fails++;
                    //candidateDisplacementGenerator.maxVectorModule /= 2.0;
                }
            }

            if (solution)
            {
                qend.Parent = n;
                return qend;
            }
            else
                return n;

        }

        public Node TryExpandOld(Node qend)
        {
            bool found = false;
            Node n = new Node();
            bool solution = true;

            //Vector2 displacement = qend.Position - Position + RepulsiveForce; //se junta con el conocimiento histórico


            //Vector2 displacement = qend.Position - this.Position;
            //Vector2 candidateNodePosition = qend.Position+RepulsiveForce;

            if (candidateDisplacement == null)
                candidateDisplacement = new angleIterator { q = this.Position, qend = qend.Position }
                            .GetVectors()
                            .GetEnumerator();

            candidateDisplacement.MoveNext();

            Vector2 displacement = candidateDisplacement.Current;
            Vector2 candidateNodePosition = displacement + Position;

            int fails = 0;
            while (!found && fails < ProblemContext.MaxAttemptsPerNodeExpansion)
            {
                //where want I go
                ProblemContext.onCandidate(this, displacement);

                Vector2? collisionPoint = ProblemContext.LocalPlanner(Position, candidateNodePosition);
                if (collisionPoint == null)
                {
                    Node parentForNewChild = this;
                    //try better parent


                    if (this.Parent != null)
                    {
                        Node parentCandidate = this.Parent;
                        do
                        {
                            collisionPoint = ProblemContext.LocalPlanner(parentCandidate.Position, candidateNodePosition);
                            if (collisionPoint == null)
                            {
                                parentForNewChild = parentCandidate;

                                parentCandidate = parentForNewChild.Parent;
                                if (parentCandidate == null)
                                    break;
                            }
                            else
                                break;

                        } while (true);
                    }

                    //OK
                    n.Position = candidateNodePosition;
                    n.Heuristic = (candidateNodePosition - qend.Position).Module();
                    n.Cost = parentForNewChild.Cost + displacement.Module();
                    n.Parent = parentForNewChild;
                    n.RepulsiveForce = displacement;
                    //n.Penalty = Penalty + 1;

                    parentForNewChild.RepulsiveForce = displacement;
                    parentForNewChild.ChildrenCount++;
                    parentForNewChild.Area.Add(n);
                    ProblemContext.onNewNode(n);

                    found = true;
                }
                else
                {
                    fails++;
                    var collisionVector = collisionPoint.Value - Position;
                    Vector2 candidateNodeVector = ((collisionVector) * r.NextDouble());


                    /*#warning mejorar esta optimización haciendo uso de la distancia bresenmham
                    

                    
                                        //is an obstacle € (0,1) -> 0 mas directo (hemos caido casi en el objetivo), ->1 perpendicular (hemos caido en un obstáculo)
                                        double clearnessRatio = collisionVector.Module() / displacement.Module();
                                        //collisionVector = collisionVector (* (1;
                                        //RepulsiveForce -= collisionVector;


                                        Vector2 tagentCollisionVectorA = new Vector2 { X = collisionVector.Y, Y = -collisionVector.X };
                                        Vector2 tagentCollisionVectorB = new Vector2 { X = -collisionVector.Y, Y = collisionVector.X };

                                        var candidateNodeVectorA = ((collisionVector * clearnessRatio + tagentCollisionVectorA * (1 - clearnessRatio)) ) / (double)fails ;
                                        var candidateNodeVectorB = ((collisionVector * clearnessRatio + tagentCollisionVectorB * (1 - clearnessRatio)) ) / (double)fails ;
                                        var candidateNodeVectorC = ((collisionVector )*0.5/ ((double)fails) );

                                        //var candidateNodeVectorA = qend.Position-this.Position+RepulsiveForce;
                                        //var candidateNodeVectorB = ((collisionVector * clearnessRatio + tagentCollisionVectorB * (1 - clearnessRatio)) ) / (double)fails ;
                                        //var candidateNodeVectorC = ((collisionVector )/ (double)fails) ;
                                        //var candidateNodeVectorC = candidateNodeVectorA;
                                        //var candidateNodeVectorB = candidateNodeVectorA;


                                        /*double afitness=  (candidateNodePositionA - candidateNodePosition).Distance();
                                        double bfitness = (candidateNodePositionB - candidateNodePosition).Distance();
                                        double cfitness = (candidateNodePositionC - candidateNodePosition).Distance();
                                         */
                    /*
                    var candidates = new[]{candidateNodeVectorA,
                                           candidateNodeVectorB,
                                           candidateNodeVectorC};

                    var candidatesInfo = candidates.Select(c => new { vector=c,fitness=(qend.Position-(c+Position)).Module()});
                    double bestFintess = candidatesInfo.Min(c => c.fitness);

                    candidateNodeVector = candidatesInfo
                        .Where(c => c.fitness == bestFintess)
                        .First().vector;

                    
                     ProblemContext.onColision(this, displacement, candidatesInfo.Select(disp=>disp.vector+Position) , collisionPoint.Value);
                     */

                    displacement = candidateNodeVector;
                    candidateNodePosition = Position + displacement;
                    ProblemContext.onColision(this, displacement, new Vector2[] { candidateNodePosition }, collisionPoint.Value);

                    QuadTreeArea workingArea = Area.FindArea(candidateNodePosition);
                    solution = false;

                    if (workingArea != null && workingArea.Area < ProblemContext.MinAreaSize && workingArea.NodeCount >= ProblemContext.NodesPerArea)
                    {
                        this.Penalty++;
                        workingArea.PenalityAllNodes();
                    }


                }
            }
            if (solution)
            {
                qend.Parent = n;
                return qend;
            }
            else
                return n;

        }
        public IEnumerable<Node> Predecesors
        {
            get
            {
                List<Node> path = new List<Node>();
                Node n = this;

                do
                {
                    path.Add(n);
                    n = n.Parent;
                }
                while (n != null);
                return path;
            }
        }

        #region Miembros de IComparable

        public int CompareTo(object obj)
        {
            var a = (this.PathQualityEstimation - ((Node)obj).PathQualityEstimation);
            if (a < 1)
                return -1;
            else if (a > 1)
                return 1;
            else if (a == 0)
                return 0;
            else
                throw new NotImplementedException();
        }

        #endregion
    }
}