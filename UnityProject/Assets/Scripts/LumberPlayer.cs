using UnityEngine;

public class LumberPlayer : MonoBehaviour
{
    public float moveSpeed = 4.5f;
    public float turnSpeed = 14f;
    public Animator animator;
    Vector2 input;

    public void SetJoystick(Vector2 value) => input = Vector2.ClampMagnitude(value, 1f);

    void Update()
    {
        Vector3 move = new Vector3(input.x, 0, input.y);
        if (move.sqrMagnitude > 0.01f)
        {
            transform.position += move * moveSpeed * Time.deltaTime;
            var target = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
        if (animator) animator.SetFloat("Speed", move.magnitude);
    }
}
