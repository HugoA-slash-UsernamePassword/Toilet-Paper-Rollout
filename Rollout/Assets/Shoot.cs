using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public KeyCode shoot = KeyCode.S;
    public float shootSpeed;
    public bool spin;
    public GameObject chain;
    public GameObject nodePrefab;
    public GameObject ballPrefab;
    public List<GameObject> nodes;
    public List<GameObject> legacynodes;
    public GameObject ball;
    //public bool returnToSender;
    public GrappleState state;
    public MoveState moveGrapple;
    public Transform hooks;
    public float hookSize = 1f;
    public float holdMultiplier = 0;
    public float endDistance;
    public Vector2 endVector;
    //public GameObject endnode;
    //public GameObject endchain;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        holdMultiplier = Mathf.Clamp(holdMultiplier,-2, 2);
        Vector2 scale1 = Vector2.one * (1.1f - 0.1f * FindObjectsOfType<SpringJoint2D>().Length);
        //transform.localScale = scale1;
        transform.GetChild(1).localScale = scale1;
        GetComponent<CircleCollider2D>().radius = scale1.x / 2;

        if (Input.GetKeyDown(shoot) && state == GrappleState.None)
        {
            Vector2 pos = (transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition));
            pos = pos.normalized;
            Debug.Log(pos);
            ball = Instantiate(ballPrefab, transform.position + (Vector3)pos, Quaternion.identity);
            ball.GetComponent<Rigidbody2D>().velocity = pos * -shootSpeed;

            GameObject _chainStart = Instantiate(chain, transform.position, Quaternion.identity);
            nodes.Add(_chainStart);

            GameObject _node = Instantiate(nodePrefab, transform.position, Quaternion.identity);
            nodes.Add(_node);
            _node.GetComponent<SpringJoint2D>().connectedBody = _chainStart.GetComponent<Rigidbody2D>();
        }
        if (Input.GetKeyDown(shoot) && state == GrappleState.Hooked)
        {

        }
        if (Input.GetKeyUp(shoot) && state == GrappleState.None)
        {
            for (int i = 1; i < nodes.Count; i++)
            {
                nodes[i].GetComponent<CircleCollider2D>().enabled = false;
            }
            Destroy(ball);
            state = GrappleState.Return;
        }
        moveGrapple = MoveState.Normal;
        if (state == GrappleState.Hooked)
        {
            if (Input.GetKey(KeyCode.W))
            {
                moveGrapple = MoveState.Less;
                holdMultiplier -= 0.2f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveGrapple = MoveState.More;
                holdMultiplier += 0.2f;
            }
            else
            {
                moveGrapple = MoveState.Hold;
                //holdMultiplier = 0;
            }
        }
        if (nodes.Count > 1)
        {
            endDistance = transform.position.x - nodes[1].transform.position.x;
            if (ball) endVector = (transform.position - ball.transform.position) / nodes.Count;
            else endVector = Vector2.zero;
            GameObject shootchain = nodes[nodes.Count - 1];
            LineRenderer rend = nodes[0].GetComponent<LineRenderer>();
            rend.SetPosition(0, transform.position);
            rend.positionCount = nodes.Count + 1;
            for (int i = 0; i < nodes.Count; i++)
            {
                rend.SetPosition(i + 1, nodes[i].transform.position);
            }
            //shootchain.GetComponent<Rigidbody2D>().velocity = (shootchain.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized * -25;
            if (state == GrappleState.Return)
            {
                nodes[1].GetComponent<Rigidbody2D>().velocity = (nodes[1].transform.position - transform.position) * -25;
                if (Vector3.Distance(transform.position, shootchain.transform.position) < 1 && nodes.Count == 2)
                {
                    state = GrappleState.None;
                    Destroy(nodes[1]);
                    nodes.RemoveAt(1);
                    rend.positionCount = 0;
                    return;
                }
            }
            else
            {
                Rigidbody2D _rb = ball.GetComponent<Rigidbody2D>();
                shootchain.GetComponent<Rigidbody2D>().velocity = (shootchain.transform.position - ball.transform.position) * -25;
                ball.GetComponent<Rigidbody2D>().AddForce((ball.transform.position - shootchain.transform.position).normalized * -5f);
                //if (Vector2.Distance(transform.position, ball.transform.position) > 6)
                //{
                //    _rb.AddForce((transform.position - ball.transform.position) * 1f);
                //    GetComponent<Rigidbody2D>().AddForce((transform.position - ball.transform.position) * -.05f);
                //}
                //ball.transform.position = Vector2.ClampMagnitude(ball.transform.position - transform.position, 8);

                for (int i = 0; i < hooks.childCount; i++)
                {
                    if (Vector2.Distance(ball.transform.position, hooks.GetChild(i).position) < hookSize)
                    {
                        ball.transform.position = hooks.GetChild(i).position;
                        //ball = null;
                        state = GrappleState.Hooked;
                        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    }
                    //if (Vector2.Distance(transform.position, hooks.GetChild(i).position) < hookSize)
                    //{
                    //    .transform.position = hooks.GetChild(i).position;
                    //    //ball = null;
                    //    state = GrappleState.Hooked;
                    //    _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    //}
                }
            }
            //endchain.transform.localPosition = Vector3.zero;
            if (Vector3.Distance(transform.position, shootchain.transform.position) < 1 + nodes.Count * 0.25f && nodes.Count > 2 && moveGrapple != MoveState.More) //Delete node
            {
                holdMultiplier -= 0.25f - (Vector3.Distance(transform.position, shootchain.transform.position)/nodes.Count);
            }
            nodes[0].GetComponent<Rigidbody2D>().velocity = (transform.position - nodes[0].transform.position) * 25;
            if(endVector.magnitude > 1) GetComponent<Rigidbody2D>().AddForce(endVector.normalized * -1);
            switch (moveGrapple)
            {
                case MoveState.More:
                    if (Vector2.Distance(transform.position, nodes[1].transform.position) < 6)
                    {
                        GetComponent<Rigidbody2D>().AddForce((transform.position - ball.transform.position).normalized * 1);
                    }
                    break;
                case MoveState.Less:
                    GetComponent<Rigidbody2D>().AddForce((transform.position - ball.transform.position).normalized * -10);
                    if (Vector2.Distance(transform.position, nodes[1].transform.position) < 6)
                    {
                        GetComponent<Rigidbody2D>().AddForce((transform.position - ball.transform.position).normalized * 1);
                    }
                    break;
                default:
                    break;
            }
            if (holdMultiplier < -1 && nodes.Count > 2 && Vector2.Distance(nodes[0].transform.position,nodes[1].transform.position) < 1) //Delete node
            {
                holdMultiplier = 0;
                Destroy(nodes[1]);
                nodes.RemoveAt(1);
                nodes[1].GetComponent<SpringJoint2D>().connectedBody = nodes[0].GetComponent<Rigidbody2D>();
            }
            if (holdMultiplier > 1 && nodes.Count <= 8) //Add node
            {
                holdMultiplier = 0;
                GameObject _node = Instantiate(nodePrefab, transform.position, Quaternion.identity);
                nodes[1].GetComponent<SpringJoint2D>().connectedBody = _node.GetComponent<Rigidbody2D>();
                nodes.Insert(1, _node);
                nodes[1].GetComponent<SpringJoint2D>().connectedBody = nodes[0].GetComponent<Rigidbody2D>();
            }
            if (!spin)
            {
                nodes[0].GetComponent<CircleCollider2D>().radius = .1f;
                //endnode.GetComponent<SpringJoint2D>().connectedBody = endchain.GetComponent<Rigidbody2D>();
                if (Vector3.Distance(transform.position, nodes[1].transform.position) > 1.5f && nodes.Count <= 8 && moveGrapple != MoveState.Less && moveGrapple != MoveState.Hold) //Add node
                {
                    holdMultiplier += 0.2f;
                }
            }
            else
            {
                transform.position = nodes[0].transform.position;
                nodes[0].GetComponent<CircleCollider2D>().radius = GetComponent<CircleCollider2D>().radius;

            }
        }
        else
        {
            state = GrappleState.None;
            if (nodes.Count >= 1) nodes[0].transform.position = transform.position;
        }
        //for
        //{
        //    Rigidbody2D _rb = ball.GetComponent<Rigidbody2D>();
        //    shootchain.GetComponent<Rigidbody2D>().velocity = (shootchain.transform.position - ball.transform.position) * -25;
        //    ball.GetComponent<Rigidbody2D>().AddForce((ball.transform.position - shootchain.transform.position).normalized * -1f);
        //    //if (Vector2.Distance(transform.position, ball.transform.position) > 6)
        //    //{
        //    //    _rb.AddForce((transform.position - ball.transform.position) * 1f);
        //    //    GetComponent<Rigidbody2D>().AddForce((transform.position - ball.transform.position) * -.05f);
        //    //}
        //    //ball.transform.position = Vector2.ClampMagnitude(ball.transform.position - transform.position, 8);

        //    for (int i = 0; i < hooks.childCount; i++)
        //    {
        //        if (Vector2.Distance(ball.transform.position, hooks.GetChild(i).position) < hookSize)
        //        {
        //            ball.transform.position = hooks.GetChild(i).position;
        //            //ball = null;
        //            state = GrappleState.Hooked;
        //            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        //        }
        //    }
        //}
        //if (Input.GetKeyUp(shoot))
        //{
        //    endnode = null;
        //    GameObject _chain = Instantiate(chain, transform);
        //    _chain.GetComponent<Rigidbody2D>().velocity = (transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized * -shootSpeed;
        //}


    }
    public enum GrappleState
    {
        None,
        Return,
        Hooked
    }
    public enum MoveState
    {
        Normal,
        More,
        Less,
        Hold
    }
}
/*  Code Graveyard
 *  
 *             SpringJoint2D[] foundSprings = FindObjectsOfType<SpringJoint2D>();
            LineRenderer rend = endchain.GetComponent<LineRenderer>();
            rend.positionCount = foundSprings.Length + 1;
            rend.SetPosition(0, endchain.transform.position);
            for (int i = 0; i < foundSprings.Length; i++)
            {
                rend.SetPosition(i + 1, foundSprings[i].transform.position);
            }
 * 
 *             //Vector2 tempVel = GetComponent<Rigidbody2D>().velocity;
            //GetComponent<Rigidbody2D>().velocity = nodes[0].GetComponent<Rigidbody2D>().velocity;
            //nodes[0].GetComponent<Rigidbody2D>().velocity = tempVel;
 *
 * 
 * 
 * 
 * 
 */
