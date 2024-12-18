using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SavageWorld.Runtime.Entities.Player.Research.UI
{
    public class UIFillAmount : MonoBehaviour, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Private fields
        [SerializeField]
        private Image _image;
        [SerializeField]
        private float _fillSpeed = 0.5f;
        [SerializeField]
        private float _currentFillAmount = 0f;
        private bool _isFill;
        private Action _fillFrame;
        #endregion

        #region Public fields
        public Action OnFrameFilled;
        #endregion

        #region Properties
        public Image Image { get => _image; set => _image = value; }
        #endregion

        #region Methods
        private void Awake()
        {
            _fillFrame += () =>
            {
                if (_isFill)
                {
                    FillFrame(_fillSpeed * Time.deltaTime);
                }
                else
                {
                    FillFrame(-_fillSpeed * Time.deltaTime);
                }
            };
        }

        private void FixedUpdate()
        {
            _fillFrame?.Invoke();
        }

        public void FillFrame(float value)
        {
            _currentFillAmount += value;
            _currentFillAmount = Mathf.Clamp01(_currentFillAmount);
            _image.fillAmount = _currentFillAmount;
            if (_currentFillAmount == 1f)
            {
                OnFrameFilled?.Invoke();
            }
        }

        public void ResetFrame()
        {
            _currentFillAmount = 0f;
            _image.fillAmount = _currentFillAmount;
            _isFill = false;
        }

        public void Stop()
        {
            _fillFrame = null;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isFill = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _isFill = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _isFill = false;
            }
        }
        #endregion
    }
}