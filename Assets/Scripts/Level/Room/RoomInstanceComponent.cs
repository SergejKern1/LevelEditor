using System;
using ScriptableUtility;
using UnityEngine;

using static Level.Room.Operations.RoomOperations;

namespace Level.Room
{
    public class RoomInstanceComponent : MonoBehaviour
    {
        public GameObject GameObject { get; private set; }
        public Transform Transform { get; private set; }
        public IContext Context => m_context;
        public ref RoomInstanceData Data => ref m_roomInstanceData;

        [SerializeField] RoomInstanceData m_roomInstanceData;
        IContext m_context;

        public void SetData(RoomInstanceData data)
        {
            Init();
            m_roomInstanceData = data;
            m_roomInstanceData.UpdateRoomPosition(Transform.position);
            m_roomInstanceData.ShowRoom();
        }

        void Awake() => Init();
        void Init()
        {
            Transform = transform;
            GameObject = gameObject;
            TryGetComponent(out m_context);
        }

        Vector3 m_lastPos;
        void Update() => UpdatePosition();

        public void UpdatePosition()
        {
            var currentPos = Transform.position;
            if (m_lastPos == currentPos) 
                return;
            m_lastPos = currentPos;
            
            m_roomInstanceData.UpdateRoomPosition(currentPos);
        }
    }
}
