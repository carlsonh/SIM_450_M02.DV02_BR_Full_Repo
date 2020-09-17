using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{

    [Header("Movement Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rb;
    public PlayerWeapon weapon;

    [Header("Combat Stats")]
    private int curAttackerId;
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    private bool flashingDamage;
    public MeshRenderer mr;


    public int id;
    public Player photonPlayer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(!photonView.IsMine || dead)
        {//Make sure update is from owning player
            return;
        }

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            weapon.TryShoot();
        }
    }

    void Move()
    {
        //get input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //calc a dir rela to facing dir
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rb.velocity.y;

        rb.velocity = dir;
    }

    void TryJump()
    {
        //Downwards ray
        Ray ray = new Ray(transform.position, Vector3.down);

        //raycast to check if can jump
        if (Physics.Raycast(ray, 1.5f))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }


    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;

        // is this not our local player?
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rb.isKinematic = true;
        }
    }



    #region Player Damage and Stats
    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if(dead)
        {
            return;
        }

        //Apply damage
        curHp -= damage;
        curAttackerId = attackerId;

        //Show Damage
        photonView.RPC("DamageFlash", RpcTarget.Others);

        //Update health UI

        //Die if dead
        if(curHp <= 0)
        {
            photonView.RPC("Die", RpcTarget.All);
        }

    }

    [PunRPC]
    void DamageFlash()
    {
        if(flashingDamage)
        {//Already showing
            return;
        }

        StartCoroutine(DamageFlashCoroutine());

        IEnumerator DamageFlashCoroutine()
        {
            flashingDamage = true;

            Color defauultColor = mr.material.color;
            mr.material.color = defaultColor.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]
    void Die()
    {
        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;

        //Host check WinCon
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }

        //Local Player?
        if(photonView.IsMine)
        {
            if(curAttacker != 0)
            {
                GameManager.instance.GetPlayer(curAttacker).photonView.RPC("AddKill", RpcTarget.All);
            }

            //Player has died, make spectator
            GetComponentInChildren<CameraController>().SetAsSpectator();

            //Disable physics, hide player
            rb.isKinematic = true;
            transform.position = new Vector3(0, -500, 0);//Coupling >:C ///The camera is removed from the player object in the CameraController
        }
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;
    }

    #endregion Player Damage


}
