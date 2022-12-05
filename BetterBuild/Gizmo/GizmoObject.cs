using System.Collections;
using UnityEngine;

namespace BetterBuild.Gizmo
{
    public class GizmoObject : SRSingleton<GizmoObject>
    {
        public float HandleSize = 90f;

        private Tool tool = Tool.Position;
        private bool isHidden = false;
        private Matrix4x4 handleMatrix;

        private Transform _trs;
        private Transform trs { get { if (_trs == null) _trs = gameObject.GetComponent<Transform>(); return _trs; } }
        public Camera cam;

        const float HANDLE_BOX_SIZE = .25f;
        const float CAP_SIZE = .07f;
        const int MAX_DISTANCE_TO_HANDLE = 15;

        private Mesh _HandleLineMesh = null, _HandleTriangleMesh = null;

        private Vector2 mouseOrigin = Vector2.zero;
        public bool draggingHandle { get; private set; }
        private int draggingAxes = 0;   // In how many directions is the handle able to move
        private Vector3 scale = Vector3.one;
        private GizmoTransform handleOrigin = GizmoTransform.identity;

        public delegate void OnHandleMoveEvent(GizmoTransform transform);
        public event OnHandleMoveEvent OnHandleMove;

        public delegate void OnHandleBeginEvent(GizmoTransform transform);
        public event OnHandleBeginEvent OnHandleBegin;

        public delegate void OnHandleFinishEvent();
        public event OnHandleFinishEvent OnHandleFinish;

        public bool IsInUse { get { return draggingHandle; } }

        private Mesh HandleLineMesh
        {
            get
            {
                if (_HandleLineMesh == null)
                {
                    _HandleLineMesh = new Mesh();
                    CreateHandleLineMesh(ref _HandleLineMesh, Vector3.one);
                }
                return _HandleLineMesh;
            }
        }

        private Mesh HandleTriangleMesh
        {
            get
            {
                if (_HandleTriangleMesh == null)
                {
                    _HandleTriangleMesh = new Mesh();
                    CreateHandleTriangleMesh(ref _HandleTriangleMesh, Vector3.one);
                }
                return _HandleTriangleMesh;
            }
        }

        private void Start()
        {
            SetIsHidden(true);

            OnHandleBegin += GizmoObject_OnHandleBegin;
        }

        private void GizmoObject_OnHandleBegin(GizmoTransform transform)
        {
            UndoManager.RegisterState(new UndoHandlemove(tool), "Moveing objects");
        }

        #region Update

        class DragOrientation
        {
            public Vector3 origin;
            public Vector3 axis;
            public Vector3 mouse;
            public Vector3 cross;
            public Vector3 offset;
            public Plane plane;

            public DragOrientation()
            {
                origin = Vector3.zero;
                axis = Vector3.zero;
                mouse = Vector3.zero;
                cross = Vector3.zero;
                offset = Vector3.zero;
                plane = new Plane(Vector3.up, Vector3.zero);
            }
        }

        DragOrientation drag = new DragOrientation();

        public void UpdateDrag()
        {
            if (isHidden)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                OnMouseDown();
            }

            if (Input.GetMouseButton(0) && draggingHandle)
            {
                Vector3 a = Vector3.zero;

                bool valid = false;

                if (draggingAxes < 2 && tool != Tool.Rotate)
                {
                    Vector3 b;
                    valid = GizmoUtility.PointOnLine(new Ray(trs.position, drag.axis), cam.ScreenPointToRay(Input.mousePosition), out a, out b);
                }
                else
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    float hit = 0f;
                    if (drag.plane.Raycast(ray, out hit))
                    {
                        a = ray.GetPoint(hit);
                        valid = true;
                    }
                }

                if (valid)
                {
                    drag.origin = trs.position;

                    switch (tool)
                    {
                        case Tool.Position:
                            {
                                trs.position = a - drag.offset;
                            }
                            break;

                        case Tool.Rotate:
                            {
                                Vector2 delta = (Vector2)Input.mousePosition - mouseOrigin;
                                mouseOrigin = Input.mousePosition;
                                float sign = GizmoUtility.CalcMouseDeltaSignWithAxes(cam, drag.origin, drag.axis, drag.cross, delta);
                                axisAngle += delta.magnitude * sign;
                                trs.localRotation = Quaternion.AngleAxis(axisAngle, drag.axis) * handleOrigin.rotation;// trs.localRotation;
                            }
                            break;

                        case Tool.Scale:
                            {
                                Vector3 v;

                                if (draggingAxes > 1)
                                {
                                    v = SetUniformMagnitude(((a - drag.offset) - trs.position));
                                }
                                else
                                {
                                    v = Quaternion.Inverse(handleOrigin.rotation) * ((a - drag.offset) - trs.position);
                                }

                                v += Vector3.one;
                                scale = v;
                                RebuildGizmoMesh(scale);
                            }
                            break;
                    }

                    OnHandleMove?.Invoke(GetTransform());

                    RebuildGizmoMatrix();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnFinishHandleMovement();
            }
        }

