using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Move : MonoBehaviour
{
    private Rigidbody2D rigid2D;
    private Shoot shoot;
    private Vector2 input;
    public float speed = 1;
    public float acc = 10;
    public float rollsize;

    public LayerMask notmeiswear;
    // Start is called before the first frame update
    void Awake()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        shoot = GetComponent<Shoot>();
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input -= shoot.endVector;
        rigid2D.AddForce(input.x * Vector2.right * speed * acc);
    }

    void Goo()
    {

    }

    void LateUpdate()
    {
        rigid2D.velocity = new Vector2(Mathf.Clamp(rigid2D.velocity.x, -speed, speed), rigid2D.velocity.y);
    }
}
/*Junk
 *        if (Physics2D.Raycast(transform.position, Vector2.down, 0.1f, notmeiswear))
        {
            groundAcc = acc;
        }
        else groundAcc = acc * 0.25f;
 * 
 */
