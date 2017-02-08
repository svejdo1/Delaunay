//package com.hoten.delaunay.voronoi.nodename.as3delaunay;

//import java.util.ArrayList;

using System;
using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    public sealed class EdgeReorderer
    {

        private List<Edge> _edges;
        private List<LR> _edgeOrientations;

        public List<Edge> get_edges()
        {
            return _edges;
        }

        public List<LR> get_edgeOrientations()
        {
            return _edgeOrientations;
        }

        public EdgeReorderer(List<Edge> origEdges, Type criterion)
        {
            if (criterion != typeof(Vertex) && criterion != typeof(Site))
            {
                throw new Exception("Edges: criterion must be Vertex or Site");
            }
            _edges = new List<Edge>();
            _edgeOrientations = new List<LR>();
            if (origEdges.Count > 0)
            {
                _edges = ReorderEdges(origEdges, criterion);
            }
        }

        public void dispose()
        {
            _edges = null;
            _edgeOrientations = null;
        }

        private List<Edge> ReorderEdges(List<Edge> origEdges, Type criterion)
        {
            int i;
            int n = origEdges.Count;
            Edge edge;
            // we're going to reorder the edges in order of traversal
            var done = new List<bool>(n);
            int nDone = 0;
            for (int k = 0; k < n; k++)
            {
                done.Add(false);
            }
            var newEdges = new List<Edge>();

            i = 0;
            edge = origEdges[i];
            newEdges.Add(edge);
            _edgeOrientations.Add(LR.Left);
            var firstPoint = (criterion == typeof(Vertex)) ? (ICoordinates)edge.LeftVertex : edge.LeftSite;
            var lastPoint = (criterion == typeof(Vertex)) ? (ICoordinates)edge.RightVertex : edge.RightSite;

            if (firstPoint == Vertex.VERTEX_AT_INFINITY || lastPoint == Vertex.VERTEX_AT_INFINITY)
            {
                return new List<Edge>();
            }

            done[i] = true;
            ++nDone;

            while (nDone < n)
            {
                for (i = 1; i < n; ++i)
                {
                    if (done[i])
                    {
                        continue;
                    }
                    edge = origEdges[i];
                    var leftPoint = (criterion == typeof(Vertex)) ? (ICoordinates)edge.LeftVertex : edge.LeftSite;
                    var rightPoint = (criterion == typeof(Vertex)) ? (ICoordinates)edge.RightVertex : edge.RightSite;
                    if (leftPoint == Vertex.VERTEX_AT_INFINITY || rightPoint == Vertex.VERTEX_AT_INFINITY)
                    {
                        return new List<Edge>();
                    }
                    if (leftPoint == lastPoint)
                    {
                        lastPoint = rightPoint;
                        _edgeOrientations.Add(LR.Left);
                        newEdges.Add(edge);
                        done[i] = true;
                    }
                    else if (rightPoint == firstPoint)
                    {
                        firstPoint = leftPoint;
                        _edgeOrientations.Insert(0, LR.Left);
                        newEdges.Insert(0, edge);
                        done[i] = true;
                    }
                    else if (leftPoint == firstPoint)
                    {
                        firstPoint = rightPoint;
                        _edgeOrientations.Insert(0, LR.Right);
                        newEdges.Insert(0, edge);

                        done[i] = true;
                    }
                    else if (rightPoint == lastPoint)
                    {
                        lastPoint = leftPoint;
                        _edgeOrientations.Add(LR.Right);
                        newEdges.Add(edge);
                        done[i] = true;
                    }
                    if (done[i])
                    {
                        ++nDone;
                    }
                }
            }

            return newEdges;
        }
    }
}