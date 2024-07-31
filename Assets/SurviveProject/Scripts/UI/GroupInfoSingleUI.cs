using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GroupInfoSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI groupId;
    [SerializeField] private TextMeshProUGUI groupMax;
    [SerializeField] private TextMeshProUGUI enemyIds;
    private SurviveRuleManager surviveRuleManager;
    private EnemyGroupDatas.EnemyGroupData data;

    public void SetGroupSingleUI(EnemyGroupDatas.EnemyGroupData taregt,int index)
    {
        data = taregt;
        groupId.text += index.ToString();
        groupMax.text += taregt.max.ToString();
        if(taregt.enemyIDs != null)
        {
            
            foreach (int id in taregt.enemyIDs)
            {
                enemyIds.text += id.ToString();
                enemyIds.text += "  ";
            }
        }
    }

    private void Awake()
    {
        groupId.text = "GROUP ID: ";
        groupMax.text = "GROUP MAX: ";
        enemyIds.text = "ENEMY ID: \n";
        surviveRuleManager = SurviveGameManager.GetInstance().surviveRuleManager;
    }

    private void Update()
    {
        if(data != null && surviveRuleManager.GetCurrentTime()>=data.endTime)
        {
            Destroy(gameObject);
        }
    }
}
