using System;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    internal sealed class EdgeList
    {
        public HalfEdge LeftEnd { get; private set; }
        public HalfEdge RightEnd { get; private set; }

        double _deltaX;
        double _xMin;

        int _hashSize;
        HalfEdge[] _hash;

        public EdgeList(double xMin, double deltaX, int sqrt_nsites)
        {
            _xMin = xMin;
            _deltaX = deltaX;
            _hashSize = 2 * sqrt_nsites;

            _hash = new HalfEdge[_hashSize];

            LeftEnd = HalfEdge.CreateDummy();
            RightEnd = HalfEdge.CreateDummy();

            LeftEnd.EdgeListLeftNeighbor = null;
            LeftEnd.EdgeListRightNeighbor = RightEnd;
            RightEnd.EdgeListLeftNeighbor = LeftEnd;
            RightEnd.EdgeListRightNeighbor = null;

            _hash[0] = LeftEnd;
            _hash[_hashSize - 1] = RightEnd;
        }

        internal void Insert(HalfEdge lb, HalfEdge newHalfEdge)
        {
            newHalfEdge.EdgeListLeftNeighbor = lb;
            newHalfEdge.EdgeListRightNeighbor = lb.EdgeListRightNeighbor;
            lb.EdgeListRightNeighbor.EdgeListLeftNeighbor = newHalfEdge;
            lb.EdgeListRightNeighbor = newHalfEdge;
        }

        internal void Remove(HalfEdge halfEdge)
        {
            halfEdge.EdgeListLeftNeighbor.EdgeListRightNeighbor = halfEdge.EdgeListRightNeighbor;
            halfEdge.EdgeListRightNeighbor.EdgeListLeftNeighbor = halfEdge.EdgeListLeftNeighbor;
            halfEdge.Edge = Edge.DELETED;
            halfEdge.EdgeListLeftNeighbor = halfEdge.EdgeListRightNeighbor = null;
        }

        internal HalfEdge GetEdgeListLeftNeighbor(Point p)
        {
            if(double.IsNaN(p.X) || double.IsNaN(p.Y)) return null;

            double dBucket = (p.X - _xMin) / _deltaX * _hashSize;
            if(double.IsNaN(dBucket))
                dBucket = 0;

            if(dBucket < int.MinValue)
                dBucket = int.MinValue;
            else if(dBucket > int.MaxValue)
                dBucket = int.MaxValue;

            int bucket = Convert.ToInt32(dBucket);
            if(bucket < 0)
                bucket = 0;

            if(bucket >= _hashSize)
                bucket = _hashSize - 1;

            HalfEdge halfEdge = GetHash(bucket);
            if(halfEdge == null)
            {
                int i = 1;
                while(i < _hashSize)
                {
                    halfEdge = GetHash(bucket - i);
                    if(halfEdge == null)
                    {
                        halfEdge = GetHash(bucket + i);
                        if(halfEdge != null)
                            break;
                    }
                    else
                    {
                        break;
                    }

                    i++;
                }
            }

            if(halfEdge == LeftEnd || (halfEdge != RightEnd && halfEdge.IsLeftOf(p)))
            {
                do
                {
                    halfEdge = halfEdge.EdgeListRightNeighbor;
                }
                while(halfEdge != RightEnd && halfEdge.IsLeftOf(p));
                halfEdge = halfEdge.EdgeListLeftNeighbor;
            }
            else
            {
                do
                {
                    halfEdge = halfEdge.EdgeListLeftNeighbor;
                }
                while(halfEdge != LeftEnd && !halfEdge.IsLeftOf(p));
            }

            if(bucket > 0 && bucket < _hashSize - 1)
            {
                _hash[bucket] = halfEdge;
            }

            return halfEdge;
        }

        HalfEdge GetHash(int b)
        {
            if(b < 0 || b >= _hashSize)
                return null;

            HalfEdge halfEdge = _hash[b];
            if(halfEdge != null && halfEdge.Edge == Edge.DELETED)
            {
                _hash[b] = null;
                return null;
            }

            return halfEdge;
        }

        public void Dispose()
        {
            HalfEdge he = LeftEnd;
            HalfEdge prev = null;
            while(he != RightEnd)
            {
                prev = he;
                he = he.EdgeListRightNeighbor;
                prev.Dispose();
            }
            LeftEnd = null;
            RightEnd.Dispose();
            RightEnd = null;
            _hash = null;
        }
    }
}
