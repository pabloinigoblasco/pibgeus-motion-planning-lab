using System;
using System.Linq;
using System.Collections.Generic;

namespace RLP
{
    public class randomExpansionIterator
    {
        public Vector2 q { get; set; }
        public Vector2 qend { get; set; }
        public double maxVectorModule { get; set; }
        Random r = new Random(Environment.TickCount);

        public IEnumerable<Vector2> GetVectors()
        {
            
            while (true)
                yield return toVector(r.NextDouble() * Math.PI * 2);
        }

        private Vector2 toVector(double currentAngle)
        {
            return new Vector2 { X = Math.Cos(currentAngle), Y = Math.Sin(currentAngle) }*maxVectorModule*r.NextDouble();
        }
    }
}