using System;
using System.Collections.Generic;

namespace coneil.Math.Voronoi
{
    // Delaunay tri, specifically
    internal sealed class Triangle
    {
        public Site[] Sites { get; private set; }

        public Triangle(Site a, Site b, Site c)
        {
            Sites = new Site[3] { a, b, c };
        }
    }
}
