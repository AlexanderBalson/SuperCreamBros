using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public GameObject player;
    public GameObject hitbox; //The hitbox for receiving damage.
    public Transform punchCollider; //The hitbox for damaging others.

    private Transform trans; //Player object's transform.
    //private Transform hitTrans;
    private BoxCollider punchBoxCollider; //Box collider of the damaging hitbox.
    private Rigidbody rb; //Rigidbody of the player.

    [SerializeField]
    private float damage; //Player damage.

    //KeyCodes for controlling this player, must be a better way to do this.
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    public KeyCode hitKey;
    public KeyCode hitBKey;
    public KeyCode duckKey;

    //Bools to control inputs for smash attacks.
    private bool lastFrameUp;
    private bool lastFrameDown;
    private bool lastFrameLeft;
    private bool lastFrameRight;
    private bool upKeyHold;
    private bool downKeyHold;
    private bool leftKeyHold;
    private bool rightKeyHold;

    public float speed; //Movement speed.
    public float turnSpeed; //Turn rotation speed.
    public float jumpPower; //Initial jump impulse.
    public float jumpSpeed; //Some other jump multiplyer.
    public int jumpMultiplyer; //Controls speed of upwards part of jump.
    public float fallMultiplyer; //Controls speed of downwards part of jump.

    private int leftRight; //1 = facing left, 2 = facing right. Bool used for?

    private bool jump; //Jumping.
    private bool duck; //Ducking.
    private float horizMove; //Horizontal movement.
    private float vertiMove; //Vertical movement.
    private bool jumping = false; //Is jumping right now?
    private bool bUp; //Is using b+up move right now?

    private bool smash = false; //Is a smash attack?
    private int attackLevel; //1 = regular, 2 = smash, 3 = ???
    //private int hitDir;
    //Controls for attack power of different move types, and cooldown between attacks.
    public float smashPower, hitPower, upBPower, downBPower, normalBPower, coolDown;
    private float attackWindow; //How long the damaging hitbox is able to damage other players during an attack.

    private int jumpCount = 0;
    private bool usedBUp;
    private bool attacking = false; //Is attacking right now?

    //Direction change turning controls.
    private bool directionChange = false;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private float rotationProgress = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trans = GetComponent<Transform>();
        //hitTrans = punchCollider.GetComponent<Transform>();
        punchBoxCollider = punchCollider.GetComponent<BoxCollider>();
    }

    private void Start()
    {
        punchBoxCollider.enabled = false;

        damage = 0.0f; //Start damage is 0;
        turnSpeed = 30.0f; //Standard turning speed.

        attackWindow = 0.05f; //Standard attack window.

        //Awkwardly set the flag based on the rotation of the player model.
        leftRight = 2;
        if (trans.rotation.y != 0.0f)
        {
            leftRight = 1;
        }
    }

    private void Update()
    {
        //Get flags for jumping and ducking
        jump = Input.GetKeyDown(jumpKey);
        duck = Input.GetKeyDown(duckKey);

        //Set flags based on key up/down, not using getKeyDown because it messes with smash attacks.
        if (Input.GetKeyDown(upKey))
        {
            upKeyHold = true;
        }
        else if (Input.GetKeyDown(downKey))
        {
            downKeyHold = true;
        }
        else if (Input.GetKeyDown(leftKey))
        {
            leftKeyHold = true;
        }
        else if (Input.GetKeyDown(rightKey))
        {
            rightKeyHold = true;
        }
        if (Input.GetKeyUp(upKey))
        {
            upKeyHold = false;
        }
        else if (Input.GetKeyUp(downKey))
        {
            downKeyHold = false;
        }
        else if (Input.GetKeyUp(leftKey))
        {
            leftKeyHold = false;
        }
        else if (Input.GetKeyUp(rightKey))
        {
            rightKeyHold = false;
        }

        //Setting some local bools in preparation for controller support,
        //which I assume will need a different arrangement.
        bool up = upKeyHold;
        bool down = downKeyHold;
        bool left = leftKeyHold;
        bool right = rightKeyHold;
        
        //Set horizontal movement, and flag for direction change.
        horizMove = 0.0f;
        if (Input.GetKey(leftKey))
        {
            horizMove = -1.0f;
            if(leftRight == 2)
            {
                directionChange = true;
            }
            leftRight = 1;
        }
        else if (Input.GetKey(rightKey))
        {
            horizMove = 1.0f;
            if(leftRight == 1)
            {
                directionChange = true;
            }
            leftRight = 2;
        }

        //If the player's done a smash attack movement with direction keys.
        if((up && !lastFrameUp) || (down && !lastFrameDown) || (left && !lastFrameLeft) || (right && lastFrameRight))
        {
            StartCoroutine(SmashCounter());
        }

        bool normalHitButton = Input.GetKeyDown(hitKey);
        bool bHitButton = Input.GetKeyDown(hitBKey);
        if (normalHitButton)
        {
            if (up) { NormalHit(1); }
            else if (down) { NormalHit(2); }
            else if (left) { NormalHit(3); }
            else if (right) { NormalHit(4); }
            else { NormalHit(0); }
        }
        if (bHitButton)
        {
            if (up) { BHit(1); }
            else if (down) { BHit(2); }
            else if (left) { BHit(3); }
            else if (right) { BHit(4); }
            else { BHit(0); }
        }
        
        //Rotate player
        if (directionChange && leftRight == 1)
        {
            //facing left
            StartRotating(180.0f);
        }
        else if (directionChange && leftRight == 2)
        {
            //facing right
            StartRotating(360.0f);
        }
        //This next part from the same source as the StartRotating function.
        if (rotationProgress < 1 && rotationProgress >= 0)
        {
            rotationProgress += Time.deltaTime * 20;
            rb.rotation = Quaternion.Lerp(startRotation, endRotation, rotationProgress);
            directionChange = false;
        }

        //Set flags for previously held direction buttons for smash attack control.
        if (upKeyHold) { lastFrameUp = true; } else { lastFrameUp = false; }
        if (downKeyHold) { lastFrameDown = true; } else { lastFrameDown = false; }
        if (leftKeyHold) { lastFrameLeft = true; } else { lastFrameLeft = false; }
        if (rightKeyHold) { lastFrameRight = true; } else { lastFrameRight = false; }
    }

    private void FixedUpdate()
    {
        //Jumping
        if(jump && (jumpCount < 2))
        {
            Vector3 jumpMovement = new Vector3(0.0f, jumpPower, 0.0f);
            rb.velocity = jumpMovement * jumpSpeed * Time.deltaTime;
            jumpCount++;
        }

        //God forbid I mess with the physics engine, but these values control the rise and fall of the jump.
        //Can get floaty and the opposite jump feeling, like Jigglypuff/Falco.
        if(rb.velocity.y > 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * jumpMultiplyer * Time.deltaTime;
        }
        else if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * fallMultiplyer * Time.deltaTime;
        }

        if (bUp) { Buppy(); }

        //Again messing with the physics engine, but on purpose for the arcade feel.
        rb.position += new Vector3(0.0f, 0.0f, (horizMove * speed * Time.deltaTime));
    }

    IEnumerator Jump()
    {
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator SmashCounter()
    {
        //Function to set smash attack flag for a limited time after smash movement input.
        smash = true;
        yield return new WaitForSeconds(0.5f);
        smash = false;
    }

    IEnumerator CoolDown(float wait)
    {
        //Function to enforce cooldown, to stop player from attacking too fast.
        yield return new WaitForSeconds(attackWindow);
        punchCollider.transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
        punchCollider.transform.localScale = new Vector3(1.0f, 2.0f, 1.5f);
        punchBoxCollider.enabled = false;
        attackLevel = 0;
        yield return new WaitForSeconds(wait);
        attacking = false;
        bUp = false;
    }

    private void NormalHit(int dir)
    {
        //For regular A button hits
        if(!attacking)
        {
            float cool = 0.0f; //Specific cooldown for attacks.
            punchBoxCollider.enabled = true;
            attacking = true;
            attackLevel = 1;
            //set attack level based on smash
            if (smash) { attackLevel = 2; Debug.Log("ass"); cool = 0.5f; }
            //Forward basic attack
            if (dir == 0)
            {
                punchCollider.localPosition = new Vector3(0.0f, 1.0f, 0.5f);
                cool = 0.05f;
            }
            //Up attack
            else if (dir == 1)
            {
                if (attackLevel == 2)
                {
                    punchCollider.localPosition = new Vector3(0.0f, 2.5f, 0.5f);
                    punchCollider.localScale = new Vector3(1.0f, 3.1f, 2.0f);
                }
                else
                {
                    punchCollider.localPosition = new Vector3(0.0f, 2.1f, 0.3f); cool = 0.3f;
                }
            }
            //Down attack
            else if (dir == 2)
            {
                if(attackLevel == 2)
                {
                    punchCollider.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
                    punchCollider.localScale = new Vector3(1.0f, 3.0f, 2.0f);
                }
                else
                {
                    punchCollider.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
                    cool = 0.3f;
                }
            }
            //Attacks left or right, could be separated
            else if (dir == 3 || dir == 4)
            {
                if (attackLevel == 2)
                {
                    punchCollider.localPosition = new Vector3(0.0f, 1.0f, 1.0f);
                    punchCollider.localScale = new Vector3(1.0f, 2.0f, 2.5f);
                }
                else
                {
                    punchCollider.localPosition = new Vector3(0.0f, 1.0f, 0.5f);
                    cool = 0.3f;
                }
            }

            StartCoroutine( CoolDown(cool));
        }
    }

    private void BHit(int dir)
    {
        //For B button hits
        if (!attacking)
        {
            float cool = 0.0f; //Specific cooldown for B attack types.
            punchBoxCollider.enabled = true;
            attacking = true;
            attackLevel = 3;
            //Forward basic attack, for no direction pressed, or left or right.
            if (dir == 0 || dir == 3 || dir == 4)
            {
                //whatever B does goes here
                cool = 0.05f;
            }
            //Up B attack
            else if (dir == 1)
            {
                //whatever up B does
                if (!usedBUp)
                {
                    bUp = true;
                    cool = 1.0f;
                }
            }
            //Down B attack
            else if (dir == 2)
            {
                //whatever down B does
                cool = 1.0f;
            }

            StartCoroutine(CoolDown(cool));
        }
    }

    public float GetHitPower()
    {
        return hitPower;
    }

    public int GetAttackLevel()
    {
        return attackLevel;
    }

    public void Hit(float hitDamage, Transform otherPlayer, int aLevel)
    {
        //Function called when player is hit by another player.

        //get vector between players for force vector. This needs to be played with to get a better reaction.
        Vector3 direction = trans.position - otherPlayer.position;
        direction.x = 0.0f;
        direction.y = Mathf.Abs(direction.y);
        direction.z *= 1.0f;
        
        damage += hitDamage * aLevel;
        //apply force
        rb.AddForce(direction * damage *20);
    }

    public void ResetDamage()
    {
        damage = 0.0f;
    }

    private void StartRotating(float yPosition)
    {
        //Taken from https://stackoverflow.com/questions/42658013/slowly-rotating-towards-angle-in-unity#42658515
        //Rotates object to a new position over a set period of time.

        // Here we cache the starting and target rotations
        startRotation = trans.rotation;
        endRotation = Quaternion.Euler(0.0f, yPosition, 0.0f);

        // This starts the rotation, but you can use a boolean flag if it's clearer for you
        rotationProgress = 0;
    }

    public void TouchedGround()
    {
        //Resets flag to allow player to jump after they touch a ground collider.
        jumpCount = 0;
        usedBUp = false;
    }

    private void Buppy()
    {
        //This function is where a player's custom up B attacks are coded.

        Vector3 bigJump = new Vector3(0.0f, 300.0f, 0.0f);
        rb.velocity = bigJump * Time.deltaTime;
        usedBUp = true;
    }
}
