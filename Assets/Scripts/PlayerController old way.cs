using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerOldWay : MonoBehaviour {

    public GameObject player;
    public GameObject hitbox;
    public Transform hitCollider;

    private Transform trans;
    private Transform hitTrans;
    private Rigidbody rb;

    [SerializeField]
    private float damage;

    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    public KeyCode hitKey;
    public KeyCode hitBKey;
    public KeyCode duckKey;

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
        trans = GetComponent<Transform>();
        hitTrans = hitCollider.GetComponent<Transform>();

        damage = 0.0f;
    }

    private void Update()
    {
        jump = Input.GetKeyDown(jumpKey);
        duck = Input.GetKeyDown(duckKey);

        //horizMove = Input.GetAxis("Horizontal");
        horizMove = 0.0f;
        if (Input.GetKey(leftKey))
        {
            horizMove = -1.0f;
        } else if (Input.GetKey(rightKey)) { horizMove = 1.0f; }

        if(Input.GetKey(upKey) || Input.GetKey(downKey) || Input.GetKey(leftKey) || Input.GetKey(rightKey))
        {
            StartCoroutine(SmashCounter());
        }
        bool normalHitButton = Input.GetKeyDown(hitKey);
        if (normalHitButton)
        {
            if (Input.GetKey(upKey))
            {
                NormalHit(1);
            }
            else if (Input.GetKey(downKey))
            {
                NormalHit(2);
            }
            else if (Input.GetKey(leftKey))
            {
                NormalHit(3);
            }
            else if (Input.GetKey(rightKey))
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
        
        if(rb.velocity.y > 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * jumpMultiplyer * Time.deltaTime;
        }
        else if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * fallMultiplyer * Time.deltaTime;
        }

        rb.position += new Vector3(0.0f, 0.0f, (horizMove * speed * Time.deltaTime));
        if(horizMove > 0)
        {
            rb.MoveRotation(Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }
        else if(horizMove < 0)
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
        if(!attacking)
        {
            attacking = true;
            float attackPower = hitPower;
            if (smash)
            {
                attackPower += smashPower;
            }

            if(dir == 0)
            {
                //no dir
                hitCollider.localPosition = new Vector3(0.0f, 1.2f, 0.5f);
            }
            else if (dir == 1)
            {
                //up
                hitCollider.localPosition = new Vector3(0.0f, 2.1f, 0.3f);
            }
            else if (dir == 2)
            {
                //down
                hitCollider.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
            }
            else if (dir == 3)
            {
                //left
                if(rb.rotation.y == 180.0f)
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

            StartCoroutine( CoolDown());
        }
    }

    private void BHit()
    {


        StartCoroutine( CoolDown());
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
        Debug.Log(direction);
        //apply force
        rb.AddForce(direction * damage);
    }

    public void ResetDamage()
    {
        damage = 0.0f;
    }
}
