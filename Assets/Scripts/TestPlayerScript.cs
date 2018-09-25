using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerScript : MonoBehaviour
{

    public GameObject player;
    public GameObject hitbox;
    public Transform hitCollider;

    private Transform trans;
    private Transform hitTrans;
    private Rigidbody rb;

    [SerializeField]
    private float damage;

    public float speed;
    public float tilt;
    public float jumpPower;
    public float jumpSpeed;
    public float jumpMultiplyer;
    public float fallMultiplyer;

    private bool jump;
    private bool duck;
    private float horizMove;
    private float vertiMove;

    private bool smash = false;
    private int hitDir;
    public float smashPower, hitPower, upBPower, downBPower, normalBPower, coolDown;

    private bool attacking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hitTrans = hitCollider.GetComponent<Transform>();

        damage = 0.0f;
    }

    private void Update()
    {
        jump = Input.GetKeyDown(KeyCode.Space);
        duck = Input.GetKeyDown(KeyCode.DownArrow);

        horizMove = Input.GetAxis("Horizontal");
        //float horizMove = 0.0f;
        //if (Input.GetKey(KeyCode.A))
        //{
        //    horizMove = -1.0f;
        //} else if (Input.GetKey(KeyCode.D)) { horizMove = 1.0f; }

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            StartCoroutine(SmashCounter());
        }
        bool normalHitButton = Input.GetKeyDown(KeyCode.Z);
        if (normalHitButton)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                NormalHit(1);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                NormalHit(2);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                NormalHit(3);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                NormalHit(4);
            }
            else
            {
                NormalHit(0);
            }
        }


        vertiMove = 0.0f;
        if (jump)
        {
            vertiMove = jumpPower;
        }
        else { vertiMove = 0.0f; }
    }

    private void FixedUpdate()
    {

        Vector3 jumpMovement = new Vector3(0.0f, vertiMove, 0.0f);

        rb.AddForce(jumpMovement * jumpSpeed * Time.deltaTime);
        //rb.velocity = jumpMovement * jumpSpeed * Time.deltaTime;

        if (rb.velocity.y > 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * jumpMultiplyer * Time.deltaTime;
        }
        else if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * fallMultiplyer * Time.deltaTime;
        }

        rb.position += new Vector3(0.0f, 0.0f, (horizMove * speed * Time.deltaTime));
        if (horizMove > 0)
        {
            rb.MoveRotation(Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }
        else if (horizMove < 0)
        {
            rb.MoveRotation(Quaternion.Euler(0.0f, 180.0f, 0.0f));
        }
    }

    IEnumerator SmashCounter()
    {
        smash = true;
        yield return new WaitForSeconds(0.5f);
        smash = false;
    }

    IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(coolDown);
        attacking = false;
        hitCollider.transform.localPosition = new Vector3(0.0f, 1.2f, 0.0f);
    }

    private void NormalHit(int dir)
    {
        if (!attacking)
        {
            attacking = true;
            float attackPower = hitPower;
            if (smash)
            {
                attackPower += smashPower;
            }

            if (dir == 0)
            {
                //no dir
                hitCollider.localPosition = new Vector3(0.0f, 1.2f, 0.5f);
            }
            else if (dir == 1)
            {
                //up
                hitCollider.localPosition = new Vector3(0.0f, 2.2f, 0.0f);
            }
            else if (dir == 2)
            {
                //down
                hitCollider.localPosition = new Vector3(0.0f, 0.25f, 0.5f);
            }
            else if (dir == 3)
            {
                //left
                if (rb.rotation.y == 180.0f)
                {
                    //left is forward
                    hitCollider.localPosition = new Vector3(0.0f, 1.2f, -0.5f);
                }
                else
                {
                    //left is back
                    hitCollider.localPosition = new Vector3(0.0f, 1.2f, 0.5f);
                }
            }
            else if (dir == 4)
            {
                //right
                if (rb.rotation.y == 0.0f)
                {
                    //right is forward
                    hitCollider.localPosition = new Vector3(0.0f, 1.2f, -0.5f);
                }
                else
                {
                    //right is back
                    hitCollider.localPosition = new Vector3(0.0f, 1.2f, 0.5f);
                }
            }

            StartCoroutine(CoolDown());
        }
    }

    private void BHit()
    {


        StartCoroutine(CoolDown());
    }

    public float GetHitPower()
    {
        return hitPower;
    }

    public void Hit(float hitDamage, Transform otherPlayer)
    {
        //get vector between players for force vector
        Vector3 direction = trans.position - otherPlayer.position;

        damage += hitDamage;
        //apply force
        rb.AddForce(direction * damage);
    }
}
