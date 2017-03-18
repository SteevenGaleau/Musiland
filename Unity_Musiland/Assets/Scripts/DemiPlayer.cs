﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class DemiPlayer : MonoBehaviour {


    // ================== //
    // ===== Joueur ===== //
    [SerializeField]
    GameObject playerprefab;
    GameObject player0;
    Rigidbody2D rigid;
    public SpriteRenderer sprite;
    Animator anim;

    // ============================ //
    // ===== Collider Checker ===== //
    public Transform groundCheck;
    public Transform leftCheck;
    public Transform rightCheck;
    public Transform topCheck;
    public Vector2 InitialHitbox = new Vector2(0.21f, 0.6f);
    public Vector2 SlideHitbox = new Vector2(0.21f, 0.2f);
    public CapsuleCollider2D characollider;
    RaycastHit2D Hit;

    // ================== //
    // ===== Camera ===== //
    [SerializeField]
    Camera maincamera;
    Vector3 decalCamOrigine;
    public Vector3 CurrentRespawnPoint;
    public Vector3 PlayerScreenPos;
    public Vector3 PlayerViewportPos;
    bool outscreenx = false;
    bool outscreeny = false;
    float espacex = 0.1f;
    float espacey = 0.1f;
    float verticaljumpview = 5;
    float goRight = 0;
    float goLeft = 0;
    float goDecal = 0.15f;
    float fallDecal = 0.0f;
    float goUp = 0;
    float goDown = 0;
    float zoomCamera = -10;
    bool isGliding = false;
    bool glideCamera = false;


    // ================= //
    // ===== AUDIO ===== //
    AudioManager audioManager;

    // ===================== //
    // ===== VARIABLES ===== //
    // ===================== //
    // === Some var === //
    float xpos = 0;
    float ypos = 0;
    float recoveryTime = 1.5f;
    float lastDamage;
    int hp;
    int hpmax = 5;

    // === Style Var === //
    public EnumList.StyleMusic playercurrentstyle = EnumList.StyleMusic.Hell;
    public bool IsHellActivable = true;
    public bool IsCalmActivable = true;
    public bool IsFestActivable = true;
    float changeTime = 1; // Cooldown pour le changement de style
    public bool istransiting = false;
    float transitime = 0;
    public bool canSwitch = true;
    float maxSpeedCalm = 5; // Vitesse max Calm
    float maxSpeedFest = 7; // Vitesse max Fest
    float maxSpeedHell = 10; // Vitesse max Hell

    float gravityScaleCalm = 1.2f;
    float gravityScaleFest = 1.5f;
    float gravityScaleHell = 1.5f;

    float moveForceHell = 20f;

    float jumpForceHell = 400;
    float jumpForceFest = 450;
    float jumpForceCalm = 500;
    float wallJumpForceFest = 500;
    float attenuationJumGap = 70;

    // === Keys === //
    private bool KeyShoot;
    private bool KeyDown;

    // === Controller Var === //
    bool ControlPause = false;
    bool bInAir;
    public bool bRun;
    bool IsHoldingDown = false;
    float lastchange = 0; // Date du dernier changement
    float initialgravity = 1; // Gravity Scale 
    float deathheight = -10; // Hauteur avant de mourir
    float mintimejump = 0.5f; // Durée entre deux sauts
    float timelastjump; // Date du dernier saut
    bool canMove = true;
    bool jumpGap = false;
    // -- Hide -- //
    bool hideUnderSnow = false;
    bool hiddedByJoystick = false;
    // -- WalLSlide -- //
    float WallSlideSpeed = -2;
    // -- H Dash -- //
    float doubletapCDDash = 0.5f; // Durée max entre deux tap pour un double tap
    float HorizontalTapCount = 0; // Nb de tap pour le double tap 
    float DashCD = 2; // CD entre deux dash
    float LastDashEnd, LastDashStart; // Moments clé du dernier dash
    float DureeDash = 0.2f; // Durée du dash
    bool airDash = false;
    // -- V Dash -- //
    float doubletapCDVDash = 0.5f; // Durée max entre deux tap pour un double tap
    float VerticalTapCount = 0; // Nb de tap pour le double tap
    float LastVDashStart, LastVDashEnd; // Moments clé du dernier Vdash
    bool IsVDashDone = false; // Si le joueur a déjà fait un VDash durant son saut1
    public bool bVDash = false; // Si le joueur est en train de Vdash 
    float DureeVDash = 1f; // Durée du Vdash
    // -- Slide -- //
    bool IsSliding; // Si le joueur est en train de slide
    float SlideDestination; // Distance d'arrivée théorique du slide
    float SlideTimeStart; // Moment où le joueur débute son slide
    float SlideLength = 3f; // Longueur du slide
    float SlideStartY;
    float SlideCD = 1.5f;
    float LastSlideEnd;

    // =============== //
    // ===== HUD ===== //
    public Text playerpdv;
    [SerializeField]
    public GameObject playerpdvgameobject;
    public Text victoiretext;
    [SerializeField]
    public GameObject victoiretextgameobject;
    [SerializeField]
    public GameObject backgroundsplash;
    [SerializeField]
    public GameObject HUDManagerGO;
    public HUDManager2 HUDManager;
    public bool isdisturbed;

    public bool IsFalling { get; private set; }

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        hp = 5;
        decalCamOrigine = maincamera.transform.position - transform.position;
        anim = GetComponent<Animator>();
        // anim.enabled = true;
        CurrentRespawnPoint = sprite.transform.position;
        if (HUDManagerGO) HUDManager = (HUDManager2)HUDManagerGO.GetComponent(typeof(HUDManager2));
        characollider = GetComponent<CapsuleCollider2D>();

        //On commence en mode calme
        playercurrentstyle = EnumList.StyleMusic.Calm;
        if (HUDManager) HUDManager.ChangeAllTiles();
        ApplyStyleCarac(playercurrentstyle);

        //rigid.transform.position = new Vector2 (168, -2); // Déplacement initial
        //rigid.transform.position = new Vector2(100, -7); // Déplacement dragon
        //rigid.transform.position = new Vector2(291, 30); // Déplacement end
        //rigid.transform.position = new Vector2(320, 10); // Déplacement foule
        //rigid.transform.position = new Vector2(450, 30); // Déplacement end
        // == AUDIO == //
        audioManager = AudioManager.instance;
        //if (audioManager == null) Debug.LogError(this + " n'a pas trouvé d'AudioManager");
        // == AUDIO == //

        //animation["Hell_Dash"].speed = 0.5f; // "f" is for C#
    }

    // Update is called once per frame
    void Update () {
        // ================================================ //
        // ===============Controle de bInAir=============== //

        if (!Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground")))
        {
            bInAir = true;
            anim.SetBool("A_IsInAir", true);
        }

        else {
            bInAir = false;
            airDash = false;
        }



        if (anim == null) anim = GetComponent<Animator>();
        PlayerScreenPos = maincamera.WorldToScreenPoint(this.transform.position);
        PlayerViewportPos = maincamera.WorldToViewportPoint(this.transform.position);

        if (!ControlPause)
        {
            // ===================================== //
            // =============== Slide =============== //
            if (IsSliding)
            {
                audioManager.PlaySoundIfNoPlaying("Fest_Glide");
                LastSlideEnd = Time.time;
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, SlideDestination, Time.deltaTime * 5), SlideStartY, 0);
                if (TestColliderTop()) // ----- Permet d'avoir une glissade plus fluide sous les objets ----- 
                {
                    if (sprite.flipX) SlideDestination -= 0.1f;
                    else SlideDestination += 0.1f;
                }
                // ----- Si on rencontre un mur ou que le joueur ne touche plus le sol, on stop la glissade ---- 
                if ((Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground")) ||
                Physics2D.Linecast(transform.position, rightCheck.position, 1 << LayerMask.NameToLayer("ground"))) ||
                !Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground"))
                )
                {
                    if (!TestColliderTop())
                    {
                        ChangeHitbox(false);
                        IsSliding = false;
                        anim.SetBool("A_IsSlide", false);
                        canSwitch = true;
                    }
                    else
                    {
                        //DoSlide();
                        anim.SetBool("A_IsSlide", true);
                    }

                }

                if (Time.time - SlideTimeStart >= 0.45f)
                {
                    if (!TestColliderTop())
                    {
                        ChangeHitbox(false);
                        IsSliding = false;
                        anim.SetBool("A_IsSlide", false);
                        canSwitch = true;
                    }
                    else
                    {
                        // DoSlide();
                        anim.SetBool("A_IsSlide", true);
                    }
                }

            }

            // ========================================= //
            // =============== DASHES ================== //
            if (bRun && Time.time > LastDashStart + DureeDash) { bRun = false; rigid.velocity = new Vector2(0, 0); anim.SetBool("A_IsDash", false); } // Si on est en dash horizontal dpeuis un certain temps : stop
            if (bVDash && Time.time > LastVDashStart + DureeVDash) { bVDash = false; } // Si on est en V Dash depuis un certain temps :stop
            if (bRun)
            {
                sprite.color = new Color32(0, 255, 0, 255);
            }
            else if (bVDash)
            {
                sprite.color = new Color32(0, 255, 0, 255);
            }
            else if (hideUnderSnow)
            {
                sprite.color = new Color32(0, 0, 255, 255);
            }
            else sprite.color = new Color32(255, 255, 255, 255); ;


            if (bRun && playercurrentstyle == EnumList.StyleMusic.Hell)
            {
                if (Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground")))
                {
                    Hit = Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground"));
                    Destructible TestdestructLeft = Hit.collider.gameObject.GetComponent("Destructible") as Destructible;
                    if (TestdestructLeft) // Si l'objet touché est destructible 
                    {
                        TestdestructLeft.Destruction(); // On le détruit
                    }
                    DestructAll TestDesctructAll = Hit.collider.gameObject.GetComponent<DestructAll>() as DestructAll;
                    if (TestDesctructAll)
                    {
                        TestDesctructAll.Launch();
                    }
                }

                if (Physics2D.Linecast(transform.position, rightCheck.position, 1 << LayerMask.NameToLayer("ground")))
                {
                    Hit = Physics2D.Linecast(transform.position, rightCheck.position, 1 << LayerMask.NameToLayer("ground"));
                    Destructible TestdestructRight = Hit.collider.gameObject.GetComponent("Destructible") as Destructible;
                    if (TestdestructRight) // Si l'objet touché est destructible 
                    {
                        TestdestructRight.Destruction(); // On le détruit
                    }
                    DestructAll TestDesctructAll = Hit.collider.gameObject.GetComponent<DestructAll>() as DestructAll;
                    if (TestDesctructAll)
                    {
                        TestDesctructAll.Launch();
                    }
                }
            }



            if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground"))) // Si les pieds du joueur touchent quelque chose
            {
                if (bVDash) // Si on est en VDash
                {

                    Hit = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground"));
                    Destructible Testdestruct = Hit.collider.gameObject.GetComponent("Destructible") as Destructible;

                    if (Testdestruct) // Si l'objet touché est destructible 
                    {
                        Testdestruct.Destruction(); // On le détruit
                        bVDash = false; //On détruit les rochers uns par uns
                    }
                    else {
                        bVDash = false;
                        audioManager.PlaySound("DiveOnGround");
                    }

                    DestructAll TestDesctructAll = Hit.collider.gameObject.GetComponent<DestructAll>() as DestructAll;
                    if (TestDesctructAll)
                    {

                        TestDesctructAll.Launch();
                    }

                }
            }


            if (bVDash)  // ====== V DASH ====== //
            {
                rigid.velocity = new Vector2(0, -moveForceHell);
            }

            if (bRun)
            { // ====== DASH HORIZONTAL ====== //
                rigid.gravityScale = 0;
                LastDashEnd = Time.time;
                if (sprite.flipX)
                    rigid.velocity = new Vector2(-moveForceHell, 0);
                else
                    rigid.velocity = new Vector2(moveForceHell, 0);

            }
            else {
                rigid.gravityScale = gravityScaleHell;
            }

            // =========================================== //
            // ================ FALL CHECK ================ //
            if (rigid.velocity.y < -6 && Physics2D.Linecast(transform.position, transform.position + new Vector3(0, -2, 0), 1 << LayerMask.NameToLayer("ground")))
            {
                rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y / 1.1f);
            }




            // =========================================== //
            // ================ WALL JUMP ================ //
            bool isWallSliding = false;

            if (playercurrentstyle == EnumList.StyleMusic.Fest)
            {
                // ----- Si il y a un mur à gauche/droite et rien en dessous et qu'on est pas en train de monter ---- 
                if ((Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground")) ||
                Physics2D.Linecast(transform.position, rightCheck.position, 1 << LayerMask.NameToLayer("ground"))) &&
                !Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground")) && rigid.velocity.y < 0
                )
                {
                    // Son slide frottement contre mur
                    isWallSliding = true;
                    anim.SetBool("A_IsWallSlide", true);
                    if (Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground"))) sprite.flipX = false;
                    if (Physics2D.Linecast(transform.position, rightCheck.position, 1 << LayerMask.NameToLayer("ground"))) sprite.flipX = true;
                    rigid.velocity = new Vector2(0, WallSlideSpeed);
                }
                else
                {
                    anim.SetBool("A_IsWallSlide", false);
                }
            }

            // == DEBUG == //
            if (Input.GetButtonDown("DebugKey"))
            {
                // audioManager.PlayRandomFromArray(1);
                // audioManager.GetSoundTime("Hell_BGM");
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                Application.LoadLevel("Level1");
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                PlayerRespawn();
                audioManager.PlaySoundIfNoPlaying("Pain_Sound");
            }
            // == DEBUG == //

            // ================================================================================ //
            // ============================== I. CONTROLS ===================================== //
            // ================================================================================ //

            // =========================================================================== //
            // ============================= I.1 MUSIC =================================== //
            // === NOTE : PAS DE PRINT ICI, LIMITER CE QU'ON MET POUR EVITER LE FREEZE === //
            // =========================================================================== //

            // ===== NEXT MUSIC ===== //
            /*if (Input.GetButtonDown("actionRight") && canMove && canSwitch)
            {
         
				StartCoroutine(Freeze(rigid.velocity));
                ChangeMusictoNext();
                HUDManager.ChangeAllTiles();
                ApplyStyleCarac(playercurrentstyle);
                UpdateBGM(playercurrentstyle);
            }*/

            // ===== PREVIOUS MUSIC ===== //
            /*if (Input.GetButtonDown("actionLeft") && canMove && canSwitch)
            {
				StartCoroutine(Freeze(rigid.velocity));
                ChangeMusictoPrevious();
                HUDManager.ChangeAllTiles();
                ApplyStyleCarac(playercurrentstyle);
                UpdateBGM(playercurrentstyle);

            }*/

            // ===== CALM MUSIC ===== //
            if (Input.GetButtonDown("calmMusic") && canMove && canSwitch && playercurrentstyle != EnumList.StyleMusic.Calm)
            {
                EnumList.StyleMusic oldone = playercurrentstyle;
                StartCoroutine(Freeze(rigid.velocity));
                playercurrentstyle = EnumList.StyleMusic.Calm;
                HUDManager.ScaleCircleTransition(35, PlayerScreenPos, oldone, playercurrentstyle);
                HUDManager.ChangeAllTiles();
                ApplyStyleCarac(playercurrentstyle);
                UpdateBGM(playercurrentstyle, oldone);
            }

            // ===== FEST MUSIC ===== //
            if (Input.GetButtonDown("festMusic") && canMove && canSwitch && playercurrentstyle != EnumList.StyleMusic.Fest)
            {
                EnumList.StyleMusic oldone = playercurrentstyle;
                StartCoroutine(Freeze(rigid.velocity));
                playercurrentstyle = EnumList.StyleMusic.Fest;
                HUDManager.ScaleCircleTransition(35, PlayerScreenPos, oldone, playercurrentstyle);
                HUDManager.ChangeAllTiles();
                ApplyStyleCarac(playercurrentstyle);
                UpdateBGM(playercurrentstyle, oldone);
            }

            // ===== HELL MUSIC ===== //
            if (Input.GetButtonDown("hellMusic") && canMove && canSwitch && playercurrentstyle != EnumList.StyleMusic.Hell)
            {
                EnumList.StyleMusic oldone = playercurrentstyle;
                StartCoroutine(Freeze(rigid.velocity));
                playercurrentstyle = EnumList.StyleMusic.Hell;
                HUDManager.ScaleCircleTransition(35, PlayerScreenPos, oldone, playercurrentstyle);
                HUDManager.ChangeAllTiles();
                ApplyStyleCarac(playercurrentstyle);
                UpdateBGM(playercurrentstyle, oldone);
            }


            // =========================================================================== //
            // ============================ I.2 MOUVEMENT ================================ //
            // =========================================================================== //
            // ===== Left/right ===== //
            if (Input.GetButtonUp("Horizontal"))
            {
                if (bRun)
                {
                    bRun = false;
                    LastDashEnd = Time.time;
                }

            }

            if (bInAir && rigid.velocity.y < 0)
            {
                IsFalling = true;
                anim.SetBool("A_IsFalling", true);
                anim.SetBool("A_IsJump", false);
            }

            // ==================================== //
            // =============== Jump  ============== //

            // ========== BUTTON DOWN ============ //
            if (Input.GetButtonDown("Jump") && canMove)
            {
                // ----- (F) SLIDE -----
                if (!bInAir && Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground")))
                {
                    //if (IsHoldingDown && !IsSliding && playercurrentstyle == EnumList.StyleMusic.Fest) { // SLIDE
                    //DoSlide ();

                    //} else if 
                    if (Time.time > timelastjump + mintimejump)
                    { // Jump !Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground"))
                        IsVDashDone = false;
                        jumpGap = true;
                        StartCoroutine(JumpGapTime());
                        switch (playercurrentstyle)
                        {
                            case EnumList.StyleMusic.Calm:
                                rigid.AddForce((new Vector3(0.0f, jumpForceCalm, 0)));
                                audioManager.PlaySoundIfNoPlaying("Calm_Jump");
                                anim.SetBool("A_IsJump", true);
                                anim.SetBool("A_IsInAir", true);
                                break;

                            case EnumList.StyleMusic.Fest:
                                rigid.AddForce((new Vector3(0.0f, jumpForceFest, 0)));
                                audioManager.PlaySoundIfNoPlaying("Fest_Jump");
                                anim.SetBool("A_IsJump", true);
                                anim.SetBool("A_IsInAir", true);
                                break;

                            case EnumList.StyleMusic.Hell:
                                rigid.AddForce((new Vector3(0.0f, jumpForceHell, 0)));
                                audioManager.PlaySoundIfNoPlaying("Hell_Jump");
                                anim.SetBool("A_IsJump", true);
                                anim.SetBool("A_IsInAir", true);
                                break;
                        }

                        timelastjump = Time.time;
                    }
                }
                else if (bInAir && playercurrentstyle == EnumList.StyleMusic.Calm && rigid.velocity.y < -4f)
                { // On ralenti le joueur dans sa chute s'il commence à planner
                    audioManager.PlaySoundIfNoPlaying("Calm_Glide");
                    rigid.velocity = new Vector2(rigid.velocity.x, 0);
                }

                // ----- (F) WALLJUMP -----
                if (isWallSliding)
                {
                    bool wallatleft = false;
                    if (Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground"))) wallatleft = true;
                    jumpGap = true;
                    StartCoroutine(JumpGapTime());
                    if (wallatleft)
                    {
                        rigid.AddForce((new Vector3(400, wallJumpForceFest, 0)));
                        audioManager.PlaySoundIfNoPlaying("Fest_WallJump");
                        // ANim

                    }
                    else
                    {
                        rigid.AddForce((new Vector3(-400, wallJumpForceFest, 0)));
                        audioManager.PlaySoundIfNoPlaying("Fest_WallJump");
                        // ANim
                    }
                } // ---------- 

                // ----- (H) V DASH -----
                if (bInAir && playercurrentstyle == EnumList.StyleMusic.Hell)
                {
                    audioManager.PlaySoundIfNoPlaying("Hell_Dash");
                    LastVDashStart = Time.time;
                    bVDash = true;
                }

            } // ========== FIN BUTTON DOWN ========== //

            // ========== BUTTON HOLDING FOR JUMPGAP ============ //
            if (Input.GetButton("Jump") && jumpGap)
            {
                switch (playercurrentstyle)
                {
                    case EnumList.StyleMusic.Calm:
                        rigid.AddForce((new Vector3(0.0f, jumpForceCalm / attenuationJumGap, 0)));
                        break;

                    case EnumList.StyleMusic.Fest:
                        rigid.AddForce((new Vector3(0.0f, jumpForceFest / attenuationJumGap, 0)));
                        break;

                    case EnumList.StyleMusic.Hell:
                        rigid.AddForce((new Vector3(0.0f, jumpForceHell / attenuationJumGap, 0)));
                        break;
                }

            }

            // ========== BUTTON UP FOR JUMPGAP ============ //
            // Si le joueur a relaché son bouton de saut alors il ne peut plus augmanter le force de celui çi en ré-appuyant
            if (Input.GetButtonUp("Jump"))
            {
                jumpGap = false;
            }

            // =========== BUTTON JUMP HOLDING =========== //
            if (Input.GetButton("Jump") && canMove)
            {

                // ----- (C) PLANER ----- 
                if (bInAir && playercurrentstyle == EnumList.StyleMusic.Calm && rigid.velocity.y < 0)
                { // Si on est en l'air
                    audioManager.PlaySoundIfNoPlaying("Calm_Glide");
                    anim.SetBool("A_IsPlan", true);
                    rigid.gravityScale = 0.10f;
                    rigid.AddForce((new Vector3(0.0f, 0.6f, 0)));
                    isGliding = true;
                }
            }
            else {

                if (bInAir && playercurrentstyle == EnumList.StyleMusic.Calm)
                { // Si on est en l'air
                    rigid.gravityScale = gravityScaleCalm;
                    isGliding = false;
                }
            }
            // ========== FIN HOLDING JUMP ============ //
            if (Input.GetButtonUp("Jump"))
            {
                audioManager.StopSoundIfPlaying("Calm_Glide");
                anim.SetBool("A_IsPlan", false);
                //anim.SetBool("A_IsJump", false);
            }

            // ==================================== //
            // =============== FIN JUMP  ========== //
            // ==================================== //

            // ========================== //
            // ========== Down ========== //
            if ((Input.GetAxis("Vertical") < -0.5f || Input.GetButton("Down")) && canMove && !bVDash)
            {
                IsHoldingDown = true; // HOlding down

                // ----- FAST FALL -----
                if (bInAir)
                {
                    anim.SetBool("A_IsFalling", true);
                    rigid.gravityScale = 3f; // FAST FALL
                }
            }

            // ========================== //
            // ========== Action ========== //

            //if ((Input.GetAxis("Vertical") < -0.5f || Input.GetButton("Down")) && canMove && !bVDash)
            if ((Input.GetButtonDown("actionRight") || Input.GetButtonDown("actionLeft")) && canMove && !bVDash)
            {

                // ----- (C) SE CACHER SOUS LA NEIGE ------
                if (playercurrentstyle == EnumList.StyleMusic.Calm && !bInAir)
                {
                    HideUnderSnow();
                    if (Input.GetAxis("Vertical") < -0.5f)// On check la différence entre le joystick et l'axe pour qu'il n'y ait pas de prob
                        hiddedByJoystick = true;
                }

                // ----- (F) SLIDE -----
                if (playercurrentstyle == EnumList.StyleMusic.Fest && !bInAir && !IsSliding)
                {
                    //if (Time.time > LastSlideEnd + SlideCD)
                    {
                        DoSlidebis();
                        anim.SetBool("A_IsSlide", true);
                    }
                }

                // ----- (H) DASH -----
                if (playercurrentstyle == EnumList.StyleMusic.Hell && !bRun && canMove)
                {
                    //if (Time.time > LastDashEnd + DashCD)
                    if (bInAir)
                    {
                        if (!airDash)
                        {
                            anim.SetBool("A_IsDash", true);
                            bRun = true;
                            LastDashStart = Time.time;
                            airDash = true;
                        }

                    }
                    else {
                        anim.SetBool("A_IsDash", true);
                        bRun = true;
                        LastDashStart = Time.time;
                    }
                }

            }

            // ============================= //
            // ========== UP JUMP ========== //
            if (Input.GetButtonUp("actionRight") || Input.GetButtonUp("actionLeft"))
            {
                IsHoldingDown = false;
                if (hideUnderSnow) UnhideUnderSnow();
            }

            //Joystick check no more usefull for buttons
            /* if (Input.GetAxis("Vertical") > -0.5f && hiddedByJoystick)
             {
                 hiddedByJoystick = false;
                 if (hideUnderSnow) UnhideUnderSnow();
             }*/
            // =============== //

        }


        // =========================================================================== //
        // =============================== HUD ======================================= //
        // =========================================================================== //
        if (backgroundsplash) backgroundsplash.transform.position = new Vector3(maincamera.transform.position.x, maincamera.transform.position.y, 0);


        // ================= //
        // ===== Death ===== //
        /*if (sprite.transform.position.y < deathheight) hp = 0; // Mort si en dessous certaine hauteur

        if (hp == 0)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }*/
        // ================= //
    } // ================================== FIN UPDATE ========================================================= //

    // ========================================================================================================= //
    // ===================================== FIXED UPDATE ====================================================== //
    // ========================================================================================================= //
    void FixedUpdate()
    {
        if (!ControlPause)
        {
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f && canMove) // Si le joueur se déplace latéralement : F() de déplacement différente selon theme en cours
            {

                switch (playercurrentstyle)
                {
                    // =============================================================================================================
                    case EnumList.StyleMusic.Hell:
                        if (!bRun && !bInAir) // S'il n'as pas double tap et qu'il n'est pas en l'air
                        {
                            rigid.velocity = new Vector2(Input.GetAxis("Horizontal") * maxSpeedHell, rigid.velocity.y); // Déplacement direct
                            anim.SetBool("A_IsWalking", true);
                            audioManager.PlaySoundIfNoPlaying("Hell_Step");
                        }
                        else if (!bRun && bInAir && Mathf.Abs(rigid.velocity.x) < maxSpeedHell)
                        {
                            rigid.AddForce((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedHell * 100 * Time.deltaTime));
                        }

                        break;

                    // =============================================================================================================
                    case EnumList.StyleMusic.Fest:
                        if (!bInAir) // S'il n'as pas double tap et qu'il n'est pas en l'air
                        {
                            rigid.velocity = new Vector2(Input.GetAxis("Horizontal") * maxSpeedFest, rigid.velocity.y); // Déplacement direct
                            audioManager.PlaySoundIfNoPlaying("Fest_Step");
                            anim.SetBool("A_IsWalking", true);
                        }
                        else if (bInAir && Mathf.Abs(rigid.velocity.x) < maxSpeedFest)
                        {
                            rigid.AddForce((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedFest * 100 * Time.deltaTime));
                        }
                        break;

                    // =============================================================================================================
                    case EnumList.StyleMusic.Calm:
                        if (!bInAir && Mathf.Abs(rigid.velocity.x) < maxSpeedCalm) // S'il n'as pas double tap et qu'il n'est pas en l'air
                        {
                            audioManager.PlaySoundIfNoPlaying("Calm_Step");
                            anim.SetBool("A_IsWalking", true);
                            rigid.AddForce((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedCalm * 100 * Time.deltaTime)); // Déplacement avec force, lent et flottant
                        }
                        else if (bInAir && rigid.velocity.x < maxSpeedCalm && Input.GetAxis("Horizontal") > 0)
                        {
                            rigid.AddForce((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedCalm * 100 * Time.deltaTime));
                        }
                        else if (bInAir && rigid.velocity.x > -maxSpeedCalm && Input.GetAxis("Horizontal") < 0)
                        {
                            rigid.AddForce((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedCalm * 100 * Time.deltaTime));
                        }
                        break;
                }
                //Accélère en peut le joueur en mode calme afin qu'il soit moins lent à démarrer
                if (Mathf.Abs(rigid.velocity.x) < 0.1f && playercurrentstyle == EnumList.StyleMusic.Calm && !bInAir)
                {
                    rigid.velocity = new Vector3(Mathf.Sign(Input.GetAxis("Horizontal")) * 2, rigid.velocity.y, 0);
                }
            }



            if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.5f && !bInAir && !bRun)
            {
                anim.SetBool("A_IsWalking", false);
                if (playercurrentstyle != EnumList.StyleMusic.Calm)
                {
                    rigid.velocity = new Vector2(0, rigid.velocity.y);  //On frene le personange si on navance plus sauf pour le mode calme
                }
            }

            // sprite.transform.position += new Vector3(Input.GetAxis("Horizontal") * slowFactor * vitesse * Time.deltaTime,0,0); // Position
            if (Input.GetAxis("Horizontal") > 0) sprite.flipX = false;
            if (Input.GetAxis("Horizontal") < 0) sprite.flipX = true;
        }
        float h = Input.GetAxis("Horizontal");

        // ================================================================================== //
        // =============================== MainCamera ======================================= //
        //print(PlayerScreenPos);

        if (!IsFalling)
        {
            if ((PlayerViewportPos.y > 0.4 + espacey) || (PlayerViewportPos.y < 0.5 - espacey)) outscreeny = true;
            else outscreeny = false;
        }
        else
        {
            if ((PlayerViewportPos.y > 0.8) || (PlayerViewportPos.y < 0.2)) outscreeny = true;
            else outscreeny = false;
        }


        if (PlayerScreenPos.x > 0.5 + espacex || PlayerScreenPos.x < 0.5 - espacex) outscreenx = true;
        else outscreenx = false;
        float translatx = 0;
        float translaty = 0;
        if (outscreenx)
        {

            translatx = Mathf.Lerp(maincamera.transform.position.x, transform.position.x, Time.deltaTime);
        }
        if (outscreeny)
        {
            translaty = Mathf.Lerp(maincamera.transform.position.y, transform.position.y, Time.deltaTime);

        }

        //------------------- Replacement de la camera en fonction de la direction de joueur --------------------//

        if (!sprite.flipX)
        {
            goRight++;
            goLeft = 0;
        }

        if (sprite.flipX)
        {
            goLeft++;
            goRight = 0;
        }

        if (goRight > 100)
        {
            goDecal = 0.15f;
        }
        if (goLeft > 100)
        {
            goDecal = -0.15f;
        }

        //------------------- Replacement de la caméra si on tombe ou non --------------------//

        if (!IsFalling)
        {
            //fallDecal = 0;
            glideCamera = false;
        }

        if (IsFalling)
        {
            //fallDecal = Mathf.Round (rigid.velocity.y) / 20;
        }

        if (isGliding)
        {
            StartCoroutine(GlideCheck());
        }


        if (rigid.velocity.y < -5 || glideCamera)
        {
            fallDecal = -0.15f;
        }
        else
            fallDecal = 0.05f;

        if (goUp > 50)
        {
            //fallDecal = 0.0f;
        }

        if (outscreenx)
        {
            if (outscreeny) maincamera.transform.position = new Vector3(translatx + goDecal, translaty + fallDecal, zoomCamera);
            else maincamera.transform.position = new Vector3(translatx + goDecal, maincamera.transform.position.y + fallDecal, zoomCamera);
        }
        else if (outscreeny) maincamera.transform.position = new Vector3(maincamera.transform.position.x + goDecal, translaty + fallDecal, zoomCamera);


        /*

        if (rigid.velocity.y < -0.1f)

                maincamera.transform.position = Vector3.Lerp(maincamera.transform.position, transform.position + decalCamOrigine + new Vector3(0, -5, 0), Time.deltaTime);
            else
                maincamera.transform.position = Vector3.Lerp(maincamera.transform.position, transform.position + decalCamOrigine, Time.deltaTime);
*/

        // =============================== MainCamera ======================================= //
        // ================================================================================== //

    }


    // ========================================================================================================= //
    // ================================== Collisions & TRIGGER ================================================= //
    // ========================================================================================================= //
    // ======================================== //
    // ============= Collision ================ //
    // ======================================== //
    void OnCollisionEnter2D(Collision2D collision)
    {
        bVDash = false;
        if ((collision.gameObject.tag == "Sol") && Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground")))
        {
            anim.SetBool("A_IsInAir", false);
            anim.SetBool("A_IsFalling", false);
            IsFalling = false;
            rigid.gravityScale = initialgravity; // Disable Fast Fall
        }
    }


    // =========================================
    void OnCollisionStay2D(Collision2D collision) // Empêche de ne plus pouvoir jump si atterrissage alors que bouton Jump maintenu
    {
        //=== Cela provoque un bug avec le jump (collision stay better than condition)
        //if ((collision.gameObject.tag == "Sol")) { anim.SetBool("isjump", false); }
        if (collision.gameObject.tag == "Damage")// && (Time.time > lastDamage + recoveryTime))
        {
            /*hp--;
            rigid.AddForce(new Vector3(0, 200, 0));
            lastDamage = Time.time;*/
            PlayerRespawn();
            audioManager.PlaySoundIfNoPlaying("Pain_Sound");

        }

    }

    // =========================================
    void OnCollisionExit2D(Collision2D collision)
    {
    }

    // ======================================== //
    // ============== Trigger= ================ //
    // ======================================== //
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "SoundMaker")
        {
            //MusicDisturber(true, 1);
            isdisturbed = true;
        }

        /*if (collision.gameObject.tag == "Damage")// && (Time.time > lastDamage + recoveryTime))
		{
			PlayerRespawn ();
			audioManager.PlaySoundIfNoPlaying("Pain_Sound");
		}*/

    }

    // =======================================
    void OnTriggerStay2D(Collider2D collision)
    {
        /*print(collision.gameObject.name);
		if(collision.gameObject.name != "Background"){
			ChangeHitbox(false);
			IsSliding = false;
			anim.SetBool("A_IsSlide", false);
			canSwitch = true;
		}*/
    }

    // =======================================
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "SoundMaker")
        {
            //MusicDisturber(false, 1);
            isdisturbed = false;
        }
    }


    // ========================================================================================================= //
    // ======================================== Fonctions ====================================================== //
    // ========================================================================================================= //

    // ===================================== //
    // ===== Changement de musique (+) ===== //
    public void ChangeMusictoNext()
    {
        if ((Time.time > lastchange + changeTime) && (transitime >= 0))
        {
            lastchange = Time.time;
            if (playercurrentstyle == EnumList.StyleMusic.Calm) playercurrentstyle = EnumList.StyleMusic.Hell;
            else { playercurrentstyle++; }
            bool test = false;
            while (test == false)
            {
                if (playercurrentstyle == EnumList.StyleMusic.Calm && IsCalmActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Fest && IsFestActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Hell && IsHellActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Calm) playercurrentstyle = EnumList.StyleMusic.Hell;
                else { playercurrentstyle++; }
            }

            //HUDManager.ScaleCircleTransition(35, PlayerScreenPos, oldone, playercurrentstyle);
        }
    }

    // ===================================== //
    // ===== Changement de musique (-) ===== //
    public void ChangeMusictoPrevious()
    {
        if ((Time.time > lastchange + changeTime) && (!istransiting))
        {

            lastchange = Time.time;
            if (playercurrentstyle == EnumList.StyleMusic.Hell) playercurrentstyle = EnumList.StyleMusic.Calm;
            else { playercurrentstyle--; }
            bool test = false;
            while (test == false)
            {
                if (playercurrentstyle == EnumList.StyleMusic.Calm && IsCalmActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Fest && IsFestActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Hell && IsHellActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Calm) playercurrentstyle = EnumList.StyleMusic.Hell;
                else { playercurrentstyle++; }
            }

            //HUDManager.ScaleCircleTransition(35, PlayerScreenPos);
        }

    }

    // ============================================= //
    // ===== Mise à jour de la musique de fond ===== //
    public void UpdateBGM(EnumList.StyleMusic newstyle, EnumList.StyleMusic oldstyle)
    {
        float avancement = 5;


        // Recup avancement
        switch (oldstyle)
        {

            case EnumList.StyleMusic.Hell:

                avancement = audioManager.GetSoundTime("Hell_BGM");
                if (newstyle == EnumList.StyleMusic.Calm) avancement = avancement * (2.5f); // OK
                else if (newstyle == EnumList.StyleMusic.Fest) avancement = avancement * (1.25f); // OK

                break;
            case EnumList.StyleMusic.Fest:

                avancement = audioManager.GetSoundTime("Fest_BGM");
                if (newstyle == EnumList.StyleMusic.Calm) avancement = avancement * 2; // OK
                else if (newstyle == EnumList.StyleMusic.Hell) avancement = avancement * (0.8f);
                break;
            case EnumList.StyleMusic.Calm:

                avancement = audioManager.GetSoundTime("Calm_BGM");
                if (newstyle == EnumList.StyleMusic.Fest) avancement = avancement * (0.5f);
                else if (newstyle == EnumList.StyleMusic.Hell) avancement = avancement * (0.4f);
                break;
        }

        //

        switch (newstyle)
        {
            case EnumList.StyleMusic.Hell:
                audioManager.PlaySound("Transition_Hell");

                if (!audioManager.IsSoundPlaying("Hell_BGM"))
                {
                    audioManager.StopSound("Fest_Foule");
                    audioManager.StopSound("Calm_Foule");
                    audioManager.StopSound("Hell_Foule");
                    audioManager.StopSound("Calm_BGM");
                    audioManager.StopSound("Fest_BGM");
                    // if (isdisturbed) audioManager.PlaySound("Hell_Foule");
                    audioManager.PlaySoundAtTime("Hell_BGM", avancement);
                }

                break;
            case EnumList.StyleMusic.Fest:
                audioManager.PlaySound("Transition_Fest");
                if (!audioManager.IsSoundPlaying("Fest_BGM"))
                {
                    audioManager.StopSound("Fest_Foule");
                    audioManager.StopSound("Calm_Foule");
                    audioManager.StopSound("Hell_Foule");
                    audioManager.StopSound("Calm_BGM");
                    audioManager.StopSound("Hell_BGM");
                    // if (isdisturbed) audioManager.PlaySound("Fest_Foule");
                    audioManager.PlaySoundAtTime("Fest_BGM", avancement);
                }
                break;
            case EnumList.StyleMusic.Calm:
                audioManager.PlaySound("Transition_Calm");
                if (!audioManager.IsSoundPlaying("Calm_BGM"))
                {
                    audioManager.StopSound("Fest_Foule");
                    audioManager.StopSound("Calm_Foule");
                    audioManager.StopSound("Hell_Foule");
                    audioManager.StopSound("Fest_BGM");
                    audioManager.StopSound("Hell_BGM");
                    // if (isdisturbed) audioManager.PlaySound("Calm_Foule");
                    audioManager.PlaySoundAtTime("Calm_BGM", avancement);
                }
                break;
        }
    }


    // ================================== //
    // ===== Se cache sous la neige ===== //
    public void HideUnderSnow()
    {
        rigid.velocity = new Vector2(0, 0);
        hideUnderSnow = true;
        canMove = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
        audioManager.PlaySoundIfNoPlaying("Calm_Hide");
        anim.SetBool("A_IsHide", true);
    }

    // ================================== //
    // ===== Sort de sous la neige ===== //
    public void UnhideUnderSnow()
    {
        hideUnderSnow = false;
        canMove = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        audioManager.PlaySoundIfNoPlaying("Calm_Unhide");
        anim.SetBool("A_IsHide", false);
    }

    // =========================================================================== //
    // ===== Fonction qui applique différents effets au perso selon le style ===== //
    void ApplyStyleCarac(EnumList.StyleMusic newstyle)
    {
        switch (newstyle)
        {
            case EnumList.StyleMusic.Hell:
                initialgravity = gravityScaleHell; // -- Gravité (Hover/Not)      
                rigid.gravityScale = initialgravity;
                break;
            case EnumList.StyleMusic.Fest:
                initialgravity = gravityScaleFest; // -- Gravité (Hover/Not)
                rigid.gravityScale = initialgravity;
                break;
            case EnumList.StyleMusic.Calm:
                initialgravity = gravityScaleCalm; // -- Gravité (Hover/Not)
                rigid.gravityScale = initialgravity;
                break;

        }
    }

    // ==================================================== //
    // ===== Fonction qui permet au joueur de respawn ===== //
    public void PlayerRespawn()
    {
        // ==== Ajouter mise en scène : son, anim... ===== //
        sprite.flipX = false;
        sprite.enabled = false;
        ControlPause = true;
        rigid.velocity = new Vector2(0, 0); // Annule toutes les forces en jeu
        canMove = false;
        bRun = false;
        bVDash = false;
        hideUnderSnow = false;
        IsSliding = false;
        anim.enabled = false;
        transform.position = CurrentRespawnPoint;
        StartCoroutine(WaitingRespawn(2));

    }

    // =========================================================================== //
    // ===== Fonction qui pemret de téléporter le joueur à un endroit précis ===== //
    public void PlayerTeleport(Vector3 destination, bool keepforce)
    {
        if (keepforce) rigid.velocity = new Vector2(0, 0);
        transform.position = destination;
    }

    public void DoSlide()
    {
        canSwitch = false;
        IsSliding = true;
        if (sprite.flipX) SlideDestination = transform.position.x - SlideLength;
        else SlideDestination = transform.position.x + SlideLength;
        SlideTimeStart = Time.time;
        SlideStartY = transform.position.y;
        ChangeHitbox(true);
    }

    public void DoSlidebis()
    {

        if (sprite.flipX) SlideDestination = transform.position.x - SlideLength;
        else SlideDestination = transform.position.x + SlideLength;
        if (!Physics2D.Linecast(transform.position, new Vector2(SlideDestination, transform.position.y), 1 << LayerMask.NameToLayer("ground")))
        {
            while (Physics2D.Linecast(new Vector2(SlideDestination, transform.position.y), new Vector2(SlideDestination, transform.position.y + 1.07f), 1 << LayerMask.NameToLayer("ground")))
            {
                if (sprite.flipX) SlideDestination -= 0.5f;
                else SlideDestination += 0.5f;
            }
            canSwitch = false;
            IsSliding = true;
            SlideTimeStart = Time.time;
            SlideStartY = transform.position.y;
            ChangeHitbox(true);
        }
        else
        {
            canSwitch = false;
            IsSliding = true;
            SlideTimeStart = Time.time;
            SlideStartY = transform.position.y;
            ChangeHitbox(true);
        }

    }

    // ==== Changement Hitbox ===== //
    public void ChangeHitbox(bool onoff)
    {
        if (onoff)
        {
            characollider.size = SlideHitbox;
            characollider.offset = new Vector2(0, -1.75f);

        }
        else
        {
            characollider.size = InitialHitbox;
            characollider.offset = new Vector2(0, 0);
        }


    }

    // ==== Freeze du joueur lors des transitions musicales ==== //
    IEnumerator Freeze(Vector2 velocity)
    {
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
        canMove = false;
        yield return new WaitForSeconds(0.3f);
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigid.velocity = velocity;
        if (bInAir)
            yield return new WaitForSeconds(0.2f);
        canMove = true;

    }

    //public bool 

    public bool TestColliderTop()
    {
        if (Physics2D.Linecast(transform.position, topCheck.position, 1 << LayerMask.NameToLayer("ground")))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator WaitingRespawn(float time)
    {
        yield return new WaitForSeconds(time);
        ControlPause = false;
        canMove = true;
        sprite.enabled = true;
        anim.enabled = true;

        audioManager.PlaySoundIfNoPlaying("Respawn");
        //maincamera.transform.position = CurrentRespawnPoint + decalCamOrigine;
    }

    IEnumerator JumpGapTime()
    {
        yield return new WaitForSeconds(0.7f);
        jumpGap = false;
    }

    IEnumerator GlideCheck()
    {
        yield return new WaitForSeconds(1.5f);

        if (isGliding)
        {
            glideCamera = true;
        }
    }

    public void MusicDisturber(bool startorend, int id)
    {
        switch (id)
        {
            case 1: // ===== FOULE ======= //
                switch (playercurrentstyle)
                {
                    case EnumList.StyleMusic.Hell:
                        if (startorend)
                        {
                            float timestamp = audioManager.GetSoundTime("Hell_BGM");
                            audioManager.StopSound("Hell_BGM");
                            audioManager.PlaySoundAtTime("Hell_Foule", timestamp);
                        }
                        else
                        {
                            float timestamp = audioManager.GetSoundTime("Hell_Foule");
                            audioManager.StopSound("Hell_Foule");
                            audioManager.PlaySoundAtTime("Hell_BGM", timestamp);
                        }
                        break;
                    case EnumList.StyleMusic.Fest:
                        if (startorend)
                        {
                            float timestamp = audioManager.GetSoundTime("Fest_BGM");
                            audioManager.StopSound("Fest_BGM");
                            audioManager.PlaySoundAtTime("Fest_Foule", timestamp);
                        }
                        else
                        {
                            float timestamp = audioManager.GetSoundTime("Fest_Foule");
                            audioManager.StopSound("Fest_Foule");
                            audioManager.PlaySoundAtTime("Fest_BGM", timestamp);
                        }

                        break;
                    case EnumList.StyleMusic.Calm:
                        if (startorend)
                        {
                            float timestamp = audioManager.GetSoundTime("Calm_BGM");
                            audioManager.StopSound("Calm_BGM");
                            audioManager.PlaySoundAtTime("Calm_Foule", timestamp);
                        }
                        else
                        {
                            float timestamp = audioManager.GetSoundTime("Calm_Foule");
                            audioManager.StopSound("Calm_Foule");
                            audioManager.PlaySoundAtTime("Calm_BGM", timestamp);
                        }

                        break;
                }
                break;

            case 2: // ===== PAUSE ====== //
                break;

        }

        if (startorend)
        {
            switch (id)
            {
                // Foule
                case 1:
                    switch (playercurrentstyle)
                    {
                        case EnumList.StyleMusic.Hell:

                            break;
                        case EnumList.StyleMusic.Fest:

                            break;
                        case EnumList.StyleMusic.Calm:

                            break;
                    }
                    break;
            }

        }
    }

}
