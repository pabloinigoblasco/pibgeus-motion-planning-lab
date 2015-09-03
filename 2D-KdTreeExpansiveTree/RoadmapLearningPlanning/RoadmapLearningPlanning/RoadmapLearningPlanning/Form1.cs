using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RLP;
using System.Threading;
using System.Diagnostics;

namespace RoadmapLearningPlanning
{
    public partial class Form1 : Form
    {
        List<Node> nodes = new List<Node>();
        List<QuadTreeArea> areas = new List<QuadTreeArea>();
        QuadTreeArea selectedArea;
        List<Vector2> collisions = new List<Vector2>();
        Node workingNode;
        Node clickedAndSelectedNode;
        public Node[] FinalPath { get; set; }
        Vector2? candidate;
        List<Vector2> candidates = new List<Vector2>();


        Thread problemThread;
        public Form1()
        {
            InitializeComponent();

            Bitmap data = (Bitmap)Bitmap.FromFile("map.jpg");
            ProblemContext.LoadFromImage((Bitmap)Bitmap.FromFile("map.jpg"));
            this.BackgroundImage = data;
            this.Width = this.BackgroundImage.Width;
            this.Height = this.BackgroundImage.Height;
            this.Paint += new PaintEventHandler(Form1_Paint);
            this.MouseMove += new MouseEventHandler(Form1_MouseMove);
            this.MouseClick += new MouseEventHandler(Form1_MouseClick);

            this.SetStyle(ControlStyles.DoubleBuffer |ControlStyles.UserPaint |ControlStyles.AllPaintingInWmPaint,true);
            this.UpdateStyles();
			this.button1.Text = "Compute EST path";
           
        }

		public void StartProblemThread()
		{
			RLPAlgorithm algorithm = new RLPAlgorithm();
			ProblemContext.NewNode += new Action<Node>(ProblemContext_NewNode);
			//ProblemContext.Candidate += new Action<Node, Vector2>(ProblemContext_Candidate);
			//ProblemContext.Collision += new Action<Node, Vector2, IEnumerable<Vector2>, Vector2>(ProblemContext_Collision);
			//ProblemContext.NewArea += new Action<QuadTreeArea>(ProblemContext_NewArea);

			problemThread = new Thread(new ThreadStart(delegate
				{
					Stopwatch timer = new Stopwatch();
					timer.Start();
					this.FinalPath = algorithm.SolveFirst(new Vector2 { X = 99, Y = 732 }, new Vector2 { X = 934, Y = 432 });
					this.BeginInvoke((Action)delegate { this.Invalidate(); });
					timer.Stop();
					BeginInvoke(new Action(delegate { System.Windows.Forms.MessageBox.Show(timer.ElapsedMilliseconds.ToString()); }));
				}));
			problemThread.Start();
			problemThread.Name = "Problem Thread";
		}

        void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            selectedArea = areas.FirstOrDefault(a => a.IsFinalNode && a.Cover(new Vector2 { X = e.X, Y = e.Y }));
            clickedAndSelectedNode = nodes.FirstOrDefault(n => (n.Position - new Vector2 { X = (double)e.X, Y = (double)e.Y }).Module() <= 5.0);
            this.Invalidate();
        }

        void Form1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        void ProblemContext_NewArea(QuadTreeArea newarea)
        {
            this.BeginInvoke((Action)delegate { interfaceNewArea(newarea); });
            //checkPoint()            
        }
        void interfaceNewArea(QuadTreeArea newarea)
        {
            this.Text = "Paused on new Area";
            this.areas.Add(newarea);
            this.Invalidate();

        }

        public void ProblemContext_Candidate(Node node, Vector2 displacement)
        {
            this.BeginInvoke((Action)delegate { interfaceCandidate(node, displacement); });
            checkPoint();
        }
        private void interfaceCandidate(Node node, Vector2 displacement)
        {
            this.Text = "Paused on candidate";
            candidate = node.Position + displacement;
            this.candidates.Clear();
            this.collisions.Clear();
            workingNode = node;
            //this.Invalidate();
        }
        private void interfaceCollision(Node n, Vector2 localRepulsiveForce, IEnumerable<Vector2> candidates, Vector2 collision)
        {
            this.Text = "Paused on collision";
            collisions.Add(collision);
            workingNode = n;
            this.candidates.Clear();
            this.candidates.AddRange(candidates);
            //this.Invalidate();
        }
        void interfaceNewNode(Node n)
        {
			lock(this)
			{
            	this.Text = "Paused on newNode";
            	nodes.Add(n);
            	this.collisions.Clear();
            	this.candidate = null;
            	//this.Invalidate();
			}
        }



        void ProblemContext_Collision(Node n, Vector2 localRepulsiveForce, IEnumerable<Vector2> candidates, Vector2 collision)
        {
            this.BeginInvoke((Action)delegate { interfaceCollision(n, localRepulsiveForce, candidates, collision); });
            checkPoint();
        }

        private void checkPoint()
        {
            //Thread.CurrentThread.Suspend();
            //Thread.Sleep(10);
        }


        void ProblemContext_NewNode(Node n)
        {
            interfaceNewNode(n);
            checkPoint();

        }

