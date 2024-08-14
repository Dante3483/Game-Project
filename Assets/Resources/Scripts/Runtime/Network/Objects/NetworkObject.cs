using UnityEngine;

namespace SavageWorld.Runtime.Network.Objects
{
    public class NetworkObject : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private long _id;
        [SerializeField]
        private NetworkObjectTypes _type;
        [SerializeField]
        private Vector2 _position;
        [SerializeField]
        private bool _isOwner = false;
        private NetworkTransform _networkTransform;
        #endregion

        #region Properties
        public long Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public NetworkObjectTypes Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        public bool IsOwner
        {
            get
            {
                return _isOwner;
            }

            set
            {
                _isOwner = value;
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
            }
        }
        #endregion

        #region Events / Delegates

        #endregion

        #region Monobehaviour Methods

        #endregion

        #region Public Methods
        private void Awake()
        {
            _networkTransform = GetComponent<NetworkTransform>();
        }

        private void Update()
        {
            _position = transform.position;
        }

        public void UpdatePosition(float x, float y)
        {
            if (_networkTransform != null)
            {
                _networkTransform.NetworkPosition = new(x, y);
            }
        }

        public void UpdateRotation(float x, float y)
        {
            if (_networkTransform != null)
            {
                _networkTransform.NetworkRotation = new(x, y);
            }
        }

        public void UpdateScale(float x, float y)
        {
            if (_networkTransform != null)
            {
                _networkTransform.NetworkScale = new(x, y);
            }
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
