using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalZoneTrigger : MonoBehaviour
{
    [SerializeField] private int zoneIndex;
    [SerializeField] private ElementType currentElementOwner;

    private void Start()
    {
        currentElementOwner = ElementType.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSetup playerSetup = other.gameObject.GetComponent<PlayerSetup>();

            if (currentElementOwner != ElementType.None && currentElementOwner != playerSetup.ElementType)
            {
                ElementalDominationGameManager.Instance.ConvertZone(zoneIndex, (int)playerSetup.ElementType);
                ElementalDominationGameManager.Instance.DeductZoneControl((int)currentElementOwner);
                currentElementOwner = playerSetup.ElementType;
            }
            else if (currentElementOwner == ElementType.None)
            {
                ElementalDominationGameManager.Instance.ConvertZone(zoneIndex, (int)playerSetup.ElementType);
                currentElementOwner = playerSetup.ElementType;
            }
        }
    }
}
