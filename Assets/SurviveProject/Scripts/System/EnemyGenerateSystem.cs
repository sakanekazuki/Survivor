using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

public class EnemyGenerateSystem : MonoBehaviour
{

    //�Ƃ肠�����������Ă݂�
    public List<Character> Enemys = new List<Character>();
    public float generateTime = 0.5f;
    private float countTime = 0f;
    private Character player;

    public Character Player { get {
            if (player == null)
            {
                SetCharacter();

            }
            return player;
        } set => player = value; }

    //�@�����̎��Ԃ̐ݒ�A�����炢�܂ŁA�Ԋu�@��x�ɏo�鐔�@���@���
    //�@�ݒ薈�ɕb���J�E���g
    //�@�����ʒu�̓v���C���[����H���ꂽ�ꏊ�A������x�̋����Ȃ�ǂ��ł��@�܂������_����

    //�ݒ薈�Ɏ��̉������f�[�^�ɉ��̑��݂��Ă��邩�Ƃ�������x�̌��x��^�����肷��H
    //������x�L�����͒��g�̍�肪�ȒP�Ȃ��̂ɂ��Čy�ʉ����K�v�Ǝv����

    //���ꂷ����ƍĔz�u����Ƃ�����
    public void SetCharacter()
    {
        Character character = LevelManager.Instance.Players[0];
        if (character != null)
        {
            Player = character;
        }


    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null)
            return;
        if(countTime >= generateTime)
        {
            float x = Random.Range(-30f, 30f);
            float y = Random.Range(-30f, 30f);
            if (Mathf.Abs(x) < 10f)
                x += x >= 0f ? 10f :  -10f;
            if (Mathf.Abs(y) < 10f)
                y += y >= 0f ? 10f : -10f;
            GameObject.Instantiate(Enemys[0], Player.transform.position + new Vector3(x, y, 0f), Quaternion.identity);
            countTime = 0f;
        }
        countTime += Time.deltaTime;

    }
}
