using EroSurvivor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseInChangeCharaId : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] SurviveCharacterSelect surviveCharacterSelect;
    [SerializeField] private int charaId;
    [SerializeField] TitleLaungageChanger changer;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (surviveCharacterSelect.isCharaSelected) return;
        surviveCharacterSelect.SelectChara(charaId);
        changer.NameChange(charaId - 1);
    }
}
