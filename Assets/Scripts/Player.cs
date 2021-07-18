using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10;
    public Transform fireball;
    public float fireball_cd = 1;
    private float _fireball_cd = 0;
    private Rigidbody2D rgb;
    private Animator anim;
    private SpriteRenderer sprite;
    private float moveX = 0;
    private float moveY = 0;
    private bool moving = false;
    private bool attacking = false;
    // Start is called before the first frame update
    void Start()
    {
        rgb = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        MathSkillCD();
        CheckAttacking();
        Attack();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
    }

    // 移动处理
    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        moveX = x == 0 ? moveX : x;
        moveY = y;

        // 旋转
        if (moveX >= 0.1f)
        {
            sprite.flipX = false;
        }
        else if (moveX < -0.1f)
        {
            sprite.flipX = true;
        }

        if (attacking) {
            rgb.velocity = new Vector3(0, 0, 0);
            moveY = 0;
            anim.SetFloat("moveX", moveX);
            anim.SetFloat("moveY", moveY);
            return;
        }
        Vector3 v = new Vector2(x * speed, y * speed);
        rgb.velocity = v;

        moving = !(x == 0 && y == 0);
        anim.SetBool("moving", moving);
        if (moving)
        {
            Vector3 position = v.normalized;
            anim.SetFloat("moveX", v.x == 0 ? (moveX > 0 ? 0.5f : -0.5f) : position.x);
            anim.SetFloat("moveY", position.y);
        }
    }

    // 计算技能CD
    private void MathSkillCD()
    {
        if (_fireball_cd > 0) { _fireball_cd -= Time.deltaTime; }
    }

    // 攻击处理
    // 按住J键触发攻击
    private void Attack()
    {
        if (_fireball_cd > 0) { return; }
        if (!Input.GetKey(KeyCode.J)) { anim.SetTrigger("stopAction");return; }
        if (attacking) { return; }
        anim.SetTrigger("attack");
    }

    // 判断是否在攻击
    private void CheckAttacking()
    {
        AnimatorStateInfo playingAnimation = anim.GetCurrentAnimatorStateInfo(0);
        attacking = playingAnimation.IsName("Base Layer.attack");
    }

    // 攻击动画完成，触发攻击
    private void AttackOver()
    {
        float facingX = anim.GetFloat("moveX");
        Vector2 position = transform.position;
        
        position.x += facingX >= 0 ? 1 : -1;
        float rotateAngle = facingX >= 0 ? -90: 90;
        GameObject.Instantiate(fireball, position, Quaternion.Euler(0, 0, rotateAngle));
        if (_fireball_cd <= 0) { _fireball_cd = fireball_cd; }
    }
}
