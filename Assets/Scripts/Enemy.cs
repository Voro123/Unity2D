using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class Enemy : MonoBehaviour
{
    public float maxhp = 30;
    public float speed = 5; // 移动速度
    public float idle_moveRange = 10; // 休闲状态下的移动范围
    public float idle_restTime = 5; // 休闲状态每次移动后的休息时长
    public float battleRange = 8; // 战斗状态切换范围
    public float exitBattleStateTime = 8; // 退出战斗状态所需时间
    private Rigidbody2D rgb;
    private Animator anim;
    private bool isHitted = false; // 是否被攻击
    private Vector3 forcePosition;
    private SpriteRenderer sprite;
    private float force; // 受到的力大小
    private Vector2 initPos; // 初始位置
    private float moveX = 0;
    private float moveY = 0;
    private bool moving = false; // 是否在移动
    private float hp;
    private float _idle_restTime = 0; 
    private int action_state = ActionStates.Idling; // 行动状态
    private Vector2 idle_movePos; // 休闲状态下的移动目标点
    private float state_dizzinessTime = 0; // 眩晕剩余时长
    public bool death;
    private float _exitBattleStateTime = 0; // 退出战斗状态的剩余时间
    // Start is called before the first frame update
    void Start()
    {
        rgb = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        hp = maxhp;
        initPos = transform.position;
        RandIdleMovePos();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 确定朝向
        if (moveX >= 0.1f)
        {
            sprite.flipX = false;
        } else if (moveX <= -0.1f)
        {
            sprite.flipX = true;
        }

        // 受到冲击的处理
        if (isHitted) {
            rgb.velocity = forcePosition * force;
            force *= 0.95f;
            return;
        }

        if (death) { return; }

        // 眩晕的处理
        if (state_dizzinessTime > 0)
        {
            SetDizzinessTime(state_dizzinessTime - Time.deltaTime);
            return;
        }

        // 各种行动状态下的相关处理
        IdleActionHandler();
        battleActionHandler();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 休闲状态下
        if (action_state == ActionStates.Idling)
        {
            // 碰撞物体时立即重新调整位置
            RandIdleMovePos();
        }
    }

    // 被攻击时
    public void Hitted(Vector3 _forcePosition, float _force, float damage)
    {
        isHitted = true;
        forcePosition = _forcePosition;
        force = _force / rgb.mass;
        anim.SetBool("hitted", true);
        Invoke("StopHitted", 0.5f);
        if (action_state != ActionStates.Battling)
        {
            ToBattleState();
        }
        if (Mathf.Abs(forcePosition.x) >= Mathf.Abs(forcePosition.y))
        {
            SetMoveX(forcePosition.x >= 0 ? 1 : -1);
            SetMoveY(0);
        } else
        {
            SetMoveX(0);
            SetMoveY(forcePosition.y >= 0 ? 1 : -1);
        }
        ReduceHP(damage);
    }

    // 被攻击后的冲击停止时
    void StopHitted()
    {
        isHitted = false;
        anim.SetBool("hitted", false);
        if (death)
        {
            rgb.velocity = new Vector2(0, 0);
            anim.SetBool("death", true);
        } else
        {
            SetDizzinessTime(0.5f / rgb.mass);
            _exitBattleStateTime = exitBattleStateTime;
        }
    }

    // 减少hp
    void ReduceHP(float damage)
    {
        hp -= damage;

        // hp归零的处理
        if (hp <= 0)
        {
            death = true;
        }
    }

    // 进入休闲状态
    void ToIdleState()
    {
        action_state = ActionStates.Idling;
        _idle_restTime = idle_restTime;
        Global.removeFollowingEnemy(transform);
    }

    // 进入战斗状态
    void ToBattleState()
    {
        action_state = ActionStates.Battling;
        _exitBattleStateTime = exitBattleStateTime;
        Global.addFollowingEnemy(transform);
    }

    // 处理休闲状态下的行动
    void IdleActionHandler()
    {
        if (action_state != ActionStates.Idling) { return; }

        // 附近有玩家时转换战斗状态
        Vector2 pos = transform.position;
        Vector2 targetPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        if (Vector2.Distance(pos, targetPos) <= battleRange)
        {
            ToBattleState();
            return;
        }

        if (_idle_restTime > 0)
        {
            if (moving) { SetMoving(false); }
            _idle_restTime -= Time.deltaTime;
            return;
        }
        
        FromMoveTo(pos, idle_movePos, 0.5f);

        if (Vector2.Distance(pos, idle_movePos) <= 0.2f)
        {
            RandIdleMovePos();
        }
    }

    // 战斗状态下的行动
    void battleActionHandler()
    {
        if (action_state != ActionStates.Battling) { return; }
        
        Vector2 pos = transform.position;
        Vector2 targetPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        float range = Vector2.Distance(pos, targetPos);
        // 靠近玩家时停止移动
        if (range <= 2)
        {
            if (moving) { SetMoving(false); }
            return;
        }
        // 远离玩家时进入休闲状态
        if (range > battleRange * 1.5f)
        {
            _exitBattleStateTime -= Time.deltaTime;
        } else
        {
            _exitBattleStateTime = exitBattleStateTime;
        }
        if (_exitBattleStateTime <= 0)
        {
            ToIdleState();
        }

        if (!moving) { SetMoving(true); }
        FromMoveTo(pos, targetPos);
    }

    void FromMoveTo(Vector2 a, Vector2 b, float mul = 1)
    {
        if (!moving) { SetMoving(true); }
        Vector2 movePos = (b - a).normalized;
        SetMoveX(movePos.x);
        SetMoveY(movePos.y);
        rgb.velocity = movePos * speed * mul;
    }

    // 休闲状态-随机下一个移动点
    void RandIdleMovePos()
    {
        if (_idle_restTime > 0) { return; }
        float targetX = initPos.x + Random.Range(-idle_moveRange, idle_moveRange);
        float targetY = initPos.y + Random.Range(-idle_moveRange, idle_moveRange);
        idle_movePos = new Vector2(targetX, targetY);
        _idle_restTime = idle_restTime * Random.Range(0.6f, 1);
    }

    void SetMoving(bool val)
    {
        moving = val;
        anim.SetBool("moving", val);
    }

    void SetMoveX(float val)
    {
        moveX = val;
        anim.SetFloat("moveX", val);
    }

    void SetMoveY(float val)
    {
        moveY = val;
        anim.SetFloat("moveY", val);
    }

    void SetDizzinessTime(float val)
    {
        state_dizzinessTime = val;
        anim.SetFloat("dizziness", val);
    }

    void Destory()
    {
        Global.removeFollowingEnemy(transform);
        GameObject.Destroy(gameObject);
    }
}

public class ActionStates
{
    public static int Idling = 0;
    public static int Battling = 1;
}