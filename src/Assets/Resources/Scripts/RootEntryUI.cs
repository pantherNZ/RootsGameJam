using UnityEngine;
using UnityEngine.UI;

public class RootEntryUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI descText;
    [SerializeField] Image image;
    [SerializeField] TMPro.TextMeshProUGUI costWaterText;
    [SerializeField] TMPro.TextMeshProUGUI costFoodText;
    private BaseRoot info;

    public void SetData( BaseRoot info )
    {
        nameText.text = info.name;
        descText.text = info.name;
        costWaterText.text = info.waterCost.ToString();
        costFoodText.text = info.waterCost.ToString();
        image.sprite = info.icon != null ? Utility.CreateSprite( info.icon ) : null;
        this.info = info;
    }

    public void CheckEnabled( int water, int food )
    {
        GetComponent<Button>().interactable = water >= info.waterCost && food >= info.foodCost;
    }
}
