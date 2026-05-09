using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshTools
{
    public struct MeshSliceResult
    {
        public Mesh Positive;
        public Mesh Negative;
        public bool Intersects;

        /// <summary>
        /// 保存一次平面切割得到的正侧、负侧结果以及是否真的发生相交。
        /// </summary>
        public MeshSliceResult(Mesh positive, Mesh negative, bool intersects)
        {
            Positive = positive;
            Negative = negative;
            Intersects = intersects;
        }
    }

    /// <summary>
    /// 用平面切割三角网格。正侧表示 Plane.GetDistanceToPoint(vertex) 大于等于 0 的一侧。
    /// </summary>
    public static class MeshPlaneSlicer
    {
        /// <summary>
        /// 切割一个 Mesh 并只返回指定侧的结果。
        /// </summary>
        public static Mesh Cut(Mesh mesh, Plane plane, bool keepPositive, bool cap = true, int capSubMesh = 0)
        {
            MeshSliceResult result = Slice(mesh, plane, cap, capSubMesh);
            return keepPositive ? result.Positive : result.Negative;
        }

        /// <summary>
        /// 在 Mesh 当前坐标空间中切割，并返回正负两侧结果。
        /// </summary>
        public static MeshSliceResult Slice(Mesh mesh, Plane plane, bool cap = true, int capSubMesh = 0)
        {
            return Slice(mesh, Matrix4x4.identity, plane, cap, capSubMesh);
        }

        /// <summary>
        /// 先把 Mesh 顶点变换到结果空间，再用同一结果空间下的平面切割。
        /// </summary>
        public static MeshSliceResult Slice(
            Mesh mesh,
            Matrix4x4 meshToResult,
            Plane planeInResult,
            bool cap = true,
            int capSubMesh = 0)
        {
            // 切割判断依赖 signed distance，所以先保证平面法线长度为 1。
            Plane plane = NormalizePlane(planeInResult);
            MeshToolMeshData data = MeshToolGeometry.ReadTriangles(mesh, meshToResult);
            int subMeshCount = cap ? Mathf.Max(data.SubMeshCount, capSubMesh + 1) : data.SubMeshCount;

            // 正负两侧各自独立构建；capSegments 用来稍后拼接切口封盖。
            MeshToolMeshBuilder positiveBuilder = new MeshToolMeshBuilder(data.Attributes, subMeshCount);
            MeshToolMeshBuilder negativeBuilder = new MeshToolMeshBuilder(data.Attributes, subMeshCount);
            List<CapSegment> capSegments = new List<CapSegment>();
            bool intersects = false;

            for (int i = 0; i < data.Triangles.Count; i++)
            {
                MeshToolTriangle triangle = data.Triangles[i];
                MeshToolVertex[] vertices =
                {
                    triangle.A,
                    triangle.B,
                    triangle.C
                };

                float[] distances =
                {
                    plane.GetDistanceToPoint(vertices[0].Position),
                    plane.GetDistanceToPoint(vertices[1].Position),
                    plane.GetDistanceToPoint(vertices[2].Position)
                };

                // 全在某一侧的三角形可以直接拷贝；跨平面的三角形才需要裁剪。
                bool positiveSide = distances[0] >= -MeshToolGeometry.Epsilon &&
                    distances[1] >= -MeshToolGeometry.Epsilon &&
                    distances[2] >= -MeshToolGeometry.Epsilon;
                bool negativeSide = distances[0] <= MeshToolGeometry.Epsilon &&
                    distances[1] <= MeshToolGeometry.Epsilon &&
                    distances[2] <= MeshToolGeometry.Epsilon;

                if (positiveSide)
                {
                    positiveBuilder.AddTriangle(triangle.A, triangle.B, triangle.C, triangle.SubMesh);
                }

                if (negativeSide)
                {
                    negativeBuilder.AddTriangle(triangle.A, triangle.B, triangle.C, triangle.SubMesh);
                }

                if (!positiveSide && !negativeSide)
                {
                    intersects = true;
                    // 三角形被平面切到时，分别裁出正侧和负侧的多边形。
                    List<MeshToolVertex> positivePolygon = ClipPolygon(vertices, distances, true);
                    List<MeshToolVertex> negativePolygon = ClipPolygon(vertices, distances, false);
                    positiveBuilder.AddPolygon(positivePolygon, triangle.SubMesh);
                    negativeBuilder.AddPolygon(negativePolygon, triangle.SubMesh);

                    // 每个被切开的三角形贡献一条切口线段，所有线段稍后会串成封盖环。
                    CapSegment capSegment;
                    if (cap && TryGetCapSegment(vertices, distances, out capSegment))
                    {
                        capSegments.Add(capSegment);
                    }
                }
            }

            // 封盖需要正负两侧使用相反朝向，保证结果仍是闭合体。
            if (cap && capSegments.Count > 0)
            {
                AddCaps(positiveBuilder, capSegments, plane, -plane.normal, capSubMesh);
                AddCaps(negativeBuilder, capSegments, plane, plane.normal, capSubMesh);
            }

            return new MeshSliceResult(
                positiveBuilder.ToMesh("MeshSlice_Positive"),
                negativeBuilder.ToMesh("MeshSlice_Negative"),
                intersects);
        }

        /// <summary>
        /// 用世界空间平面切割 MeshFilter，并只返回指定侧结果。
        /// </summary>
        public static Mesh Cut(
            MeshFilter source,
            Plane worldPlane,
            bool keepPositive,
            bool cap = true,
            int capSubMesh = 0,
            Transform resultSpace = null)
        {
            MeshSliceResult result = Slice(source, worldPlane, cap, capSubMesh, resultSpace);
            return keepPositive ? result.Positive : result.Negative;
        }

        /// <summary>
        /// 用世界空间平面切割 MeshFilter，并返回 resultSpace 本地空间下的正负结果。
        /// resultSpace 为 null 时，结果使用 source 的本地空间。
        /// </summary>
        public static MeshSliceResult Slice(
            MeshFilter source,
            Plane worldPlane,
            bool cap = true,
            int capSubMesh = 0,
            Transform resultSpace = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.sharedMesh == null)
            {
                throw new ArgumentException("MeshFilter has no sharedMesh.", nameof(source));
            }

            // 把源 Mesh 和世界平面都变换到同一个结果空间里再切。
            Transform targetSpace = resultSpace != null ? resultSpace : source.transform;
            Matrix4x4 worldToResult = targetSpace != null ? targetSpace.worldToLocalMatrix : Matrix4x4.identity;
            Matrix4x4 meshToResult = worldToResult * source.transform.localToWorldMatrix;
            Plane planeInResult = MeshToolGeometry.TransformPlane(worldPlane, worldToResult);

            return Slice(source.sharedMesh, meshToResult, planeInResult, cap, capSubMesh);
        }

        /// <summary>
        /// 归一化平面法线，同时按相同比例调整 distance。
        /// </summary>
        private static Plane NormalizePlane(Plane plane)
        {
            Vector3 normal = plane.normal;
            float magnitude = normal.magnitude;
            if (magnitude <= MeshToolGeometry.Epsilon)
            {
                throw new ArgumentException("Plane normal cannot be zero.", nameof(plane));
            }

            return new Plane(normal / magnitude, plane.distance / magnitude);
        }

        /// <summary>
        /// 使用 Sutherland-Hodgman 思路，把三角形裁到平面指定侧。
        /// </summary>
        private static List<MeshToolVertex> ClipPolygon(MeshToolVertex[] vertices, float[] distances, bool keepPositive)
        {
            List<MeshToolVertex> output = new List<MeshToolVertex>(4);
            for (int i = 0; i < vertices.Length; i++)
            {
                int nextIndex = (i + 1) % vertices.Length;
                MeshToolVertex current = vertices[i];
                MeshToolVertex next = vertices[nextIndex];
                float currentDistance = distances[i];
                float nextDistance = distances[nextIndex];
                bool currentInside = keepPositive
                    ? currentDistance >= -MeshToolGeometry.Epsilon
                    : currentDistance <= MeshToolGeometry.Epsilon;
                bool nextInside = keepPositive
                    ? nextDistance >= -MeshToolGeometry.Epsilon
                    : nextDistance <= MeshToolGeometry.Epsilon;

                // 按“当前点/下一点是否在保留侧”决定保留端点还是插入交点。
                if (currentInside && nextInside)
                {
                    AddCleanVertex(output, next);
                }
                else if (currentInside && !nextInside)
                {
                    AddCleanVertex(output, IntersectEdge(current, next, currentDistance, nextDistance));
                }
                else if (!currentInside && nextInside)
                {
                    AddCleanVertex(output, IntersectEdge(current, next, currentDistance, nextDistance));
                    AddCleanVertex(output, next);
                }
            }

            // 裁剪后首尾可能落在同一个切割交点，移除重复点避免退化面。
            if (output.Count > 1 && MeshToolGeometry.SamePosition(output[0].Position, output[output.Count - 1].Position))
            {
                output.RemoveAt(output.Count - 1);
            }

            return output;
        }

        /// <summary>
        /// 根据两端到平面的有符号距离，插值得到边和平面的交点顶点。
        /// </summary>
        private static MeshToolVertex IntersectEdge(
            MeshToolVertex a,
            MeshToolVertex b,
            float distanceA,
            float distanceB)
        {
            float denominator = distanceA - distanceB;
            if (Mathf.Abs(denominator) <= MeshToolGeometry.Epsilon)
            {
                return a;
            }

            float t = Mathf.Clamp01(distanceA / denominator);
            return MeshToolVertex.Lerp(a, b, t);
        }

        /// <summary>
        /// 追加顶点时合并连续重复点。
        /// </summary>
        private static void AddCleanVertex(List<MeshToolVertex> vertices, MeshToolVertex vertex)
        {
            if (vertices.Count == 0 || !MeshToolGeometry.SamePosition(vertices[vertices.Count - 1].Position, vertex.Position))
            {
                vertices.Add(vertex);
            }
        }

        /// <summary>
        /// 从一个跨平面的三角形中提取切口线段。
        /// </summary>
        private static bool TryGetCapSegment(MeshToolVertex[] vertices, float[] distances, out CapSegment segment)
        {
            bool hasPositive = false;
            bool hasNegative = false;

            // 只有同时存在正负两侧顶点时，三角形才真正贡献切口边。
            for (int i = 0; i < distances.Length; i++)
            {
                hasPositive |= distances[i] > MeshToolGeometry.Epsilon;
                hasNegative |= distances[i] < -MeshToolGeometry.Epsilon;
            }

            if (!hasPositive || !hasNegative)
            {
                segment = default(CapSegment);
                return false;
            }

            List<MeshToolVertex> intersections = new List<MeshToolVertex>(2);
            // 原始顶点刚好在平面上时，它本身就是切口端点。
            for (int i = 0; i < vertices.Length; i++)
            {
                if (Mathf.Abs(distances[i]) <= MeshToolGeometry.Epsilon)
                {
                    AddUniqueIntersection(intersections, vertices[i]);
                }
            }

            // 边两端异号时，边与平面之间存在一个交点。
            for (int i = 0; i < vertices.Length; i++)
            {
                int next = (i + 1) % vertices.Length;
                float distanceA = distances[i];
                float distanceB = distances[next];
                if ((distanceA > MeshToolGeometry.Epsilon && distanceB < -MeshToolGeometry.Epsilon) ||
                    (distanceA < -MeshToolGeometry.Epsilon && distanceB > MeshToolGeometry.Epsilon))
                {
                    AddUniqueIntersection(intersections, IntersectEdge(vertices[i], vertices[next], distanceA, distanceB));
                }
            }

            if (intersections.Count < 2)
            {
                segment = default(CapSegment);
                return false;
            }

            if (intersections.Count > 2)
            {
                // 容差附近可能收集到多于两个点，取距离最远的一对作为稳定线段。
                SelectFarthestPair(intersections);
            }

            segment = new CapSegment(intersections[0], intersections[1]);
            return !MeshToolGeometry.SamePosition(segment.A.Position, segment.B.Position);
        }

        /// <summary>
        /// 向交点列表加入一个不重复的交点。
        /// </summary>
        private static void AddUniqueIntersection(List<MeshToolVertex> intersections, MeshToolVertex vertex)
        {
            for (int i = 0; i < intersections.Count; i++)
            {
                if (MeshToolGeometry.SamePosition(intersections[i].Position, vertex.Position))
                {
                    return;
                }
            }

            intersections.Add(vertex);
        }

        /// <summary>
        /// 从候选交点中保留距离最远的一对。
        /// </summary>
        private static void SelectFarthestPair(List<MeshToolVertex> intersections)
        {
            int bestA = 0;
            int bestB = 1;
            float bestDistance = -1f;

            for (int i = 0; i < intersections.Count - 1; i++)
            {
                for (int j = i + 1; j < intersections.Count; j++)
                {
                    float distance = (intersections[i].Position - intersections[j].Position).sqrMagnitude;
                    if (distance > bestDistance)
                    {
                        bestDistance = distance;
                        bestA = i;
                        bestB = j;
                    }
                }
            }

            MeshToolVertex a = intersections[bestA];
            MeshToolVertex b = intersections[bestB];
            intersections.Clear();
            intersections.Add(a);
            intersections.Add(b);
        }

        /// <summary>
        /// 把所有切口线段串成环，并为每个环生成封盖三角形。
        /// </summary>
        private static void AddCaps(
            MeshToolMeshBuilder builder,
            List<CapSegment> segments,
            Plane plane,
            Vector3 capNormal,
            int capSubMesh)
        {
            PlaneBasis basis = PlaneBasis.FromNormal(plane.normal);
            CapLoopBuilder loopBuilder = new CapLoopBuilder();

            // 先把独立线段加入图结构，再由图结构追踪闭合环。
            for (int i = 0; i < segments.Count; i++)
            {
                loopBuilder.AddSegment(segments[i].A, segments[i].B);
            }

            List<List<MeshToolVertex>> loops = loopBuilder.BuildLoops();
            for (int i = 0; i < loops.Count; i++)
            {
                AddCapLoop(builder, loops[i], basis, capNormal, capSubMesh);
            }
        }

        /// <summary>
        /// 为一个切口闭合环生成封盖面。
        /// </summary>
        private static void AddCapLoop(
            MeshToolMeshBuilder builder,
            List<MeshToolVertex> loop,
            PlaneBasis basis,
            Vector3 capNormal,
            int capSubMesh)
        {
            if (loop.Count < 3)
            {
                return;
            }

            List<MeshToolVertex> vertices = new List<MeshToolVertex>(loop.Count);
            List<Vector2> projected = new List<Vector2>(loop.Count);
            Vector3 normal = capNormal.normalized;
            Vector4 tangent = new Vector4(basis.AxisU.x, basis.AxisU.y, basis.AxisU.z, 1f);

            // 把三维切口点投影到切割平面的二维坐标中，方便做耳切三角化。
            for (int i = 0; i < loop.Count; i++)
            {
                MeshToolVertex vertex = loop[i];
                vertex.Normal = normal;
                vertex.Tangent = tangent;
                vertex.Uv = Project(vertex.Position, basis);
                vertex.Uv2 = vertex.Uv;
                vertices.Add(vertex);
                projected.Add(vertex.Uv);
            }

            float area = SignedArea(projected);
            if (Mathf.Abs(area) <= MeshToolGeometry.Epsilon)
            {
                return;
            }

            // 耳切算法假设输入为逆时针多边形，面积为负时先翻转。
            if (area < 0f)
            {
                vertices.Reverse();
                projected.Reverse();
            }

            List<int> triangles = Triangulate(projected);
            bool reverseWinding = Vector3.Dot(normal, basis.Normal) < 0f;

            // 正负两侧的封盖法线相反，所以需要按目标法线决定三角形绕序。
            for (int i = 0; i + 2 < triangles.Count; i += 3)
            {
                MeshToolVertex a = vertices[triangles[i]];
                MeshToolVertex b = vertices[triangles[i + 1]];
                MeshToolVertex c = vertices[triangles[i + 2]];

                if (reverseWinding)
                {
                    builder.AddTriangle(a, c, b, capSubMesh);
                }
                else
                {
                    builder.AddTriangle(a, b, c, capSubMesh);
                }
            }
        }

        /// <summary>
        /// 将三维点投影到平面局部二维坐标。
        /// </summary>
        private static Vector2 Project(Vector3 position, PlaneBasis basis)
        {
            return new Vector2(Vector3.Dot(position, basis.AxisU), Vector3.Dot(position, basis.AxisV));
        }

        /// <summary>
        /// 计算二维多边形的有符号面积，正值表示逆时针。
        /// </summary>
        private static float SignedArea(List<Vector2> polygon)
        {
            float area = 0f;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 current = polygon[i];
                Vector2 next = polygon[(i + 1) % polygon.Count];
                area += current.x * next.y - next.x * current.y;
            }

            return area * 0.5f;
        }

        /// <summary>
        /// 用耳切法把二维简单多边形三角化。
        /// </summary>
        private static List<int> Triangulate(List<Vector2> polygon)
        {
            List<int> triangles = new List<int>();
            List<int> indices = new List<int>(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                indices.Add(i);
            }

            int guard = polygon.Count * polygon.Count;
            // 每次找到一个“耳朵”三角形就从多边形中剪掉一个点。
            while (indices.Count > 3 && guard-- > 0)
            {
                bool foundEar = false;
                for (int i = 0; i < indices.Count; i++)
                {
                    int previous = indices[(i + indices.Count - 1) % indices.Count];
                    int current = indices[i];
                    int next = indices[(i + 1) % indices.Count];

                    // 非凸角不可能是耳朵。
                    if (!IsConvex(polygon[previous], polygon[current], polygon[next]))
                    {
                        continue;
                    }

                    // 候选耳朵内部不能包含其它顶点，否则会生成跨边三角形。
                    bool containsPoint = false;
                    for (int j = 0; j < indices.Count; j++)
                    {
                        int index = indices[j];
                        if (index == previous || index == current || index == next)
                        {
                            continue;
                        }

                        if (PointInTriangle(polygon[index], polygon[previous], polygon[current], polygon[next]))
                        {
                            containsPoint = true;
                            break;
                        }
                    }

                    if (containsPoint)
                    {
                        continue;
                    }

                    triangles.Add(previous);
                    triangles.Add(current);
                    triangles.Add(next);
                    indices.RemoveAt(i);
                    foundEar = true;
                    break;
                }

                if (!foundEar)
                {
                    // 容差或自交数据导致找不到耳朵时，退回扇形三角化，保证有结果输出。
                    AddFanFallback(triangles, indices);
                    indices.Clear();
                    break;
                }
            }

            if (indices.Count == 3)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }

            return triangles;
        }

        /// <summary>
        /// 判断二维三点是否形成凸角。
        /// </summary>
        private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
        {
            return Cross(b - a, c - b) > MeshToolGeometry.Epsilon;
        }

        /// <summary>
        /// 判断点 p 是否在三角形 abc 内部或边上。
        /// </summary>
        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float ab = Cross(b - a, p - a);
            float bc = Cross(c - b, p - b);
            float ca = Cross(a - c, p - c);
            return ab >= -MeshToolGeometry.Epsilon &&
                bc >= -MeshToolGeometry.Epsilon &&
                ca >= -MeshToolGeometry.Epsilon;
        }

        /// <summary>
        /// 计算二维向量叉积的 z 分量。
        /// </summary>
        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        /// <summary>
        /// 当耳切失败时，使用第一个点作为扇形中心补一个保底三角化结果。
        /// </summary>
        private static void AddFanFallback(List<int> triangles, List<int> indices)
        {
            if (indices.Count < 3)
            {
                return;
            }

            int anchor = indices[0];
            for (int i = 1; i + 1 < indices.Count; i++)
            {
                triangles.Add(anchor);
                triangles.Add(indices[i]);
                triangles.Add(indices[i + 1]);
            }
        }

        private struct CapSegment
        {
            public MeshToolVertex A;
            public MeshToolVertex B;

            /// <summary>
            /// 创建一条切口线段。
            /// </summary>
            public CapSegment(MeshToolVertex a, MeshToolVertex b)
            {
                A = a;
                B = b;
            }
        }

        private struct PlaneBasis
        {
            public Vector3 Normal;
            public Vector3 AxisU;
            public Vector3 AxisV;

            /// <summary>
            /// 根据法线创建平面上的一组正交二维基向量。
            /// </summary>
            public static PlaneBasis FromNormal(Vector3 normal)
            {
                Vector3 safeNormal = normal.normalized;
                Vector3 axisU = Vector3.Cross(safeNormal, Vector3.up);
                if (axisU.sqrMagnitude <= MeshToolGeometry.EpsilonSqr)
                {
                    // 法线接近世界 Y 轴时，改用 X 轴构造，避免叉积为零。
                    axisU = Vector3.Cross(safeNormal, Vector3.right);
                }

                axisU.Normalize();
                Vector3 axisV = Vector3.Cross(safeNormal, axisU).normalized;

                return new PlaneBasis
                {
                    Normal = safeNormal,
                    AxisU = axisU,
                    AxisV = axisV
                };
            }
        }

        private sealed class CapLoopBuilder
        {
            private const float PointToleranceSqr = MeshToolGeometry.EpsilonSqr * 16f;

            private readonly List<GraphPoint> points = new List<GraphPoint>();
            private readonly HashSet<EdgeKey> unusedEdges = new HashSet<EdgeKey>();

            /// <summary>
            /// 把一条切口线段加入无向图中。
            /// </summary>
            public void AddSegment(MeshToolVertex a, MeshToolVertex b)
            {
                int ai = FindOrAddPoint(a);
                int bi = FindOrAddPoint(b);
                if (ai == bi)
                {
                    return;
                }

                // EdgeKey 会自动排序端点，因此 A-B 和 B-A 被视为同一条边。
                EdgeKey edge = new EdgeKey(ai, bi);
                if (!unusedEdges.Add(edge))
                {
                    return;
                }

                if (!points[ai].Neighbors.Contains(bi))
                {
                    points[ai].Neighbors.Add(bi);
                }

                if (!points[bi].Neighbors.Contains(ai))
                {
                    points[bi].Neighbors.Add(ai);
                }
            }

            /// <summary>
            /// 从未使用边集合中追踪所有闭合切口环。
            /// </summary>
            public List<List<MeshToolVertex>> BuildLoops()
            {
                List<List<MeshToolVertex>> loops = new List<List<MeshToolVertex>>();
                while (unusedEdges.Count > 0)
                {
                    EdgeKey startEdge = FirstUnusedEdge();
                    unusedEdges.Remove(startEdge);

                    // 从任意未使用边出发，一直沿相邻边走到回到起点。
                    int start = startEdge.A;
                    int previous = startEdge.A;
                    int current = startEdge.B;
                    List<int> loop = new List<int>
                    {
                        start,
                        current
                    };

                    int guard = points.Count * points.Count;
                    while (current != start && guard-- > 0)
                    {
                        int next = FindNext(current, previous, start, loop.Count);
                        if (next < 0)
                        {
                            break;
                        }

                        unusedEdges.Remove(new EdgeKey(current, next));
                        previous = current;
                        current = next;

                        if (current != start)
                        {
                            loop.Add(current);
                        }
                    }

                    if (current == start && loop.Count >= 3)
                    {
                        // 只有闭合且至少三个点的路径才能生成封盖面。
                        loops.Add(ToVertexLoop(loop));
                    }
                }

                return loops;
            }

            /// <summary>
            /// 查找已有近似点，找不到时创建新图点。
            /// </summary>
            private int FindOrAddPoint(MeshToolVertex vertex)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if ((points[i].Vertex.Position - vertex.Position).sqrMagnitude <= PointToleranceSqr)
                    {
                        return i;
                    }
                }

                points.Add(new GraphPoint(vertex));
                return points.Count - 1;
            }

            /// <summary>
            /// 在当前点的邻接点中找下一条还没使用过的边。
            /// </summary>
            private int FindNext(int current, int previous, int start, int loopCount)
            {
                GraphPoint point = points[current];
                int fallback = -1;

                for (int i = 0; i < point.Neighbors.Count; i++)
                {
                    int neighbor = point.Neighbors[i];
                    EdgeKey edge = new EdgeKey(current, neighbor);
                    if (!unusedEdges.Contains(edge))
                    {
                        continue;
                    }

                    // 走回起点时，至少要已经形成三条边，避免马上折返成两点环。
                    if (neighbor == start && loopCount > 2)
                    {
                        return neighbor;
                    }

                    if (neighbor != previous)
                    {
                        return neighbor;
                    }

                    fallback = neighbor;
                }

                return fallback;
            }

            /// <summary>
            /// 取出一条还没被追踪过的边。
            /// </summary>
            private EdgeKey FirstUnusedEdge()
            {
                foreach (EdgeKey edge in unusedEdges)
                {
                    return edge;
                }

                return default(EdgeKey);
            }

            /// <summary>
            /// 把图点索引路径转换成顶点路径。
            /// </summary>
            private List<MeshToolVertex> ToVertexLoop(List<int> indices)
            {
                List<MeshToolVertex> loop = new List<MeshToolVertex>(indices.Count);
                for (int i = 0; i < indices.Count; i++)
                {
                    loop.Add(points[indices[i]].Vertex);
                }

                return loop;
            }

            private sealed class GraphPoint
            {
                public MeshToolVertex Vertex;
                public List<int> Neighbors = new List<int>();

                /// <summary>
                /// 创建切口图中的一个节点。
                /// </summary>
                public GraphPoint(MeshToolVertex vertex)
                {
                    Vertex = vertex;
                }
            }

            private struct EdgeKey : IEquatable<EdgeKey>
            {
                public int A;
                public int B;

                /// <summary>
                /// 创建无向边键，内部会把端点排序以便去重。
                /// </summary>
                public EdgeKey(int a, int b)
                {
                    if (a < b)
                    {
                        A = a;
                        B = b;
                    }
                    else
                    {
                        A = b;
                        B = a;
                    }
                }

                /// <summary>
                /// 判断两条无向边是否连接同一对图点。
                /// </summary>
                public bool Equals(EdgeKey other)
                {
                    return A == other.A && B == other.B;
                }

                /// <summary>
                /// 判断对象是否为相同的无向边键。
                /// </summary>
                public override bool Equals(object obj)
                {
                    return obj is EdgeKey other && Equals(other);
                }

                /// <summary>
                /// 计算无向边键的哈希值。
                /// </summary>
                public override int GetHashCode()
                {
                    unchecked
                    {
                        return (A * 397) ^ B;
                    }
                }
            }
        }
    }
}
