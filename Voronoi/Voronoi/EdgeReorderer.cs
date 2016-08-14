using System;
using System.Collections.Generic;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    internal sealed class EdgeReorderer
    {
        public enum Criterion { VERTEX, SITE }

        internal List<Edge> Edges { get; private set; }
        internal List<LR> EdgeOrientations { get; private set; }

        public EdgeReorderer(List<Edge> originalEdges, Criterion criterion)
        {
            Edges = new List<Edge>();
            EdgeOrientations = new List<LR>();
            if(originalEdges.Count > 0)
                Edges = ReorderEdges(originalEdges, criterion);
        }

        List<Edge> ReorderEdges(List<Edge> originalEdges, Criterion criterion)
        {
            int n = originalEdges.Count;
            bool[] done = new bool[n];
            int nDone = 0;

            var newEdges = new List<Edge>();
            Edge edge = originalEdges[0];
            newEdges.Add(edge);
            EdgeOrientations.Add(LR.LEFT);
            Point firstPoint = criterion == Criterion.SITE ? edge.LeftSite.Coord : edge.LeftVertex;
            Point lastPoint = criterion == Criterion.SITE ? edge.RightSite.Coord : edge.RightVertex;

            if(firstPoint.AtInfinity() || lastPoint.AtInfinity())
                return new List<Edge>();

            done[0] = true;
            nDone++;

            while(nDone < n)
            {
                for(int i = 1; i < n; i++)
                {
                    if(done[i]) continue;

                    edge = originalEdges[i];
                    Point leftPoint = criterion == Criterion.VERTEX ? edge.LeftVertex : edge.LeftSite.Coord;
                    Point rightPoint = criterion == Criterion.VERTEX ? edge.RightVertex : edge.RightSite.Coord;
                    if(leftPoint.AtInfinity() || rightPoint.AtInfinity())
                        return new List<Edge>();

                    if(leftPoint.Equals(lastPoint))
                    {
                        lastPoint = rightPoint;
                        EdgeOrientations.Add(LR.LEFT);
                        newEdges.Add(edge);
                        done[i] = true;
                    }
                    else if(rightPoint.Equals(firstPoint))
                    {
                        firstPoint = leftPoint;
                        EdgeOrientations.Insert(0, LR.LEFT);
                        newEdges.Insert(0, edge);
                        done[i] = true;
                    }
                    else if(leftPoint.Equals(firstPoint))
                    {
                        firstPoint = rightPoint;
                        EdgeOrientations.Insert(0, LR.RIGHT);
                        newEdges.Insert(0, edge);
                        done[i] = true;
                    }
                    else if(rightPoint.Equals(lastPoint))
                    {
                        lastPoint = leftPoint;
                        EdgeOrientations.Add(LR.RIGHT);
                        newEdges.Add(edge);
                        done[i] = true;
                    }

                    if(done[i]) nDone++;
                }
            }

            return newEdges;
        }
    }
}
