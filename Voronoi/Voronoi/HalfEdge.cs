using System;
using System.Collections.Generic;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    internal sealed class HalfEdge
    {
        public HalfEdge EdgeListLeftNeighbor;
        public HalfEdge EdgeListRightNeighbor;
        public HalfEdge NextInPriorityQueue;

        public LR LeftRight;
        public Edge Edge;
        public Point Vertex;
        public double YStar;

        public static Point GetIntersection(HalfEdge h0, HalfEdge h1)
        {
            Edge e0 = h0.Edge;
            Edge e1 = h1.Edge;
            if(e0 == null || e1 == null) return new Point();

            if(e0.RightSite == e1.RightSite) return new Point();

            double determinant = e0.A * e1.B - e0.B * e1.A;
            if(-1.0e-10 < determinant && determinant < 1.0e-10)
            {
                // edges are parallel
                return new Point();
            }

            double intersectionX = (e0.C * e1.B - e1.C * e0.B) / determinant;
            double intersectionY = (e1.C * e0.A - e0.C * e1.A) / determinant;

            HalfEdge halfEdge;
            Edge edge;
            if(Site.SortByYThenX(e0.RightSite, e1.RightSite) < 0)
            {
                halfEdge = h0;
                edge = e0;
            }
            else
            {
                halfEdge = h1;
                edge = e1;
            }

            bool rightOfSite = intersectionX >= edge.RightSite.Coord.X;
            if((rightOfSite && halfEdge.LeftRight == LR.LEFT) || (!rightOfSite && halfEdge.LeftRight == LR.RIGHT))
            {
                return new Point();
            }

            return new Point(intersectionX, intersectionY);
        }

        internal bool IsLeftOf(Point p)
        {
            Site topSite = Edge.RightSite;
            bool rightOfSite = p.X > topSite.Coord.X;

            if(rightOfSite && LeftRight == LR.LEFT)
                return true;

            if(!rightOfSite && LeftRight == LR.RIGHT)
                return false;

            bool above = false;

            if(Edge.A == 1)
            {
                double dyp = p.Y - topSite.Coord.Y;
                double dxp = p.X - topSite.Coord.X;
                bool fast = false;
                if((!rightOfSite && Edge.B < 0) || (rightOfSite && Edge.B >= 0))
                {
                    above = dyp >= Edge.B * dxp;
                    fast = above;
                }
                else
                {
                    above = p.X + p.Y * Edge.B > Edge.C;
                    if(Edge.B < 0)
                    {
                        above = !above;
                    }
                    if(!above)
                    {
                        fast = true;
                    }
                }

                if(!fast)
                {
                    double dxs = topSite.Coord.X - Edge.LeftSite.Coord.X;
                    above = Edge.B * (dxp * dxp - dyp * dyp) < dxs * dyp * (1f + 2f * dxp / dxs + Edge.B * Edge.B);
                    if(Edge.B < 0)
                    {
                        above = !above;
                    }
                }
            }
            else
            {
                double y1 = Edge.C - Edge.A * p.X;
                double t1 = p.Y - y1;
                double t2 = p.X - topSite.Coord.X;
                double t3 = y1 - topSite.Coord.Y;
                above = t1 * t1 > t2 * t2 + t3 * t3;
            }

            return LeftRight == LR.LEFT ? above : !above;
        }

        #region CONSTRUCT/DESTRUCT
        private static List<HalfEdge> s_pool = new List<HalfEdge>();

        public static HalfEdge CreateDummy()
        {
            return Create(null, LR.LEFT);
        }

        public static HalfEdge Create(Edge edge, LR lr)
        {
            if(s_pool.Count > 0)
            {
                HalfEdge he = s_pool[s_pool.Count - 1];
                s_pool.RemoveAt(s_pool.Count - 1);
                he.Init(edge, lr);
                return he;
            }
            else
            {
                return new HalfEdge(edge, lr);
            }
        }

        private HalfEdge(Edge edge, LR lr)
        {
            Init(edge, lr);
        }

        void Init(Edge edge, LR lr)
        {
            Edge = edge;
            LeftRight = lr;
            NextInPriorityQueue = null;
        }

        public void Dispose()
        {
            if(EdgeListLeftNeighbor != null || EdgeListRightNeighbor != null) return;
            if(NextInPriorityQueue != null) return;

            s_pool.Add(this);
        }
        #endregion
    }
}
