using System;
using System.Linq;
using System.Collections.Generic;

namespace RLP
{
    public class angleIterator
    {
        public Vector2 q { get; set; }
        public Vector2 qend { get; set; }
        private double vectorModule;
        public IEnumerable<Vector2> GetVectors()
        {
            Vector2 vcurrent = qend - q;
            vectorModule = vcurrent.Module();
            int steps = 8;

            double originalAngle = Math.Atan((qend.Y - q.Y) / (qend.X - q.X));
            if (qend.X - q.X < 0)
                originalAngle += Math.PI;
            yield return vcurrent;

            double startAngle = Math.PI / 8;
            Random r = new Random(System.DateTime.Now.Millisecond);
            while (true)
            {
                double currentAngle = startAngle;
                for (int i = 1; i < steps; i++)
                {

                    if (i % 2 != 0)
                    {
                        yield return toVector(currentAngle + originalAngle);
                        yield return toVector(-currentAngle + originalAngle);
                        yield return toVector(r.NextDouble() * Math.PI * 2);
                    }
                    

                    currentAngle += startAngle;
                }
                yield return toVector(currentAngle + originalAngle);

                startAngle = startAngle / 2.0;
                steps *= 2;
                
            }
        }

        private Vector2 toVector(double currentAngle)
        {
            return new Vector2 { X = Math.Cos(currentAngle), Y = Math.Sin(currentAngle) }*vectorModule;
        }
    }
}