using System;
using System.Collections.Generic;
using System.Linq;

namespace RLP
{
    public class QuadTreeArea
    {
        public string Name { get; set; }
        public QuadTreeArea NWest { get; set; }
        public QuadTreeArea NEast { get; set; }
        public QuadTreeArea SWest { get; set; }
        public QuadTreeArea SEast { get; set; }
        public IEnumerable<QuadTreeArea> Children { get { return new[] { NWest, NEast, SWest, SEast }; } }
        public IEnumerable<QuadTreeArea> Descendency
        {
            get
            {
                if (IsFinalNode)
                    return System.Linq.Enumerable.Empty<QuadTreeArea>();
                else
                {
                    var chldDesc = Children.SelectMany(c => c.Descendency);
                    return Children.Union(chldDesc);
                }
            }
        }
        public int Depth
        {
            get
            {
                int depth = 0;
                QuadTreeArea area = this;
                while (area.Parent != null)
                {
                    area = area.Parent;
                    depth++;
                }
                return depth;
            }
        }
        public int TotalNodeCount
        {
            get
            {
                if (IsFinalNode)
                    return Nodes.Count;
                else
                    return Children.Sum(c => c.TotalNodeCount);
            }
        }
        public QuadTreeArea Antecesor
        {
            get
            {
                QuadTreeArea area = this;
                while (area.Parent != null)
                {
                    area = area.Parent;
                }
                return area;
            }
        }
        public QuadTreeArea Parent { get; set; }

        public bool IsFinalNode { get; private set; }
        List<Node> Nodes { get; set; }


        /*Origin*/
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
        public double Width { get { return this.Right - this.Left; } }
        public double Height { get { return this.Top - this.Bottom; } }

        public double Area { get; set; }
        public Node BestNode { get; set; }

        public int NodeCount { get { return Nodes.Count; } }


        public QuadTreeArea(double left, double top, double right, double bottom, double newHalfWidth, double newHalfHeight, double area)
        {
            IsFinalNode = true;
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Area = area;

            this.CenterX = left + newHalfWidth;
            this.CenterY = bottom + newHalfHeight;

            Nodes = new List<Node>(ProblemContext.NodesPerArea + 1);
            ProblemContext.onNewArea(this);
        }

        public QuadTreeArea(double left, double top, double right, double bottom)
            : this(left, top, right, bottom, Math.Abs((right - left) / 2.0), Math.Abs((top - bottom) / 2.0), Math.Abs((top - bottom) * (right - left)))
        { }

        public Node GetBestNode()
        {
            if (IsFinalNode)
                return BestNode;
            else
            {
                //recursiveCall
                var bestNodes = (Descendency.Where(k => k.BestNode != null).Select(k => k.BestNode));
                return bestNodes.Min();
            }
        }
        public Node GetBestNodeFromMostUndenseArea()
        {
            var areas = Descendency.Where(a => a.IsFinalNode && a.Density > 0.0).OrderBy(a=>a.Density).ThenBy(a => a.BestNode);
                
              QuadTreeArea bestArea= areas.FirstOrDefault();
            if (bestArea == null)
                return GetBestNode();
            else
                return bestArea.GetBestNode();
        }

        public bool Cover(Vector2 Position)
        {
            return Position.X >= this.Left && Position.X <= this.Right
                    && Position.Y >= this.Bottom && Position.Y <= this.Top;
        }
        public bool Cover(Node newNode)
        {
            return Cover(newNode.Position);
        }
        private QuadTreeArea GetCoveringChildArea(Vector2 Position)
        {
            QuadTreeArea selectedArea = null;
            if (Position.Y > this.CenterY)
            {
                if (Position.X < this.CenterX)
                    selectedArea = NWest;
                else
                    selectedArea = NEast;
            }
            else
            {
                if (Position.X < this.CenterX)
                    selectedArea = SWest;
                else
                    selectedArea = SEast;
            }
            return selectedArea;
        }
        private QuadTreeArea GetCoveringChildArea(Node n)
        {
            return GetCoveringChildArea(n.Position);
        }

        public void Add(Node node)
        {
            if (this.Cover(node))
            {
                if (IsFinalNode)
                {

                    if (Nodes.Count == ProblemContext.NodesPerArea && Area>=ProblemContext.MinAreaSize)
                    {
                        //SUBDIVISION
                        subdivide();
                        Nodes.Add(node);

                        foreach (Node n in Nodes)
                        {
                            QuadTreeArea selectedArea=GetCoveringChildArea(n);
                            selectedArea.Add(n);
                        }

                        this.Nodes.Clear();
                        this.Nodes = null;
                    }
                    else
                    {
                        //NORMAL ADDITION
                        //BASE CASE

                        Nodes.Add(node);
                        node.Area = this;
                        if (this.BestNode == null || node.PathQualityEstimation < this.BestNode.PathQualityEstimation)
                            this.BestNode = node;
                    }

                }
                else
                {
                    //RECURSIVE DOWN CASE
                    GetCoveringChildArea(node).Add(node);
                }
            }
            else
            {
                if (this.Parent == null)
                {

                    bool north = node.Position.Y > this.Top;
                    bool west = node.Position.X < this.Left;
                    bool south = node.Position.Y < this.Bottom;
                    bool east = node.Position.X > this.Right;

                    double width = this.Width;
                    double height = this.Height;

                    if (south || east)
                    {
                        Parent = new QuadTreeArea(this.Left, this.Top, this.Right + width, this.Bottom - height);
                        Parent.NWest = this;
                    }
                    else if (north || west)
                    {
                        Parent = new QuadTreeArea(this.Left - width, this.Top + height, this.Right, this.Bottom);
                        Parent.SEast = this;
                    }
                    else
                        throw new NotImplementedException();

                    this.Name = (north ? "N" : "S") + (east ? "E" : "W");
                    Parent.subdivide();

                }

                this.Parent.Add(node);
            }
        }

        private void subdivide()
        {
            if (NWest == null)
            {
                this.NWest = new QuadTreeArea(Left, Top, CenterX, CenterY);
                this.NWest.Parent = this;
                this.NWest.Name = "NW";
            }
            if (NEast == null)
            {
                this.NEast = new QuadTreeArea(CenterX, Top, Right, CenterY);
                this.NEast.Parent = this;
                this.NEast.Name = "NE";
            }
            if (SWest == null)
            {
                this.SWest = new QuadTreeArea(Left, CenterY, CenterX, Bottom);
                this.SWest.Parent = this;
                this.SWest.Name = "SW";
            }
            if (SEast == null)
            {
                this.SEast = new QuadTreeArea(CenterX, CenterY, Right, Bottom);
                this.SEast.Parent = this;
                this.SEast.Name = "SE";
            }

            this.IsFinalNode = false;
        }

        internal QuadTreeArea FindArea(Vector2 candidateNodePosition)
        {
            if (this.Cover(candidateNodePosition))
            {
                if (IsFinalNode)
                    return this;
                else
                {
                    QuadTreeArea area = this.GetCoveringChildArea(candidateNodePosition);
                    return area.FindArea(candidateNodePosition);
                }
            }
            else
            {
                if (this.Parent == null)
                    return null;
                else
                    return Parent.FindArea(candidateNodePosition);

            }
        }
        public override string ToString()
        {
            return Name + " - macro:" + IsFinalNode;
        }
        public double Density { get { return this.Nodes.Count / this.Area; } }



        internal void PenalityAllNodes()
        {
            foreach (var n in Nodes)
                n.Penalty++;
        }
    }
}