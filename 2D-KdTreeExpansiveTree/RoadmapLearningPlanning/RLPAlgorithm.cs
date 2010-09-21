using System;
using System.Collections.Generic;
using System.Linq;

namespace RLP
{
    public class RLPAlgorithm
    {
        public int CurrentIteration{get;set;}
        public Node[] SolveFirst(Vector2 vqinit, Vector2 vqend)
        {

            QuadTreeArea currentArea = new QuadTreeArea(Math.Min(vqinit.X, vqend.X),
                                                Math.Max(vqinit.Y, vqend.Y),
                                                Math.Max(vqinit.X, vqend.X),
                                                Math.Min(vqinit.Y, vqend.Y));


            Node qend = new Node { Position = vqend};
            Node qinit = new Node { Position = vqinit };
            qinit.Heuristic = (vqend - vqinit).Module();

            currentArea.Add(qinit);
            ProblemContext.onNewNode(qinit);
            
            int CurrentIteration = 0;
            while (CurrentIteration < ProblemContext.MaxGenerations)
            {
                Node bestNode = currentArea.GetBestNodeFromMostUndenseArea();
                Node newNode = bestNode.TryExpand(qend);

                if (currentArea.Parent != null)
                    currentArea = currentArea.Antecesor;
                
                if (newNode == qend)
                {
                    return newNode.Predecesors.ToArray();
                }
                CurrentIteration++;
            }

            return null;
        }
    }
}