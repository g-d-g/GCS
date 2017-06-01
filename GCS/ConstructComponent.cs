﻿using System.Linq;
using System.Collections.Generic;
using Grid.Framework;
using Grid.Framework.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GCS
{
    public class ConstructComponent : Renderable
    {
        private DrawState _drawState = DrawState.NONE;
        private bool _wasDrawing = false;
        private Vector2 _lastPoint = new Vector2();
        private List<Shape> _shapes;
        private Vector2 _pos;

        private readonly float _nearDistance = 4;
        private readonly float _nearDotDistance = 8;
        private List<(Shape, float)> _nearShapes;
        private List<Shape> _selectedShapes;

        public ConstructComponent()
        {
            _shapes = new List<Shape>();
            _nearShapes = new List<(Shape, float)>();
            _selectedShapes = new List<Shape>();
            OnCamera = false;
        }

        public void Clear()
        {
            _shapes.Clear();
        }

        public void ChangeState(DrawState state)
            => _drawState = state;

        private void UpdateLists(SpriteBatch sb)
        {
            foreach (var s in _shapes)
            {
                s.Draw(sb);
            }
        }

        private void AddShape(Shape shape)
        {
            var keypoints = new List<Shape>();
            foreach (var s in _shapes)
            {
                var dots = from d in Geometry.GetIntersect(shape, s)
                           select new Dot(d);
                if (dots.Count() != 0)
                    keypoints.AddRange(dots.Where(d => !_shapes.Contains(d)));
            }
            _shapes.Add(shape);
            _shapes.AddRange(keypoints);
        }

        public override void Update()
        {
            base.Update();
            //_pos = Camera.Current.GetRay(Mouse.GetState().Position.ToVector2());
            _pos = Mouse.GetState().Position.ToVector2();
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (_drawState == DrawState.CIRCLE || _drawState == DrawState.SEGMENT || _drawState == DrawState.LINE)
                {
                    if (!_wasDrawing)
                    {
                        _lastPoint = _pos;
                        _wasDrawing = true;
                    }
                }
            }
            if (_wasDrawing && Mouse.GetState().LeftButton == ButtonState.Released)
            {
                Dot last = new Dot(_lastPoint);
                AddShape(last);
                if (_drawState == DrawState.CIRCLE)
                {
                    float radius = Vector2.Distance(_pos, _lastPoint);
                    AddShape(new Circle(last, radius));
                }
                else if (_drawState == DrawState.SEGMENT)
                {
                    var p = new Dot(_pos);
                    AddShape(new Segment(last, p));
                    AddShape(p);
                }
                else if(_drawState == DrawState.LINE)
                {
                    var p = new Dot(_pos);
                    AddShape(new Line(last, p));
                    AddShape(p);
                }
                _wasDrawing = false;
                _drawState = DrawState.NONE;
            }

            if(_drawState == DrawState.NONE)
            {
                //선택, 가까이있는 점 선택
                foreach (var s in _shapes)
                {
                    var dist = Geometry.GetNearestDistance(s, _pos);
                    if (dist <= (s is Dot ? _nearDotDistance : _nearDistance))
                    {
                        if (!s.Focused)
                        {
                            _nearShapes.Add((s, dist));
                            s.Focused = true;
                        }
                    }
                    else if (s.Focused)
                    {
                        for(int i = 0; i< _nearShapes.Count;i++)
                        {
                            if(_nearShapes[i].Item1 == s)
                            {
                                _nearShapes.RemoveAt(i);
                                break;
                            }
                        }
                        s.Focused = false;
                    }
                }

                if(Scene.CurrentScene.IsLeftMouseDown && _nearShapes.Count > 0)
                {
                    Shape nearest = _nearShapes[0].Item1;
                    float dist = _nearDistance;
                    foreach (var (s, d) in _nearShapes)
                    {
                        if(s is Dot)
                        {
                            // 다 끝났다 그지 깽깽이들아!! 점이 우선순위 최고다!
                            nearest = s;
                            break;
                        }
                        if(dist > d)
                        {
                            nearest = s;
                            dist = d;
                        }
                    }
                    if (nearest.Selected)
                    {
                        _selectedShapes.Remove(nearest);
                        nearest.Selected = false;
                    }
                    else
                    {
                        _selectedShapes.Add(nearest);
                        nearest.Selected = true;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            //_pos = Camera.Current.GetRay(Mouse.GetState().Position.ToVector2());
            sb.BeginAA();
            _pos = Mouse.GetState().Position.ToVector2();
            if (_wasDrawing && _drawState == DrawState.CIRCLE)
            {
                float radius = (_pos - _lastPoint).Length();
                GUI.DrawCircle(sb, _lastPoint, radius, 2, Color.DarkGray, 100);
            }
            else if (_wasDrawing && _drawState == DrawState.SEGMENT)
            {
                GUI.DrawLine(sb, _lastPoint, _pos, 2, Color.DarkGray);
            }
            else if(_wasDrawing && _drawState == DrawState.LINE)
            {
                new Line(new Dot(_lastPoint), new Dot(_pos)).Draw(sb);
            }

            UpdateLists(sb);
            sb.End();
        }
    }
}
