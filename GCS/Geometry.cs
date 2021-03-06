﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace GCS
{

    public static class Geometry
    {
        public static Vector2[] GetIntersect(Shape shape1, Shape shape2)
        {
            if (shape1 is Segment)
            {
                if (shape2 is Segment)
                    return getIntersect(shape1 as Segment, shape2 as Segment);
                else if (shape2 is Line)
                    return getIntersect(shape2 as Line, shape1 as Segment);
                else if (shape2 is Circle)
                    return getIntersect(shape1 as Segment, shape2 as Circle);
            }
            else if (shape1 is Line)
            {
                if (shape2 is Segment)
                    return getIntersect(shape1 as Line, shape2 as Segment);
                else if (shape2 is Line)
                    return getIntersect(shape1 as Line, shape2 as Line);
                else if (shape2 is Circle)
                    return getIntersect(shape1 as Line, shape2 as Circle);
            }
            else if (shape1 is Circle)
            {
                if (shape2 is Segment)
                    return getIntersect(shape2 as Segment, shape1 as Circle);
                else if (shape2 is Line)
                    return getIntersect(shape2 as Line, shape1 as Circle);
                else if (shape2 is Circle)
                    return getIntersect(shape1 as Circle, shape2 as Circle);
            }
            if (shape1 is Dot || shape2 is Dot) return new Vector2[] { };
            throw new ArgumentException("뀨;;");
        }

        private static Vector2[] getIntersect(Segment line1, Segment line2)
        {
            if (line1.Grad == line2.Grad && line1.Yint == line2.Yint) { throw new NotFiniteNumberException("일치"); }
            else if (line1.Grad == line2.Grad) { return new Vector2[] { }; }

            float intersectx = (line2.Yint - line1.Yint) / (line1.Grad - line2.Grad);

            Vector2 intersect = new Vector2(intersectx, intersectx * line1.Grad + line1.Yint);
            if (Vector2.Distance(intersect, line1.Point1.Coord) < Vector2.Distance(line1.Point1.Coord, line1.Point2.Coord) &&
                Vector2.Distance(intersect, line1.Point2.Coord) < Vector2.Distance(line1.Point1.Coord, line1.Point2.Coord) &&
                Vector2.Distance(intersect, line2.Point1.Coord) < Vector2.Distance(line2.Point1.Coord, line2.Point2.Coord) &&
                Vector2.Distance(intersect, line2.Point2.Coord) < Vector2.Distance(line2.Point1.Coord, line2.Point2.Coord))
            {
                return new Vector2[] { intersect };
            }
            else return new Vector2[] { };
        }

        private static Vector2[] getIntersect(Line line1, Segment line2)
        {
            if (line1.Grad == line2.Grad && line1.Yint == line2.Yint) { throw new NotFiniteNumberException("일치"); }
            else if (line1.Grad == line2.Grad) { return new Vector2[] { }; }

            float intersectx = (line2.Yint - line1.Yint) / (line1.Grad - line2.Grad);
            Vector2 intersect = new Vector2(intersectx, intersectx * line1.Grad + line1.Yint);
            if (Vector2.Distance(intersect, line2.Point1.Coord) < Vector2.Distance(line2.Point1.Coord, line2.Point2.Coord) &&
                Vector2.Distance(intersect, line2.Point2.Coord) < Vector2.Distance(line2.Point1.Coord, line2.Point2.Coord))
            {
                return new Vector2[] { intersect };
            }
            else return new Vector2[] { };

        }

        private static Vector2[] getIntersect(Line line1, Line line2)
        {
            if (line1.Grad == line2.Grad && line1.Yint == line2.Yint) { throw new NotFiniteNumberException("일치"); }
            else if (line1.Grad == line2.Grad) { return new Vector2[] { }; }

            float intersectx = (line2.Yint - line1.Yint) / (line1.Grad - line2.Grad);
            return new Vector2[] { new Vector2(intersectx, intersectx * line1.Grad + line1.Yint) };
        }

        private static Vector2[] getIntersect(Line line, Circle circle)
        {
            Vector2 d = line.Point2.Coord - line.Point1.Coord;
            float dr = d.Length();
            float D = (line.Point1.Coord.X - circle.Center.Coord.X) * (line.Point2.Coord.Y - circle.Center.Coord.Y)
                - (line.Point2.Coord.X - circle.Center.Coord.X) * (line.Point1.Coord.Y - circle.Center.Coord.Y);
            float discriminant = circle.Radius * circle.Radius * dr * dr - D * D;

            if (discriminant < 0)
                return new Vector2[] { };
            else if (discriminant == 0)
                return new[] { new Vector2(D * d.Y / (dr * dr) + circle.Center.Coord.X, -D * d.X / (dr * dr) + circle.Center.Coord.Y) };
            else
            {
                float x = D * d.Y / (dr * dr) + circle.Center.Coord.X;
                float y = -D * d.X / (dr * dr) + circle.Center.Coord.Y;
                float sgnDy = d.Y < 0 ? -1 : 1;
                float xd = sgnDy * d.X * (float)Math.Sqrt(discriminant) / (dr * dr);
                float yd = Math.Abs(d.Y) * (float)Math.Sqrt(discriminant) / (dr * dr);

                return new[]
                {
                    new Vector2(x + xd, y + yd),
                    new Vector2(x - xd, y - yd)
                };
            }
        }

        private static Vector2[] getIntersect(Segment line, Circle circle)
        {
            Vector2 d = line.Point2.Coord - line.Point1.Coord;
            float dr = d.Length();
            float D = (line.Point1.Coord.X - circle.Center.Coord.X) * (line.Point2.Coord.Y - circle.Center.Coord.Y)
                - (line.Point2.Coord.X - circle.Center.Coord.X) * (line.Point1.Coord.Y - circle.Center.Coord.Y);
            float discriminant = circle.Radius * circle.Radius * dr * dr - D * D;

            if (discriminant < 0)
                return new Vector2[] { };
            else if (discriminant == 0)
                return new[] { new Vector2(D * d.Y / (dr * dr) + circle.Center.Coord.X, -D * d.X / (dr * dr) + circle.Center.Coord.Y) };
            else
            {
                float x = D * d.Y / (dr * dr) + circle.Center.Coord.X;
                float y = -D * d.X / (dr * dr) + circle.Center.Coord.Y;
                float sgnDy = d.Y < 0 ? -1 : 1;
                float xd = sgnDy * d.X * (float)Math.Sqrt(discriminant) / (dr * dr);
                float yd = Math.Abs(d.Y) * (float)Math.Sqrt(discriminant) / (dr * dr);

                Vector2 intersect1 = new Vector2(x + xd, y + yd);
                Vector2 intersect2 = new Vector2(x - xd, y - yd);
                float len = Vector2.Distance(line.Point1.Coord, line.Point2.Coord);

                if (Vector2.Distance(intersect1, line.Point1.Coord) < len && Vector2.Distance(intersect1, line.Point2.Coord) < len)
                {
                    if (Vector2.Distance(intersect2, line.Point1.Coord) < len && Vector2.Distance(intersect2, line.Point2.Coord) < len)
                        return new Vector2[] { intersect1, intersect2 };
                    else return new Vector2[] { intersect1 };
                }
                else if (Vector2.Distance(intersect2, line.Point1.Coord) < len && Vector2.Distance(intersect2, line.Point2.Coord) < len)
                    return new Vector2[] { intersect2 };
                else return new Vector2[] { };
            }
        }

        private static Vector2[] getIntersect(Circle circle1, Circle circle2)
        {
            float distance = (float)Math.Sqrt(Math.Pow(circle2.Center.Coord.X - circle1.Center.Coord.X, 2) + Math.Pow(circle2.Center.Coord.Y - circle1.Center.Coord.Y, 2));

            if (distance == 0) return new Vector2[] { };
            else if (distance > circle1.Radius + circle2.Radius) //두 원이 밖에 있으면서 만나지 않음.
            {
                return new Vector2[] { };

            }
            else if ((float)Math.Abs(circle1.Radius - circle2.Radius) > distance) // 두 원 중 하나가 서로를 포함.
            {
                return new Vector2[] { };
            }
            else
            {
                float d = (circle1.Center.Coord - circle2.Center.Coord).Length();
                float r1 = circle1.Radius;
                float r2 = circle2.Radius;

                float x = (d * d + r1 * r1 - r2 * r2) / (2 * d);

                Vector2 p1 = circle1.Center.Coord + (circle2.Center.Coord - circle1.Center.Coord) * x / d;
                Vector2 v1 = circle1.Center.Coord - circle2.Center.Coord;
                v1 = new Vector2(-v1.Y, v1.X);
                Vector2 p2 = p1 + v1;
                return getIntersect(new Line(new Dot(p1), new Dot(p2)), circle1);
            }
        }

        public static Vector2 GetNearest(Shape shape, Vector2 point)
        {
            if (shape is Segment) return getNearest(shape as Segment, point);
            if (shape is Line) return getNearest(shape as Line, point);
            if (shape is Circle) return getNearest(shape as Circle, point);
            if (shape is Dot) return (shape as Dot).Coord;
            throw new ArgumentException("뀨우;");
        }

        private static Vector2 getNearest(Line line, Vector2 point)
        {
            Vector2 temppoint = point - new Vector2(0, line.Yint);
            Line templine = new Line(new Dot(line.Point1.Coord - new Vector2(0, line.Yint)), new Dot(line.Point2.Coord - new Vector2(0, line.Yint)));
            Line orthogonal = new Line(new Dot(temppoint), new Dot(temppoint.X + templine.Grad * 10, temppoint.Y - 10));
            Vector2 result = getIntersect(templine, orthogonal)[0] + new Vector2(0, line.Yint);
            return result;
        }

        private static Vector2 getNearest(Segment line, Vector2 point)
        {
            Vector2 temppoint = point - new Vector2(0, line.Yint);
            Line templine = new Line(new Dot(line.Point1.Coord - new Vector2(0, line.Yint)), new Dot(line.Point2.Coord - new Vector2(0, line.Yint)));
            Line orthogonal = new Line(new Dot(temppoint), new Dot(temppoint.X + templine.Grad * 10, temppoint.Y - 10));
            Vector2 result = getIntersect(templine, orthogonal)[0] + new Vector2(0, line.Yint);
            return Vector2.Clamp(result, Vector2.Min(line.Point1.Coord, line.Point2.Coord), Vector2.Max(line.Point1.Coord, line.Point2.Coord));
        }

        private static Vector2 getNearest(Circle circle, Vector2 point)
        {
            if (circle.Center.Coord == point) { return circle.Center.Coord; }
            Vector2[] res = getIntersect(new Line(circle.Center, new Dot(point)), circle);
            return Vector2.Distance(res[0], point) < Vector2.Distance(res[1], point) ? res[0] : res[1];
        }

        public static float GetNearestDistance(Shape shape, Vector2 point)
        {
            if (shape is Line)
            {
                Line line = (Line)shape;
                return (float)(Math.Abs(line.Grad * point.X - point.Y + line.Yint) / Math.Sqrt(line.Grad * line.Grad + 1));
            }
            if (shape is Segment)
            {
                Segment line = (Segment)shape;
                return Vector2.Distance(point, getNearest(line, point));
            }
            if (shape is Circle) return Math.Abs(Vector2.Distance((shape as Circle).Center.Coord, point) - ((shape as Circle).Radius));
            if (shape is Dot) return Vector2.Distance(point, (shape as Dot).Coord);

            throw new ArgumentException("뀨우;;;");
        }
        public static Vector2 Rotate(Vector2 vec, float angle)
        {
                return new Vector2((float)(vec.X * Math.Cos(angle) - vec.Y * Math.Sin(angle)), (float)(vec.X * Math.Sin(angle) + vec.Y * Math.Cos(angle)));
        }
    }
}
