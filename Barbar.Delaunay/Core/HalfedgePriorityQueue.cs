using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    /// <summary>
    /// Also known as heap
    /// </summary>
    public sealed class HalfedgePriorityQueue
    {
        private List<Halfedge> _hash;
        private int _count;
        private int _minBucket;
        private int _hashsize;
        private double _ymin;
        private double _deltay;

        public HalfedgePriorityQueue(double ymin, double deltay, int sqrt_nsites)
        {
            _ymin = ymin;
            _deltay = deltay;
            _hashsize = 4 * sqrt_nsites;
            Initialize();
        }

        public void dispose()
        {
            // get rid of dummies
            for (int i = 0; i < _hashsize; ++i)
            {
                _hash[i].dispose();
            }
            _hash.Clear();
            _hash = null;
        }

        private void Initialize()
        {
            int i;

            _count = 0;
            _minBucket = 0;
            _hash = new List<Halfedge>(_hashsize);
            // dummy Halfedge at the top of each hash
            for (i = 0; i < _hashsize; ++i)
            {
                _hash.Add(Halfedge.CreateDummy());
                _hash[i].nextInPriorityQueue = null;
            }
        }

        public void Insert(Halfedge halfEdge)
        {
            Halfedge previous, next;
            int insertionBucket = Bucket(halfEdge);
            if (insertionBucket < _minBucket)
            {
                _minBucket = insertionBucket;
            }
            previous = _hash[insertionBucket];
            while ((next = previous.nextInPriorityQueue) != null
                    && (halfEdge.ystar > next.ystar || (halfEdge.ystar == next.ystar && halfEdge.vertex.X > next.vertex.X)))
            {
                previous = next;
            }
            halfEdge.nextInPriorityQueue = previous.nextInPriorityQueue;
            previous.nextInPriorityQueue = halfEdge;
            ++_count;
        }

        public void Remove(Halfedge halfEdge)
        {
            Halfedge previous;
            int removalBucket = Bucket(halfEdge);

            if (halfEdge.vertex != null)
            {
                previous = _hash[removalBucket];
                while (previous.nextInPriorityQueue != halfEdge)
                {
                    previous = previous.nextInPriorityQueue;
                }
                previous.nextInPriorityQueue = halfEdge.nextInPriorityQueue;
                _count--;
                halfEdge.vertex = null;
                halfEdge.nextInPriorityQueue = null;
                halfEdge.dispose();
            }
        }

        private int Bucket(Halfedge halfEdge)
        {
            int theBucket = (int)((halfEdge.ystar - _ymin) / _deltay * _hashsize);
            if (theBucket < 0)
            {
                theBucket = 0;
            }
            if (theBucket >= _hashsize)
            {
                theBucket = _hashsize - 1;
            }
            return theBucket;
        }

        private bool IsEmpty(int bucket)
        {
            return (_hash[bucket].nextInPriorityQueue == null);
        }

        /// <summary>
        /// move _minBucket until it contains an actual Halfedge (not just the dummy
        /// at the top);
        /// </summary>
        private void AdjustMinBucket()
        {
            while (_minBucket < _hashsize - 1 && IsEmpty(_minBucket))
            {
                ++_minBucket;
            }
        }

        public bool IsEmpty()
        {
            return _count == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>coordinates of the Halfedge's vertex in V*, the transformed Voronoi diagram</returns>
        public PointDouble Min()
        {
            AdjustMinBucket();
            Halfedge answer = _hash[_minBucket].nextInPriorityQueue;
            return new PointDouble(answer.vertex.X, answer.ystar);
        }

        /// <summary>
        /// remove and return the min Halfedge
        /// </summary>
        /// <returns></returns>
        public Halfedge ExtractMin()
        {
            Halfedge answer;

            // get the first real Halfedge in _minBucket
            answer = _hash[_minBucket].nextInPriorityQueue;

            _hash[_minBucket].nextInPriorityQueue = answer.nextInPriorityQueue;
            _count--;
            answer.nextInPriorityQueue = null;

            return answer;
        }
    }
}