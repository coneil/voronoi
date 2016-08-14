using System;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    internal sealed class HalfEdgePriorityQueue
    {
        HalfEdge[] _hash;
        int _count;
        int _minBucket;
        int _hashSize;
        double _yMin;
        double _deltaY;

        public HalfEdgePriorityQueue(double yMin, double deltaY, int sqrt_nsites)
        {
            _yMin = yMin;
            _deltaY = deltaY;
            _hashSize = 4 * sqrt_nsites;
            Init();
        }

        void Init()
        {
            _hash = new HalfEdge[_hashSize];
            for(int i = 0; i < _hashSize; i++)
            {
                _hash[i] = HalfEdge.CreateDummy();
                _hash[i].NextInPriorityQueue = null;
            }
        }

        public void Insert(HalfEdge halfEdge)
        {
            HalfEdge previous, next;
            int insertionBucket = Bucket(halfEdge);
            if(insertionBucket < _minBucket)
            {
                _minBucket = insertionBucket;
            }

            previous = _hash[insertionBucket];
            next = previous.NextInPriorityQueue;
            while(next != null && (halfEdge.YStar > next.YStar || (halfEdge.YStar == next.YStar && halfEdge.Vertex.X > next.Vertex.X)))
            {
                previous = next;
            }

            halfEdge.NextInPriorityQueue = previous.NextInPriorityQueue;
            previous.NextInPriorityQueue = halfEdge;
            _count++;
        }

        public void Remove(HalfEdge halfEdge)
        {
            HalfEdge previous;
            int removalBucket = Bucket(halfEdge);

            if(!halfEdge.Vertex.IsUnassigned())
            {
                previous = _hash[removalBucket];
                while(previous.NextInPriorityQueue != halfEdge)
                {
                    previous = previous.NextInPriorityQueue;
                }
                previous.NextInPriorityQueue = halfEdge.NextInPriorityQueue;
                _count--;
                halfEdge.Vertex.Reset();
                halfEdge.NextInPriorityQueue = null;
                halfEdge.Dispose();
            }
        }

        int Bucket(HalfEdge he)
        {
            int bucket = Convert.ToInt32((he.YStar - _yMin) / _deltaY * _hashSize);
            bucket = bucket < 0 ? 0 : bucket;
            bucket = bucket >= _hashSize ? _hashSize - 1 : bucket;
            return bucket;
        }

        bool IsEmpty(int bucket)
        {
            return _hash[bucket].NextInPriorityQueue == null;
        }

        void AdjustMinBucket()
        {
            while(_minBucket < _hashSize - 1 && IsEmpty(_minBucket))
            {
                _minBucket++;
            }
        }

        public bool IsEmpty()
        {
            return _count == 0;
        }

        public Point Min()
        {
            AdjustMinBucket();
            HalfEdge min = _hash[_minBucket].NextInPriorityQueue;
            return new Point(min.Vertex.X, min.YStar);
        }

        public HalfEdge ExtractMin()
        {
            HalfEdge result = _hash[_minBucket].NextInPriorityQueue;

            _hash[_minBucket].NextInPriorityQueue = result.NextInPriorityQueue;
            _count--;
            result.NextInPriorityQueue = null;

            return result;
        }

        public void Dispose()
        {
            foreach(var he in _hash)
            {
                he.Dispose();
            }
            _hash = null;
        }
    }
}
