using System.Collections.Generic;
using Player.ActionHandlers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Camera
{
    public class CameraMover : MonoBehaviour
    {
        [SerializeField] private CameraHolder _cameraHolder;
        [SerializeField] private bool _invert;
        [Range(10f, 100f)] [SerializeField] private float _dragVelocity = 50f;
        [SerializeField] private bool _inertia;
        [Range(3f, 30f)] [SerializeField] private float _smoothness = 10f;

        [SerializeField] private Vector2 _boundMin;
        [SerializeField] private Vector2 _boundMax;
        [SerializeField] private bool _resetPositionOnSceneLoad = true;

        private ClickHandler _clickHandler;
        private Vector3 _defaultPosition;
        private Vector3 _dragPosition;
        private bool _isDragging;
        private Vector2 _previousPosition;

        private void Awake()
        {
            _defaultPosition = transform.position; // if camera has not 0 coordinates by X and/or Y
            _clickHandler = ClickHandler.Instance;
        }

        private void OnEnable()
        {
            _clickHandler.ScreenDragStartEvent += OnStartDrag;
            _clickHandler.ScreenDragEvent += OnDrag;
            _clickHandler.ScreenDragEndEvent += OnDragEnd;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            _clickHandler.ScreenDragStartEvent -= OnStartDrag;
            _clickHandler.ScreenDragEvent -= OnDrag;
            _clickHandler.ScreenDragEndEvent -= OnDragEnd;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_resetPositionOnSceneLoad)
            {
                ResetPosition();
            }
        }

        private void OnStartDrag(Vector2 position)
        {
            // Check if pointer is over any UI element
            if (IsPointerOverUIElement(position))
            {
                return;
            }

            // Check if pointer is over any Sprite
            var worldPosition = _cameraHolder.MainCamera.ScreenToWorldPoint(position);
            if (IsPointerOver2DSprite(worldPosition))
            {
                return;
            }

            _previousPosition = position;
            _isDragging = true;
        }

        private void OnDragEnd(Vector2 position)
        {
            _isDragging = false;
            if (!_inertia)
            {
                _dragPosition = transform.position - _defaultPosition;
            }
        }

        private void OnDrag(Vector2 pos)
        {
            if (!_isDragging)
            {
                return;
            }

            var delta = (Vector3)(pos - _previousPosition) / Screen.height;
            _previousPosition = pos;
            _dragPosition += delta * (Time.deltaTime * 50 * _dragVelocity * (_invert ? -1 : 1));
            _dragPosition.x = Mathf.Clamp(_dragPosition.x - _defaultPosition.x, _boundMin.x, _boundMax.x);
            _dragPosition.y = Mathf.Clamp(_dragPosition.y - _defaultPosition.y, _boundMin.y, _boundMax.y);
        }

        private void ResetPosition()
        {
            transform.position = _defaultPosition;
            _dragPosition = Vector3.zero;
            _isDragging = false;
        }

        private void Update()
        {
            if (_isDragging || _inertia)
            {
                var newValue = _defaultPosition + _dragPosition;
                transform.position = Vector3.Lerp(transform.position, newValue, Time.deltaTime * _smoothness);
            }
        }

        private bool IsPointerOverUIElement(Vector2 pointerPosition)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = pointerPosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        private bool IsPointerOver2DSprite(Vector2 worldPosition)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null)
            {
                return true;
            }

            return false;
        }
    }
}
