using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class Enemy : MonoBehaviour
{
    public float maxhp = 30;
    public float speed = 5; // �ƶ��ٶ�
    public float idle_moveRange = 10; // ����״̬�µ��ƶ���Χ
    public float idle_restTime = 5; // ����״̬ÿ���ƶ������Ϣʱ��
    public float battleRange = 8; // ս��״̬�л���Χ
    public float exitBattleStateTime = 8; // �˳�ս��״̬����ʱ��
    private Rigidbody2D rgb;
    private Animator anim;
    private bool isHitted = false; // �Ƿ񱻹���
    private Vector3 forcePosition;
    private SpriteRenderer sprite;
    private float force; // �ܵ�������С
    private Vector2 initPos; // ��ʼλ��
    private float moveX = 0;
    private float moveY = 0;
    private bool moving = false; // �Ƿ����ƶ�
    private float hp;
    private float _idle_restTime = 0; 
    private int action_state = ActionStates.Idling; // �ж�״̬
    private Vector2 idle_movePos; // ����״̬�µ��ƶ�Ŀ���
    private float state_dizzinessTime = 0; // ѣ��ʣ��ʱ��
    public bool death;
    private float _exitBattleStateTime = 0; // �˳�ս��״̬��ʣ��ʱ��
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
        // ȷ������
        if (moveX >= 0.1f)
        {
            sprite.flipX = false;
        } else if (moveX <= -0.1f)
        {
            sprite.flipX = true;
        }

        // �ܵ�����Ĵ���
        if (isHitted) {
            rgb.velocity = forcePosition * force;
            force *= 0.95f;
            return;
        }

        if (death) { return; }

        // ѣ�εĴ���
        if (state_dizzinessTime > 0)
        {
            SetDizzinessTime(state_dizzinessTime - Time.deltaTime);
            return;
        }

        // �����ж�״̬�µ���ش���
        IdleActionHandler();
        battleActionHandler();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ����״̬��
        if (action_state == ActionStates.Idling)
        {
            // ��ײ����ʱ�������µ���λ��
            RandIdleMovePos();
        }
    }

    // ������ʱ
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

    // ��������ĳ��ֹͣʱ
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

    // ����hp
    void ReduceHP(float damage)
    {
        hp -= damage;

        // hp����Ĵ���
        if (hp <= 0)
        {
            death = true;
        }
    }

    // ��������״̬
    void ToIdleState()
    {
        action_state = ActionStates.Idling;
        _idle_restTime = idle_restTime;
        Global.removeFollowingEnemy(transform);
    }

    // ����ս��״̬
    void ToBattleState()
    {
        action_state = ActionStates.Battling;
        _exitBattleStateTime = exitBattleStateTime;
        Global.addFollowingEnemy(transform);
    }

    // ��������״̬�µ��ж�
    void IdleActionHandler()
    {
        if (action_state != ActionStates.Idling) { return; }

        // ���������ʱת��ս��״̬
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

    // ս��״̬�µ��ж�
    void battleActionHandler()
    {
        if (action_state != ActionStates.Battling) { return; }
        
        Vector2 pos = transform.position;
        Vector2 targetPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        float range = Vector2.Distance(pos, targetPos);
        // �������ʱֹͣ�ƶ�
        if (range <= 2)
        {
            if (moving) { SetMoving(false); }
            return;
        }
        // Զ�����ʱ��������״̬
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

    // ����״̬-�����һ���ƶ���
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