﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager2 : MonoBehaviour {
    [SerializeField]
    public GameObject cercletransition;
    [SerializeField]
    public GameObject particlesplash;
    Image particlesplashimage;
    [SerializeField]
    public DemiPlayer Theplayer;
    [SerializeField]
    public Text score;

    [SerializeField]
    public GameObject[] halotab;
    [SerializeField]
    public GameObject[] innerhalotab;
    [SerializeField]
    public GameObject[] outerhalotab;

    // ===== CERCLE ===== //
    public float circletransitingtimer;
    public float circletransitingscale;
    public float transitime;
    float departtransi;
    float farleft, farright, starttransipoint, farest;

    public int notesQuantity = 0;

    // == HALO == //
    public float minimum = 0.0f;
    public float maximum = 1f;
    public float duration = 0.5f;
    private float startTime;

    // ===== Ornement ===== //
    MusicSwitcher OrnementScript;
    float OrnOpacity;
    // Use this for initialization
    void Start()
    {
        particlesplashimage = particlesplash.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        score.text = notesQuantity.ToString();

        //print("transitime="+transitime);
        if (circletransitingtimer > 0)
        {

            circletransitingtimer -= Time.deltaTime;
            float initscale = cercletransition.transform.localScale.x;
            float Currentscale = Mathf.Lerp(cercletransition.transform.localScale.x, circletransitingscale, 1 - (circletransitingtimer / 9f));
            cercletransition.transform.localScale = new Vector3(Currentscale, Currentscale, 1);
            float t = (Time.time - startTime) / duration;
            foreach (GameObject halo in halotab)
            {
                halo.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, Mathf.SmoothStep(maximum, minimum, t));
            }
        }

        if (transitime > 0)
        {
            float opacity = Mathf.Lerp(1, 0, 1 - (transitime - 8));

            particlesplashimage.color = new Color(255, 255, 255, opacity);

            TransitionChangement();
        }
        else
        {
            //Theplayer.istransiting = false;
            particlesplash.SetActive(false);

        }
    }

    // ===== Scale the circle ===== //
    public void ScaleCircleTransition(float scalefinale, Vector3 PlayerPos, EnumList.StyleMusic oldone, EnumList.StyleMusic newone)
    {
        startTime = Time.time;
        cercletransition.transform.position = PlayerPos;
        cercletransition.transform.localScale = new Vector3(0, 0, 0);
        circletransitingscale = scalefinale;
        circletransitingtimer = 9f;

        foreach (GameObject halo in halotab)
        {
            halo.SetActive(true);
        }

        foreach (GameObject innerhalo in innerhalotab)
        {
            switch (newone)
            {
                case EnumList.StyleMusic.Hell:
                    innerhalo.GetComponent<SpriteRenderer>().sprite = innerhalo.GetComponent<MusicSwitcher>().HellTile;
                    break;

                case EnumList.StyleMusic.Fest:
                    innerhalo.GetComponent<SpriteRenderer>().sprite = innerhalo.GetComponent<MusicSwitcher>().FestTile;
                    break;

                case EnumList.StyleMusic.Calm:
                    innerhalo.GetComponent<SpriteRenderer>().sprite = innerhalo.GetComponent<MusicSwitcher>().CalmTile;
                    break;
            }
        }

        foreach (GameObject outerhalo in outerhalotab)
        {
            switch (oldone)
            {
                case EnumList.StyleMusic.Hell:
                    outerhalo.GetComponent<SpriteRenderer>().sprite = outerhalo.GetComponent<MusicSwitcher>().HellTile;
                    break;

                case EnumList.StyleMusic.Fest:
                    outerhalo.GetComponent<SpriteRenderer>().sprite = outerhalo.GetComponent<MusicSwitcher>().FestTile;
                    break;

                case EnumList.StyleMusic.Calm:
                    outerhalo.GetComponent<SpriteRenderer>().sprite = outerhalo.GetComponent<MusicSwitcher>().CalmTile;
                    break;
            }
        }

    }


    // ===== Fonction qui peret d'initialiser la transition en analysant la map ===== //
    public void ChangeAllTiles()
    {
        starttransipoint = Theplayer.sprite.transform.position.x;
        farleft = 10; farright = -10; transitime = 0.5f;


        // ===== CHANGEMENT DES TILES + BCKG ===== //
        MusicSwitcher[] tabmagik = (MusicSwitcher[])FindObjectsOfType(typeof(MusicSwitcher)); // Recup' tout les items avec le script de changement
        foreach (MusicSwitcher themetile in tabmagik) // Parcours
        {
            themetile.isdone = false;
            if (themetile.gameObject.transform.position.x <= farleft) farleft = themetile.gameObject.transform.position.x; // Objet le plus à gauche 
            if (themetile.gameObject.transform.position.x >= farright) farright = themetile.gameObject.transform.position.x; // Objet le plus à droite
        }

        if (Mathf.Abs(starttransipoint - farleft) > Mathf.Abs(starttransipoint - farright)) farest = Mathf.Abs(starttransipoint - farleft);
        else farest = Mathf.Abs(starttransipoint - farright);

        transitime = 9.5f;
    }

    // ======================================================================================================== //
    // ===== Fonction qui permet d'avoir une transformation des tiles en forme de cercle depuis le joueur ===== //
    public void TransitionChangement()
    {
        particlesplash.SetActive(true);
        OrnementScript = (MusicSwitcher)particlesplash.GetComponent(typeof(MusicSwitcher));

        transitime -= Time.deltaTime;
        //print(farest);
        float cap = Mathf.Lerp(0, 200, 1 - (transitime / 10));
        //print("cap" + cap + " // Transitime = "+transitime +" === "+ (1 - (transitime / 2) ));
        MusicSwitcher[] tabmagik = (MusicSwitcher[])FindObjectsOfType(typeof(MusicSwitcher)); // Recup' tout les items avec le script de changement
        //int tak = 0;
        foreach (MusicSwitcher themetile in tabmagik) // Parcours
        {
            if (!themetile.skip)
            {
                MusicSwitcher script = (MusicSwitcher)themetile.GetComponent(typeof(MusicSwitcher)); // Recup' leur script
                script.isdone = true;
                script.ChangeTheme(Theplayer.playercurrentstyle);
                OrnementScript.ChangeImageSrc(Theplayer.playercurrentstyle);
            }

            /*
            if (
                (themetile.gameObject.transform.position.x > (Theplayer.sprite.transform.position.x - cap)) &&
                (themetile.gameObject.transform.position.x < (Theplayer.sprite.transform.position.x + cap)) &&
                (themetile.gameObject.transform.position.y > (Theplayer.sprite.transform.position.y - cap)) &&
                (themetile.gameObject.transform.position.y < (Theplayer.sprite.transform.position.y + cap))
                )
            {
                if (!script.isdone)
                {
                    script.isdone = true;
                    script.ChangeTheme(Theplayer.playercurrentstyle);
                    OrnementScript.ChangeImageSrc(Theplayer.playercurrentstyle);
                }

            } */
        }

    }

}
