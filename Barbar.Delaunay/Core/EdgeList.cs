using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    public sealed class EdgeList
    {

        private double _deltax;
        private double _xmin;
        private int _hashsize;
        private List<Halfedge> _hash;
        public Halfedge leftEnd;
        public Halfedge rightEnd;

        public void dispose()
        {
            Halfedge halfEdge = leftEnd;
            Halfedge prevHe;
            while (halfEdge != rightEnd)
            {
                prevHe = halfEdge;
                halfEdge = halfEdge.edgeListRightNeighbor;
                prevHe.dispose();
            }
            leftEnd = null;
            rightEnd.dispose();
            rightEnd = null;

            _hash.Clear();
            _hash = null;
        }

        public EdgeList(double xmin, double deltax, int sqrt_nsites)
        {
            _xmin = xmin;
            _deltax = deltax;
            _hashsize = 2 * sqrt_nsites;

            _hash = new List<Halfedge>(_hashsize);

            // two dummy Halfedges:
            leftEnd = Halfedge.CreateDummy();
            rightEnd = Halfedge.CreateDummy();
            leftEnd.edgeListLeftNeighbor = null;
            leftEnd.edgeListRightNeighbor = rightEnd;
            rightEnd.edgeListLeftNeighbor = leftEnd;
            rightEnd.edgeListRightNeighbor = null;

            for (int i = 0; i < _hashsize; i++)
            {
                _hash.Add(null);
            }

            _hash[0] = leftEnd;
            _hash[_hashsize - 1] = rightEnd;
        }

        /// <summary>
        /// Insert newHalfedge to the right of lb
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="newHalfedge"></param>
        public void Insert(Halfedge lb, Halfedge newHalfedge)
        {
            newHalfedge.edgeListLeftNeighbor = lb;
            newHalfedge.edgeListRightNeighbor = lb.edgeListRightNeighbor;
            lb.edgeListRightNeighbor.edgeListLeftNeighbor = newHalfedge;
            lb.edgeListRightNeighbor = newHalfedge;
        }

        /// <summary>
        /// This function only removes the Halfedge from the left-right list. We
        /// cannot dispose it yet because we are still using it.
        /// </summary>
        /// <param name="halfEdge"></param>
        public void Remove(Halfedge halfEdge)
        {
            halfEdge.edgeListLeftNeighbor.edgeListRightNeighbor = halfEdge.edgeListRightNeighbor;
            halfEdge.edgeListRightNeighbor.edgeListLeftNeighbor = halfEdge.edgeListLeftNeighbor;
            halfEdge.edge = Edge.DELETED;
            halfEdge.edgeListLeftNeighbor = halfEdge.edgeListRightNeighbor = null;
        }

        /// <summary>
        /// Find the rightmost Halfedge that is still left of p
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Halfedge EdgeListLeftNeighbor(PointDouble p)
        {
            int i, bucket;
            Halfedge halfEdge;

            // Use hash table to get close to desired halfedge
            bucket = (int)((p.X - _xmin) / _deltax * _hashsize);
            if (bucket < 0)
            {
                bucket = 0;
            }
            if (bucket >= _hashsize)
            {
                bucket = _hashsize - 1;
            }
            halfEdge = GetHash(bucket);
            if (halfEdge == null)
            {
                for (i = 1; true; ++i)
                {
                    if ((halfEdge = GetHash(bucket - i)) != null)
                    {
                        break;
                    }
                    if ((halfEdge = GetHash(bucket + i)) != null)
                    {
                        break;
                    }
                }
            }
            // Now search linear list of halfedges for the correct one
            if (halfEdge == leftEnd || (halfEdge != rightEnd && halfEdge.IsLeftOf(p)))
            {
                do
                {
                    halfEdge = halfEdge.edgeListRightNeighbor;
                } while (halfEdge != rightEnd && halfEdge.IsLeftOf(p));
                halfEdge = halfEdge.edgeListLeftNeighbor;
            }
            else
            {
                do
                {
                    halfEdge = halfEdge.edgeListLeftNeighbor;
                } while (halfEdge != leftEnd && !halfEdge.IsLeftOf(p));
            }

            // Update hash table and reference counts
            if (bucket > 0 && bucket < _hashsize - 1)
            {
                _hash[bucket] = halfEdge;
            }
            return halfEdge;
        }

        /// <summary>
        /// Get entry from hash table, pruning any deleted nodes
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private Halfedge GetHash(int b)
        {
            if (b < 0 || b >= _hashsize)
            {
                return null;
            }
            var halfEdge = _hash[b];
            if (halfEdge != null && halfEdge.edge == Edge.DELETED)
            {
                // Hash table points to deleted halfedge.  Patch as necessary.
                _hash[b] = null;
                // still can't dispose halfEdge yet!
                return null;
            }
            return halfEdge;
        }
    }
}