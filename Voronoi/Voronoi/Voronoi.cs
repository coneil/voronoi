using System;
using System.Collections.Generic;
using System.Linq;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    public sealed class Voronoi
    {
        static void Main(string[] args)
        {
            const int NUM_POINTS = 1 << 14;
            const int SIZE = 1000;

            Random rnd = new Random();
            List<Point> points = new List<Point>();
            for(int i = 0; i < NUM_POINTS; i++)
            {
                var p = new Point(rnd.NextDouble() * SIZE, rnd.NextDouble() * SIZE);
                points.Add(p);
            }
            
            
            var v = new Voronoi(points, new List<uint>(), new Rectangle(0, 0, SIZE, SIZE));
            System.Diagnostics.Debug.WriteLine(v.Edges.Count);
        }

        public Rectangle PlotBounds { get; private set; }
        public List<Edge> Edges { get; private set; }

        SiteList _sites;
        Dictionary<Point, Site> _sitesIndexedByLocation;

        public Voronoi(List<Point> points, List<uint> colors, Rectangle plotBounds)
        {
            _sites = new SiteList();
            _sitesIndexedByLocation = new Dictionary<Point, Site>();
            
            AddSites(points, colors);

            PlotBounds = plotBounds;
            Edges = new List<Edge>();

            RunFortunes();
        }

        public void Dispose()
        {
            if(_sites != null) _sites.Dispose();
            if(Edges != null)
            {
                foreach(var edge in Edges)
                {
                    edge.Dispose();
                }
                Edges.Clear();
            }
            _sitesIndexedByLocation.Clear();
        }

        void AddSites(List<Point> points, List<uint> colors)
        {
            for(int i = 0; i < points.Count; i++)
            {
                AddSite(points[i], colors.Count > i ? colors[i] : 0, i);
            }
        }

        void AddSite(Point p, uint c, int index)
        {
            double weight = new System.Random().NextDouble() * 100;
            Site site = Site.Create(p, index, weight, c);
            _sites.Add(site);
            _sitesIndexedByLocation[p] = site;
        }

        public List<Point> GetRegion(Point p)
        {
            Site site;
            if(_sitesIndexedByLocation.TryGetValue(p, out site))
            {
                return site.GetRegion(PlotBounds);
            }

            return new List<Point>();
        }

        public List<Point> GetNeighborSitesForSite(Point coord)
        {
            Site site;
            if(_sitesIndexedByLocation.TryGetValue(coord, out site))
            {
                var sites = site.GetNeighborSites();
                return sites.Select(x => x.Coord).ToList();   
            }

            return new List<Point>();
        }

        public List<Circle> GetCircles()
        {
            return _sites.GetCircles();
        }

        public List<LineSegment> GetVoronoiBoundaryForSite(Point coord)
        {
            return Utils.GetVisibleLineSegments(Utils.SelectEdgesForSitePoint(coord, Edges));
        }

        public List<LineSegment> GetDelaunayLinesForSite(Point coord)
        {
            return Utils.GetDelaunayLinesForEdges(Utils.SelectEdgesForSitePoint(coord, Edges));
        }

        public List<LineSegment> GetVoronoiDiagram()
        {
            return Utils.GetVisibleLineSegments(Edges);
        }

        public List<LineSegment> GetDelaunayTriangulation()
        {
            return Utils.GetDelaunayLinesForEdges(Edges);
        }

        public List<LineSegment> Hull()
        {
            return Utils.GetDelaunayLinesForEdges(GetHullEdges());
        }

        List<Edge> GetHullEdges()
        {
            return Edges.Where(x => x.IsPartOfConvexHull()).ToList();
        }

        public List<Point> GetHullPointsInOrder()
        {
            var edges = GetHullEdges();
            var points = new List<Point>();


            if(edges.Count == 0) return points;

            var reorderer = new EdgeReorderer(edges, EdgeReorderer.Criterion.SITE);
            edges = reorderer.Edges;
            var orientations = reorderer.EdgeOrientations;

            LR orientation;
            for(int i = 0; i < edges.Count; i++)
            {
                orientation = orientations[i];
                if(orientation == LR.LEFT)
                {
                    points.Add(edges[i].LeftSite.Coord);
                }
                else
                {
                    points.Add(edges[i].RightSite.Coord);
                }
            }
            return points;
        }

        public List<LineSegment> GetSpanningTree(string type)
        {
            var segments = Utils.GetDelaunayLinesForEdges(Edges);
            return Kruskal(segments, type);
        }

        public List<List<Point>> GetRegions()
        {
            return _sites.GetRegions(PlotBounds);
        }

        public List<Point> GetSiteCoords()
        {
            return _sites.GetSiteCoords();
        }

        void RunFortunes()
        {
            Rectangle dataBounds = _sites.GetSitesBounds();
            int sqrt_nsites = Convert.ToInt32(System.Math.Sqrt(_sites.Count + 4));
            var heap = new HalfEdgePriorityQueue(dataBounds.Y, dataBounds.Height, sqrt_nsites);
            var edgeList = new EdgeList(dataBounds.X, dataBounds.Width, sqrt_nsites);
            var halfEdges = new List<HalfEdge>();
            var vertices = new List<Point>();

            var bottomMostSite = _sites.Next();
            var newSite = _sites.Next();
            Point newintstar = new Point(0, 0);

            Point vertex;
            Site bottomSite, topSite, tempSite;
            HalfEdge lbnd, rbnd, llbnd, rrbnd, bisector;
            Edge edge;

            Func<HalfEdge, Site> rightRegion = (HalfEdge he) => {
                if(he.Edge == null)
                    return bottomMostSite;

                if(he.LeftRight == LR.LEFT)
                {
                    return he.Edge.RightSite;
                }
                return he.Edge.LeftSite;
            };

            Func<HalfEdge, Site> leftRegion = (HalfEdge he) => {
                if(he.Edge == null)
                    return bottomMostSite;

                if(he.LeftRight == LR.LEFT)
                {
                    return he.Edge.LeftSite;
                }
                return he.Edge.RightSite;
            };
            
            while(true)
            {
                if(!heap.IsEmpty())
                {
                    newintstar = heap.Min();
                }

                if(newSite != null && (heap.IsEmpty() || CompareSiteByPoint(newSite, newintstar) < 0))
                {
                    // new site is smallest

                    // Step 8
                    lbnd = edgeList.GetEdgeListLeftNeighbor(newSite.Coord);
                    rbnd = lbnd.EdgeListRightNeighbor;
                    bottomSite = rightRegion(lbnd);

                    // Step 9
                    edge = Edge.CreateBisectingEdge(bottomSite, newSite);
                    Edges.Add(edge);

                    bisector = HalfEdge.Create(edge, LR.LEFT);
                    halfEdges.Add(bisector);
                    // Step 10
                    edgeList.Insert(lbnd, bisector);

                    // Step 11
                    vertex = HalfEdge.GetIntersection(lbnd, bisector);
                    if(!vertex.IsUnassigned())
                    {
                        vertices.Add(vertex);
                        heap.Remove(lbnd);
                        lbnd.Vertex = vertex;
                        lbnd.YStar = vertex.Y + Point.Distance(newSite.Coord, vertex);
                        heap.Insert(lbnd);
                    }

                    lbnd = bisector;
                    bisector = HalfEdge.Create(edge, LR.RIGHT);
                    halfEdges.Add(bisector);

                    // Second half of Step 10
                    edgeList.Insert(lbnd, bisector);

                    // Second half of Step 11
                    vertex = HalfEdge.GetIntersection(bisector, rbnd);
                    if(!vertex.IsUnassigned())
                    {
                        vertices.Add(vertex);
                        bisector.Vertex = vertex;
                        bisector.YStar = vertex.Y + Point.Distance(newSite.Coord, vertex);
                        heap.Insert(bisector);
                    }

                    newSite = _sites.Next();
                }
                else if(!heap.IsEmpty())
                {
                    // intersection is smallest
                    lbnd = heap.ExtractMin();
                    llbnd = lbnd.EdgeListLeftNeighbor;
                    rbnd = lbnd.EdgeListRightNeighbor;
                    rrbnd = rbnd.EdgeListRightNeighbor;
                    bottomSite = leftRegion(lbnd);
                    topSite = rightRegion(rbnd);

                    Point v = lbnd.Vertex;
                    lbnd.Edge.SetVertex(lbnd.LeftRight, v);
                    rbnd.Edge.SetVertex(rbnd.LeftRight, v);
                    edgeList.Remove(lbnd);
                    heap.Remove(rbnd);
                    edgeList.Remove(rbnd);
                    LR leftRight = LR.LEFT;
                    if(bottomSite.Coord.Y > topSite.Coord.Y)
                    {
                        tempSite = bottomSite;
                        bottomSite = topSite;
                        topSite = tempSite;
                        leftRight = LR.RIGHT;
                    }

                    edge = Edge.CreateBisectingEdge(bottomSite, topSite);
                    Edges.Add(edge);
                    bisector = HalfEdge.Create(edge, leftRight);
                    halfEdges.Add(bisector);
                    edgeList.Insert(llbnd, bisector);
                    edge.SetVertex(leftRight == LR.LEFT ? LR.RIGHT : LR.LEFT, v);

                    vertex = HalfEdge.GetIntersection(llbnd, bisector);
                    if(!vertex.IsUnassigned())
                    {
                        vertices.Add(vertex);
                        heap.Remove(llbnd);
                        llbnd.Vertex = vertex;
                        llbnd.YStar = vertex.Y + Point.Distance(bottomSite.Coord, vertex);
                        heap.Insert(llbnd);
                    }

                    vertex = HalfEdge.GetIntersection(bisector, rrbnd);
                    if(!vertex.IsUnassigned())
                    {
                        vertices.Add(vertex);
                        bisector.Vertex = vertex;
                        bisector.YStar = vertex.Y + Point.Distance(bottomSite.Coord, vertex);
                        heap.Insert(bisector);
                    }
                }
                else
                {
                    break;
                }

            }

            heap.Dispose();
            edgeList.Dispose();

            foreach(var he in halfEdges)
            {
                he.ForceDispose();
            }
            halfEdges.Clear();

            foreach(var e in Edges)
            {
                e.ClipVertices(PlotBounds.Left, PlotBounds.Right, PlotBounds.Top, PlotBounds.Bottom);
            }

            vertices.Clear();
        }

        int CompareSiteByPoint(Site s1, Point p1)
        {
            if(s1.Coord.Y < p1.Y) return -1;
            if(s1.Coord.Y > p1.Y) return 1;
            if(s1.Coord.X < p1.X) return -1;
            if(s1.Coord.X > p1.X) return 1;
            return 0;
        }

        internal List<LineSegment> Kruskal(List<LineSegment> lineSegments, string type = "minimum")
        {
            Dictionary<Point, Node> nodes = new Dictionary<Point, Node>();
            List<LineSegment> mst = new List<LineSegment>();
            var pool = Node.Pool;

            if(type == "maximum")
            {
                lineSegments = lineSegments.OrderBy(x => Point.Distance(x.P0, x.P1)).ToList();
            }
            else
            {
                lineSegments = lineSegments.OrderByDescending(x => Point.Distance(x.P0, x.P1)).ToList();
            }

            for(int i = lineSegments.Count - 1; i >= 0; i--)
            {
                LineSegment lineSegment = lineSegments[i];

                Node rootOfSet0, rootOfSet1;

                Node node0 = nodes.ContainsKey(lineSegment.P0) ? nodes[lineSegment.P0] : null;
                if(node0 == null)
                {
                    if(pool.Count > 0)
                    {
                        node0 = pool[pool.Count - 1];
                        pool.RemoveAt(pool.Count - 1);
                    }
                    else
                    {
                        node0 = new Node();
                    }

                    rootOfSet0 = node0.Parent = node0;
                    node0.TreeSize = 1;

                    nodes[lineSegment.P0] = node0;
                }
                else
                {
                    rootOfSet0 = FindNode(node0);
                }

                Node node1 = nodes.ContainsKey(lineSegment.P1) ? nodes[lineSegment.P1] : null;
                if(node1 == null)
                {
                    if(pool.Count > 0)
                    {
                        node1 = pool[pool.Count - 1];
                        pool.RemoveAt(pool.Count - 1);
                    }
                    else
                    {
                        node1 = new Node();
                    }

                    rootOfSet1 = node1.Parent = node1;
                    node1.TreeSize = 1;

                    nodes[lineSegment.P1] = node1;
                }
                else
                {
                    rootOfSet1 = FindNode(node1);
                }

                if(rootOfSet0 != rootOfSet1)
                {
                    mst.Add(lineSegment);

                    int treeSize0 = rootOfSet0.TreeSize;
                    int treeSize1 = rootOfSet1.TreeSize;
                    if(treeSize0 >= treeSize1)
                    {
                        rootOfSet1.Parent = rootOfSet0;
                        rootOfSet0.TreeSize += treeSize1;
                    }
                    else
                    {
                        rootOfSet0.Parent = rootOfSet1;
                        rootOfSet1.TreeSize += treeSize0;
                    }
                }
            }

            foreach(var node in nodes)
            {
                pool.Add(node.Value);
            }

            nodes.Clear();

            return mst;
        }

        internal Node FindNode(Node node)
        {
            if(node.Parent == null || node.Parent == node)
            {
                return node;
            }
            else
            {
                Node root = FindNode(node.Parent);
                node.Parent = root;
                return root;
            }
        }

        internal class Node
        {
            public static List<Node> Pool = new List<Node>();

            public Node Parent;
            public int TreeSize;
        }
    }
}