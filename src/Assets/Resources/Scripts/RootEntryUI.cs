using UnityEngine;
using UnityEngine.UI;

public class RootEntryUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI nameText;
    [SerializeField] TMPro.TextMeshProUGUI descText;
    [SerializeField] Image image;
    [SerializeField] TMPro.TextMeshProUGUI costWaterText;
    [SerializeField] TMPro.TextMeshProUGUI costFoodText;
    private RootData info;

    public void SetData( RootData info )
    {
        nameText.text = info.rootName;
        descText.text = info.description;
        costWaterText.text = info.cost.water.ToString();
        costFoodText.text = info.cost.food.ToString();
        image.sprite = info.icon != null ? Utility.CreateSprite( info.icon ) : null;
        this.info = info;
    }

    public void CheckEnabled( Resource res )
    {
        GetComponent<Button>().interactable =
            res.water >= info.cost.water &&
            res.food >= info.cost.food &&
            res.energy >= info.cost.energy;
    }
}