        float axisAngle = 0f;

        /**
         * Sets all the components of a vector to the component with the largest magnitude.
         */
        Vector3 SetUniformMagnitude(Vector3 a)
        {
            float max = Mathf.Abs(a.x) > Mathf.Abs(a.y) && Mathf.Abs(a.x) > Mathf.Abs(a.z) ? a.x : Mathf.Abs(a.y) > Mathf.Abs(a.z) ? a.y : a.z;

            a.x = max;
            a.y = max;
            a.z = max;

            return a;
        }

        void OnMouseDown()
        {
            scale = Vector3.one;

            Vector3 a, b;
            drag.offset = Vector3.zero;
            Axis plane;

            axisAngle = 0f;

            draggingHandle = CheckHandleActivated(Input.mousePosition, out plane);

            mouseOrigin = Input.mousePosition;
            handleOrigin.SetTRS(trs);

            drag.axis = Vector3.zero;
            draggingAxes = 0;

            if (draggingHandle)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if ((plane & Axis.X) == Axis.X)
                {
                    draggingAxes++;
                    drag.axis = trs.right;
                    drag.plane.SetNormalAndPosition(trs.right.normalized, trs.position);
                }

                if ((plane & Axis.Y) == Axis.Y)
                {
                    draggingAxes++;
                    if (draggingAxes > 1)
                        drag.plane.SetNormalAndPosition(Vector3.Cross(drag.axis, trs.up).normalized, trs.position);
                    else
                        drag.plane.SetNormalAndPosition(trs.up.normalized, trs.position);
                    drag.axis += trs.up;
                }

                if ((plane & Axis.Z) == Axis.Z)
                {
                    draggingAxes++;
                    if (draggingAxes > 1)
                        drag.plane.SetNormalAndPosition(Vector3.Cross(drag.axis, trs.forward).normalized, trs.position);
                    else
                        drag.plane.SetNormalAndPosition(trs.forward.normalized, trs.position);
                    drag.axis += trs.forward;
                }

                if (draggingAxes < 2)
                {
                    if (GizmoUtility.PointOnLine(new Ray(trs.position, drag.axis), ray, out a, out b))
                        drag.offset = a - trs.position;

                    float hit = 0f;

                    if (drag.plane.Raycast(ray, out hit))
                    {
                        drag.mouse = (ray.GetPoint(hit) - trs.position).normalized;
                        drag.cross = Vector3.Cross(drag.axis, drag.mouse);
                    }
                }
                else
                {
                    float hit = 0f;

                    if (drag.plane.Raycast(ray, out hit))
                    {
                        drag.offset = ray.GetPoint(hit) - trs.position;
                        drag.mouse = (ray.GetPoint(hit) - trs.position).normalized;
                        drag.cross = Vector3.Cross(drag.axis, drag.mouse);
                    }
                }

                OnHandleBegin?.Invoke(GetTransform());
            }
        }

        bool CheckHandleActivated(Vector2 mousePosition, out Axis plane)
        {
            plane = (Axis)0x0;

            if (tool == Tool.Position || tool == Tool.Scale)
            {
                float sceneHandleSize = GizmoUtility.GetHandleSize(cam, trs.position);

                // cen
                Vector2 cen = cam.WorldToScreenPoint(trs.position);

                // up
                Vector2 up = cam.WorldToScreenPoint((trs.position + (trs.up + trs.up * CAP_SIZE * 4f) * (sceneHandleSize * HandleSize)));

                // right
                Vector2 right = cam.WorldToScreenPoint((trs.position + (trs.right + trs.right * CAP_SIZE * 4f) * (sceneHandleSize * HandleSize)));

                // forward
                Vector2 forward = cam.WorldToScreenPoint((trs.position + (trs.forward + trs.forward * CAP_SIZE * 4f) * (sceneHandleSize * HandleSize)));

                // First check if the plane boxes have been activated

                Vector3 cameraMask = GizmoUtility.DirectionMask(trs, cam.transform.forward);

                Vector2 p_right = (cen + ((right - cen) * cameraMask.x) * HANDLE_BOX_SIZE);
                Vector2 p_up = (cen + ((up - cen) * cameraMask.y) * HANDLE_BOX_SIZE);
                Vector2 p_forward = (cen + ((forward - cen) * cameraMask.z) * HANDLE_BOX_SIZE);

                // x plane
                if (GizmoUtility.PointInPolygon(new Vector2[] {
                cen, p_up,
                p_up, (p_up+p_forward) - cen,
                (p_up+p_forward) - cen, p_forward,
                p_forward, cen
                }, mousePosition))
                    plane = Axis.Y | Axis.Z;
                // y plane
                else if (GizmoUtility.PointInPolygon(new Vector2[] {
                cen, p_right,
                p_right, (p_right+p_forward)-cen,
                (p_right+p_forward)-cen, p_forward,
                p_forward, cen
                }, mousePosition))
                    plane = Axis.X | Axis.Z;
                // z plane
                else if (GizmoUtility.PointInPolygon(new Vector2[] {
                cen, p_up,
                p_up, (p_up + p_right) - cen,
                (p_up + p_right) - cen, p_right,
                p_right, cen
                }, mousePosition))
                    plane = Axis.X | Axis.Y;
                else
                if (GizmoUtility.DistancePointLineSegment(mousePosition, cen, up) < MAX_DISTANCE_TO_HANDLE)
                    plane = Axis.Y;
                else if (GizmoUtility.DistancePointLineSegment(mousePosition, cen, right) < MAX_DISTANCE_TO_HANDLE)
                    plane = Axis.X;
                else if (GizmoUtility.DistancePointLineSegment(mousePosition, cen, forward) < MAX_DISTANCE_TO_HANDLE)
                    plane = Axis.Z;
                else
                    return false;

                return true;
            }
            else
            {
                Vector3[][] vertices = GizmoMesh.GetRotationVertices(16, 1f);

                float best = Mathf.Infinity;

                Vector2 cur, prev = Vector2.zero;
                plane = Axis.X;

                for (int i = 0; i < 3; i++)
                {
                    cur = cam.WorldToScreenPoint(vertices[i][0]);

                    for (int n = 0; n < vertices[i].Length - 1; n++)
                    {
                        prev = cur;
                        cur = cam.WorldToScreenPoint(handleMatrix.MultiplyPoint3x4(vertices[i][n + 1]));

                        float dist = GizmoUtility.DistancePointLineSegment(mousePosition, prev, cur);

                        if (dist < best && dist < MAX_DISTANCE_TO_HANDLE)
                        {
                            Vector3 viewDir = (handleMatrix.MultiplyPoint3x4((vertices[i][n] + vertices[i][n + 1]) * .5f) - cam.transform.position).normalized;
                            Vector3 nrm = transform.TransformDirection(vertices[i][n]).normalized;

                            if (Vector3.Dot(nrm, viewDir) > .5f)
                                continue;

                            best = dist;

                            switch (i)
                            {
                                case 0: // Y
                                    plane = Axis.Y; // Axis.X | Axis.Z;
                                    break;

                                case 1: // Z
                                    plane = Axis.Z;// Axis.X | Axis.Y;
                                    break;

                                case 2: // X
                                    plane = Axis.X;// Axis.Y | Axis.Z;
                                    break;
                            }
                        }
                    }
                }

                if (best < MAX_DISTANCE_TO_HANDLE + .1f)
                {
                    return true;
                }
            }

            return false;
        }

        public GizmoTransform GetTransform()
        {
            return new GizmoTransform(
                trs.position,
                trs.localRotation,
                scale);
        }

        void OnFinishHandleMovement()
        {
            RebuildGizmoMesh(Vector3.one);
            RebuildGizmoMatrix();

            OnHandleFinish?.Invoke();

            StartCoroutine(SetDraggingFalse());
        }

        IEnumerator SetDraggingFalse()
        {
            yield return new WaitForEndOfFrame();
            draggingHandle = false;
        }
        #endregion

        private void OnRenderObject()
        {
            if (isHidden || Camera.current != cam)
                return;

            switch (tool)
            {
                case Tool.Position:
                case Tool.Scale:
                    Globals.HandleOpaqueMaterial.SetPass(0);
                    Graphics.DrawMeshNow(HandleLineMesh, handleMatrix);
                    Graphics.DrawMeshNow(HandleTriangleMesh, handleMatrix, 1);  // Cones

                    Globals.HandleTransparentMaterial.SetPass(0);
                    Graphics.DrawMeshNow(HandleTriangleMesh, handleMatrix, 0);  // Box
                    break;

                case Tool.Rotate:
                    Globals.HandleRotateMaterial.SetPass(0);
                    Graphics.DrawMeshNow(HandleLineMesh, handleMatrix);
                    break;
            }
        }

        //public void SetTarget(Transform trans)
        //{
        //    if (m_TargetObject != null)
        //    {
        //        foreach (MeshFilter meshFilter in m_TargetObject.GetComponentsInChildren<MeshFilter>())
        //        {
        //            if (meshFilter.GetComponent<ObjectHighlight>())
        //            {
        //                Destroy(meshFilter.GetComponent<ObjectHighlight>());
        //            }
        //        }
        //    }

        //    if (trans == null)
        //    {
        //        SetIsHidden(true);
        //        m_TargetObject = null;

        //        return;
        //    }

        //    m_TargetObject = trans;

        //    transform.position = trans.position;
        //    transform.localRotation = transform.localRotation;

        //    //RebuildGizmoMatrix();
        //    //RebuildGizmoMesh(Vector3.one);

        //    foreach (MeshFilter meshFilter in m_TargetObject.GetComponentsInChildren<MeshFilter>())
        //    {
        //        meshFilter.gameObject.AddComponent<ObjectHighlight>();
        //    }

        //    SetTool(Tool.Position);
        //    SetIsHidden(false);
        //}

        public void SetTRS(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            trs.position = position;
            trs.localRotation = rotation;
            trs.localScale = scale;

            RebuildGizmoMatrix();
        }

        public void SetTool(Tool tool)
        {
            if (this.tool != tool)
            {
                this.tool = tool;
                RebuildGizmoMesh(Vector3.one);
            }
        }

        public Tool GetTool()
        {
            return tool;
        }

        public void SetIsHidden(bool isHidden)
        {
            draggingHandle = false;
            this.isHidden = isHidden;
        }

        public bool GetIsHidden()
        {
            return this.isHidden;
        }

        public void OnCameraMove()
        {
            RebuildGizmoMesh(Vector3.one);
            RebuildGizmoMatrix();
        }

        private void RebuildGizmoMatrix()
        {
            float handleSize = GizmoUtility.GetHandleSize(cam, trs.position);
            Matrix4x4 scale = Matrix4x4.Scale(Vector3.one * handleSize * HandleSize);

            handleMatrix = transform.localToWorldMatrix * scale;
        }

        private void RebuildGizmoMesh(Vector3 scale)
        {
            if (_HandleLineMesh == null)
                _HandleLineMesh = new Mesh();

            if (_HandleTriangleMesh == null)
                _HandleTriangleMesh = new Mesh();

            CreateHandleLineMesh(ref _HandleLineMesh, scale);
            CreateHandleTriangleMesh(ref _HandleTriangleMesh, scale);
        }

        private void CreateHandleLineMesh(ref Mesh mesh, Vector3 scale)
        {
            switch (tool)
            {
                case Tool.Position:
                case Tool.Scale:
                    GizmoMesh.CreatePositionLineMesh(ref mesh, trs, scale, cam, HANDLE_BOX_SIZE);
                    break;

                case Tool.Rotate:
                    GizmoMesh.CreateRotateMesh(ref mesh, 48, 1f);
                    break;

                default:
                    return;
            }
        }

        private void CreateHandleTriangleMesh(ref Mesh mesh, Vector3 scale)
        {
            if (tool == Tool.Position)
                GizmoMesh.CreateTriangleMesh(ref mesh, trs, scale, cam, Globals.ConeMesh, HANDLE_BOX_SIZE, CAP_SIZE);
            else if (tool == Tool.Scale)
                GizmoMesh.CreateTriangleMesh(ref mesh, trs, scale, cam, Globals.CubeMesh, HANDLE_BOX_SIZE, CAP_SIZE);
        }
    }
}
