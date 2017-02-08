using System;
using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    public sealed class SiteList
    {

        private List<Site> _sites;
        private int _currentIndex;
        private bool _sorted;

        public SiteList()
        {
            _sites = new List<Site>();
            _sorted = false;
        }

        public void dispose()
        {
            if (_sites != null)
            {
                foreach (var site in _sites)
                {
                    site.dispose();
                }
                _sites.Clear();
                _sites = null;
            }
        }

        public int Push(Site site)
        {
            _sorted = false;
            _sites.Add(site);
            return _sites.Count;
        }

        public int Count
        {
            get { return _sites.Count; }
        }

        public Site Next()
        {
            if (!_sorted)
            {
                throw new Exception("SiteList::Next():  sites have not been sorted");
            }
            if (_currentIndex < _sites.Count)
            {
                return _sites[_currentIndex++];
            }
            else
            {
                return null;
            }
        }

        public Rectangle GetSitesBounds()
        {
            if (_sorted == false)
            {
                Site.SortSites(_sites);
                _currentIndex = 0;
                _sorted = true;
            }
            double xmin, xmax, ymin, ymax;
            if (_sites.Count == 0)
            {
                return new Rectangle(0, 0, 0, 0);
            }
            xmin = double.MaxValue;
            xmax = double.MinValue;
            foreach (var site in _sites)
            {
                if (site.X < xmin)
                {
                    xmin = site.X;
                }
                if (site.X > xmax)
                {
                    xmax = site.X;
                }
            }
            // here's where we assume that the sites have been sorted on y:
            ymin = _sites[0].Y;
            ymax = _sites[_sites.Count - 1].Y;

            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public List<PointDouble> GetSiteCoordinates()
        {
            var coords = new List<PointDouble>();
            foreach (var site in _sites)
            {
                coords.Add(site.Coordinates);
            }
            return coords;
        }

        /// <summary>
        /// </summary>
        /// <returns>the largest circle centered at each site that fits in its region; if the region is infinite, return a circle of radius 0.</returns>
        public List<Circle> GetCircles()
        {
            var circles = new List<Circle>();
            foreach (var site in _sites)
            {
                double radius = 0;
                Edge nearestEdge = site.NearestEdge();

                //!nearestEdge.isPartOfConvexHull() && (radius = nearestEdge.sitesDistance() * 0.5);
                if (!nearestEdge.IsPartOfConvexHull)
                {
                    radius = nearestEdge.SitesDistance * 0.5;
                }
                circles.Add(new Circle(site.X, site.Y, radius));
            }
            return circles;
        }

        public List<List<PointDouble>> GetRegions(Rectangle plotBounds)
        {
            var regions = new List<List<PointDouble>>();
            foreach (var site in _sites)
            {
                regions.Add(site.GetRegion(plotBounds));
            }
            return regions;
        }
    }
}