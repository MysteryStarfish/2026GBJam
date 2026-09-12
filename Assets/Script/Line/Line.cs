using UnityEngine;
using UnityEngine.InputSystem;

public class Line : MonoBehaviour
{
    [SerializeField] private PlayerInputReader playerInputReader;

    void OnEnable()
    {
        playerInputReader.OnAttack += HandleAttack;
    }

    void OnDisable()
    {
        playerInputReader.OnAttack -= HandleAttack;
    }

    private void HandleAttack()
    {
        throw new System.NotImplementedException();
    }
}
