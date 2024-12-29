using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputReader : MonoBehaviour
    {
        Vector2 selected;
        bool fire = false;
        public Vector2 Selected => selected;
        public event Action Fire;
        public void OnSelect(InputValue value)
        {
            SelectInput(value.Get<Vector2>());
        }
        public void OnFire(InputValue value)
        {
            FireInput(value.isPressed);
        }
        public void FireInput(bool newFireState)
        {
            fire = newFireState;
            Debug.Log(Camera.main.ScreenToWorldPoint(new Vector3(selected.x, selected.y, 4)));
            if (fire)
            {
                Fire?.Invoke();
            }
            fire = false;
        }
        public void SelectInput(Vector2 newSelected)
        {
            selected = newSelected;
        }
    }
}