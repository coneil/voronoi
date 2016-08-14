using System;
using System.Collections.Generic;
using System.Linq;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    public enum LR { LEFT, RIGHT }

    public sealed class Edge
    {
        private static List<Edge> s_pool = new List<Edge>();
        private static int _nEdges = 0;

        internal static Edge DELETED = new Edge();

        // ax + by = c
        internal double A;
        internal double B;
        internal double C;

        // original points for the edge
        internal Point LeftVertex { get;  private set; }
        internal Point RightVertex { get; private set; }

        // left/right vertices adjusted to fit within bounds
        internal Dictionary<LR, Point> ClippedVertices;
        // the 2 sites this edge runs alongside
        private Dictionary<LR, Site> _sites;
        private int _edgeIndex;

        internal bool Visible { get { return ClippedVertices != null; } }

        internal Site LeftSite 
        { 
            get { return _sites[LR.LEFT]; }
            set { _sites[LR.LEFT] = value; }
        }

        internal Site RightSite
        {
            get { return _sites[LR.RIGHT]; }
            set { _sites[LR.RIGHT] = value; }
        }

        internal bool IsPartOfConvexHull()
        {
            return LeftVertex.IsUnassigned() || RightVertex.IsUnassigned();
        }

        internal double SitesDistance()
        {
            return Point.Distance(LeftVertex, RightVertex);
        }

        internal LineSegment VoronoiEdge
        {
            get
            {
                if(!Visible) return new LineSegment();
                return new LineSegment(ClippedVertices[LR.LEFT], ClippedVertices[LR.RIGHT]);
            }
        }

        internal LineSegment DelaunayLine
        {
            get
            {
                return new LineSegment(LeftSite.Coord, RightSite.Coord);
            }
        }

        internal void ClipVertices(double xMin, double xMax, double yMin, double yMax)
        {
            Point v0, v1;
            double x0, x1, y0, y1;

            if(A == 1 && B >= 0)
            {
                v0 = RightVertex;
                v1 = LeftVertex;
            }
            else
            {
                v0 = LeftVertex;
                v1 = RightVertex;
            }

            if(A == 1)
            {
                y0 = yMin;
                if(!v0.IsUnassigned() && v0.Y > yMin)
                {
                    y0 = v0.Y;
                }
                if(y0 > yMax)
                {
                    return;
                }
                x0 = C - B * y0;

                y1 = yMax;
                if(!v1.IsUnassigned() && v1.Y < yMax)
                {
                    y1 = v1.Y;
                }
                if(y1 < yMin)
                {
                    return;
                }
                x1 = C - B * y1;

                if((x0 > xMax && x1 > xMax) || (x0 < xMin && x1 < xMin))
                {
                    return;
                }

                if(x0 > xMax)
                {
                    x0 = xMax;
                    y0 = (C - x0) / B;
                }
                else if(x0 < xMin)
                {
                    x0 = xMin;
                    y0 = (C - x0) / B;
                }

                if(x1 > xMax)
                {
                    x1 = xMax;
                    y1 = (C - x1) / B;
                }
                else if(x1 < xMin)
                {
                    x1 = xMin;
                    y1 = (C - x1) / B;
                }
            }
            else
            {
                x0 = xMin;
                if(!v0.IsUnassigned() && v0.X > xMin)
                {
                    x0 = v0.X;
                }
                if(x0 > xMax)
                {
                    return;
                }
                y0 = C - A * x0;

                x1 = xMax;
                if(!v1.IsUnassigned() && v1.X < xMax)
                {
                    x1 = v1.X;
                }
                if(x1 < xMin)
                {
                    return;
                }
                y1 = C - A * x1;

                if((y0 > yMax && y1 > yMax) || (y0 < yMin && y1 < yMin))
                {
                    return;
                }

                if(y0 > yMax)
                {
                    y0 = yMax;
                    x0 = (C - y0) / A;
                }
                else if(y0 < yMin)
                {
                    y0 = yMin;
                    x0 = (C - y0) / A;
                }

                if(y1 > yMax)
                {
                    y1 = yMax;
                    x1 = (C - y1) / A;
                }
                else if(y1 < yMin)
                {
                    y1 = yMin;
                    x1 = (C - y1) / A;
                }
            }

            ClippedVertices = new Dictionary<LR, Point>();
            if(v0.Equals(LeftVertex))
            {
                ClippedVertices[LR.LEFT] = new Point(x0, y0);
                ClippedVertices[LR.RIGHT] = new Point(x1, y1);
            }
            else
            {
                ClippedVertices[LR.RIGHT] = new Point(x0, y0);
                ClippedVertices[LR.LEFT] = new Point(x1, y1);
            }
        }

        #region CONSTRUCT/DESTRUCT
        internal static Edge CreateBisectingEdge(Site site0, Site site1)
        {
            double dx = site1.Coord.X - site0.Coord.X;
            double dy = site1.Coord.Y - site0.Coord.Y;
            double absdx = dx > 0 ? dx : -dx;
            double absdy = dy > 0 ? dy : -dy;
            double c = site0.Coord.X * dx + site0.Coord.Y * dy + (dx * dx + dy * dy) * 0.5f;
            double a, b;
            if(absdx > absdy)
            {
                a = 1;
                b = dy / dx;
                c /= dx;
            }
            else
            {
                b = 1;
                a = dx / dy;
                c /= dy;
            }

            Edge e = Edge.Create();
            e.LeftSite = site0;
            e.RightSite = site1;
            
            // TODO
            // site0.AddEdge(e);
            // site1.AddEdge(e);

            e.A = a;
            e.B = b;
            e.C = c;

            return e;
        }

        private Edge()
        {
            _edgeIndex = _nEdges++;
            Init();
        }

        void Init()
        {
            _sites = new Dictionary<LR, Site>();
        }

        static Edge Create()
        {
            Edge e;
            if(s_pool.Count > 0)
            {
                e = s_pool[s_pool.Count - 1];
                s_pool.RemoveAt(s_pool.Count - 1);
                e.Init();
            }
            else
            {
                e = new Edge();
            }

            return e;
        }

        public void Dispose()
        {
            _sites = null;
            ClippedVertices = null;
            LeftVertex.Reset();
            RightVertex.Reset();
            s_pool.Add(this);
        }
        #endregion
    }
}
