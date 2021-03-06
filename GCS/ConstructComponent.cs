﻿using System.Linq;
using System.Collections.Generic;
using Grid.Framework;
using Grid.Framework.Components;
using Grid.Framework.GUIs;
using GCS.Rules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GCS
{
    public class ConstructComponent : Renderable
    {
        private DrawState _drawState = DrawState.NONE;
        private bool _wasDrawing = false;
        private bool _isDragging = false;
        private Dot _lastPoint = new Dot(0, 0);
        private List<Shape> _shapes;
        private Vector2 _pos;
        private List<Shape> _nearShapes;
        private List<Shape> _selectedShapes;
        private List<IParentRule> _currentRules;
        private Stack<ImportantAction> _doneActions;
        private MenuStrip _shapeMenuStrip;

        public ConstructComponent()
        {
            _shapes = new List<Shape>();
            _nearShapes = new List<Shape>();
            _selectedShapes = new List<Shape>();
            _currentRules = new List<IParentRule>();
            _doneActions = new Stack<ImportantAction>();
            OnCamera = false;
        }

        public override void Start()
        {
            base.Start();

            _shapeMenuStrip = new MenuStrip();
            _shapeMenuStrip.Items.Add(new MenuStripItem("Delete"));
            Scene.CurrentScene.GuiManager.GetComponent<GUIManager>().GUIs.Add(_shapeMenuStrip);
        }

        public void Clear()
        {
            _doneActions.Push(new ImportantAction(userActions.CLEAR, _shapes.ToArray() , null));
            _shapes.Clear();
            _selectedShapes.Clear();
        }

        public void DeleteSelected()
        {
            Delete(_selectedShapes);
            _doneActions.Push(new ImportantAction(userActions.DELETE, _selectedShapes.ToArray(), null));
            _selectedShapes.Clear();
        }

        private void Delete(IEnumerable<Shape> target)
        {
            var targetList = target.ToList();
            var parents = from s in target
                          where s is Dot
                          from p in (s as Dot).dotParents
                          select p;

            targetList.AddRange(parents.ToArray());

            foreach (var shape in targetList)
            {
                _shapes.Remove(shape);
                foreach (IParentRule r in _currentRules)
                {
                    if (r.IsParent(shape))
                    {
                        shape.DetachRule(r);
                    }
                }
            }
        }

        public void Undo() 
        {
            if (_doneActions.Count == 0) return;
            ImportantAction ac = _doneActions.Pop();
            switch (ac.action)
            {
                case userActions.CREATE:
                    Delete(ac.subject.ToList());
                    
                    while (_doneActions.Count != 0)
                    {
                        if (!_shapes.Contains(_doneActions.Peek().subject[0]))
                            _doneActions.Pop();
                        else break;
                    }
                    break;
                case userActions.CLEAR:
                    _shapes.AddRange(ac.subject.ToList());
                    break;
                case userActions.MOVE:
                    break;
                case userActions.DELETE:
                    break;
                case userActions.MERGE:
                    break;
            }

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
            if (_shapes.Contains(shape)) return;
            _shapes.Add(shape);
        }

        private void Select(Shape shape)
        {
            shape.Selected = true;
            shape.UnSelect = false;
            _selectedShapes.Add(shape);
        }

        private void Unselect(Shape shape)
        {
            shape.UnSelect = true;
            shape.Selected = false;
            _selectedShapes.Remove(shape);
        }

        private void UnselectAll()
        {
            _selectedShapes.ForEach(s => { s.UnSelect = true; s.Selected = false; });
            _selectedShapes.Clear();
        }

        public override void Update()
        {
            base.Update();
            //_pos = Camera.Current.GetRay(Mouse.GetState().Position.ToVector2());
            _pos = Mouse.GetState().Position.ToVector2();
            foreach (var s in _shapes) s.Update(_pos);
            //선택, 가까이있는 점 선택
            _nearShapes = _shapes.Where(s => s.Focused).ToList();

            UpdateRightClick();
            UpdateAdding();
            UpdateAttach();
            UpdateSelect();
            UpdateDrag();
        }

        private void UpdateAdding()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (_drawState != DrawState.NONE)
                {
                    if (!_wasDrawing)
                    {
                        _lastPoint = GetDot(_pos);
                        _wasDrawing = true;
                    }
                    else if (_drawState == DrawState.DOT)
                        _lastPoint.MoveTo(_pos);
                }
            }
            if (_wasDrawing && Mouse.GetState().LeftButton == ButtonState.Released)
            {
                if (_drawState == DrawState.DOT)
                {
                    AddShape(_lastPoint);
                    _doneActions.Push(new ImportantAction(userActions.CREATE, new Shape[] { _lastPoint }, null));
                }
                else
                {
                    var p = GetDot(_pos);
                    AddShape(p);
                    AddShape(_lastPoint);
                    _doneActions.Push(new ImportantAction(userActions.CREATE, new Shape[] { p }, null));
                    _doneActions.Push(new ImportantAction(userActions.CREATE, new Shape[] { _lastPoint }, null));
                    Shape sp = null;
                    if (_drawState == DrawState.CIRCLE)
                    {
                        sp = new Circle(_lastPoint, p);
                        AddShape(sp);
                    }
                    else if (_drawState == DrawState.SEGMENT)
                    {
                        sp = new Segment(_lastPoint, p);
                        AddShape(sp);
                    }
                    else if (_drawState == DrawState.LINE)
                    {
                        sp = new Line(_lastPoint, p);
                        AddShape(sp);
                    }
                    else if (_drawState == DrawState.VECTOR)
                    {
                        sp = new Vector(_lastPoint, p);
                        AddShape(sp);
                    }
                    _doneActions.Push(new ImportantAction(userActions.CREATE, new Shape[] { sp }, null));
                }
                _wasDrawing = false;
                _drawState = DrawState.NONE;
            }
        }

        private void UpdateAttach()
        {
            if (_drawState == DrawState.NONE)
            {
                if (Scene.CurrentScene.IsMiddleMouseDown)
                {
                    if (_selectedShapes.Count == 1)
                    {
                        if (_selectedShapes[0] is Dot)
                        {
                            if (_nearShapes.Count > 0)
                            {
                                var parent = GetDot(_pos);
                                AddShape(parent);
                                parent.Attach(_selectedShapes[0] as Dot);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSelect()
        {
            if (_drawState == DrawState.NONE)
            {
                if (_nearShapes.Count > 0)
                {
                    Shape nearest = _nearShapes[0];

                    float dist = int.MaxValue;
                    foreach (var s in _nearShapes)
                    {
                        if (s is Dot)
                        {
                            // 다 끝났다 그지 깽깽이들아!! 점이 우선순위 최고다!
                            nearest = s;
                            break;
                        }
                        if (dist > s.Distance)
                        {
                            nearest = s;
                            dist = s.Distance;
                        }
                    }

                    if (Scene.CurrentScene.IsLeftMouseDown)
                    {
                        if (!nearest.Selected)
                        {
                            _selectedShapes.ForEach(s => s.UnSelect = true);

                            NEAR_LOOP:
                            _selectedShapes.Add(nearest);
                            if (nearest is Dot && (nearest as Dot).Rule is FollowRule)
                            {
                                var p = ((nearest as Dot).Rule as FollowRule).Parent;
                                if (nearest != p)
                                {
                                    nearest = p;
                                    goto NEAR_LOOP;
                                }
                            }
                            nearest.Selected = true;
                            nearest.UnSelect = false;
                        }
                        else
                            nearest.UnSelect = true;
                    }

                    if (Scene.CurrentScene.IsLeftMouseUp)
                    {
                        if (nearest.Selected && nearest.UnSelect)
                        {
                            _selectedShapes.Remove(nearest);
                            nearest.Selected = false;
                        }
                    }
                }
                else
                {
                    if (Scene.CurrentScene.IsLeftMouseUp)
                    {
                        UnselectAll();
                    }
                }
            }
        }

        private void UpdateDrag()
        {
            if (_drawState == DrawState.NONE)
            {
                if (_isDragging || Scene.CurrentScene.IsLeftMouseClicking && Scene.CurrentScene.IsMouseMoved)
                {
                    var diff = Scene.CurrentScene.MousePosition - Scene.CurrentScene.LastMousePosition;
                    if (_isDragging || _selectedShapes.Any(s => s.IsEnoughClose(_pos)))
                    {
                        if (!_isDragging) _isDragging = true;
                        _selectedShapes.ForEach(s => { s.UnSelect = false; s.Move(diff.ToVector2()); });
                    }
                    if (Scene.CurrentScene.IsLeftMouseUp)
                        _isDragging = false;
                }
            }
        }

        private void UpdateRightClick()
        {
            // 우선 테스트 정도로 작동 대충 되도록 짜봤음
            if(_drawState == DrawState.NONE)
            {
                if(_nearShapes.Count == 1)
                {
                    if(Scene.CurrentScene.IsRightMouseUp)
                    {
                        UnselectAll();
                        Select(_nearShapes[0]);
                        _shapeMenuStrip.Show(Scene.CurrentScene.MousePosition.X, Scene.CurrentScene.MousePosition.Y);
                    }
                }

                if (_shapeMenuStrip.IsSelected)
                    if (_shapeMenuStrip.SelectedItem.Text == "Delete")
                        DeleteSelected();
            }
        }

        private Dot GetDot(Vector2 coord)
            => GetDot(coord, _nearShapes);

        /// <summary>
        /// 가까운 점이 있다면 그 점을, 없다면 새 점을
        /// </summary>
        private Dot GetDot(Vector2 coord, List<Shape> nears)
        {
            Dot nearestDot = null;
            Shape nearest = null;
            float distDot = int.MaxValue;
            float dist = int.MaxValue;
            foreach (var s in _nearShapes)
            {
                if (s is Dot)
                {
                    if (s.IsEnoughClose(coord))
                    {
                        nearestDot = s as Dot;
                        distDot = s.Distance;
                    }
                }
                else
                {
                    if (s.Distance <= dist)
                    {
                        nearest = s;
                        dist = s.Distance;
                    }
                }
            }
            if (nearestDot == null) // 가장 가까운게 점이 아니라면
            {
                if (nears.Count == 0)
                    return new Dot(coord);
                if (nears.Count == 1)
                {
                    return OneShapeRuleDot(nearest, coord);
                }
                else if (nears.Count == 2)
                {
                    Vector2[] intersects = Geometry.GetIntersect(nears[0], nears[1]);
                    if (intersects.Length != 0)
                    {
                        Dot dot;
                        if (intersects.Length == 2)
                        {
                            dot = Vector2.Distance(coord, intersects[0]) < Vector2.Distance(intersects[1], coord) ? new Dot(intersects[0]) : new Dot(intersects[1]);
                        }
                        else dot = new Dot(intersects[0]);
                        //dot = new Dot(intersects[0]);
                        IntersectRule rule = new IntersectRule(dot, nears[0], nears[1]);
                        _currentRules.Add(rule);
                        return dot;
                    }
                    else return OneShapeRuleDot(nearest, coord);
                }
                else return OneShapeRuleDot(nearest, coord);
            }
            else if (nearestDot is Dot) return nearestDot as Dot;
            else return new Dot(coord);
        }

        private Dot OneShapeRuleDot(Shape nearest, Vector2 coord)
        {
            Dot dot = new Dot(Geometry.GetNearest(nearest, coord));
            _currentRules.Add(nearest.GetNearDot(dot));
            return dot;
        }

        public override void Draw(SpriteBatch sb)
        {
            //_pos = Camera.Current.GetRay(Mouse.GetState().Position.ToVector2());
            sb.BeginAA();
            _pos = Mouse.GetState().Position.ToVector2();
            if (_wasDrawing)
            {
                if (_drawState == DrawState.CIRCLE)
                {
                    float radius = (_pos - _lastPoint.Coord).Length();
                    GUI.DrawCircle(sb, _lastPoint.Coord, radius, 2, Color.DarkGray, 100);
                }
                else if (_drawState == DrawState.SEGMENT)
                {
                    GUI.DrawLine(sb, _lastPoint.Coord, _pos, 2, Color.DarkGray);
                }
                else if (_drawState == DrawState.VECTOR)
                {
                    new Vector(_lastPoint, new Dot(_pos)).Draw(sb);
                }
                else if (_drawState == DrawState.LINE)
                {
                    new Line(new Dot(_lastPoint.Coord), new Dot(_pos)).Draw(sb);
                }
                else if (_drawState == DrawState.DOT)
                {
                    _lastPoint.Draw(sb);
                }
            }

            UpdateLists(sb);
            sb.End();
        }
    }
}
