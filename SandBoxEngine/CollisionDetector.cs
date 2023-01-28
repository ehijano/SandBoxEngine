using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SandBoxEngine
{
    class CollisionDetector
    {
        MyVector collisionNormal;
        Polygon polygon1, polygon2;
        List<MyVector> edges;
        MyVector X12;
        public CollisionDetector(Polygon poligon1, Polygon poligon2)
        {
            // For each edge of both polygons:
            // - Find the axis perpendicular to the current edge.
            // - Project both polygons on that axis.
            // - If these projections don't overlap, the polygons don't intersect(exit the loop).
            // - Once we have found that the polygons are going to collide, we calculate the translation needed to push the polygons apart. 
            // - The axis on which the projection overlapping is minimum will be the one on which the collision will take place. We will push the first polygon along this axis.
            this.polygon1 = poligon1;
            this.polygon2 = poligon2;
            this.edges = AllEdges(polygon1, polygon2);
            this.X12 = polygon1.CenterOfMass().Substract(polygon2.CenterOfMass());
        }

        private List<MyVector> AllEdges(Polygon p1, Polygon p2)
        {
            List<MyVector> e = new List<MyVector>();
            for (int i = 0; i < p1.edges.Count(); i++)
            {
                e.Add(p1.edges.ElementAt(i));
            }
            for (int i = 0; i < p2.edges.Count(); i++)
            {
                e.Add(p2.edges.ElementAt(i));
            }
            return e;
        }

        public Boolean intersect()
        {
            double projectedIntersection = -1;

            for (int i = 0; i < edges.Count(); i++)
            {
                MyVector p = edges.ElementAt(i).Perpendicular();
                if (p.DotProduct(X12) < 0)
                {
                    p = p.DivideBy(-1);
                }
                double pmin1, pmax1, pmin2, pmax2;
                (pmin1, pmax1) = minmaxProjection(polygon1.points, p);
                (pmin2, pmax2) = minmaxProjection(polygon2.points, p);

                double pmax = Math.Min(pmax1, pmax2);
                double pmin = Math.Max(pmin1, pmin2);

                if (pmax < pmin)
                {
                    return false;
                }
                else
                {
                    if ((projectedIntersection < 0) || (pmax - pmin < projectedIntersection))
                    {
                        projectedIntersection = pmax - pmin;
                        collisionNormal = p;
                    }
                }
            }
            return true;
        }

        private (MyVector, MyVector) FindBestEdge(Polygon poly, MyVector n)
        {
            int bestPoint = 0;
            int polygonSize = poly.points.Count();
            double pointProjection = poly.points.ElementAt(0).DotProduct(n);
            for (int pointIndex = 1; pointIndex < polygonSize; pointIndex++)
            {
                double newPointProjection = poly.points.ElementAt(pointIndex).DotProduct(n);
                if (pointProjection > newPointProjection)
                {
                    pointProjection = newPointProjection;
                    bestPoint = pointIndex;
                }
            }

            int previousPoint = (bestPoint - 1) % polygonSize;
            if (previousPoint < 0) { previousPoint += polygonSize; }
            int nextPoint = (bestPoint + 1) % polygonSize;

            MyVector pP = poly.points.ElementAt(previousPoint);
            MyVector pB = poly.points.ElementAt(bestPoint);
            MyVector pN = poly.points.ElementAt(nextPoint);

            MyVector edge1 = pB.Substract(pP).Normalize();
            MyVector edge2 = pN.Substract(pB).Normalize();

            if (Math.Abs(edge1.DotProduct(n)) < Math.Abs(edge2.DotProduct(n)))
            {
                return (pB, pP);
            }
            else
            {
                return (pB, pN);
            }
        }

        public MyVector contactPoint()
        {
            // Find colliding edges
            MyVector p1Best, p1Next, p2Best, p2Next; // These are the points of the two colliding edges
            MyVector piB, piN, prB, prN; // These will be the points of the colliding edges once a reference and an incident edge have been chosen
            (p1Best, p1Next) = FindBestEdge(polygon1, collisionNormal);
            (p2Best, p2Next) = FindBestEdge(polygon2, collisionNormal.DivideBy(-1));

            // Reference vs incident edge
            int flip = 1;
            MyVector edge1 = p1Best.Substract(p1Next).Normalize();
            MyVector edge2 = p2Best.Substract(p2Next).Normalize();
            MyVector referenceEdge, incidentEdge;
            if (Math.Abs(edge1.DotProduct(collisionNormal)) < Math.Abs(edge2.DotProduct(collisionNormal)))
            {
                referenceEdge = edge1;
                incidentEdge = edge2;
                piB = p2Best;
                piN = p2Next;
                prB = p1Best;
                prN = p1Next;
                // normal not modified, as polygon 1 is the reference, and the normal points towards the reference
                flip = 1;
            }
            else
            {
                referenceEdge = edge2;
                incidentEdge = edge1;
                piB = p1Best;
                piN = p1Next;
                prB = p2Best;
                prN = p2Next;
                // normal modified as polygon 2 is the one containing the reference, and so the normal must now point towards the second polygon
                flip = -1;
            }

            // Clipping.
            // First clip (prN)
            if (piN.DotProduct(referenceEdge) < prN.DotProduct(referenceEdge))
            {
                double eta = (prN.DotProduct(referenceEdge) - piN.DotProduct(referenceEdge)) / incidentEdge.DotProduct(referenceEdge);
                piN = piN.Add(incidentEdge.MultiplyBy(eta));
            }
            if (piB.DotProduct(referenceEdge) < prN.DotProduct(referenceEdge))
            {
                double eta = (prN.DotProduct(referenceEdge) - piB.DotProduct(referenceEdge)) / incidentEdge.DotProduct(referenceEdge);
                piB = piB.Add(incidentEdge.MultiplyBy(eta));
            }

            // Second clip (prB)
            if (piN.DotProduct(referenceEdge) > prB.DotProduct(referenceEdge))
            {
                double eta = (prB.DotProduct(referenceEdge) - piN.DotProduct(referenceEdge)) / incidentEdge.DotProduct(referenceEdge);
                piN = piN.Add(incidentEdge.MultiplyBy(eta));
            }
            if (piB.DotProduct(referenceEdge) > prB.DotProduct(referenceEdge))
            {
                double eta = (prB.DotProduct(referenceEdge) - piB.DotProduct(referenceEdge)) / incidentEdge.DotProduct(referenceEdge);
                piB = piB.Add(incidentEdge.MultiplyBy(eta));
            }

            // Third clip (along normal)
            MyVector normalToRef = collisionNormal.MultiplyBy(flip);
            if (piN.DotProduct(normalToRef) < prN.DotProduct(normalToRef)) // prN can be replaced by prB, as the entire edge is perp to normal
            {
                piN = piB;
            }
            if (piB.DotProduct(normalToRef) < prN.DotProduct(normalToRef)) // prN can be replaced by prB, as the entire edge is perp to normal
            {
                piB = piN;
            }


            return piB.Add(piN).DivideBy(2);

        }

        private (double, double) minmaxProjection(List<MyVector> points, MyVector p)
        {
            double pmin, pmax;

            pmin = points.ElementAt(0).DotProduct(p);
            pmax = points.ElementAt(0).DotProduct(p);

            for (int i = 0; i < points.Count(); i++)
            {
                pmin = Math.Min(pmin, points.ElementAt(i).DotProduct(p));
                pmax = Math.Max(pmax, points.ElementAt(i).DotProduct(p));
            }


            return (pmin, pmax);
        }

    }
}
