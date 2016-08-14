using System;
using System.Collections.Generic;
using System.Linq;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    internal sealed class SiteList
    {
        List<Site> _sites;
        int _currentIndex;
        bool _sorted;

        public SiteList()
        {
            _sites = new List<Site>();
            _sorted = false;
        }

        public void Dispose()
        {
            foreach(var site in _sites)
            {
                site.Dispose();
            }
            _sites.Clear();
        }

        public int Add(Site site)
        {
            _sorted = false;
            _sites.Add(site);
            return _sites.Count - 1;
        }

        public int Count { get { return _sites.Count; } }

        public Site Next()
        {
            if(!_sorted)
            {
                throw new Exception("SiteList::next(): sites have not been sorted");
            }
            if(_currentIndex < _sites.Count)
            {
                _currentIndex++;
                return _sites[_currentIndex - 1];
            }
            return null;
        }

        internal Rectangle GetSitesBounds()
        {
            if(!_sorted)
            {
                _sites.Sort(Site.SortAndReindex);
                _currentIndex = 0;
                _sorted = true;
            }

            if(_sites.Count == 0)
            {
                return new Rectangle(0, 0, 0, 0);
            }

            double xMin = double.MaxValue;
            double xMax = double.MinValue;
            foreach(var site in _sites)
            {
                if(site.Coord.X < xMin)
                {
                    xMin = site.Coord.X;
                }
                if(site.Coord.X > xMax)
                {
                    xMax = site.Coord.X;
                }
            }

            double yMin = _sites[0].Coord.Y;
            double yMax = _sites[_sites.Count - 1].Coord.Y;

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public List<Point> GetSiteCoords()
        {
            return _sites.Select(x => x.Coord).ToList();
        }

        // For infinitely sized regions, return a circle of radius 0
        public List<Circle> GetCircles()
        {
            var circles = new List<Circle>();
            foreach(var site in _sites)
            {
                double radius = 0;
                Edge nearest = site.GetNearestEdge();
                if(!nearest.IsPartOfConvexHull())
                {
                    radius = nearest.SitesDistance * 0.5f;
                }
                circles.Add(new Circle(site.Coord.X, site.Coord.Y, radius));
            }
            return circles;
        }

        public List<List<Point>> GetRegions(Rectangle plotBounds)
        {
            var regions = new List<List<Point>>();
            foreach(var site in _sites)
            {
                regions.Add(site.GetRegion(plotBounds));
            }
            return regions;
        }
    }
}