        Pen finalPathPen = new Pen(Brushes.Red, 3);
        private void button1_Click(object sender, EventArgs e)
        {
			StartProblemThread ();
			if (problemThread.IsAlive) {
				//problemThread.Resume ();
				this.Text = "Running";
			}
        }
        Font f = new Font("arial", 9);
        Pen p = new Pen(Brushes.Red);
        void Form1_Paint(object sender, PaintEventArgs e)
        {
			lock (this) {
				//pintando todos los nodos existentes
				if (nodes.Any ()) {
					double bestFitness = nodes.Min (n => n.PathQualityEstimation);
					foreach (var node in nodes.OrderBy(n => n.PathQualityEstimation)) {
						PointF nodepos = node.Position.ToPointF ();
						if (node.Parent != null) {
							PointF parentpos = node.Parent.Position.ToPointF ();

							int col = Convert.ToInt32 (255.0 * bestFitness / node.PathQualityEstimation);
							p.Color = Color.FromArgb (col, col, 255);
							e.Graphics.DrawLine (p, nodepos, parentpos);
						}
						e.Graphics.FillRectangle (Brushes.Blue, (float)node.Position.X - 1.0f, (float)node.Position.Y - 1.0f, 3.0f, 3.0f);
						//e.Graphics.DrawString("C:"+(int)node.Cost+" H:"+(int)node.Heuristic+" F:" + node.PathQualityEstimation,f,Brushes.Black,nodepos);
					}
				}

				if (FinalPath != null)
					e.Graphics.DrawLines (finalPathPen, FinalPath.Select (p => p.Position.ToPointF ()).ToArray ());



				if (workingNode != null) {
					PointF mainPoint = workingNode.Position.ToPointF ();
					PointF repulsiveForcePoint = (workingNode.RepulsiveForce + workingNode.Position).ToPointF ();

					//pintando el nodo de trabajo
					e.Graphics.FillEllipse (Brushes.Lime, mainPoint.X - 3.0f, mainPoint.Y - 3.0f, 7.0f, 7.0f);

					//pintando la fuerza de repulsión
					e.Graphics.DrawLine (Pens.LightGray, mainPoint, repulsiveForcePoint);



					if (clickedAndSelectedNode != null) {
						Node node = clickedAndSelectedNode;
						Rectangle rectangleLayout = new Rectangle (node.Position.ToPoint (), new Size (150, 100));
						e.Graphics.FillRectangle (Brushes.White, rectangleLayout);
						e.Graphics.DrawRectangle (Pens.Black, rectangleLayout);
						e.Graphics.DrawString (
							"Cost:" + (int)node.Cost
							+ "\nHeu:" + (int)node.Heuristic
							+ "\nFit:" + node.PathQualityEstimation
							+ "\nArea: " + node.Area.Name
							+ "\nChildCount: " + node.ChildrenCount
                        , f, Brushes.Black, rectangleLayout);
					}

					//pintando el candidato
					if (candidate != null) {
						PointF candidatePoint = candidate.Value.ToPointF ();
						e.Graphics.FillRectangle (Brushes.Yellow, candidatePoint.X - 3, candidatePoint.Y - 3, 5.0f, 5.0f);
						e.Graphics.DrawLine (Pens.Yellow, mainPoint, candidatePoint);
					}

					//pinando candidatos postColisión
					foreach (var candi in candidates) {
						PointF candidatePoint = candi.ToPointF ();
						e.Graphics.FillRectangle (Brushes.Orange, candidatePoint.X - 3, candidatePoint.Y - 3, 5.0f, 5.0f);
						e.Graphics.DrawLine (Pens.Orange, mainPoint, candidatePoint);
					}

					//pintando todas las colisiones
					foreach (var c in collisions) {
						PointF p = new PointF ((float)c.X, (float)c.Y);
						e.Graphics.DrawLine (Pens.Red, p, mainPoint);
						e.Graphics.FillRectangle (Brushes.LightBlue, p.X - 2.0f, p.Y - 2.0f, 5.0f, 5.0f);
					}


					foreach (var area in areas) {
						e.Graphics.DrawRectangle (Pens.Red, (float)area.Left, (float)area.Bottom, (float)area.Width, (float)area.Height);
						if (area == selectedArea && area.IsFinalNode && area.Parent != null) {
							string toString;

							if (area.Bottom > area.Parent.Bottom)
								toString = "north";
							else
								toString = "south";

							if (area.Left > area.Parent.Left)
								toString += "-east";
							else
								toString += "-west";


							toString += " D:" + (area.Depth + 1) * area.TotalNodeCount
							+ "-" + area.Area + "-"
							+ area.Density;
							e.Graphics.DrawString (toString, f, Brushes.Red, new PointF ((float)area.Left, (float)area.Bottom));
						}
					}

				}
			}
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            problemThread.Abort();
        }


    }
    public static class ExtensionsMethods
    {
        public static PointF ToPointF(this Vector2 v)
        {
            return new PointF((float)v.X, (float)v.Y);
        }
        public static Point ToPoint(this Vector2 v)
        {
            return new Point((int)v.X, (int)v.Y);
        }
    }

}
