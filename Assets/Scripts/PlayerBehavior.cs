using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehavior : MonoBehaviour
{
    // Controls
    private const KeyCode CROUCH_KEY = KeyCode.LeftShift;
    private const KeyCode JUMP_KEY = KeyCode.Space;
    private const KeyCode WALK_KEY = KeyCode.Q;
    private const KeyCode RUN_KEY = KeyCode.W;
    private const KeyCode INTERACT_KEY = KeyCode.E;
    private const KeyCode ROTATE_CW = KeyCode.A;
    private const KeyCode ROTATE_CCW = KeyCode.D;

    private const string TREASURE_TAG = "Treasure";
    private const string CHEST_TAG = "Chest";
    
    private float runAnimSpeed = 1.5f;
    private float runAccel = 0.5f;
    private float camRotateSpeed = 100f;
    private float walkSpeed = 3000f;
    private float maxRunSpeed = 2.5f;
    private float minRunSpeed = 1.8f;
    private float runJumpStrength = 1.5f;
    private float runJumpForwardStrength = 6f;
    private float jumpStrength = 10f;

    private Animator anim;
    private Rigidbody rb;
    private Vector3 movement;

    // UI
    private int health = 100;
    public ProgressBar healthBar;
    private bool isDead = false;
    private int deathPointLoss = 500;

    public TreasureUIBehavior treasureUI;
    private int treasure = 0;
    
    public Text scoreTitle;
    private int score = 0;
    private int treasurePointGain = 1000;

    public GameObject deathScreen;
    public Button respawnButton;
    public Vector3 respawnPoint;
    private bool isRespawning = false;

    public GameObject winScreen;

    // Movement temoraries
    private bool inTreasureRange = false;
    private int treasureCollisions = 0;
    private GameObject treasureInRange;

    private bool inChestRange = false;
    private int chestCollisions = 0;
    public GameObject chest;
    private int holdingTreasure = 0;

    private bool isRunJumpingUp = false;
    private bool isRunJumpingForward = false;
    private bool isJumping = false;

    private GameObject foot;
    private BoxCollider footCollider;
    
    public LayerMask groundLayer;

    // For Reed
    public bool isIdle = false;
    public bool isAttacking;
    private BoxCollider fist;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        foot = transform.GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetChild(0).gameObject;
        fist = transform.GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(7).GetChild(0).GetChild(0).GetChild(1).gameObject.GetComponent<BoxCollider>();
        footCollider = foot.GetComponent<BoxCollider>();
        respawnButton.onClick.AddListener(delegate { isRespawning = true; });
        if (healthBar) {
            healthBar.BarValue = health;
        } else {
            Debug.Log("PlayerBehavior.cs - Could not find health bar");
        }
    }

    // Update is called once per frame
    void Update()
    {
        movement = Vector3.zero;

        if (isDead) return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            isIdle = true;
        } else {
            isIdle = false;
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Cross Punch")) {
            isAttacking = true;
            fist.enabled = true;
        } else {
            isAttacking = false;
            fist.enabled = false;
        }

        if (Input.GetKeyDown(INTERACT_KEY)) {
            if (inChestRange && holdingTreasure > 0) {
                treasureUI.SwitchColor();
                anim.SetTrigger("PutDown");
                score += treasurePointGain * holdingTreasure;
                scoreTitle.text = "Score: " + score;
                holdingTreasure = 0;
                if (treasureUI.HasWon()) {
                    winScreen.SetActive(true);
                    isDead = true;
                }
            } else if (inTreasureRange) {
                anim.SetTrigger("Pickup");
                    Destroy(treasureInRange);
                    treasureUI.BarValue = ++treasure;
                    treasureUI.SwitchColor();
                    ++holdingTreasure;
                    inTreasureRange = false;
            } else {
                anim.SetTrigger("Punch");
            }
        } else if (Input.GetKey(CROUCH_KEY)) {
            anim.SetBool("IsCrouching", true);
        } else {
            anim.SetBool("IsCrouching", false);
            if (Input.GetKeyDown(JUMP_KEY)) {
                if (IsGrounded()) {
                    anim.SetTrigger("Jump");
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Running") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Running Jump") && !anim.IsInTransition(0)) {
                        isJumping = true;
                    }
                }
            } else if (Input.GetKey(RUN_KEY)) {
                anim.SetBool("IsRunning", true);
                runAnimSpeed += runAccel * Time.deltaTime;
                if (runAnimSpeed > maxRunSpeed) {
                    runAnimSpeed = maxRunSpeed;
                }
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Running")) {
                    anim.speed = runAnimSpeed;
                    if (anim.IsInTransition(0)) {
                        isRunJumpingUp = true;
                        isRunJumpingForward = true;
                    }
                } else {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Running Jump") && !anim.IsInTransition(0)) {
                        isRunJumpingForward = true;
                    }
                    anim.speed = 1;
                }
            } else {
                runAnimSpeed = minRunSpeed;
                anim.speed = 1;
                anim.SetBool("IsRunning", false);
                if (Input.GetKey(WALK_KEY)) {
                    anim.SetBool("IsWalking", true);
                    movement = transform.forward * walkSpeed;
                } else {
                    anim.SetBool("IsWalking", false);
                }
            }
        }
    }

    void FixedUpdate() {
        if (isRespawning) {
            isRespawning = false;
            isDead = false;
            deathScreen.SetActive(false);
            rb.MovePosition(respawnPoint);
            anim.SetTrigger("Respawn");
            health = 100;
            healthBar.BarValue = health;
        }

        if (isDead) return;
        
        float rot = 0f;
        if (Input.GetKey(ROTATE_CCW)) {
            rot += camRotateSpeed;
        }
        if (Input.GetKey(ROTATE_CW)) {
            rot -= camRotateSpeed;
        }
        Vector3 rotation = Vector3.up * rot;
        Quaternion angleRot = Quaternion.Euler(rotation * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * angleRot);

        if (isRunJumpingUp) {
            rb.AddForce(transform.up * runJumpStrength, ForceMode.Impulse);
            // Debug.Log("in");
            isRunJumpingUp = false;
        }
        if (isRunJumpingForward) {
            rb.AddForce(transform.forward * runAnimSpeed * runJumpForwardStrength, ForceMode.Impulse);
            // Debug.Log("in");
            isRunJumpingForward = false;
        }

        if (isJumping) {
            rb.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
            // Debug.Log("in");
            isJumping = false;
        }
    }

    public void Damage(int damage) {
        health -= damage;
        healthBar.BarValue = health;
        if (health <= 0) {
            Death();
        }
    }

    private void Death() {
        isDead = true;
        anim.SetTrigger("Death");
        score -= deathPointLoss;
        scoreTitle.text = "Score: " + score;
        deathScreen.SetActive(true);
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == TREASURE_TAG) {
            inTreasureRange = true;
            ++treasureCollisions;
            treasureInRange = other.gameObject;
        } else if (other.tag == CHEST_TAG) {
            inChestRange = true;
            ++chestCollisions;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == TREASURE_TAG) {
            if (--treasureCollisions == 0) {
                inTreasureRange = false;
            }
        } else if (other.tag == CHEST_TAG) {
            if (--chestCollisions == 0) {
                inChestRange = false;
            }
        }
    }
    
    private bool IsGrounded() {
        Vector3 colliderBottom = new Vector3(footCollider.bounds.center.x, footCollider.bounds.min.y, footCollider.bounds.center.z);
        bool grounded = Physics.CheckBox(footCollider.bounds.center, new Vector3(foot.transform.localScale.x, foot.transform.localScale.y, foot.transform.localScale.z), foot.transform.rotation, groundLayer, QueryTriggerInteraction.Ignore);
        return grounded;
    }

}
