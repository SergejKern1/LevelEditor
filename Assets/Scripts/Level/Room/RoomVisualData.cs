using System.Collections.Generic;
using Core.Extensions;
using Core.Unity.Extensions;
using UnityEngine;

namespace Level.Room
{
    public struct RoomVisualData
    {
        readonly Transform m_floorVisualsParent;
        readonly Transform m_ceilingVisualsParent;

        public RoomVisualData(Transform floor, Transform ceiling)
        {
            m_floorVisualsParent = floor;
            m_ceilingVisualsParent = ceiling;
            m_floorVisuals = new List<PlatformVisualData>();
            m_ceilingVisuals = new List<PlatformVisualData>();
        }

        readonly List<PlatformVisualData> m_floorVisuals;
        readonly List<PlatformVisualData> m_ceilingVisuals;

        public bool IsValid =>
            m_floorVisualsParent != null && m_ceilingVisualsParent != null && 
            m_floorVisuals != null && m_ceilingVisuals != null;

        public int FloorCount => m_floorVisuals?.Count ?? -1;
        public int CeilingCount => m_ceilingVisuals?.Count ?? -1;

        /// <summary>
        /// Sets the Level the player is currently at, eg. what should be visible
        /// </summary>
        /// <param name="lvl">the level</param>
        public void SetVisibleHeight(int lvl)
        {
            //int ceilLvl = lvl;
            //if (ceilLvl >= _CeilingVisuals.Count)
            //    ceilLvl = _CeilingVisuals.Count -1;
            if (!IsValid)
                return;

            if (FloorCount != (CeilingCount + 1))
            {
                Debug.LogError("FloorCount != (CeilingCount + 1)");
                return;
            }

            var i = 0;
            for (; i < CeilingCount; ++i)
            {
                GetCeilingVisuals(i).Go.SetActive(lvl == i);
                GetFloorVisuals(i).Go.SetActive(i <= lvl+1);
            }
            GetFloorVisuals(i).Go.SetActive(i <= lvl + 1);
            // cannot remember why i >= lvl - 1 (which means for last level we cannot go higher then one above or it will not show)
            //_FloorVisuals[i].Go.SetActive(i >= lvl - 1 && i <= lvl + 1);
        }

        public void UpdateVisuals(int maxLevel, Material floorMaterial, Material ceilingMaterial)
        {
            var i = 0;
            for (; i < maxLevel; ++i)
            {
                EnsureVisuals(m_ceilingVisualsParent, m_ceilingVisuals, i, ceilingMaterial);
                EnsureVisuals(m_floorVisualsParent, m_floorVisuals, i, floorMaterial, true);
            }
            // we always need one extra floor visual, because we render 2 for each level
            EnsureVisuals(m_floorVisualsParent, m_floorVisuals, i, floorMaterial, true);

            var killLoopCeilings = Mathf.Max(m_ceilingVisualsParent.childCount, m_ceilingVisuals.Count);
            var killLoopFloors = Mathf.Max(m_floorVisualsParent.childCount, m_floorVisuals.Count);
            var loop = Mathf.Max(killLoopCeilings, killLoopFloors);

            for (var j = loop-1; j >= i; --j)
            {
                RemoveVisuals(m_ceilingVisualsParent, m_ceilingVisuals, j);
                if (j > i)
                    RemoveVisuals(m_floorVisualsParent, m_floorVisuals, j);
            }
        }

        public PlatformVisualData GetFloorVisuals(int levelIdx) => m_floorVisuals[levelIdx];
        public PlatformVisualData GetCeilingVisuals(int levelIdx) => m_ceilingVisuals[levelIdx];

        public void Destroy()
        {
            Destroy(m_floorVisuals);
            Destroy(m_ceilingVisuals);
        }

        static void Destroy(IEnumerable<PlatformVisualData> visuals)
        {
            if (visuals == null)
                return;
            foreach (var v in visuals)
            {
                if (v.Mesh != null) 
                    v.Mesh.DestroyEx();
            }
        }

        static void RemoveVisuals(Transform parent, List<PlatformVisualData> visuals, int idx)
        {
            if (parent.childCount > idx)
                parent.GetChild(idx).gameObject.DestroyEx();

            if (!idx.IsInRange(visuals))
                return;
            var mesh = visuals[idx].Mesh;
            if (mesh != null)
                mesh.DestroyEx();

            visuals.RemoveAt(idx);
        }

        static void EnsureVisuals(Transform parent, IList<PlatformVisualData> visuals, int idx, Material m, 
            bool needCollider = false)
        {
            GetOrCreateFor(parent, idx, needCollider, out var visualGo, out var coll, out var rend, out var filter);
            SetMeshAndMaterial(m, rend, filter, coll, out var mesh);
            SetVisualData(visuals, idx, coll, filter, visualGo, rend, mesh);
        }

        static void SetVisualData(IList<PlatformVisualData> visuals, int idx, MeshCollider coll, MeshFilter filter, GameObject visualGo,
            MeshRenderer rend, Mesh mesh)
        {
            var data = new PlatformVisualData()
            {
                Collider = coll,
                Filter = filter,
                Go = visualGo,
                Renderer = rend,
                Mesh = mesh,
            };

            while (visuals.Count <= idx)
                visuals.Add(default);

            visuals[idx] = data;
        }

        static void SetMeshAndMaterial(Material m, Renderer rend, MeshFilter filter, MeshCollider coll, 
            out Mesh mesh)
        {
            rend.sharedMaterial = m;
            mesh = filter.sharedMesh;
            if (mesh != null) 
                return;
            mesh = new Mesh();
            filter.mesh = mesh;
            if (coll != null)
                coll.sharedMesh = mesh;
        }

        static void GetOrCreateFor(Transform parent, int idx, bool needCollider, out GameObject visualGo, out MeshCollider coll,
            out MeshRenderer rend, out MeshFilter filter)
        {
            coll = null;
            if (parent.childCount <= idx)
            {
                parent.CreateChildObject(idx.ToString(), out visualGo, out var visualTr, HideFlags.None,
                    typeof(MeshRenderer), typeof(MeshFilter));
                visualTr.localPosition = new Vector3(0, idx, 0);
            }
            else
            {
                var tr = parent.GetChild(idx);
                visualGo = tr.gameObject;
                visualGo.TryGetComponent(out coll);
            }

            visualGo.TryGetComponent(out rend);
            visualGo.TryGetComponent(out filter);
            if (coll == null && needCollider)
                coll = visualGo.AddComponent<MeshCollider>();
        }
    }
}
