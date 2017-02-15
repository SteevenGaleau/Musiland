﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class Player : MonoBehaviour {
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

    float maxSpeedCalm = 5; // Vitesse max Calm
    float maxSpeedFest = 6; // Vitesse max Fest
	float maxSpeedHell = 7; // Vitesse max Hell

    float gravityScaleCalm = 1.2f;
    float gravityScaleFest = 1.5f;
    float gravityScaleHell = 1.5f;

    float moveForceHell = 20f;

	float jumpForceHell = 600;
	float jumpForceFest = 650;
	float jumpForceCalm = 700;
	float wallJumpForceFest = 550;


    // === Keys === //
    private bool KeyShoot;
    private bool KeyDown;

    // === Controller Var === //
    bool bInAir;
    public bool bRun;
    bool IsHoldingDown = false;
    float lastchange = 0; // Date du dernier changement
    float initialgravity = 1; // Gravity Scale 
    float deathheight = -10; // Hauteur avant de mourir
    float mintimejump = 0.5f; // Durée entre deux sauts
    float timelastjump; // Date du dernier saut
	bool canMove = true;
    // -- Hide -- //
	bool hideUnderSnow = false;
    // -- WalLSlide -- //
    float WallSlideSpeed = -2;
    // -- H Dash -- //
    float doubletapCDDash = 0.5f; // Durée max entre deux tap pour un double tap
    float HorizontalTapCount = 0; // Nb de tap pour le double tap 
    float DashCD = 2; // CD entre deux dash
    float LastDashEnd, LastDashStart; // Moments clé du dernier dash
    float DureeDash = 0.12f; // Durée du dash
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
    public HUDManager HUDManager;

    // ========================================================================================================= //
    // ============================================= START ===================================================== //
    // ========================================================================================================= //
    void Start () {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        hp = 5;
        decalCamOrigine = maincamera.transform.position - transform.position;
        anim = GetComponent<Animator>();
       // anim.enabled = true;
        CurrentRespawnPoint = sprite.transform.position;
        HUDManager = (HUDManager)HUDManagerGO.GetComponent(typeof(HUDManager));
        characollider = GetComponent<CapsuleCollider2D>();

		//On commence en mode calme
		playercurrentstyle = EnumList.StyleMusic.Calm;
		HUDManager.ChangeAllTiles();
		ApplyStyleCarac(playercurrentstyle);
    }

    // ========================================================================================================= //
    // =========================================== UPDATE ====================================================== //
    // ========================================================================================================= //
    void Update () {

        PlayerScreenPos = maincamera.WorldToScreenPoint(this.transform.position);
        
        // ===================================== //
        // =============== Slide =============== //
        if (IsSliding)
        {
			LastSlideEnd = Time.time;
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, SlideDestination, Time.time-SlideTimeStart), SlideStartY, 0);
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
                } else
                {
                    DoSlide();
                }

            }
           
            if (Time.time - SlideTimeStart >= 0.45f)
            {
                if (!TestColliderTop())
                {
                    ChangeHitbox(false);
                    IsSliding = false;
                } else
                {
                    DoSlide();
                }
            }

        }

        // ========================================= //
        // =============== DASHES ================== //
        if (doubletapCDDash > 0) { doubletapCDDash -= Time.deltaTime; }
        if (doubletapCDVDash > 0) { doubletapCDVDash -= Time.deltaTime; }
        if (bRun && Time.time > LastDashStart + DureeDash) { bRun = false;  rigid.velocity = new Vector2(0, 0); } // Si on est en dash horizontal dpeuis un certain temps : stop
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
        else sprite.color = new Color32(255,255,255, 255); ;


        if (bRun && playercurrentstyle == EnumList.StyleMusic.Hell)
        {
            if (Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground")))
            {
                Hit = Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground"));
                Destructible Testdestruct = Hit.collider.gameObject.GetComponent("Destructible") as Destructible;
                if (Testdestruct) // Si l'objet touché est destructible 
                {
                    Testdestruct.Destruction(); // On le détruit
                }
            }

            if (Physics2D.Linecast(transform.position, rightCheck.position, 1 << LayerMask.NameToLayer("ground")))
            {
                Hit = Physics2D.Linecast(transform.position, rightCheck.position, 1 << LayerMask.NameToLayer("ground"));
                Destructible Testdestruct = Hit.collider.gameObject.GetComponent("Destructible") as Destructible;
                if (Testdestruct) // Si l'objet touché est destructible 
                {
                    Testdestruct.Destruction(); // On le détruit
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
                } else { bVDash = false; }

            }
        }

       
        if (bVDash)  // ====== V DASH ====== //
        {
            rigid.velocity = new Vector2(0, -moveForceHell);
        }

		if (bRun) { // ====== DASH HORIZONTAL ====== //
			rigid.gravityScale = 0;
			LastDashEnd = Time.time;
			if (sprite.flipX)
				rigid.velocity = new Vector2 (-moveForceHell, 0);
			else
				rigid.velocity = new Vector2 (moveForceHell, 0);

		} else {
			rigid.gravityScale = gravityScaleHell;
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
                isWallSliding = true;
                rigid.velocity = new Vector2 (0, WallSlideSpeed);
            }
        }
        
        // == DEBUG == //
        if (Input.GetButton("DebugKey"))
        {
            IsHellActivable = true;
        }
        // == DEBUG == //
        
        // ================================================================================ //
        // ============================== I. CONTROLS ===================================== //
        // ================================================================================ //

        // =========================================================================== //
        // ============================= I.1 MUSIC =================================== //
        // === NOTE : PAS DE PRINT ICI, LIMITER CE QU'ON MET POUR EVITER LE FREEZE === //
        // =========================================================================== //

        if (Input.GetButtonDown("ChangeMusicPlus") && canMove) {
			StartCoroutine (Freeze());
            ChangeMusictoNext();
            HUDManager.ChangeAllTiles();
            ApplyStyleCarac(playercurrentstyle);
        }

        // ===== PREVIOUS MUSIC ===== //
        if (Input.GetButtonDown("ChangeMusicMinus") && canMove)
        {
			StartCoroutine (Freeze());
            ChangeMusictoPrevious();
            HUDManager.ChangeAllTiles();
            ApplyStyleCarac(playercurrentstyle);
 
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
            anim.SetBool("isrunning", false);
        }

        // ==================================== //
        // =============== Jump  ============== //

        // ========== BUTTON DOWN ============ //
		if (Input.GetButtonDown("Jump") && canMove)
        {
            // ----- (F) SLIDE -----
			if (!bInAir && Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("ground"))) {
				if (IsHoldingDown && !IsSliding && playercurrentstyle == EnumList.StyleMusic.Fest) { // SLIDE
					DoSlide ();
                      
				} else if (Time.time > timelastjump + mintimejump) { // Jump
					//bInAir = true; === Problème with colliders
					IsVDashDone = false;
					StartCoroutine (setJump ());
					switch (playercurrentstyle) {
					case EnumList.StyleMusic.Calm:
						rigid.AddForce ((new Vector3 (0.0f, jumpForceCalm, 0)));
						break;

					case EnumList.StyleMusic.Fest:
						rigid.AddForce ((new Vector3 (0.0f, jumpForceFest, 0)));
						break;

					case EnumList.StyleMusic.Hell:
						rigid.AddForce ((new Vector3 (0.0f, jumpForceHell, 0)));
						break;
					}
                   
					timelastjump = Time.time;
				}
			} else if (bInAir && playercurrentstyle == EnumList.StyleMusic.Calm) { // On ralenti le joueur dans sa chute s'il commence à planner
				rigid.velocity = new Vector2(rigid.velocity.x, 0);
			}

            // ----- (F) WALLJUMP -----
            if (isWallSliding)
            {
                bool wallatleft = false;
                if (Physics2D.Linecast(transform.position, leftCheck.position, 1 << LayerMask.NameToLayer("ground"))) wallatleft = true;
                if (wallatleft)
                {
					rigid.AddForce((new Vector3(400, wallJumpForceFest, 0)));
                    // Jouer son
                    // ANim
                }
                else
                {
					rigid.AddForce((new Vector3(-400, wallJumpForceFest, 0)));
                    // Jouer son
                    // ANim
                }
            } // ---------- 

            // ----- (H) V DASH -----
			if (bInAir && playercurrentstyle == EnumList.StyleMusic.Hell) {
               
                LastVDashStart = Time.time;
                bVDash = true;
			}

        } // ========== FIN BUTTON DOWN ========== //

        // =========== BUTTON JUMP HOLDING =========== //
		if (Input.GetButton ("Jump") && canMove) {

            // ----- (C) PLANER ----- 
            if (bInAir && playercurrentstyle == EnumList.StyleMusic.Calm && rigid.velocity.y < 0) { // Si on est en l'air
				rigid.gravityScale = 0.1f;
				rigid.AddForce ((new Vector3(0.0f,0.6f,0)));
			}
		} else {
            
			if (bInAir && playercurrentstyle == EnumList.StyleMusic.Calm) { // Si on est en l'air
				rigid.gravityScale = gravityScaleCalm;
			} 
		}
        // ========== FIN HOLDING JUMP ============ //

        // ==================================== //
        // =============== FIN JUMP  ========== //
        // ==================================== //

        // ========================== //
        // ========== Down ========== //
		if ((Input.GetAxis("Vertical") < -0.5f || Input.GetButton("Down")) && canMove)
        {

            IsHoldingDown = true; // HOlding down

            // ----- FAST FALL -----
            if (bInAir) rigid.gravityScale = 3f; // FAST FALL

            // ----- (C) SE CACHER SOUS LA NEIGE ------
            if (playercurrentstyle == EnumList.StyleMusic.Calm && !bInAir)   HideUnderSnow();

            // ----- (F) SLIDE -----
            if (playercurrentstyle == EnumList.StyleMusic.Fest && !bInAir && !IsSliding)
            {
                if (Time.time > LastSlideEnd + SlideCD)
                {
                    DoSlide();
                }
            }

            // ----- (H) DASH SOL ----- 
            if (playercurrentstyle == EnumList.StyleMusic.Hell && !bInAir)
            {
                if (Time.time > LastDashEnd + DashCD)
                {
                    bRun = true;
                    LastDashStart = Time.time;
                }
            }

        }

        // ============================= //
        // ========== UP JUMP ========== //
        if (Input.GetAxis("Vertical") < -0.5f || Input.GetButtonUp("Down")) {
            IsHoldingDown = false;
			if (hideUnderSnow) UnhideUnderSnow();
		}
        // =============== //


        // =========================================================================== //
        // =============================== HUD ======================================= //
        // =========================================================================== //
		/*if(rigid.velocity.y < -0.1f)
			maincamera.transform.position = Vector3.Lerp(maincamera.transform.position, transform.position + decalCamOrigine + new Vector3( 0, -3, 0), Time.deltaTime*3);
		else*/
			//maincamera.transform.position = Vector3.Lerp(maincamera.transform.position, transform.position + decalCamOrigine, Time.deltaTime);

		backgroundsplash.transform.position = new Vector3(maincamera.transform.position.x, maincamera.transform.position.y, 0);
        //particlesplash.transform.position = new Vector3(maincamera.transform.position.x, maincamera.transform.position.y, 0);
        //playerpdv = playerpdvgameobject.GetComponent<Text>();
        //playerpdv.text = "HP: " + hp + "/"+hpmax;

        // ================= //
        // ===== Death ===== //
        if (sprite.transform.position.y < deathheight) hp = 0; // Mort si en dessous certaine hauteur

        if (hp == 0)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Application.Quit();
        }
        // ================= //
    } // ================================== FIN UPDATE ========================================================= //

    // ========================================================================================================= //
    // ===================================== FIXED UPDATE ====================================================== //
    // ========================================================================================================= //
    void FixedUpdate()
    {
		float h = Input.GetAxis ("Horizontal"); 

		// =============================== MainCamera ======================================= //
		if(rigid.velocity.y < -0.1f)
			maincamera.transform.position = Vector3.Lerp(maincamera.transform.position, transform.position + decalCamOrigine + new Vector3( 0, -5, 0), Time.deltaTime);
		else
			maincamera.transform.position = Vector3.Lerp(maincamera.transform.position, transform.position + decalCamOrigine, Time.deltaTime);

		if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f && canMove) // Si le joueur se déplace latéralement : F() de déplacement différente selon theme en cours
        {

            switch (playercurrentstyle)
            {
                // =============================================================================================================
                case EnumList.StyleMusic.Hell:
				if (!bRun && !bInAir) // S'il n'as pas double tap et qu'il n'est pas en l'air
                {
					rigid.velocity = new Vector2(Input.GetAxis("Horizontal") * maxSpeedHell, rigid.velocity.y); // Déplacement direct
                } 
				else if(!bRun && bInAir && Mathf.Abs(rigid.velocity.x) < maxSpeedHell){
					rigid.AddForce ((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedHell * 100 * Time.deltaTime));
				}

                    break;

                // =============================================================================================================
                case EnumList.StyleMusic.Fest:
					if (!bInAir) // S'il n'as pas double tap et qu'il n'est pas en l'air
					{
						rigid.velocity = new Vector2(Input.GetAxis("Horizontal") * maxSpeedFest, rigid.velocity.y); // Déplacement direct
					} 
					else if(bInAir && Mathf.Abs(rigid.velocity.x) < maxSpeedFest){
						rigid.AddForce ((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedFest * 100 * Time.deltaTime));
					}
                    break;

                // =============================================================================================================
                case EnumList.StyleMusic.Calm:
					if (!bInAir && Mathf.Abs(rigid.velocity.x) < maxSpeedCalm) // S'il n'as pas double tap et qu'il n'est pas en l'air
					{
						rigid.AddForce((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedCalm * 100 * Time.deltaTime)); // Déplacement avec force, lent et flottant
					}
					else if(bInAir && Mathf.Abs(rigid.velocity.x) < maxSpeedCalm){
						rigid.AddForce ((new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f) * maxSpeedCalm * 100 * Time.deltaTime));
					}
					break;
            }
        }

		//On frene le personange si on navance plus sauf pour le mode calme
		if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.5f && !bInAir && ! bRun && playercurrentstyle != EnumList.StyleMusic.Calm) {
			rigid.velocity = new Vector2 (0, rigid.velocity.y);
		}

       // sprite.transform.position += new Vector3(Input.GetAxis("Horizontal") * slowFactor * vitesse * Time.deltaTime,0,0); // Position
        if (Input.GetAxis("Horizontal") > 0) sprite.flipX = false;
        if (Input.GetAxis("Horizontal") < 0) sprite.flipX = true;
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
		if ((collision.gameObject.tag == "Sol") && Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("ground"))) {
			bInAir = false;
			anim.SetBool("isjump", false);
			rigid.gravityScale = initialgravity; // Disable Fast Fall

		}
	}

    
    // =========================================
    void OnCollisionStay2D(Collision2D collision) // Empêche de ne plus pouvoir jump si atterrissage alors que bouton Jump maintenu
    {
		//=== Cela provoque un bug avec le jump (collision stay better than condition)
        //if ((collision.gameObject.tag == "Sol")) { bInAir = false; anim.SetBool("isjump", false); }
		if (collision.gameObject.tag == "Damage")// && (Time.time > lastDamage + recoveryTime))
        {
            /*hp--;
            bInAir = true;
            rigid.AddForce(new Vector3(0, 200, 0));
            lastDamage = Time.time;*/
			PlayerRespawn ();
        }
        
    }

    // =========================================
    void OnCollisionExit2D (Collision2D collision)
    {
    }

    // ======================================== //
    // ============== Trigger= ================ //
    // ======================================== //
    void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    // =======================================
    void OnTriggerStay2D(Collider2D collision)
    {
        
    }

    // =======================================
    void OnTriggerExit2D(Collider2D collision)
    {
    }


    // ========================================================================================================= //
    // ======================================== Fonctions ====================================================== //
    // ========================================================================================================= //

    // ===================================== //
    // ===== Changement de musique (+) ===== //
    public void ChangeMusictoNext()
    {
        if ( (Time.time > lastchange + changeTime) && (transitime >= 0))
        {
            lastchange = Time.time;
            if (playercurrentstyle == EnumList.StyleMusic.Calm)playercurrentstyle = EnumList.StyleMusic.Hell;
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
            
            HUDManager.ScaleCircleTransition(35, PlayerScreenPos);
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
            else {playercurrentstyle--; }
            bool test = false;
            while (test == false)
            {
                if (playercurrentstyle == EnumList.StyleMusic.Calm && IsCalmActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Fest && IsFestActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Hell && IsHellActivable) test = true;
                else if (playercurrentstyle == EnumList.StyleMusic.Calm) playercurrentstyle = EnumList.StyleMusic.Hell;
                else { playercurrentstyle++; }
            }

            HUDManager.ScaleCircleTransition(35, PlayerScreenPos);
        }

    }
    
	// ================================== //
	// ===== Se cache sous la neige ===== //
	public void HideUnderSnow()
	{
		rigid.velocity = new Vector2(0,0);
		hideUnderSnow = true;
		canMove = false;
		GetComponent<Collider2D> ().enabled = false;
		rigid.constraints = RigidbodyConstraints2D.FreezeAll;
	}

	// ================================== //
	// ===== Sort de sous la neige ===== //
	public void UnhideUnderSnow()
	{
		hideUnderSnow = false;				
		canMove = true;
		GetComponent<Collider2D> ().enabled = true;
		rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	// =========================================================================== //
	// ===== Fonction qui applique différents effets au perso selon le style ===== //
	void ApplyStyleCarac (EnumList.StyleMusic newstyle)  
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
        rigid.velocity = new Vector2(0, 0); // Annule toutes les forces en jeu
        transform.position = CurrentRespawnPoint;
		maincamera.transform.position = CurrentRespawnPoint + decalCamOrigine;
    }

    // =========================================================================== //
    // ===== Fonction qui pemret de téléporter le joueur à un endroit précis ===== //
    public void PlayerTeleport(Vector3 destination, bool keepforce)
    {
        if (keepforce) rigid.velocity = new Vector2(0, 0);
        transform.position = destination;
    }

    // =============== //
    // ===== ??? ===== //
	IEnumerator setJump(){
		yield return new WaitForSeconds (0.1f);
		bInAir = true;
	}

    // ================ //
    // ===== DASH ===== //
	IEnumerator Dash()
    {
		if (Time.time > LastDashEnd + DashCD)
		{
			bRun = true;
			LastDashStart = Time.time;
		}
		yield return new WaitForSeconds (0.5f);
		//bRun = false;
    }

    public void DoSlide()
    {
        IsSliding = true;
        if (sprite.flipX) SlideDestination = transform.position.x - SlideLength;
        else SlideDestination = transform.position.x + SlideLength;
        SlideTimeStart = Time.time;
        SlideStartY = transform.position.y;
        ChangeHitbox(true);
    }

    // ==== Changement Hitbox ===== //
    public void ChangeHitbox(bool onoff)
    {
        if (onoff)
        {
            characollider.size = SlideHitbox;
            characollider.offset = new Vector2(0, 0.19f);
                
        } else
        {
            characollider.size = InitialHitbox;
            characollider.offset = new Vector2(0, 0);
        }


    }

	// ==== Freeze du joueur lors des transitions musicales ==== //
	IEnumerator Freeze(){
		rigid.constraints = RigidbodyConstraints2D.FreezeAll;
		canMove = false;
		yield return new WaitForSeconds (0.5f);
		rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
		canMove = true;
	}

    public bool TestColliderTop()
    {
        if (Physics2D.Linecast(transform.position,topCheck.position, 1 << LayerMask.NameToLayer("ground")))
        {
            return true;
        } else
        {
            return false;
        }
       

    }

}