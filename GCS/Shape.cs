﻿using System;
using Grid.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GCS
{
    public abstract partial class Shape
    {
        private static readonly float _nearDistance = 5;

        private ShapeManager _manager;

        internal List<Shape> Parents { get; private set; } = new List<Shape>();
        internal List<Shape> Childs { get; private set; } = new List<Shape>();

        public bool Enabled { get; set; } = true;

        public string Name { get; set; }
        public float Border { get; set; } = 2f;
        public Color Color { get; set; } = Color.Black;
        public bool Focused { get; set; } = false;
        public bool Selected { get; set; } = false;
        public abstract event Action Moved;
        /// <summary>
        /// 마우스를 떼면 Selected가 false가 되어야 하는가?
        /// </summary>
        internal bool UnSelect { get; set; } = false;
        /// <summary>
        /// 커서와의 거리
        /// </summary>
        public float Distance { get; set; } = -1;
        public virtual void Draw(SpriteBatch sb)
        {
            Color = Color.Black;
            if (Focused) Color = Color.Orange;
            if (Selected) Color = Color.Cyan;
        }
        public abstract void Move(Vector2 add);

        public Shape(ShapeManager manager)
        {
            _manager = manager;
        }

        public virtual bool IsEnoughClose(Vector2 coord)
            => Distance <= _nearDistance;

        public virtual void Update(Vector2 cursor)
        {
            Distance = Geometry.GetNearestDistance(this, cursor);
            if (IsEnoughClose(cursor))
                Focused = true;
            else if (Focused)
                Focused = false;
        }

        public void Delete()
        {
            _manager.DeleteShapes(Childs);
            foreach (var child in Childs)
                child.Delete();
        }

        public virtual void MovedFromParent() { }
        public virtual void MovedFromChild() { }
    }

    public class Circle : Shape
    {
        public static int Sides = 100;
        private Dot _center;
        public Dot Center
        {
            get => _center;
            set
            {
                if (_center != null) _center.Moved -= _center_Moved;
                _center = value;
                if (_center != null) _center.Moved += _center_Moved;

                Moved?.Invoke();
            }
        }
        private void _center_Moved()
        {
            Moved?.Invoke();
        }

        private Dot _another;
        public Dot Another
        {
            get => _another;
            set
            {
                if (_another != null) _another.Moved -= _another_Moved;
                _another = value;
                if (_another != null) _another.Moved += _another_Moved;
                Moved?.Invoke();
            }
        }

        private void _another_Moved()
        {
            Moved?.Invoke();
        }

        public float Radius => Vector2.Distance(Center.Coord, Another.Coord);
        public override event Action Moved;

        public Circle(Dot center, Dot another)
        {
            Center = center;
            Another = another;
            Center.dotParents.Add(this);
            Another.dotParents.Add(this);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (!(Enabled && Center.Enabled && Another.Enabled)) return;
            base.Draw(sb);
            GUI.DrawCircle(sb, Center.Coord, Radius, Border, Color, Sides);
        }

        public override void Move(Vector2 add)
        {
            if (!Center.Selected)
                Center.Move(add);
            if (!Another.Selected)
                Another.Move(add);
            Moved?.Invoke();
        }

        public override IParentRule GetNearDot(Dot dot)
            => new Rules.CircleRule(dot, this);
    }

    public abstract class LineLike : Shape
    {
        protected Dot _p1;
        public Dot Point1 { get => _p1; set { _p1 = value; _p1.Moved += dot_Moved; dot_Moved(); } }
        protected Dot _p2;
        public Dot Point2 { get => _p2; set { _p2 = value; _p2.Moved += dot_Moved; dot_Moved(); } }

        protected float _grad;
        public float Grad { get => _grad; set { _grad = value; dot_Moved(); } }
        protected float _yint;
        public float Yint { get => _yint; set { _yint = value; dot_Moved(); } }

        public override event Action Moved;

        public LineLike(Dot p1, Dot p2)
        {
            _p1 = p1;
            _p2 = p2;
            _p1.Moved += () => { ResetAB(); Moved?.Invoke(); };
            _p2.Moved += () => { ResetAB(); Moved?.Invoke(); };
            Point1.dotParents.Add(this);
            Point2.dotParents.Add(this);
            ResetAB();
        }

        protected void dot_Moved()
        {
            ResetAB();
            Moved?.Invoke();
        }

        protected void ResetAB()
        {
            _grad = (Point2.Coord - Point1.Coord).Y / (Point2.Coord - Point1.Coord).X;
            _yint = (Point1.Coord.Y) - Grad * Point1.Coord.X;
        }

        protected void ResetPoints()
        {
            _p1 = new Dot(0, Yint);
            _p2 = new Dot(1, Grad + Yint);
            _p1.Moved += dot_Moved;
            _p2.Moved += dot_Moved;
        }

        public override void Move(Vector2 add)
        {
            if (!_p1.Selected)
                _p1.Move(add);
            if (!_p2.Selected)
                _p2.Move(add);
            ResetAB();

            Moved?.Invoke();
        }
    }

    public partial class Line : LineLike
    {
        public Line(Dot p1, Dot p2) : base(p1, p2)
        {

        }
        
        
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            if (!(Enabled && Point1.Enabled && Point2.Enabled)) return;
            GUI.DrawLine(sb, new Vector2(0, Yint), new Vector2(Scene.CurrentScene.ScreenBounds.X, Scene.CurrentScene.ScreenBounds.X * Grad + Yint), Border, Color);
        }
        
        public override IParentRule GetNearDot(Dot dot)
            => new Rules.LineRule(dot, this);
    }

    public class Segment : LineLike
    {
        public Segment(Dot p1, Dot p2) : base(p1, p2)
        {

        }
        
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            if (!(Enabled && Point1.Enabled && Point2.Enabled)) return;
            GUI.DrawLine(sb, Point1.Coord, Point2.Coord, Border, Color);
        }

        public Line ToLine()
        {
            return new Line(Point1, Point2);
        }

        public override IParentRule GetNearDot(Dot dot)
            => new Rules.SegmentRule(dot, this);
    }
    
    public class Vector : Segment
    {
        readonly static float arrowAngle = (float)Math.PI/6;
        readonly static float arrowlength = 20;

        //Point2 가 종점임.
        public Vector(Dot p1, Dot p2) : base(p1, p2)
        {
           
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            Vector2 del = Point2.Coord - Point1.Coord;
            Vector2 delta1, delta2;
            float angle = (float)Math.Atan(del.Y/del.X);
            Vector2 Initvector = new Vector2(Point1.Coord.X + Vector2.Distance(Point1.Coord, Point2.Coord), Point1.Coord.Y);
            delta1 = new Vector2((float)(Initvector.X - arrowlength*Math.Cos(arrowAngle)) , (float)(Initvector.Y + arrowlength*Math.Sin(arrowAngle)));
            delta2 = new Vector2((float)(Initvector.X - arrowlength * Math.Cos(arrowAngle)), (float)(Initvector.Y - arrowlength * Math.Sin(arrowAngle)));
            if(del.X >0)
            {
                GUI.DrawLine(sb, Point2.Coord, Point1.Coord + Geometry.Rotate(delta1 - Point1.Coord, angle), Border, Color);
                GUI.DrawLine(sb, Point2.Coord, Point1.Coord + Geometry.Rotate(delta2 - Point1.Coord, angle), Border, Color);
            }
            else
            {
                GUI.DrawLine(sb, Point2.Coord, Point1.Coord - Geometry.Rotate(delta1 - Point1.Coord, angle), Border, Color);
                GUI.DrawLine(sb, Point2.Coord, Point1.Coord - Geometry.Rotate(delta2 - Point1.Coord, angle), Border, Color);
            }

        }
    }

    public class Dot : Shape
    {
        private static readonly float _nearDotDistance = 10;

        private Vector2 _coord;
        public Vector2 Coord { get => _coord; set { MoveTo(_coord); } }
        public List<Shape> dotParents = new List<Shape>();
        private IParentRule _rule;
        public IParentRule Rule
        {
            get => _rule;
            set
            {
                if (_rule != null) _rule.MoveTo -= _rule_MoveTo;
                _rule = value;
                if (value != null) value.MoveTo += _rule_MoveTo;
            }
        }

        public override event Action Moved;

        public Dot(Vector2 coord)
        {
            _coord = coord;
            Color = Color.OrangeRed;
        }

        public Dot(float x, float y) : this(new Vector2(x, y))
        { }

        public override bool Equals(object obj)
        {
            var o = obj as Dot;
            return o == null ? false : Coord.Equals(o.Coord);
        }

        public override int GetHashCode()
            => Coord.GetHashCode();

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            if (!Enabled) return;
            GUI.DrawCircle(sb, Coord, 4f, Border, Color, 20);
            // GUI.DrawPoint(sb, Coord, Border, Color);
        }

        private void _rule_MoveTo(Vector2 obj)
        {
            if (fixed_triggerd) return;
            MoveTo(obj);
        }

        bool fixed_triggerd = false;
        public void MoveTo(Vector2 to)
        {
            fixed_triggerd = true;
            if (Rule != null)
            {
                _coord = Rule.FixedCoord(to);
            }
            else
                _coord = to;

            Moved?.Invoke();
            fixed_triggerd = false;
        }

        public override void Move(Vector2 add)
            => MoveTo(Coord + add);

        public override bool IsEnoughClose(Vector2 coord)
            => Distance <= _nearDotDistance;

        public override IParentRule GetNearDot(Dot dot)
        {
            throw new ArgumentException("Dot에서 이 메서드가 호출되면 안되지 바보야");
        }

        public void Attach(Dot dot)
        {
            dot._coord = this.Coord;
            Rules.FollowRule rule = new Rules.FollowRule(dot, this);
        }

        public void Detach(Dot dot)
        {
            dot.Rule = null;
            dot.Rule.Dispose();
        }

        internal void SetCoordForce(Vector2 coord)
        {
            _coord = coord;
        }
    }
}
