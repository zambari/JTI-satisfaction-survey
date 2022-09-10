using UnityEngine;

public class CursorVisibilityController : MonoBehaviour
{
    [SerializeField]
    private bool visibleOnStart = false;

    private void Start()
    {
        Cursor.visible = visibleOnStart;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
            Cursor.visible = !Cursor.visible;
    }
}